using Sandbox;

namespace ProjectBullet.Core.Gameplay;

public class Walking : PlayerComponent
{
	public float StopSpeed => 150f;
	public float StepSize => 18.0f;
	public float GroundAngle => 46.0f;
	public float DefaultSpeed => 280f;
	public float WalkSpeed => 140f;
	public float GroundFriction => 4.0f;
	public float MaxNonJumpVelocity => 140.0f;
	public float SurfaceFriction { get; set; } = 1f;
	public float Acceleration => 6f;
	public float DuckAcceleration => 5f;
	public float WishSpeed => 200f;

	// todo(lotuspar): OPTIMIZE!!!
	private Controller Controller => Entity.Controller;

	public void Simulate( IClient cl )
	{
		if ( Controller.GroundEntity != null )
		{
			WalkMove();
		}

		CategorizePosition( Controller.GroundEntity != null );
	}

	public void FrameSimulate( IClient cl )
	{
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

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		Controller.Position = trace.EndPosition;
	}

	private void WalkMove()
	{
		var ctrl = Controller;

		var wishVel = ctrl.GetWishVelocity( true );
		var friction = GroundFriction * SurfaceFriction;

		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
		ctrl.ApplyFriction( StopSpeed, friction );

		var accel = Acceleration;

		ctrl.Velocity = ctrl.Velocity.WithZ( 0 );
		ctrl.Accelerate( wishVel.Normal, wishVel.Length, 0, accel );
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

			ctrl.Move();
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
		if ( Controller.GroundEntity == null )
		{
			return;
		}

		Controller.LastGroundEntity = Controller.GroundEntity;
		Controller.GroundEntity = null;
		SurfaceFriction = 1.0f;
	}

	public void SetGroundEntity( Entity entity )
	{
		Controller.LastGroundEntity = Controller.GroundEntity;
		Controller.LastVelocity = Controller.Velocity;

		Controller.GroundEntity = entity;

		if ( Controller.GroundEntity != null )
		{
			Controller.Velocity = Controller.Velocity.WithZ( 0 );
			Controller.BaseVelocity = Controller.GroundEntity.Velocity;
		}
	}

	public void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		var point = Controller.Position - Vector3.Up * 2;
		var vBumpOrigin = Controller.Position;
		bool bMovingUpRapidly = Controller.Velocity.z > MaxNonJumpVelocity;
		bool bMoveToEndPos = false;

		if ( Controller.GroundEntity != null )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( bMovingUpRapidly )
		{
			ClearGroundEntity();
			return;
		}

		var pm = Controller.TraceBBox( vBumpOrigin, point, 4.0f );

		var angle = Vector3.GetAngle( Vector3.Up, pm.Normal );
		Controller.CurrentGroundAngle = angle;

		if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundEntity();
			bMoveToEndPos = false;

			if ( Controller.Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundEntity( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Controller.Position = pm.EndPosition;
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
