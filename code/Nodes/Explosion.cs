using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.Player;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 580 )]
[Energy( 8.0f )]
[Connector( "on_hit", Order = 0, EnergyOutAmount = 8f, DisplayName = "One" )]
[Node( DisplayName = "Explosion", Description = "Cheap and simple damage" )]
public partial class Explosion : WeaponNode
{
	public partial class ExplosionEntity : Entity
	{
		private Explosion Explosion;

		public ExplosionEntity( Explosion explosion )
		{
			Explosion = explosion;

			Transmit = TransmitType.Always;

			_timeUntilComplete = ExplosionTime;

			if ( Game.IsServer )
			{
				DeleteAsync( ExplosionTime );
			}
		}

		public ExplosionEntity() { }

		private static float ExplosionTime => 1.0f;

		private TimeUntil _timeUntilComplete;
		[Net] protected float Size { get; set; } = 15.0f;

		protected virtual float Speed => 250.0f;
		protected virtual float MaxSize => 140.0f;
		
		private readonly List<BasePlayer> _hitPlayers = new();

		[Event.Tick.Server]
		private void Tick()
		{
			Size += Speed * Time.Delta;
			if ( Size > MaxSize )
			{
				Size = MaxSize;
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

	protected override float Execute( float energy, ExecuteInfo info )
	{
		if ( Game.IsServer )
		{
			var explosion = new ExplosionEntity( this ) { Position = info.ImpactPoint, Owner = BasePlayer };
		}

		return 0.0f;
	}
}
