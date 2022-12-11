using Sandbox;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Client -> server communication for the node system
/// </summary>
public static class NodeCmd
{
	private static WeaponNodeEntity GetWeaponNodeEntity( int networkIdent )
	{
		if ( Entity.FindByIndex( networkIdent ) is WeaponNodeEntity wne )
		{
			return wne;
		}

		return null;
	}

	private static NodeExecutionEntity GetNodeExecutor( int networkIdent )
	{
		if ( Entity.FindByIndex( networkIdent ) is NodeExecutionEntity ne )
		{
			return ne;
		}

		return null;
	}

	[ConCmd.Server]
	private static void SetConnector( int targetNetworkIdent, string identifier, int newValueNetworkIdent )
	{
		Game.AssertServer();

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
	}

	/// <summary>
	/// Send SetConnector request to the server - will connect the provided connector of target to newValue
	/// </summary>
	/// <param name="target">WeaponNodeEntity to change connector of</param>
	/// <param name="identifier">Connector identifier</param>
	/// <param name="newValue">New value</param>
	public static void SetConnector( WeaponNodeEntity target, string identifier, WeaponNodeEntity newValue )
	{
		Game.AssertClient();
		SetConnector( target.NetworkIdent, identifier, newValue.NetworkIdent );
	}

	[ConCmd.Server]
	private static void DisconnectConnector( int targetNetworkIdent, string identifier )
	{
		Game.AssertServer();

		var target = GetWeaponNodeEntity( targetNetworkIdent );
		if ( target == null )
		{
			Log.Error( $"SetConnector failed: target not found - index {targetNetworkIdent}" );
			return;
		}

		target.DisconnectConnector( identifier );
	}

	/// <summary>
	/// Send DisconnectConnector request to the server - will clear the connections of the provided node
	/// </summary>
	/// <param name="target">WeaponNodeEntity to change connector of</param>
	/// <param name="identifier">Connector identifier</param>
	public static void DisconnectConnector( WeaponNodeEntity target, string identifier )
	{
		Game.AssertClient();
		DisconnectConnector( target.NetworkIdent, identifier );
	}

	[ConCmd.Server]
	private static void SetEntryNode( int executorNetworkIdent, int newValueNetworkIdent )
	{
		Game.AssertServer();

		var executor = GetNodeExecutor( executorNetworkIdent );
		if ( executor == null )
		{
			Log.Error( $"SetConnector failed: executor not found - index {executorNetworkIdent}" );
			return;
		}

		var newValue = GetWeaponNodeEntity( newValueNetworkIdent );
		if ( newValue == null )
		{
			Log.Error( $"SetConnector failed: newValue not found - index {newValueNetworkIdent}" );
			return;
		}

		executor.EntryNode = newValue;
	}

	/// <summary>
	/// Send SetEntryNode request to the server - will set the entry node of provided node executor to provided value
	/// </summary>
	/// <param name="nodeExecutionEntity">Node execution entity</param>
	/// <param name="newValue">WeaponNodeEntity value</param>
	public static void SetEntryNode( NodeExecutionEntity nodeExecutionEntity, WeaponNodeEntity newValue )
	{
		Game.AssertClient();
		SetEntryNode( nodeExecutionEntity.NetworkIdent, newValue.NetworkIdent );
	}

	[ConCmd.Server]
	private static void ClearEntryNode( int executorNetworkIdent )
	{
		Game.AssertServer();

		var executor = GetNodeExecutor( executorNetworkIdent );
		if ( executor == null )
		{
			Log.Error( $"SetConnector failed: executor not found - index {executorNetworkIdent}" );
			return;
		}

		executor.EntryNode = null;
	}

	/// <summary>
	/// Send ClearEntryNode request to the server - will clear the entry node of provided node executor
	/// </summary>
	/// <param name="nodeExecutionEntity">Node execution entity</param>
	public static void ClearEntryNode( NodeExecutionEntity nodeExecutionEntity )
	{
		Game.AssertClient();
		ClearEntryNode( nodeExecutionEntity.NetworkIdent );
	}
}
