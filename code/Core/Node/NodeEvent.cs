using Sandbox;

namespace ProjectBullet.Core.Node;

public static partial class NodeEvent
{
	private const string ConnectorChangedEvent = "connectorchanged";

	public class ConnectorChangedAttribute : EventAttribute
	{
		public ConnectorChangedAttribute() : base( ConnectorChangedEvent ) { }
	}

	[ClientRpc]
	public static void OnConnectorChangedRpc()
	{
		// this is just a hacky way to update energies
		Event.Run( ConnectorChangedEvent );
	}
}
