using Sandbox;

namespace ProjectBullet.Player;

public abstract partial class BasePlayer
{
	public bool IsAlive => LifeState == Sandbox.LifeState.Alive;
	public bool IsDead => LifeState == LifeState.Dead;
	[Net, Predicted] protected TimeUntil TimeUntilRespawn { get; set; }

	/// <summary>
	/// Time between death and respawn
	/// </summary>
	public virtual float RespawnDelay => 5.0f;

	/// <summary>
	/// Max player health
	/// </summary>
	public virtual float MaxHealth => 100.0f;

	public override void OnKilled()
	{
		GameManager.Current?.OnKilled( this );

		HandleDeathShared();
	}

	protected void HandleDeathShared()
	{
		HandleDeathClient();
		HandleDeath();
	}

	[ClientRpc]
	public void HandleDeathClient()
	{
		HandleDeath();
	}

	[ClientRpc]
	public void RunClientRespawn()
	{
		ClientRespawn();
	}

	protected virtual void ClientRespawn()
	{
	}

	protected virtual void HandleDeath()
	{
		LifeState = LifeState.Dead;

		TimeUntilRespawn = RespawnDelay;

		EnableDrawing = false;
		EnableHitboxes = false;
	}

	protected virtual void SimulateWhileDead()
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

	[ClientRpc]
	public void DidDamage( Vector3 pos, float amount, float healthinv )
	{
		Sound.FromScreen( "blip" )
			.SetPitch( 1 + healthinv * 1 );
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

		var preDamage = Health;
		base.TakeDamage( info );

		this.ProceduralHitReaction( info );

		Log.Info( $"{info.Attacker} did {preDamage - Health} dmg to {this} - {Health}/{MaxHealth}" );
	}
}
