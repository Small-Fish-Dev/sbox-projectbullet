using Sandbox;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Client -> server communication for the node system
/// </summary>
public static class NodeServer
{
	private static WeaponNode GetWeaponNodeEntity( int networkIdent )
	{
		if ( Entity.FindByIndex( networkIdent ) is WeaponNode wne )
		{
			return wne;
		}

		return null;
	}

	private static NodeExecutor GetNodeExecutor( int networkIdent )
	{
		if ( Entity.FindByIndex( networkIdent ) is NodeExecutor ne )
		{
			return ne;
		}

		return null;
	}

	private static Player CallerPlayer => ConsoleSystem.Caller.Pawn as Player;

	[ConCmd.Server]
	private static void SetConnector( int targetNetworkIdent, string identifier, int newValueNetworkIdent )
	{
		Game.AssertServer();

		if ( !CallerPlayer?.CanUseEditor ?? false )
		{
			Log.Info( "SetConnector failed: target not allowed to use editor" );
			return;
		}

		var target = GetWeaponNodeEntity( targetNetworkIdent );
		if ( target == null )
		{
			Log.Error( $"SetConnector failed: target not found - index {targetNetworkIdent}" );
			return;
		}

		var newValue = GetWeaponNodeEntity( newValueNetworkIdent );
		if ( newValue == null )
		{
			Log.Error( $"SetConnector failed: newValue not found - index {targetNetworkIdent}" );
			return;
		}

		target.SetConnector( identifier, newValue );

		Events.Shared.Node.RunConnectorChanged( target.Player );
	}

	/// <summary>
	/// Send SetConnector request to the server - will connect the provided connector of target to newValue
	/// </summary>
	/// <param name="target">WeaponNodeEntity to change connector of</param>
	/// <param name="identifier">Connector identifier</param>
	/// <param name="newValue">New value</param>
	public static void SetConnector( WeaponNode target, string identifier, WeaponNode newValue )
	{
		Game.AssertClient();
		SetConnector( target.NetworkIdent, identifier, newValue.NetworkIdent );
	}

	[ConCmd.Server]
	private static void DisconnectConnector( int targetNetworkIdent, string identifier )
	{
		Game.AssertServer();

		if ( !CallerPlayer?.CanUseEditor ?? false )
		{
			Log.Info( "DisconnectConnector failed: target not allowed to use editor" );
			return;
		}

		var target = GetWeaponNodeEntity( targetNetworkIdent );
		if ( target == null )
		{
			Log.Error( $"DisconnectConnector failed: target not found - index {targetNetworkIdent}" );
			return;
		}

		target.DisconnectConnector( identifier );

		Events.Shared.Node.RunConnectorChanged( target.Player );
	}

	/// <summary>
	/// Send DisconnectConnector request to the server - will clear the connections of the provided node
	/// </summary>
	/// <param name="target">WeaponNodeEntity to change connector of</param>
	/// <param name="identifier">Connector identifier</param>
	public static void DisconnectConnector( WeaponNode target, string identifier )
	{
		Game.AssertClient();
		DisconnectConnector( target.NetworkIdent, identifier );
	}

	[ConCmd.Server]
	private static void SetEntryNode( int executorNetworkIdent, int newValueNetworkIdent )
	{
		Game.AssertServer();

		if ( !CallerPlayer?.CanUseEditor ?? false )
		{
			Log.Info( "SetEntryNode failed: target not allowed to use editor" );
			return;
		}

		var executor = GetNodeExecutor( executorNetworkIdent );
		if ( executor == null )
		{
			Log.Error( $"SetEntryNode failed: executor not found - index {executorNetworkIdent}" );
			return;
		}

		var newValue = GetWeaponNodeEntity( newValueNetworkIdent );
		if ( newValue == null )
		{
			Log.Error( $"SetEntryNode failed: newValue not found - index {newValueNetworkIdent}" );
			return;
		}

		executor.EntryNode = newValue;

		Events.Shared.Node.RunConnectorChanged( executor.Player );
	}

