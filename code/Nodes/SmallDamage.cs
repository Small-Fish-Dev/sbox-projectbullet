using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 280 )]
[Energy( 5.0f )]
[Node( DisplayName = "Damage", Description = "Cheap and simple damage" )]
public partial class CheapDamage : WeaponNodeEntity, IGoalNode
{
	protected override float Execute( float energy, ExecuteInfo info )
	{
		if ( info.Victim == null )
		{
			return 0.0f;
		}

		using ( Prediction.Off() )
		{
			var damageInfo = info.ToDamageInfo( 7.0f );
			info.Victim.TakeDamage( damageInfo );
		}

		return 0.0f;
	}
}

[ShopItem( 580 )]
[Energy( 5.0f )]
[Node( DisplayName = "More Damage", Description = "Cheap and simple damage" )]
public class MoreDamage : WeaponNodeEntity, IGoalNode
{
	protected override float Execute( float energy, ExecuteInfo info )
	{
		if ( info.Victim == null )
		{
			return 0.0f;
		}

		using ( Prediction.Off() )
		{
			var damageInfo = info.ToDamageInfo( 14.0f );
			info.Victim.TakeDamage( damageInfo );
		}

		return 0.0f;
	}
}
