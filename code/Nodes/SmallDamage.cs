using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 280 )]
[Energy( 5.0f, Estimated = true )]
[Node( DisplayName = "Damage", Description = "Cheap and simple damage" )]
public class CheapDamage : WeaponNodeEntity, IGoalNode
{
	public override float Execute( float energy, ExecuteInfo info )
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
[Energy( 5.0f, Estimated = true )]
[Node( DisplayName = "More Damage", Description = "Cheap and simple damage" )]
public class MoreDamage : WeaponNodeEntity, IGoalNode
{
	public override float Execute( float energy, ExecuteInfo info )
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

[ShopItem( 300 )]
[Energy( 12.0f, Estimated = true )]
[Connector( "on_one", Order = 0, EnergyOutPercentage = 0.5f, DisplayName = "One" )]
[Connector( "on_two", Order = 1, EnergyOutPercentage = 0.5f, DisplayName = "Two" )]
[Node( DisplayName = "Splitter", Description = "Cheap and simple splitter" )]
public class BasicSplitter : WeaponNodeEntity
{
	public override float Execute( float energy, ExecuteInfo info )
	{
		ExecuteConnector( "on_one", info );
		ExecuteConnector( "on_two", info );

		return 0.0f;
	}
}
