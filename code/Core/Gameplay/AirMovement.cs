using Sandbox;

namespace ProjectBullet.Core.Gameplay;

public class AirMovement : PlayerMechanic
{
	private static float Gravity => 700.0f;
	private static float AirControl => 30.0f;
	private static float AirAcceleration => 300.0f;

	protected override void Simulate()
	{
		var ctrl = Controller;
		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		ctrl.Velocity += new Vector3( 0, 0, ctrl.BaseVelocity.z ) * Time.Delta;
		ctrl.BaseVelocity = ctrl.BaseVelocity.WithZ( 0 );
		var airVelocity = ctrl.Velocity;

		if ( GroundEntity.IsValid() )
		{
			return;
		}

		var wishVel = ctrl.GetWishVelocity( true );
		ctrl.Accelerate( wishVel.Normal, wishVel.Length, AirControl, AirAcceleration );
		ctrl.Velocity += ctrl.BaseVelocity;

		ctrl.Move();

		ctrl.Velocity -= ctrl.BaseVelocity;
		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		// set GroundEntity etc
		ctrl.GetMechanic<Walking>().CategorizePosition( false );

		// check if we just landed....
		if ( GroundEntity.IsValid() && airVelocity.Abs().z > 600 )
		{
			Player.Health -= 10.0f;
		}
	}

	protected override bool ShouldStart() => true;
}
