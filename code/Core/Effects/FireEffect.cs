using Sandbox;

namespace ProjectBullet.Core.Effects;

public class FireEffect : BaseEffect
{
	protected override float Lifespan => 1;

	public FireEffect( Entity creator ) : base( creator )
	{
	}

	public FireEffect() {}
	
	[Event.Tick]
	private void OnTick()
	{
		if ( Entity is { IsAlive: true } )
		{
			// Entity.TakeDamage( new DamageInfo { Damage = 1 } );
			//Entity.Velocity = Entity.Velocity.LerpTo( Vector3.Zero, Time.Delta * 15.0f );
		}
	}
}
