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
	}

	protected override bool ShouldStart() => true;
}
