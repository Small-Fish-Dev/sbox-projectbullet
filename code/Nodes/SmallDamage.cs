using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 280 )]
[Energy( 5.0f, Estimated = true )]
[Node( DisplayName = "Damage", Description = "Cheap and simple damage" )]
public class CheapDamage : WeaponNodeEntity
{
	public override float Execute( float energy, Entity target, Vector3 point )
	{
		if ( target == null )
		{
			return 0.0f;
		}

		using ( Prediction.Off() )
		{
			var damageInfo = DamageInfo.FromBullet( point, 32, 7.0f )
				.WithAttacker( Owner )
				.WithWeapon( this );

			target.TakeDamage( damageInfo );
		}

		return 0.0f;
	}
}
