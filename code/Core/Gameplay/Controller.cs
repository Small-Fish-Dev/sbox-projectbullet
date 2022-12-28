using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Core.Gameplay;

/// <summary>
/// Player movement controller - this is mostly from DevulTj's (sbox-template.fps) PlayerController
/// </summary>
public partial class Controller : EntityComponent<Player>, ISingletonComponent
{
	public Vector3 LastVelocity { get; set; }
	public Entity LastGroundEntity { get; set; }
	public Entity GroundEntity { get; set; }
	public Vector3 BaseVelocity { get; set; }
	public Vector3 GroundNormal { get; set; }
	public float CurrentGroundAngle { get; set; }

	public Player Player => Entity;

	/// <summary>
	/// A list of mechanics used by the player controller.
	/// </summary>
	private IEnumerable<PlayerMechanic> Mechanics => Entity.Components.GetAll<PlayerMechanic>();

	/// <summary>
	/// Position accessor for the Player.
	/// </summary>
	public Vector3 Position
	{
		get => Player.Position;
		set => Player.Position = value;
	}

	public Vector3 Velocity
	{
		get => Player.Velocity;
		set => Player.Velocity = value;
	}

	/// <summary>
	/// This will set LocalEyePosition when we Simulate.
	/// </summary>
	private float EyeHeight => BestMechanic?.EyeHeight ?? 64f;

	[Net, Predicted] public float CurrentEyeHeight { get; set; } = 64f;

	private Vector3 MoveInputScale => BestMechanic?.MoveInputScale ?? Vector3.One;

	private float WishSpeed => BestMechanic?.WishSpeed ?? 180f;

	/// <summary>
	/// The "best" mechanic is the mechanic with the highest priority, defined by SortOrder.
	/// </summary>
	private PlayerMechanic BestMechanic =>
		Mechanics.OrderByDescending( x => x.SortOrder ).FirstOrDefault( x => x.IsActive );

	private static float BodyGirth => 32f;

	/// <summary>
	/// The player's hull, we'll use this to calculate stuff like collision.
	/// </summary>
	private BBox Hull
	{
		get
		{
			var girth = BodyGirth * 0.5f;
			var baseHeight = CurrentEyeHeight;

			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, baseHeight * 1.1f );

			return new BBox( mins, maxs );
		}
	}

	public T GetMechanic<T>() where T : PlayerMechanic
	{
		foreach ( var mechanic in Mechanics )
		{
			if ( mechanic is T val ) return val;
		}

		return null;
	}

	public bool IsMechanicActive<T>() where T : PlayerMechanic
	{
		return GetMechanic<T>()?.IsActive ?? false;
	}

	private void SimulateEyes()
	{
		Player.EyeRotation = Player.ViewAngles.ToRotation();
		Player.EyeLocalPosition = Vector3.Up * CurrentEyeHeight;
	}

	private void SimulateMechanics()
	{
		foreach ( var mechanic in Mechanics )
		{
			mechanic.TrySimulate( this );

			if ( Entity == null )
			{
				return;
			}

			if ( !Entity.IsDead )
			{
				continue;
			}

			AdjustEyeHeight();
			return;
		}

		// Adjust eye height
		AdjustEyeHeight();
	}

	private void AdjustEyeHeight()
	{
		var target = EyeHeight;
		var trace = TraceBBox( Position, Position, 0, 10f );
		if ( !trace.Hit || !(target > CurrentEyeHeight) )
		{
			CurrentEyeHeight = CurrentEyeHeight.LerpTo( target, Time.Delta * 10f );
		}
	}

	public virtual void Simulate( IClient cl )
	{
		SimulateEyes();
		SimulateMechanics();
	}

	public virtual void FrameSimulate( IClient cl )
	{
		SimulateEyes();
	}

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	protected virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs,
		float liftFeet = 0.0f,
		float liftHead = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		if ( liftHead > 0 )
		{
			end += Vector3.Up * liftHead;
		}

		var tr = Trace.Ray( start, end )
			.Size( mins, maxs )
			.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
			.WithoutTags( "prop" )
			.Ignore( Player )
			.Run();

		return tr;
	}

	/// <summary>
	/// This calls TraceBBox with the right sized bbox. You should derive this in your controller if you 
	/// want to use the built in functions
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		var hull = Hull;
		return TraceBBox( start, end, hull.Mins, hull.Maxs, liftFeet, liftHead );
	}

	public Vector3 GetWishVelocity( bool zeroPitch = false )
	{
		var result = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 );
		result *= MoveInputScale;

		var inSpeed = result.Length.Clamp( 0, 1 );
		result *= Player.ViewAngles.WithPitch( 0f ).ToRotation();

		if ( zeroPitch )
		{
			result.z = 0;
		}

		result = result.Normal * inSpeed;
		result *= WishSpeed;
		var ang = CurrentGroundAngle.Remap( 0, 45, 1, 0.6f );
		result *= ang;

		return result;
	}

	public void Accelerate( Vector3 wishDir, float wishSpeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishSpeed > speedLimit )
		{
			wishSpeed = speedLimit;
		}

		var currentSpeed = Velocity.Dot( wishDir );
		var addSpeed = wishSpeed - currentSpeed;

		if ( addSpeed <= 0 )
		{
			return;
		}

		var accelSpeed = acceleration * Time.Delta * wishSpeed;

		if ( accelSpeed > addSpeed )
		{
			accelSpeed = addSpeed;
		}

		Velocity += wishDir * accelSpeed;
	}

	public void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
	{
		var speed = Velocity.Length;
		if ( speed.AlmostEqual( 0f ) )
		{
			return;
		}

		if ( BestMechanic?.FrictionOverride != null )
		{
			frictionAmount = BestMechanic.FrictionOverride.Value;
		}

		var control = speed < stopSpeed ? stopSpeed : speed;
		var drop = control * Time.Delta * frictionAmount;

		// Scale the velocity
		var newSpeed = speed - drop;
		if ( newSpeed < 0 ) newSpeed = 0;

		if ( newSpeed == speed )
		{
			return;
		}

		newSpeed /= speed;
		Velocity *= newSpeed;
	}

	public void StepMove( float groundAngle = 46f, float stepSize = 18f )
	{
		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Hull )
			.Ignore( Player )
			.WithoutTags( "player" );
		mover.MaxStandableAngle = groundAngle;

		mover.TryMoveWithStep( Time.Delta, stepSize );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	public void Move( float groundAngle = 46f )
	{
		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Hull )
			.Ignore( Player )
			.WithoutTags( "player" );
		mover.MaxStandableAngle = groundAngle;

		mover.TryMove( Time.Delta );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}
}
