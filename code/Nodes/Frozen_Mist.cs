using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.UI;
using Sandbox;
using Player = ProjectBullet.Core.Player;

namespace ProjectBullet.Nodes;

// ReSharper disable once InconsistentNaming
[ShopItem( 1300 )]
[Energy( 20.0f )]
[Connector( "tick", Order = 0, EnergyOutAmount = 3f, DisplayName = "Tick" )]
[Value( "velocity_removed", Absolute = 1.0f )]
[Node( DisplayName = "Chilling Mist",
	UsageInfo = "Creates a translucent sphere that slows players inside it - <velocity_removed> a tick" )]
public partial class Chilling_Mist : WeaponNode
{
	// ReSharper disable once InconsistentNaming
	public partial class Chilling_Mist_Entity : ModelEntity
	{
		[Net, Predicted] protected float Size { get; set; } = 15.0f;

		private TimeUntil _timer;

		public Chilling_Mist_Entity()
		{
			Transmit = TransmitType.Always;
			_timer = 5;
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/dev/sphere.vmdl" );

			SetMaterialOverride( "materials/smoke_test.vmat" );

			Scale = 3.8f;
		}

		private readonly List<Player> _hitPlayers = new();

		[Event.Tick.Server]
		private void Tick()
		{
			if ( _timer )
			{
				foreach ( var player in _hitPlayers )
				{
					ClientExitSmoke( To.Single( player ) );
				}

				Delete();
				return;
			}

			foreach ( var player in All.OfType<Player>() )
			{
				if ( !(player.Position.Distance( Position ) <= 150.0f) )
				{
					if ( _hitPlayers.Contains( player ) )
					{
						ClientExitSmoke( To.Single( player ) );
						_hitPlayers.Remove( player );
					}

					continue;
				}

				if ( !_hitPlayers.Contains( player ) )
				{
					ClientEnterSmoke( To.Single( player ) );
					_hitPlayers.Add( player );
				}
			}
		}

		[ClientRpc]
		public void ClientEnterSmoke()
		{
			
		}

		[ClientRpc]
		public void ClientExitSmoke()
		{
			
		}
	}

	protected override float Execute( float energy, ExecuteInfo info )
	{
		if ( Game.IsServer )
		{
			var mist = new Chilling_Mist_Entity() { Position = info.ImpactPoint, Owner = Player };
		}

		return 0.0f;
	}
}
