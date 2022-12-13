﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.Player.Strafe;
using Sandbox;

namespace ProjectBullet.Player;

public abstract partial class BasePlayer : AnimatedEntity
{
	public Inventory Inventory => Components.Get<Inventory>();

	[Net] public IList<NodeExecutionEntity> NodeExecutors { get; private set; } = new List<NodeExecutionEntity>();

	[Net, Predicted] protected StrafeController Controller { get; set; }

	private ClothingContainer _clothing;

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
			if ( !property.PropertyType.IsSubclassOf( typeof(NodeExecutionEntity) ) )
			{
				continue;
			}

			if ( property.GetValue( this ) is not NodeExecutionEntity instance )
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
		EnableHitboxes = true;

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
		EnableHitboxes = false;

		_clothing ??= new ClothingContainer();
		_clothing.LoadFromClient( Client );
		_clothing.DressEntity( this );

		RunClientRespawn();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( IsDead )
		{
			SimulateWhileDead();
			return;
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

		var line = 0;
		DebugOverlay.ScreenText( $"{Client.Name}, health {Health}/{MaxHealth}",
			Vector2.One * 20, line++, Color.Cyan );
		foreach ( var ne in NodeExecutors )
		{
			var color = ne.Energy >= ne.MinimumEnergy ? Color.Green : Color.Red;
			DebugOverlay.ScreenText(
				$"{ne.DisplayName} {ne.Energy.Floor()}/{ne.MaxEnergy} - need {ne.MinimumEnergy} {ne.TimeUntilAction}",
				Vector2.One * 20, line++, color );
		}

		foreach ( var player in All.OfType<BasePlayer>() )
		{
			if ( player == this )
			{
				continue;
			}

			DebugOverlay.Text(
				$"{player.Health}/{player.MaxHealth}",
				player.EyePosition, 0, Color.Orange );
		}
	}
}