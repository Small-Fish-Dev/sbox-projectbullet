using System;
using System.Linq;
using System.Text.Json;
using ProjectBullet.Core.Node.Description;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Core.Node;

public static class NodeCmd
{
	[ConCmd.Server( "pb_update_executor" )]
	public static void UpdateExecutor( int executorNetworkIdent, string json )
	{
		var executor =
			(ConsoleSystem.Caller.Pawn as BasePlayer)!.NodeExecutors.SingleOrDefault( v =>
				v.NetworkIdent == executorNetworkIdent );

		if ( executor == null )
		{
			Log.Warning(
				$"{ConsoleSystem.Caller.Name} tried to update executor but executor was not found - index {executorNetworkIdent}" );
			return;
		}

		// Try to parse JSON
		SerializableGraph graph = null;
		try
		{
			graph = SerializableGraph.Deserialize( json, executor );
		}
		catch ( Exception e )
		{
			Log.Warning( $"Failed to parse serialized executor from {ConsoleSystem.Caller.Name}" );
			Log.Info( e );
			return;
		}

		foreach ( var graphConnector in graph.Connectors )
		{
			Log.Info( $"connector {graphConnector}: {graphConnector.Identifier}" );
		}

		foreach ( var weaponNode in graph.Nodes )
		{
			if ( weaponNode.InventoryItemUid == Guid.Empty )
			{
				// this is the fake starting node
				// so next node should be entry

				Log.Info( "entry connectors" );
				foreach ( var weaponNodeConnector in weaponNode.Connectors )
				{
					Log.Info(
						$"cn {weaponNodeConnector} {weaponNodeConnector.Identifier} {weaponNodeConnector.Connected} {weaponNodeConnector.ConnectedNode}"
					);
				}

				var connector = weaponNode.Connectors.Single();
				if ( connector.ConnectedNode == null )
				{
					Log.Info( "entry node not connected to anything, returning now" );
					return;
				}

				executor.EntryNode = connector.ConnectedNode.Instance;
				continue;
			}

			weaponNode.Instance?.ResetConnections();
			Log.Info( $"!! {weaponNode.DisplayName}" );

			foreach ( var connector in weaponNode.Connectors )
			{
				if ( connector.ConnectedNode != null )
				{
					Log.Info(
						$"setting {weaponNode.DisplayName}->{connector.Identifier} to {connector.ConnectedNode.DisplayName}" );
					weaponNode.Instance?.SetConnector( connector.Identifier, connector.ConnectedNode.Instance );
				}
			}

			if ( weaponNode.Instance != null )
			{
				weaponNode.Instance.Owner = executor;
			}
		}
	}

	public static void UpdateExecutor( NodeExecutionEntity executor, string json ) =>
		UpdateExecutor( executor.NetworkIdent, json );
}
