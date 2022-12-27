using Sandbox;

namespace ProjectBullet.Core.Gameplay;

public class Walking : PlayerMechanic
{
	private static float StopSpeed => 150f;
	private static float StepSize => 18.0f;
	private static float GroundAngle => 46.0f;
	private static float GroundFriction => 4.0f;
	private static float MaxNonJumpVelocity => 140.0f;
	private float SurfaceFriction { get; set; } = 1f;
	private static float Acceleration => 6f;

	protected override bool ShouldStart()
	{
		return true;
	}

	public override float? WishSpeed => 200f;

	protected override void Simulate()
	{
		if ( GroundEntity != null )
		{
			WalkMove();
		}

		CategorizePosition( Controller.GroundEntity != null );
	}

	/// <summary>
	/// Try to keep a walking player on the ground when running down slopes etc.
	/// </summary>
	private void StayOnGround()
	{
		var start = Controller.Position + Vector3.Up * 2;
		var end = Controller.Position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = Controller.TraceBBox( Controller.Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = Controller.TraceBBox( start, end );

		if ( trace.Fraction is <= 0 or >= 1 )
		{
			return;
		}

		if ( trace.StartedSolid )
		{
			return;
		}

		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle )
		{
			return;
		}

		Controller.Position = trace.EndPosition;
	}

	private void WalkMove()
	{
		var ctrl = Controller;

		var wishVel = ctrl.GetWishVelocity( true );
		var friction = GroundFriction * SurfaceFriction;

		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
		ctrl.ApplyFriction( StopSpeed, friction );

		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
		ctrl.Accelerate( wishVel.Normal, wishVel.Length, 0, Acceleration );
		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );

		// Add in any base velocity to the current velocity.
		ctrl.Velocity += ctrl.BaseVelocity;

		try
		{
			if ( ctrl.Velocity.Length < 1.0f )
			{
				ctrl.Velocity = Vector3.Zero;
				return;
			}

			var dest = (ctrl.Position + ctrl.Velocity * Time.Delta).WithZ( ctrl.Position.z );
			var pm = ctrl.TraceBBox( ctrl.Position, dest );

			if ( pm.Fraction == 1 )
			{
				ctrl.Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			ctrl.StepMove();
		}
		finally
		{
			ctrl.Velocity -= ctrl.BaseVelocity;
		}

		StayOnGround();
	}

	/// <summary>
	/// We're no longer on the ground, remove it
	/// </summary>
	public void ClearGroundEntity()
	{
		if ( GroundEntity == null ) return;

		LastGroundEntity = GroundEntity;
		GroundEntity = null;
		SurfaceFriction = 1.0f;
	}

	private void SetGroundEntity( Entity entity )
	{
		LastGroundEntity = GroundEntity;
		LastVelocity = Velocity;

		GroundEntity = entity;

		if ( GroundEntity == null )
		{
			return;
		}

		Velocity = Velocity.WithZ( 0 );
		Controller.BaseVelocity = GroundEntity.Velocity;
	}

	public void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		var point = Position - Vector3.Up * 2;
		var bumpOrigin = Position;
		var isMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
		var shouldMoveToEndPos = false;

		if ( GroundEntity != null )
		{
			shouldMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			shouldMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( isMovingUpRapidly )
		{
			ClearGroundEntity();
			return;
		}

		var pm = Controller.TraceBBox( bumpOrigin, point, 4.0f );

		var angle = Vector3.GetAngle( Vector3.Up, pm.Normal );
		Controller.CurrentGroundAngle = angle;

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			shouldMoveToEndPos = false;

			if ( Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( shouldMoveToEndPos && pm is { StartedSolid: false, Fraction: > 0.0f and < 1.0f } )
		{
			Position = pm.EndPosition;
		}
	}

	private void UpdateGroundEntity( TraceResult tr )
	{
		Controller.GroundNormal = tr.Normal;

		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		SetGroundEntity( tr.Entity );
	}
}
