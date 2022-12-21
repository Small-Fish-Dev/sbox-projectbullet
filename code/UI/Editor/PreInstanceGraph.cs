using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core;
using ProjectBullet.Player;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.UI.Editor;

public partial class PreInstanceGraph
{
	private readonly List<Node> _nodes = new();

	/// <summary>
	/// Visualizer - this could be null
	/// </summary>
	public GraphVisualizer GraphVisualizer { get; set; }

	/// <summary>
	/// Current node executor being visualised
	/// </summary>
	public NodeExecutionEntity NodeExecutor { get; set; }

	/// <summary>
	/// Node inventory - not always up to date
	/// </summary>
	public List<WeaponNode> GraphInventory { get; set; } = new();

	/// <summary>
	/// Read-only list of created nodes
	/// </summary>
	public IEnumerable<Node> Nodes => _nodes.AsReadOnly();

	/// <summary>
	/// List of connectors - note that this uses the Nodes list so it won't work after being deserialized.
	/// </summary>
	public IEnumerable<Connector> Connectors => Nodes.SelectMany( v => v.Connectors );

	/// <summary>
	/// Find node by WeaponNodeEntity instance
	/// </summary>
	/// <param name="entity">WeaponNodeEntity</param>
	/// <returns>Node or null</returns>
	public Node GetNodeByEntity( WeaponNode entity ) => _nodes.SingleOrDefault( v => v.Instance == entity );

	public EntryNode Entry { get; }

	public PreInstanceGraph( NodeExecutionEntity nodeExecutor, GraphVisualizer graphVisualizer )
	{
		Event.Register( this );

		GraphVisualizer = graphVisualizer;

		// Create entry node
		Entry = new EntryNode( this );
		_nodes.Add( Entry );

		NodeExecutor = nodeExecutor;

		// Add nodes from inventory
		var inventory = (Game.LocalPawn as BasePlayer)?.Inventory;
		if ( inventory != null )
		{
			foreach ( var weaponNodeEntity in inventory.Items.OfType<WeaponNode>() )
			{
				if ( !weaponNodeEntity.InUse )
				{
					// Add inactive nodes to GraphInventory
					GraphInventory.Add( weaponNodeEntity );
				}
				else
				{
					// Add active nodes to the graph!
					if ( weaponNodeEntity.Owner == NodeExecutor )
					{
						PerformAction( new AddNodeToGraphAction( weaponNodeEntity ), false );
					}
				}
			}
		}

		// Resolve node links
		foreach ( var node in _nodes )
		{
			foreach ( var connector in node.Connectors )
			{
				var instance = node.Instance?.GetConnectedNode( connector.Identifier );
				if ( instance != null )
				{
					PerformAction( new ConnectConnectorToNodeAction( node.Instance, connector.Identifier, instance ),
						false );
				}
			}
		}

		// Set entry node if possible
		if ( nodeExecutor?.EntryNode == null )
		{
			Log.Warning( "Provided NodeExecutionEntity has no entry node" );
			return;
		}

		// todo: why is this commented?? what else is adding the entry node to the graph???
		//PerformAction( new AddNodeToGraphAction( nodeExecutor.EntryNode ), false );
		PerformAction( new ConnectEntryNodeAction( nodeExecutor.EntryNode ), false );
	}

	~PreInstanceGraph()
	{
		Event.Unregister( this );
	}

	private readonly List<Action> _actionHistory = new();
	private int _actionPointer = 0;

	public object PerformAction( Action action, bool addToHistory )
	{
		if ( addToHistory )
		{
			_actionHistory.Add( action );
		}

		return action.Perform( this );
	}

	public void AddToGraph( WeaponNode entity )
	{
		PerformAction( new AddNodeToGraphAction( entity ), true );
	}

	public void RemoveFromGraph( Node node )
	{
		if ( node is EntryNode )
		{
			return;
		}

		foreach ( var connector in node.Connectors )
		{
			connector.Disconnect( true );
		}

		node.Previous?.Disconnect( true );

		PerformAction( new RemoveNodeFromGraphAction( node.Instance ), true );
	}

	[Events.Client.Node.ConnectorChanged]
	public void EstimateAllEnergyOutputs()
	{
		foreach ( var connector in Connectors )
		{
			connector.EstimateEnergyOutput();

			connector.Parent?.Element.StateHasChanged();

			connector.Element?.StateHasChanged();

			connector.ConnectedNode?.InputElement?.StateHasChanged();
		}
	}

	/// <summary>
	/// Update on new inventory item event
	/// </summary>
	/// <param name="entity">New item entity</param>
	[Events.Client.Workshop.NewItem]
	private void OnNewItem( WeaponNode entity )
	{
		GraphInventory.Add( entity );
		GraphVisualizer.InventoryMenu.StateHasChanged();
	}
}
