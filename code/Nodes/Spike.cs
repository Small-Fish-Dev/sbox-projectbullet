using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.Player;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 580 )]
[Energy( 15.0f )]
[Connector( "on_hit", Order = 0, EnergyOutAmount = 15f, DisplayName = "One" )]
[Node( DisplayName = "Spike", Description = "Cheap and simple damage" )]
public partial class Spike : WeaponNodeEntity
{
	public class SpikePart : BasePhysics
	{
		private Spike Spike;

		public SpikePart( Spike spike )
		{
			Spike = spike;

			Transmit = TransmitType.Always;
		}

		public SpikePart() { }

		public override void Spawn()
		{
			base.Spawn();

			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Vector3.Zero, 5.0f );
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( eventData.Other.Entity is SpikePart )
			{
				return;
			}

			base.OnPhysicsCollision( eventData );

			if ( Game.IsServer )
			{
				var info = new ExecuteInfo
				{
					ImpactPoint = Position, Victim = eventData.Other.Entity, Attacker = Spike.BasePlayer
				};
				Spike?.ExecuteConnector( "on_hit", info );
			}

			Delete();
		}

		[Event.Client.Frame]
		private void Frame()
		{
			DebugOverlay.Sphere( Position, 5.0f, Color.Cyan, 0.0f, true );
		}
	}

	protected override float Execute( float energy, ExecuteInfo info )
	{
		if ( Game.IsServer )
		{
			const float amount = 5.0f;
			for ( var i = 0; i < amount; i++ )
			{
				var spikePart =
					new SpikePart( this ) { Position = info.ImpactPoint + Vector3.Up * 16, Owner = BasePlayer };
				var yaw = ((i / amount) * 360) - 180;
				var angles = new Angles( -47, yaw, 0 );
				spikePart.Velocity = angles.ToRotation().Forward * 222;
			}
		}

		return 0.0f;
	}
}
