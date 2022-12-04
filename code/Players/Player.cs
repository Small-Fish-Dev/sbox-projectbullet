using System.Collections.Generic;
using ProjectBullet.Players.Strafe;
using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet.Players;

public abstract partial class Player : AnimatedEntity
{
	[Net, Predicted] private PawnController Controller { get; set; }
	[Net, Predicted] private PawnAnimator Animator { get; set; }

	private ClothingContainer _clothing;

	private CameraMode CameraMode
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	private Inventory Inventory
	{
		get => Components.Get<Inventory>();
		set => Components.Add( value );
	}

	[Net] public List<Weapon> Weapons { get; set; } = new();

	public override void Spawn()
	{
		EnableLagCompensation = true;

		Tags.Add( "player" );

		Inventory = new Inventory();
		
		foreach (var partDescription in WeaponStorage.PartDescriptions)
		{
			if (partDescription.TypeDescription.Name != "StartPart")
				Inventory.PartItems.Add( new PartInventoryItem(partDescription) );
		}

		base.Spawn();
	}

	public virtual void Respawn()
	{
		Host.AssertServer();

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		WaterLevel = 0;

		CreateHull();

		Game.Current?.MoveToSpawnpoint( this );
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

		CameraMode = new StrafeCamera();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		_clothing ??= new ClothingContainer();
		_clothing.LoadFromClient( Client );
		_clothing.DressEntity( this );
	}

	public virtual void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
		EnableHitboxes = true;
	}

	public override void Simulate( Client cl )
	{
		Controller?.Simulate( cl, this, Animator );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl, this, Animator );
	}
}
