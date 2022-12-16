using ProjectBullet.UI.HUD;
using Sandbox;

namespace ProjectBullet.Player;

public abstract partial class BasePlayer
{
	public bool IsAlive => LifeState == LifeState.Alive;
	public bool IsDead => LifeState == LifeState.Dead;
	[Net, Predicted] protected TimeUntil TimeUntilRespawn { get; set; }

	/// <summary>
	/// Time between death and respawn
	/// </summary>
	protected virtual float RespawnDelay => 5.0f;

	/// <summary>
	/// Max player health
	/// </summary>
	protected virtual float MaxHealth => 100.0f;

	/// <summary>
	/// The information for the last piece of damage this player took.
	/// </summary>
	private DamageInfo LastDamage { get; set; }

	public override void OnKilled()
	{
		GameManager.Current?.OnKilled( this );

		HandleDeathClient();
		HandleDeath();
	}

	[ClientRpc]
	public void HandleDeathClient()
	{
		HandleDeath();
	}

	protected virtual void HandleDeath()
	{
		LifeState = LifeState.Dead;

		TimeUntilRespawn = RespawnDelay;

		EnableDrawing = false;
		EnableAllCollisions = false;
		EnableHitboxes = false;

		if ( Game.IsClient )
		{
			CreateRagdoll( Controller.Velocity, LastDamage.Position, LastDamage.Force,
				LastDamage.BoneIndex, LastDamage.HasTag( "bullet" ), LastDamage.HasTag( "blast" ) );
		}
	}

	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv )
	{
		Sound.FromScreen( "hitsound" );

		Game.RootPanel.AddChild<Hitmarker>();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( info.Attacker is BasePlayer player )
		{
			if ( player != this && player.Team != PlayerTeam.None && player.Team == Team )
			{
				return; // 
			}

			player.DidDamage( To.Single( player ), info.Position, info.Damage, Health.LerpInverse( 0, 100 ) );
		}

		LastDamage = info;

		if ( info.Hitbox.HasTag( "head" ) )
		{
			info.Damage *= 1.3f;
		}

		var preDamage = Health;
		base.TakeDamage( info );

		this.ProceduralHitReaction( info );

		Log.Info( $"{info.Attacker} did {preDamage - Health} dmg to {this} - {Health}/{MaxHealth}" );
	}
}
