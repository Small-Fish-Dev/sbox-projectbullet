using ProjectBullet.Strafe;
using Sandbox;

namespace ProjectBullet;

public abstract partial class BasePlayer : Player
{
	private ClothingContainer _clothing;

	public override void Spawn()
	{
		EnableLagCompensation = true;

		Tags.Add( "player" );

		base.Spawn();
	}

	public override void Respawn()
	{
		Host.AssertServer();

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		WaterLevel = 0;

		CreateHull();

		GameManager.Current?.MoveToSpawnpoint( this );
		ResetInterpolation();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new StrafeController()
		{
			AirAcceleration = 1200,
			WalkSpeed = 235,
			SprintSpeed = 265,
			DefaultSpeed = 260,
			AutoJump = true,
			Acceleration = 5,
			GroundFriction = 6 //Do this just for safety if player respawns inside friction volume.
		};

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		_clothing ??= new ClothingContainer();
		_clothing.LoadFromClient( Client );
		_clothing.DressEntity( this );
	}

	public override void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
		EnableHitboxes = true;
	}

	public override void Simulate( Client cl )
	{
		Controller?.Simulate( cl, this );

		foreach ( var nodeExecutionEntity in ActiveNodeExecutors )
		{
			nodeExecutionEntity.Simulate( cl );
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		if ( !cl.IsOwnedByLocalClient )
		{
			return;
		}

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.Position = EyePosition;
		Camera.FirstPersonViewer = this;
		Camera.FieldOfView = 103.0f;
		Camera.ZNear = 1f;
		Camera.ZFar = 5000.0f;
	}
}
