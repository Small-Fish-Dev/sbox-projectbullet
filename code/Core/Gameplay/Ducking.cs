using Sandbox;

namespace ProjectBullet.Core.Gameplay;

public class Ducking : PlayerMechanic
{
	public override int SortOrder => 9;
	public override float? WishSpeed => 120f;
	public override float? EyeHeight => 40f;

	protected override bool ShouldStart() => Input.Down( InputButton.Duck );
}
