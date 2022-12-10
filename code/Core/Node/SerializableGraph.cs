using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectBullet.Core.Shop;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.Core.Node;

public class SerializableGraph
{
	public class Connector
	{
		public struct EditorDataContainer
		{
			public Panel Element;
		}

		/// <summary>
		/// Parent of this connector - this needs to be set by the WeaponNode
		/// </summary>
		[JsonIgnore]
		public WeaponNode Parent { get; private set; }

		public ConnectorAttribute ConnectorAttribute;

		public virtual float? GetEnergyOutput()
		{
			if ( ConnectorAttribute == null )
			{
				return null;
			}

			if ( ConnectorAttribute.EnergyOutAmount != 0.0f )
			{
				return ConnectorAttribute.EnergyOutAmount;
			}

			var previousOutput = Parent.Previous?.GetEnergyOutput();
			if ( previousOutput == null )
			{
				return null;
			}

			previousOutput -= Parent.Instance?.EnergyUsage ?? 0.0f;

			return previousOutput * ConnectorAttribute.EnergyOutPercentage;
		}

		/// <summary>
		/// Connector identifier - this is set in <see cref="ConnectorAttribute"/>
		/// </summary>
		public string Identifier { get; set; }

		public EditorDataContainer EditorData;

		public WeaponNode ConnectedNode { get; set; }

		[JsonIgnore] public bool Connected => ConnectedNode != null;

		/// <summary>
		/// Connect to a node
		/// </summary>
		/// <param name="node">Node</param>
		public void Connect( WeaponNode node )
		{
			ConnectedNode = node;
			node.Previous = this;
		}

		/// <summary>
		/// Disconnect from a node
		/// </summary>
		public void Disconnect()
		{
			if ( !Connected )
			{
				return;
			}

			ConnectedNode.Previous = null;
			ConnectedNode = null;
		}

		[JsonConstructor]
		public Connector( string identifier, WeaponNode connectedNode )
		{
			Identifier = identifier;
			ConnectedNode = connectedNode;
		}

		public Connector( WeaponNode parent, ConnectorAttribute attribute )
		{
			Parent = parent;
			Identifier = attribute.Identifier;
			ConnectorAttribute = attribute;
		}

		protected Connector( WeaponNode parent )
		{
			Parent = parent;
		}
	}

	public class WeaponNode
	{
		public struct EditorDataContainer
		{
			public Panel Element;
			public Panel InputElement;
			public int LastX;
			public int LastY;
		}

		[JsonIgnore] public Connector Previous { get; set; }

		public Guid InventoryItemUid { get; private set; }

		public List<Connector> Connectors { get; set; } = new();

		[JsonIgnore] public virtual string DisplayName => Instance.Description.NodeAttribute.DisplayName;

		/// <summary>
		/// WeaponNodeEntity for this weapon node
		/// </summary>
		[JsonIgnore]
		public WeaponNodeEntity Instance { get; set; }

		public EditorDataContainer EditorData;

		/// <summary>
		/// The parent / root graph
		/// </summary>
		public SerializableGraph Graph;

		private void CreateConnectors()
		{
			foreach ( var connectorAttribute in Instance.Description.ConnectorAttributes )
			{
				Connectors.Add( new Connector( this, connectorAttribute ) );
			}
		}

		/// <summary>
		/// Whether or not this node is isolated from other nodes
		/// </summary>
		[JsonIgnore]
		public bool Isolated => Previous == null && Connectors.All( v => !v.Connected );

		/// <summary>
		/// Cut this node off from other nodes
		/// </summary>
		public void Isolate()
		{
			// first, set our connector instances to null...
			foreach ( var connector in Connectors )
			{
				connector.Disconnect();
			}

			// disconnect from previous connector
			Previous?.Disconnect();
		}

		protected WeaponNode( SerializableGraph graph )
		{
			Graph = graph;
			Instance = null;
			InventoryItemUid = Guid.Empty;
		}

		public WeaponNode( SerializableGraph graph, WeaponNodeEntity weaponNodeEntity )
		{
			Graph = graph;
			Instance = weaponNodeEntity;
			InventoryItemUid = weaponNodeEntity?.Uid ?? Guid.Empty;

			CreateConnectors();

			foreach ( var connector in Connectors )
			{
				var connectedNodeInstance = Instance.GetConnectedNode( connector.Identifier );
				if ( connectedNodeInstance == null )
				{
					continue;
				}

				connector.Connect( Graph.AddNode( connectedNodeInstance ) );
			}
		}

