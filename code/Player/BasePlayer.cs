﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.Player.Strafe;
using ProjectBullet.UI.HUD;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.Player;

public abstract partial class BasePlayer : AnimatedEntity
{
	public Inventory Inventory => Components.Get<Inventory>();

	[Net] public IList<NodeExecutor> NodeExecutors { get; private set; } = new List<NodeExecutor>();

	[Net, Predicted] private StrafeController Controller { get; set; }

	private ClothingContainer _clothing;
	
	protected virtual NodeExecutor MainExecutor => NodeExecutors.First();

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

	public override void Spawn()
	{
		Components.Create<Inventory>();

		EnableLagCompensation = true;

		Tags.Add( "player" );

		base.Spawn();
	}

	public virtual void Respawn()
	{
		LifeState = LifeState.Alive;
		Health = MaxHealth;
		Velocity = Vector3.Zero;

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		GameManager.Current?.MoveToSpawnpoint( this );

		ResetInterpolation();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new StrafeController()
		{
			AirAcceleration = 1000,
			WalkSpeed = 240,
			SprintSpeed = 245,
			DefaultSpeed = 260,
			AutoJump = false,
			Acceleration = 5,
			GroundFriction = 5 //Do this just for safety if player respawns inside friction volume.
		};

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableAllCollisions = true;
		EnableHitboxes = true;

		_clothing ??= new ClothingContainer();
		_clothing.LoadFromClient( Client );
		_clothing.DressEntity( this );

		RunClientRespawn();
	}

	[ClientRpc]
	public void RunClientRespawn()
	{
		ClientRespawn();
	}

	protected virtual void ClientRespawn()
	{
		if ( !IsLocalPawn )
		{
			//var hit = Game.RootPanel.AddChild<OtherHealth>();
			//hit.BasePlayer = this;
		}
	}
	
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( IsDead )
		{
			if ( LifeState != LifeState.Dead )
			{
				return;
			}

			if ( !(TimeUntilRespawn <= 0) || !Game.IsServer )
			{
				return;
			}

			Respawn();
		}

		EyeSimulate();

		Controller?.Simulate( cl, this );

		foreach ( var nodeExecutor in NodeExecutors )
		{
			nodeExecutor.Simulate( cl );
		}

		AnimationSimulate();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( IsDead )
		{
			return;
		}

		Controller?.Simulate( cl, this );

		CameraFrameSimulate();
	}
}
