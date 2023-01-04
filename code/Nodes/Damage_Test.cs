using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;

namespace ProjectBullet.Nodes;

// ReSharper disable once InconsistentNaming
[ShopItem( 300 )]
[Energy( 1.0f )]
[Value( "damage", Absolute = 5.0f )]
[Node( DisplayName = "Damage Test",
	UsageInfo = "Does <damage> damage" )]
public class Damage_Test : WeaponNode, IGoalNode
{
	protected override float Execute( float energy, ExecuteInfo info )
	{
		throw new System.NotImplementedException();
	}
}
