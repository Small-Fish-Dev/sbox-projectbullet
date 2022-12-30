using System;
using Sandbox;

namespace ProjectBullet.Core.Gameplay;

public class Jumping : PlayerMechanic
{
	public override int SortOrder => 25;

	private static float Cooldown => 0.5f;
	private static float Gravity => 700f;

	protected override bool ShouldStart()
	{
		return Input.Pressed( InputButton.Jump ) && Controller.GroundEntity.IsValid();
	}

	protected override void OnStart()
	{
		Simulate();
	}

	protected override void OnStop() => TimeUntilCanStart = Cooldown;

	protected override void Simulate()
	{
		if ( !Controller.GroundEntity.IsValid() )
		{
			return;
		}

		Velocity = Velocity.WithZ( Velocity.z + 250f * 3.0f );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Controller.GetMechanic<Walking>()
			.ClearGroundEntity();

		Controller.Player.PlaySound( "sounds/player/foley/gear/player.jump.gear.sound" );
	}
}
