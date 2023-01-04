using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectBullet.Core.Node;

namespace ProjectBullet.UI.WorkshopElements;

public partial class GraphController
{
	public class Node
	{
		public GraphNode Element;
		public GraphNodeIn InputElement;

		public Connector Previous;
		public bool IsConnected => Previous != null;
		public readonly WeaponNode Instance;

		public virtual string DisplayName { get; init; }
		public virtual string UsageInfo { get; init; }

		public GraphController Root { get; }

		protected readonly List<Connector> _connectors = new();
		public IEnumerable<Connector> Connectors => _connectors.AsReadOnly();

		/// <summary>
		/// Get connector by identifier
		/// </summary>
		/// <param name="identifier">Connector ID</param>
		/// <returns>Connector or null</returns>
		public Connector GetConnector( string identifier ) =>
			_connectors.SingleOrDefault( v => v.Attribute.Identifier == identifier );

		protected Node( GraphController root )
		{
			Root = root;

			DisplayName = "Unknown";
		}

		public Node( GraphController root, WeaponNode instance )
		{
			Root = root;
			Instance = instance;

			if ( Instance?.Description == null )
			{
				return;
			}

			DisplayName = Instance.Description.NodeAttribute?.DisplayName ?? "Unknown Node";
			UsageInfo = Instance.Description.NodeAttribute?.UsageInfo ?? "Unknown Node";

			foreach ( var connectorAttribute in Instance.Description.ConnectorAttributes )
			{
				_connectors.Add( new Connector( this, connectorAttribute ) );
			}
		}
	}

	public class EntryNode : Node
	{
		public EntryConnector Connector;

		public EntryNode( GraphController root ) : base( root )
		{
			Connector = new EntryConnector( this );
			_connectors.Add( Connector );

			DisplayName = root.NodeExecutor.DisplayName;
			UsageInfo = root.NodeExecutor.UsageInfo;
		}
	}
}
