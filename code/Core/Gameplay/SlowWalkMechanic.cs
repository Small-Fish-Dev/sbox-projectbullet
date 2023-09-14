using Sandbox;

namespace ProjectBullet.Core.Gameplay;

public class SlowWalkMechanic : PlayerMechanic
{
	public override int SortOrder => 10;
	public override float? WishSpeed => 110f;
	protected override bool ShouldStart() => Input.Down( InputButton.Run ) && Player.InputDirection.Length != 0;
}
