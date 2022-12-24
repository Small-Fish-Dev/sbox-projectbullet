using System;
using System.Collections.Generic;
using ProjectBullet.Core.CharacterTools;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Core;

public abstract partial class Player : AnimatedEntity
{
	/// <summary>
	/// The controller is responsible for player movement and setting up EyePosition / EyeRotation.
	/// </summary>
	[BindComponent]
	public Gameplay.Controller Controller { get; }

	/// <summary>
	/// The animator is responsible for animating the player's current model.
	/// </summary>
	[BindComponent]
	public PlayerAnimator Animator { get; }

	/// <summary>
	/// The inventory contains player items for a match
	/// </summary>
	[BindComponent]
	public Inventory Inventory { get; }

	[Net] public IList<NodeExecutor> NodeExecutors { get; private set; } = new List<NodeExecutor>();

	public ClothingContainer Clothing { get; protected set; }

	public bool IsAlive => LifeState == LifeState.Alive;
	public bool IsDead => LifeState == LifeState.Dead;
	public bool CanUseEditor => Tags.Has( "can_workshop" );
	[Net, Predicted] protected TimeUntil TimeUntilRespawn { get; set; }

	[Net] public HoldableWeapon HoldableWeapon { get; protected set; }

	public override void Spawn()
	{
		Components.Create<Inventory>();

		EnableLagCompensation = true;

		Tags.Add( "player" );

		base.Spawn();
	}

	public virtual void Respawn()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		LifeState = LifeState.Alive;
		Health = MaxHealth;
		Velocity = Vector3.Zero;

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableAllCollisions = true;
		EnableHitboxes = true;

		SetModel( "models/citizen/citizen.vmdl" );

		Components.RemoveAny<PlayerMechanic>();
		Components.Create<Gameplay.Controller>();
		Components.Create<Gameplay.Walking>();
		Components.Create<Gameplay.Ducking>();
		Components.Create<Gameplay.Jumping>();
		Components.Create<Gameplay.Sneaking>();
		Components.Create<Gameplay.AirMovement>();

		Components.Create<CitizenAnimator>();

		GameManager.Current?.MoveToSpawnpoint( this );
		ResetInterpolation();

		ClientRpcRespawn( To.Single( Client ) );

		CreateHoldableWeapon();

		Clothing ??= new ClothingContainer();
		if ( OutfitJson == null )
		{
			Clothing.LoadFromClient( Client );
			Log.Info( $"Player custom outfit: {Clothing.Serialize()}" );
		}
		else
		{
			Clothing.Deserialize( OutfitJson );
		}

		Clothing.DressEntity( this );
	}

	[ClientRpc]
	public void ClientRpcRespawn()
	{
		ClientRespawn();
	}

	protected virtual void ClientRespawn()
	{
	}

	protected virtual void CreateHoldableWeapon()
	{
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( IsDead )
		{
			if ( !(TimeUntilRespawn <= 0) || !Game.IsServer )
			{
				return;
			}

			Respawn();
		}

		Controller?.Simulate( cl );

		foreach ( var nodeExecutor in NodeExecutors )
		{
			nodeExecutor.Simulate( cl );
		}

		Animator?.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( IsDead )
		{
			return;
		}

		Controller?.FrameSimulate( cl );

		foreach ( var nodeExecutor in NodeExecutors )
		{
			nodeExecutor.FrameSimulate( cl );
		}

		Animator?.Simulate( cl );

		Camera.Position = EyePosition;
		Camera.Rotation = EyeRotation;
		Camera.FieldOfView = 103.0f;
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 0.5f;
	}

	/// <summary>
	/// Register all node executors
	/// </summary>
	protected void RegisterNodeExecutors()
	{
		Game.AssertServer();

		var typeDescription = TypeLibrary.GetType( GetType() );
		if ( typeDescription == null )
		{
			throw new Exception( "Couldn't register node executors: TypeLibrary fail" );
		}

		foreach ( var property in typeDescription.Properties )
		{
			if ( !property.PropertyType.IsSubclassOf( typeof(NodeExecutor) ) )
			{
				continue;
			}

			if ( property.GetValue( this ) is not NodeExecutor instance )
			{
				continue;
			}

			instance.Owner = this;
			instance.Parent = this;
			NodeExecutors.Add( instance );

			Log.Info( $"RegisterNodeExecutors: registered {property.PropertyType.Name}" );
		}
	}
}
