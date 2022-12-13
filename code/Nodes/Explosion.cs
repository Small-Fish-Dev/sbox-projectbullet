﻿using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.Player;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 580 )]
[Energy( 8.0f, Estimated = true )]
[Connector( "on_hit", Order = 0, EnergyOutAmount = 8f, DisplayName = "One" )]
[Node( DisplayName = "Explosion", Description = "Cheap and simple damage" )]
public partial class Explosion : WeaponNodeEntity
{
	public partial class ExplosionEntity : Entity
	{
		private Explosion Explosion;

		public ExplosionEntity( Explosion explosion )
		{
			Explosion = explosion;

			Transmit = TransmitType.Always;

			TimeUntilComplete = ExplosionTime;

			if ( Game.IsServer )
			{
				DeleteAsync( ExplosionTime );
			}
		}

		public ExplosionEntity() { }

		public float ExplosionTime => 3.0f;

		public TimeUntil TimeUntilComplete;
		[Net] public float Size { get; set; } = 15.0f;

		private List<BasePlayer> _hitPlayers = new();

		[Event.Tick.Server]
		private void Tick()
		{
			Size += 250.0f * Time.Delta;
			if ( Size > 140 )
			{
				Size = 140;
			}

			foreach ( var player in All.OfType<BasePlayer>() )
			{
				if ( _hitPlayers.Contains( player ) )
				{
					continue;
				}

				if ( !(player.Position.Distance( Position ) <= Size) )
				{
					continue;
				}

				_hitPlayers.Add( player );
				Log.Info( $"hit player {player}" );


				if ( Explosion == null )
				{
					continue;
				}

				Log.Info( "running connector" );
				var info = new ExecuteInfo { ImpactPoint = Position, Victim = player, Attacker = Explosion.BasePlayer };
				Explosion.ExecuteConnector( "on_hit", info );
			}
		}

		[Event.Client.Frame]
		private void Frame()
		{
			DebugOverlay.Sphere( Position, Size, Color.Orange, 0.0f, true );
		}
	}

	public override float Execute( float energy, ExecuteInfo info )
	{
		if ( Game.IsServer )
		{
			var explosion = new ExplosionEntity( this ) { Position = info.ImpactPoint, Owner = BasePlayer };
		}

		return 0.0f;
	}
}