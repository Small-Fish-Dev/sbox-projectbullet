using Sandbox;

namespace ProjectBullet.Core;

public static class GameEvent
{
	private const string PlayerNodesUpdate = "playernodesupdate";

	public class PlayerNodesUpdateAttribute : EventAttribute
	{
		public PlayerNodesUpdateAttribute() : base( PlayerNodesUpdate ) { }
	}
}
