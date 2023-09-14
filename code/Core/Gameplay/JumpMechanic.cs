using Sandbox;

namespace ProjectBullet.Core.Gameplay;

public class JumpMechanic : PlayerMechanic
{
	public override int SortOrder => 25;

	private static float Cooldown => 0.5f;
	private static float Gravity => 700f;
	private static float JumpMultiplier => 1.0f;

	protected override bool ShouldStart() => Input.Pressed( InputButton.Jump ) && Controller.GroundEntity.IsValid();

	protected override void OnStart() => Simulate();

	protected override void OnStop() => TimeUntilCanStart = Cooldown;

	protected override void Simulate()
	{
		if ( !Controller.GroundEntity.IsValid() )
		{
			return;
		}

		Velocity = Velocity.WithZ( Velocity.z + 250f * JumpMultiplier );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<WalkMechanic>()
			.ClearGroundEntity();
	}
}
