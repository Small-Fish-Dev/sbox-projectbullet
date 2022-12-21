using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.Player;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 580 )]
[Energy( 8.0f )]
[Connector( "on_hit", Order = 0, EnergyOutAmount = 10f, DisplayName = "One" )]
[Node( DisplayName = "Mist", Description = "Cheap and simple damage" )]
public partial class Mist : WeaponNodeEntity
{
	public partial class MistEntity : Entity
	{
		private Mist Mist;

		public MistEntity( Mist mist )
		{
			Mist = mist;

			Transmit = TransmitType.Always;

			_timeUntilComplete = MistTime;

			if ( Game.IsServer )
			{
				DeleteAsync( MistTime );
			}
		}

		public MistEntity() { }

		private static float MistTime => 1.0f;

		private TimeUntil _timeUntilComplete;
		[Net] protected float Size { get; set; } = 15.0f;

		protected virtual float Speed => 250.0f;
		protected virtual float MaxSize => 140.0f;

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
				if ( !(player.Position.Distance( Position ) <= Size) )
				{
					continue;
				}

				if ( Mist == null )
				{
					continue;
				}

				Log.Info( "running connector" );
				var info = new ExecuteInfo { ImpactPoint = Position, Victim = player, Attacker = Mist.BasePlayer };
				Mist.ExecuteConnector( "on_hit", info );
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
			var mist = new MistEntity( this ) { Position = info.ImpactPoint, Owner = BasePlayer };
		}

		return 0.0f;
	}
}
