using Sandbox;

namespace ProjectBullet.Core.Node;

public static partial class NodeEvent
{
	private const string ClientConnectorChangedEvent = "clientconnectorchanged";
	public static partial class Client
	{
		public class ConnectorChangedAttribute : EventAttribute
		{
			public ConnectorChangedAttribute() : base( ClientConnectorChangedEvent ) { }
		}
	}
	
	[ClientRpc]
	public static void ConnectorChangedRpc()
	{
		Event.Run( ClientConnectorChangedEvent );
	}
	
	private const string ServerConnectorChangedEvent = "serverconnectorchanged";
	public static class Server
	{
		public class ConnectorChangedAttribute : EventAttribute
		{
			public ConnectorChangedAttribute() : base( ServerConnectorChangedEvent ) { }
		}
	}

	public static void RunConnectorChanged( Entity player )
	{
		Game.AssertServer();

		Event.Run( ServerConnectorChangedEvent, player );

		ConnectorChangedRpc( To.Single( player ) );
	}
}
