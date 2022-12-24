using ProjectBullet.Core.Node;

namespace ProjectBullet.UI.Workshop;

public partial class PreInstanceGraph
{
	public abstract class Action
	{
		/// <summary>
		/// SubActions are grouped with main actions and are all activated at once
		/// </summary>
		public bool IsSubAction = false;

		public virtual string DisplayName => "";
		public abstract object Perform( PreInstanceGraph root );
		public abstract Action CreateOpposite();
	}

	public class AddNodeToGraphAction : Action
	{
		public override string DisplayName => "Node added";

		private readonly WeaponNode _entity;

		public AddNodeToGraphAction( WeaponNode entity ) => _entity = entity;

		public override object Perform( PreInstanceGraph root )
		{
			if ( root.GraphInventory.Contains( _entity ) )
			{
				root.GraphInventory.Remove( _entity );
			}

			var node = new Node( root, _entity );
			root._nodes.Add( node );

			root.GraphVisualizer?.AddChild( new GraphNode( node, root.GraphVisualizer ) );

			NodeServer.AddNodeToExecutor( _entity, root.NodeExecutor );

			return node;
		}

		public override Action CreateOpposite() => new RemoveNodeFromGraphAction( _entity );
	}

	public class RemoveNodeFromGraphAction : Action
	{
		public override string DisplayName => "Node removed";

		private readonly WeaponNode _entity;

		public RemoveNodeFromGraphAction( WeaponNode entity ) => _entity = entity;

		public override object Perform( PreInstanceGraph root )
		{
			var node = root.GetNodeByEntity( _entity );
			if ( node == null )
			{
				Log.Warning( $"Failed {GetType().Name} action! Node not found" );
				return null;
			}

			if ( node is EntryNode )
			{
				return null;
			}

			node.Element.Delete();

			root._nodes.Remove( node );

			root.GraphInventory.Add( _entity );

			NodeServer.RemoveNodeFromExecutor( _entity );

			root.GraphVisualizer.InventoryMenu.StateHasChanged();

			return null;
		}

		public override Action CreateOpposite() => new AddNodeToGraphAction( _entity );
	}

	public class ConnectConnectorToNodeAction : Action
	{
		public override string DisplayName => "Connector connected";

		private readonly WeaponNode _main;
		private readonly WeaponNode _target;
		private readonly string _identifier;

		public ConnectConnectorToNodeAction( WeaponNode main, string identifier, WeaponNode target )
		{
			_main = main;
			_identifier = identifier;
			_target = target;
		}

		public override object Perform( PreInstanceGraph root )
		{
			var main = root.GetNodeByEntity( _main );
			if ( main == null )
			{
				Log.Warning( $"Failed {GetType().Name} action! Main entity not found" );
				return null;
			}

			var target = root.GetNodeByEntity( _target );
			if ( target == null )
			{
				Log.Warning( $"Failed {GetType().Name} action! Target entity not found" );
				return null;
			}

			var connector = main.GetConnector( _identifier );
			if ( connector == null )
			{
				Log.Warning( $"Failed {GetType().Name} action! Connector {_identifier} not found" );
				return null;
			}

			connector.ConnectedNode = target;
			target.Previous = connector;

			connector.Element?.StateHasChanged();
			connector.ConnectedNode?.InputElement?.StateHasChanged();

			NodeServer.SetConnector( _main, _identifier, _target );

			Log.Info( $"{GetType().Name} connected {connector}->{target}" );

			return null;
		}

		public override Action CreateOpposite() => new ClearNodeConnectorAction( _main, _identifier );
	}

	public class ClearNodeConnectorAction : Action
	{
		public override string DisplayName => "Connector cleared";

		private readonly WeaponNode _main;
		private readonly string _identifier;
		private WeaponNode _lastTarget;

		public ClearNodeConnectorAction( WeaponNode main, string identifier )
		{
			_main = main;
			_identifier = identifier;
		}

		public override object Perform( PreInstanceGraph root )
		{
			var main = root.GetNodeByEntity( _main );
			if ( main == null )
			{
				Log.Warning( $"Failed {GetType().Name} action! Main entity not found" );
				return null;
			}

			var connector = main.GetConnector( _identifier );
			if ( connector == null )
			{
				Log.Warning( $"Failed {GetType().Name} action! Connector {_identifier} not found" );
				return null;
			}

			var inputElement = connector.ConnectedNode?.InputElement;

			if ( connector.ConnectedNode != null )
			{
				_lastTarget = connector.ConnectedNode.Instance;
				connector.ConnectedNode.Previous = null;
			}

			connector.Element.StateHasChanged();
			inputElement?.StateHasChanged();

			connector.ConnectedNode = null;

			NodeServer.DisconnectConnector( _main, _identifier );

			Log.Info( $"{GetType().Name} disconnected {connector} -/> {_lastTarget}" );

			return null;
		}

		public override Action CreateOpposite() => new ConnectConnectorToNodeAction( _main, _identifier, _lastTarget );
	}

	public class ConnectEntryNodeAction : Action
	{
		public override string DisplayName => "Entry node connected";

		private readonly WeaponNode _target;

		public ConnectEntryNodeAction( WeaponNode target )
		{
			_target = target;
		}

		public override object Perform( PreInstanceGraph root )
		{
			var target = root.GetNodeByEntity( _target );
			if ( target == null )
			{
				Log.Warning( $"Failed {GetType().Name} action! Target entity not found" );
				return null;
			}

			var connector = root.Entry.Connector;
			connector.ConnectedNode = target;
			target.Previous = connector;

			NodeServer.SetEntryNode( root.NodeExecutor, _target );

			connector.Element?.StateHasChanged();
			target.InputElement?.StateHasChanged();

			Log.Info( $"{GetType().Name} connected {connector}->{target}" );

			return null;
		}

		public override Action CreateOpposite() => new DisconnectEntryNodeAction();
	}

	public class DisconnectEntryNodeAction : Action
	{
		public override string DisplayName => "Entry node disconnected";

		private WeaponNode _lastTarget;

		public DisconnectEntryNodeAction()
		{
		}

		public override object Perform( PreInstanceGraph root )
		{
			var connector = root.Entry.Connector;
			var inputElement = connector.ConnectedNode?.InputElement;

			if ( connector.ConnectedNode != null )
			{
				_lastTarget = connector.ConnectedNode.Instance;
				connector.ConnectedNode.Previous = null;
			}

			connector.ConnectedNode = null;

			connector.Element.StateHasChanged();
			inputElement?.StateHasChanged();

			NodeServer.ClearEntryNode( root.NodeExecutor );

			Log.Info( $"{GetType().Name} disconnected {connector} -/> {_lastTarget}" );

			return null;
		}

		public override Action CreateOpposite() => new ConnectEntryNodeAction( _lastTarget );
	}
}
