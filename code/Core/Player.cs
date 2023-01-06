using System;
using System.Collections.Generic;
using ProjectBullet.Core.CharacterTools;
using ProjectBullet.Core.Node;
using ProjectBullet.MapEnts;
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

	private PersistentData _persistent;

	/// <summary>
	/// <see cref="PersistentData"/> for this player. Needs to be initialized elsewhere.
	/// </summary>
	public PersistentData Persistent
	{
		get
		{
			if ( _persistent != null )
			{
				return _persistent;
			}

			_persistent = PersistentData.Get( Client );
			return _persistent;
		}
	}

	/// <summary>
	/// List of player <see cref="NodeExecutor"/>s. Should be initialized by the child class.
	/// </summary>
	[Net] public IList<NodeExecutor> NodeExecutors { get; private set; } = new List<NodeExecutor>();

	/// <summary>
	/// Remaining time to respawn
	/// </summary>
	[Net, Predicted] public TimeUntil TimeUntilRespawn { get; private set; }

	private ClothingContainer Clothing { get; set; }

	public bool IsAlive => LifeState == LifeState.Alive;
	public bool IsDead => LifeState == LifeState.Dead;

	/// <summary>
	/// Whether or not the player is allowed to use the workshop
	/// </summary>
	public bool CanUseWorkshop
	{
		get => Tags.Has( "can_workshop" ) || MapConfig.Instance.EnableWorkshopAnywhere;
		set
		{
			Game.AssertServer();
			Tags.Set( "can_workshop", value );
		}
	}

	/// <summary>
	/// Whether or not the player is in the money area
	/// </summary>
	public bool InMoneyArea
	{
		get => Tags.Has( "in_money_area" );
		set
		{
			Game.AssertServer();
			Tags.Set( "in_money_area", value );
		}
	}
	
	[Net] public HoldableWeapon HoldableWeapon { get; protected set; }

	public override void Spawn()
	{
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
		ViewAngles = ViewAngles.WithPitch( 0 );
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
			if ( _lastRagdoll != null )
			{
				Camera.Rotation = Rotation.Lerp( Camera.Rotation,
					Rotation.LookAt( _lastRagdoll.PhysicsBody.Position - Camera.Position ), 5.0f * Time.Delta );
			}

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
