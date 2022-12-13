using Sandbox;

namespace ProjectBullet;

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

		LifeState = LifeState.Dead;

		TimeUntilRespawn = RespawnDelay;

		EnableDrawing = false;
		EnableHitboxes = false;
	}

	public virtual void SimulateWhileDead()
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
}
