﻿using ProjectBullet.Core.Node;

namespace ProjectBullet.UI.WorkshopElements;

public partial class GraphController
{
	public class Connector
	{
		/// <summary>
		/// Parent of this connector / container for this connector
		/// </summary>
		public Node Parent { get; init; }

		public virtual string DisplayName { get; }
		public GraphNodeOut Element;
		public readonly ConnectorAttribute Attribute;
		public virtual string Identifier => Attribute.Identifier;

		public GraphController Root => Parent.Root;

		/// <summary>
		/// The node that this connector is connected to
		/// </summary>
		public Node ConnectedNode;

		public bool IsConnected => ConnectedNode != null;

		public void Disconnect( bool isSubAction = false )
		{
			if ( Parent is EntryNode )
			{
				Root.PerformAction( new DisconnectEntryNodeAction() { IsSubAction = isSubAction }, true );
			}
			else
			{
				Root.PerformAction(
					new ClearNodeConnectorAction( Parent.Instance, Identifier ) { IsSubAction = isSubAction }, true );
			}
		}

		public void ConnectTo( Node node, bool isSubAction = false )
		{
			if ( Parent is EntryNode )
			{
				Root.PerformAction( new ConnectEntryNodeAction( node.Instance ) { IsSubAction = isSubAction }, true );
			}
			else
			{
				Root.PerformAction(
					new ConnectConnectorToNodeAction( Parent.Instance, Identifier, node.Instance )
					{
						IsSubAction = isSubAction
					},
					true );
			}
		}

		public virtual float? EstimateConnectorOutput() =>
			Parent.Instance.EstimateConnectorOutput( Identifier, Root.NodeExecutor, true );

		public Connector( Node parent, ConnectorAttribute attribute )
		{
			Parent = parent;
			Attribute = attribute;
			DisplayName = attribute.DisplayName;
		}

		protected Connector( Node parent )
		{
			Parent = parent;
			DisplayName = "Unknown";
		}
	}

	public class EntryConnector : Connector
	{
		public override string DisplayName => "Start";
		public override string Identifier => "on_entry";

		public override float? EstimateConnectorOutput() => Root.NodeExecutor.MaxEnergy;

		public EntryConnector( Node parent ) : base( parent )
		{
		}
	}
}