	/// <summary>
	/// Send SetEntryNode request to the server - will set the entry node of provided node executor to provided value
	/// </summary>
	/// <param name="nodeExecutor">Node execution entity</param>
	/// <param name="newValue">WeaponNodeEntity value</param>
	public static void SetEntryNode( NodeExecutor nodeExecutor, WeaponNode newValue )
	{
		Game.AssertClient();
		SetEntryNode( nodeExecutor.NetworkIdent, newValue.NetworkIdent );
	}

	[ConCmd.Server]
	private static void ClearEntryNode( int executorNetworkIdent )
	{
		Game.AssertServer();

		if ( !CallerPlayer?.CanUseEditor ?? false )
		{
			Log.Info( "ClearEntryNode failed: target not allowed to use editor" );
			return;
		}

		var executor = GetNodeExecutor( executorNetworkIdent );
		if ( executor == null )
		{
			Log.Error( $"ClearEntryNode failed: executor not found - index {executorNetworkIdent}" );
			return;
		}

		executor.EntryNode = null;

		Events.Shared.Node.RunConnectorChanged( executor.Player );
	}

	/// <summary>
	/// Send ClearEntryNode request to the server - will clear the entry node of provided node executor
	/// </summary>
	/// <param name="nodeExecutor">Node execution entity</param>
	public static void ClearEntryNode( NodeExecutor nodeExecutor )
	{
		Game.AssertClient();
		ClearEntryNode( nodeExecutor.NetworkIdent );
	}

	[ConCmd.Server]
	private static void AddNodeToExecutor( int targetNetworkIdent, int executorNetworkIdent )
	{
		Game.AssertServer();

		if ( !CallerPlayer?.CanUseEditor ?? false )
		{
			Log.Info( "AddNodeToExecutor failed: target not allowed to use editor" );
			return;
		}

		var target = GetWeaponNodeEntity( targetNetworkIdent );
		if ( target == null )
		{
			Log.Error( $"AddNodeToExecutor failed: target not found - index {targetNetworkIdent}" );
			return;
		}

		var executor = GetNodeExecutor( executorNetworkIdent );
		if ( executor == null )
		{
			Log.Error( $"AddNodeToExecutor failed: executor not found - index {executorNetworkIdent}" );
			return;
		}

		target.Owner = executor;
		target.Parent = executor;

		Events.Shared.Node.RunConnectorChanged( executor.Player );
	}

	/// <summary>
	/// Send AddNodeToExecutor request to the server - will set owner of target to the executor
	/// </summary>
	/// <param name="target">WeaponNodeEntity to change owner of</param>
	/// <param name="nodeExecutor">New owner</param>
	public static void AddNodeToExecutor( WeaponNode target, NodeExecutor nodeExecutor )
	{
		Game.AssertClient();
		AddNodeToExecutor( target.NetworkIdent, nodeExecutor.NetworkIdent );
	}

	[ConCmd.Server]
	private static void RemoveNodeFromExecutor( int targetNetworkIdent )
	{
		Game.AssertServer();

		if ( !CallerPlayer?.CanUseEditor ?? false )
		{
			Log.Info( "RemoveNodeFromExecutor failed: target not allowed to use editor" );
			return;
		}

		var target = GetWeaponNodeEntity( targetNetworkIdent );
		if ( target == null )
		{
			Log.Error( $"RemoveNodeFromExecutor failed: target not found - index {targetNetworkIdent}" );
			return;
		}

		// find the pawn...
		if ( target.Owner is not NodeExecutor { Owner: Player } executor )
		{
			Log.Error( "RemoveNodeFromExecutor failed: Owner.Owner not a Player" );
			return;
		}

		target.Owner = executor.Owner;
		target.Parent = executor.Owner;

		Events.Shared.Node.RunConnectorChanged( executor.Player );
	}

	/// <summary>
	/// Send RemoveNodeFromExecutor request to the server - will set owner of target back to pawn (if possible)
	/// </summary>
	/// <param name="target">WeaponNodeEntity to change owner of</param>
	public static void RemoveNodeFromExecutor( WeaponNode target )
	{
		Game.AssertClient();
		RemoveNodeFromExecutor( target.NetworkIdent );
	}
}
