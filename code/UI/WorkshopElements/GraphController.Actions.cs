﻿using System;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;

namespace ProjectBullet.UI.WorkshopElements;

public partial class GraphController
{
	public abstract class Action
	{
		/// <summary>
		/// SubActions are grouped with main actions and are all activated at once
		/// </summary>
		public bool IsSubAction = false;

		public virtual string DisplayName => "";
		public abstract object Perform( GraphController root );
		public abstract Action CreateOpposite();
	}

	public class AddNodeToGraphAction : Action
	{
		public override string DisplayName => "Node added";

		private readonly WeaponNode _entity;

		public AddNodeToGraphAction( WeaponNode entity ) => _entity = entity;

		public override object Perform( GraphController root )
		{
			if ( root.GraphInventory.Contains( _entity ) )
			{
				root.GraphInventory.Remove( _entity );
			}

			var node = new Node( root, _entity );
			root._nodes.Add( node );

			root.Visualizer?.AddChild( new GraphNode( node ) );

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

		public override object Perform( GraphController root )
		{
			var node = root.GetNodeByEntity( _entity );
			switch ( node )
			{
				case null:
					Log.Warning( $"Failed {GetType().Name} action! Node not found" );
					return null;
				case EntryNode:
					return null;
			}

			node.Element.Delete();

			root._nodes.Remove( node );

			root.GraphInventory.Add( _entity );

			NodeServer.RemoveNodeFromExecutor( _entity );

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

		public override object Perform( GraphController root )
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

			if ( target == main )
			{
				Log.Warning( $"Failed {GetType().Name} action! Main entity == target entity" );
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

		public override object Perform( GraphController root )
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

			connector.Element?.StateHasChanged();
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

		public override object Perform( GraphController root )
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

		public override object Perform( GraphController root )
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

	public class BuyItemAction : Action
	{
		public override string DisplayName => "Item bought";

		private readonly ShopHostEntity.StockedItem _item;

		public BuyItemAction( ShopHostEntity.StockedItem item ) => _item = item;

		public override object Perform( GraphController root )
		{
			ShopServer.BuyItem( _item );
			return null;
		}

		public override Action CreateOpposite() => new UndoBuyItemAction( _item );
	}

	public class UndoBuyItemAction : Action
	{
		public override string DisplayName => "Item buy undone";

		private readonly ShopHostEntity.StockedItem _item;

		public UndoBuyItemAction( ShopHostEntity.StockedItem item ) => _item = item;

		public override object Perform( GraphController root )
		{
			Log.Warning( "UndoBuyItemAction no impl" );
			return null;
		}

		public override Action CreateOpposite() => new BuyItemAction( _item );
	}

	public class SetNodeLocationAction : Action
	{
		public override string DisplayName => "Node moved";

		private readonly object _target;
		private readonly Vector2 _startPosition;
		private readonly Vector2 _endPosition;

		public SetNodeLocationAction( object target, Vector2 startPosition, Vector2 endPosition )
		{
			_target = target;
			_startPosition = startPosition;
			_endPosition = endPosition;
		}

		public override object Perform( GraphController root )
		{
			switch ( _target )
			{
				case NodeExecutor nodeExecutor:
					nodeExecutor.LastEditorPos = _endPosition;

					root.Entry.Element.UpdateHold( _endPosition, Vector2.One );
					break;
				case WeaponNode node:
					{
						node.LastEditorPos = _endPosition;

						var nodeData = root.GetNodeByEntity( node );
						if ( nodeData == null )
						{
							Log.Warning( $"Failed {GetType().Name} action! Target entity not found" );
							return null;
						}

						nodeData.Element.UpdateHold( _endPosition, Vector2.One );
						break;
					}
				default:
					throw new InvalidOperationException(
						$"SetNodeLocationAction has invalid target type {_target.GetType().Name}" );
			}


			return null;
		}

		public override Action CreateOpposite() => new SetNodeLocationAction( _target, _endPosition, _startPosition );
	}
}
