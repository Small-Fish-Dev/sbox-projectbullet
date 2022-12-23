using Sandbox;

namespace ProjectBullet.Core.Gameplay;

/// <summary>
/// Player movement controller - inspired by / using code from DevulTj's sbox-template.fps
/// </summary>
public partial class Controller : PlayerComponent
{
	public Vector3 LastVelocity { get; set; }
	public Entity LastGroundEntity { get; set; }
	public Entity GroundEntity { get; set; }
	public Vector3 BaseVelocity { get; set; }
	public Vector3 GroundNormal { get; set; }
	public float CurrentGroundAngle { get; set; }

	public Player Player => Entity;

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

	protected virtual float BodyGirth => 32f;
	[Net, Predicted] protected float CurrentEyeHeight { get; set; } = 64f;

	/// <summary>
	/// The player's hull, we'll use this to calculate stuff like collision.
	/// </summary>
	public BBox Hull
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

	/// <summary>
	/// Apply friction to the player controller / velocity
	/// </summary>
	public void ApplyFriction( float stopSpeed, float frictionAmount = 1.0f )
	{
		var speed = Velocity.Length;
		if ( speed.AlmostEqual( 0f ) )
		{
			return;
		}

		var control = speed < stopSpeed ? stopSpeed : speed;
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity...
		var newSpeed = speed - drop;
		if ( newSpeed < 0 )
		{
			newSpeed = 0;
		}

		if ( newSpeed == speed )
		{
			return;
		}

		// set velocity ->
		Velocity *= newSpeed / speed;
	}

	/// <summary>
	/// Accelerate player controller / velocity
	/// </summary>
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

	public void Move()
	{
		// build MoveHelper...
		var helper = new MoveHelper( Position, Velocity ) { MaxStandableAngle = 46.0f };

		helper.Trace = helper.Trace.Size( Hull )
			.Ignore( Player )
			.IncludeClientside()
			.WithoutTags( "player" );

		helper.TryUnstuck();

		// attempt movement ->
		var result = helper.TryMoveWithStep( Time.Delta, 18.0f );

		// move to new pos / vel!
		Position = helper.Position;
		Velocity = helper.Velocity;
	}

	public Vector3 GetWishVelocity( bool zeroPitch = false )
	{
		var result = new Vector3( Player.InputDirection.x, Player.InputDirection.y, 0 );

		var inSpeed = result.Length.Clamp( 0, 1 );

		result *= Player.ViewAngles.WithPitch( 0f ).ToRotation();

		if ( zeroPitch )
		{
			result.z = 0;
		}

		result = result.Normal * inSpeed * 180.0f;

		result *= CurrentGroundAngle.Remap( 0, 45, 1, 0.6f );

		return result;
	}

	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f,
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

	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		var hull = Hull;
		return TraceBBox( start, end, hull.Mins, hull.Maxs, liftFeet, liftHead );
	}
}
