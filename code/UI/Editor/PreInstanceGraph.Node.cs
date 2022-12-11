using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectBullet.Core.Node;

namespace ProjectBullet.UI.Editor;

public partial class PreInstanceGraph
{
	public class Node
	{
		public GraphNode Element;
		public GraphNodeIn InputElement;

		public Connector Previous;
		public bool IsConnected => Previous != null;
		public readonly WeaponNodeEntity Instance = null;

		public virtual string DisplayName { get; }

		public PreInstanceGraph Root { get; }

		protected readonly List<Connector> _connectors = new();
		public IEnumerable<Connector> Connectors => _connectors.AsReadOnly();

		/// <summary>
		/// Get connector by identifier
		/// </summary>
		/// <param name="identifier">Connector ID</param>
		/// <returns>Connector or null</returns>
		public Connector GetConnector( string identifier ) =>
			_connectors.SingleOrDefault( v => v.Attribute.Identifier == identifier );

		protected Node( PreInstanceGraph root )
		{
			Root = root;

			DisplayName = "Unknown";
		}

		public Node( PreInstanceGraph root, WeaponNodeEntity instance )
		{
			Root = root;
			Instance = instance;

			if ( Instance?.Description == null )
			{
				return;
			}

			DisplayName = Instance.Description.NodeAttribute?.DisplayName ?? "Unknown Node";

			foreach ( var connectorAttribute in Instance.Description.ConnectorAttributes )
			{
				_connectors.Add( new Connector( this, connectorAttribute ) );
			}
		}
	}

	public class EntryNode : Node
	{
		public override string DisplayName => "Entry";
		public EntryConnector Connector { get; init; }

		public EntryNode( PreInstanceGraph root ) : base( root )
		{
			Connector = new EntryConnector( this );
			_connectors.Add( Connector );
		}
	}
}