		[JsonConstructor]
		public WeaponNode( Guid inventoryItemUid )
		{
			InventoryItemUid = inventoryItemUid;

			if ( InventoryItemUid != Guid.Empty )
			{
				// this is an inventory item
				var item = Inventory.FindAny( InventoryItemUid );

				// make sure it's of type WeaponNodeEntity
				if ( item is not WeaponNodeEntity wne )
				{
					throw new Exception( $"No inventory item found with UID {inventoryItemUid}" );
				}

				Instance = wne;
			}
		}
	}

	public class EntryConnector : Connector
	{
		public float EnergyOutput = 100.0f;

		public override float? GetEnergyOutput() => EnergyOutput;

		[JsonConstructor]
		public EntryConnector( WeaponNode connectedNode ) : base( null )
		{
			Identifier = "entry";
			ConnectedNode = connectedNode;
		}

		public EntryConnector( string identifier, WeaponNode weaponNode ) : base( weaponNode )
		{
			Identifier = identifier;
		}
	}

	public class EntryWeaponNode : WeaponNode
	{
		public EntryConnector EntryConnector;

		public override string DisplayName => "Entry";

		public EntryWeaponNode( SerializableGraph graph ) : base( graph )
		{
			EntryConnector = new EntryConnector( this );
			Connectors.Add( EntryConnector );
		}

		[JsonConstructor]
		public EntryWeaponNode( Guid inventoryItemUid ) : base( inventoryItemUid )
		{
		}
	}

	/// <summary>
	/// List of created notes
	/// </summary>
	[JsonIgnore]
	public List<WeaponNode> Nodes { get; } = new();

	/// <summary>
	/// List of connectors - note that this uses the Nodes list so it won't work after being deserialized.
	/// </summary>
	[JsonIgnore]
	public IEnumerable<Connector> Connectors => Nodes.SelectMany( v => v.Connectors );

	/// <summary>
	/// All nodes that aren't isolated. This is the opposite of <see cref="IsolatedNodes"/>
	/// </summary>
	[JsonIgnore]
	public IEnumerable<WeaponNode> UsedNodes => Nodes.Where( v => !v.Isolated );

	/// <summary>
	/// All isolated nodes. This is the opposite of <see cref="UsedNodes"/>
	/// </summary>
	[JsonIgnore]
	public IEnumerable<WeaponNode> IsolatedNodes => Nodes.Where( v => v.Isolated );

	public EntryWeaponNode EntryNode { get; set; }

	public SerializableGraph()
	{
		EntryNode = new EntryWeaponNode( this );
		Nodes.Add( EntryNode );
	}

	/// <summary>
	/// Add a node to the SerializableGraph
	/// </summary>
	/// <param name="weaponNodeEntity">Related <see cref="WeaponNodeEntity"/></param>
	/// <returns>New WeaponNode</returns>
	public WeaponNode AddNode( WeaponNodeEntity weaponNodeEntity )
	{
		var node = new WeaponNode( this, weaponNodeEntity );
		Nodes.Add( node );
		return node;
	}

	/// <summary>
	/// Remove a node 
	/// </summary>
	/// <param name="node">Node to remove</param>
	public void RemoveNode( WeaponNode node )
	{
		node.Isolate();

		node.Graph = null;

		Nodes.Remove( node );
	}

	public static SerializableGraph CreateFromNodeExecutor( NodeExecutionEntity nodeExecutionEntity )
	{
		var result = new SerializableGraph();
		if ( nodeExecutionEntity.EntryNode == null )
		{
			Log.Warning( "Provided NodeExecutionEntity has no entry node" );
			return result;
		}

		var node = result.AddNode( nodeExecutionEntity.EntryNode );
		result.EntryNode.EntryConnector.Connect( node );
		return result;
	}

	public static SerializableGraph Deserialize( string json, NodeExecutionEntity nodeExecutor = null )
	{
		var result = JsonSerializer.Deserialize<SerializableGraph>( json );

		void AddNode( WeaponNode weaponNode )
		{
			result.Nodes.Add( weaponNode );
			foreach ( var connector in weaponNode.Connectors.Where( connector => connector.ConnectedNode != null ) )
			{
				AddNode( connector.ConnectedNode );
			}
		}

		AddNode( result.EntryNode );

		if ( nodeExecutor != null )
		{
			result.EntryNode.Instance = nodeExecutor.EntryNode;
		}

		return result;
	}

	public string Serialize() => JsonSerializer.Serialize( this );
}
