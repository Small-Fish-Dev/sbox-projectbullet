using System.Linq;
using Sandbox;

namespace ProjectBullet.Core;

public abstract partial class Player
{
	/// <summary>
	/// The information for the last piece of damage this player took.
	/// </summary>
	public DamageInfo LastDamage { get; protected set; }

	public override void OnKilled()
	{
		if ( !IsAlive )
		{
			return;
		}

		CreateRagdoll( Controller.Velocity, LastDamage.Position, LastDamage.Force,
			LastDamage.BoneIndex, LastDamage.HasTag( "bullet" ), LastDamage.HasTag( "blast" ) );

		LifeState = LifeState.Dead;
		EnableAllCollisions = false;
		EnableDrawing = false;

		TimeUntilRespawn = BaseRespawnDelay;

		HoldableWeapon?.Delete();

		Controller?.Remove();
		Animator?.Remove();

		// Disable all children as well.
		Children.OfType<ModelEntity>()
			.ToList()
			.ForEach( x => x.EnableDrawing = false );

		GameManager.Current?.OnKilled( this );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( !IsAlive )
		{
			return;
		}

		if ( info.Attacker is Player player )
		{
			if ( player != this )
			{
				if ( !player.IsOnOppositeTeam && !Util.MapConfig.IsFreeForAll )
				{
					// not FFA and the player is on our team, return
					return;
				}
			}
		}

		LastDamage = info;

		this.ProceduralHitReaction( info, 0.05f );

		base.TakeDamage( info );

		if ( Health <= 0 )
		{
			OnKilled();
		}
	}
}
