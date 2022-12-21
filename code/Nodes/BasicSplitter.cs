using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;

namespace ProjectBullet.Nodes;

[ShopItem( 300 )]
[Energy( 2.0f, Estimated = true )]
[Connector( "on_one", Order = 0, EnergyOutPercentage = 0.2f, DisplayName = "One" )]
[Connector( "on_two", Order = 1, EnergyOutPercentage = 0.2f, DisplayName = "Two" )]
[Node( DisplayName = "Mini Splitter", Description = "Cheap and simple splitter" )]
public class MiniSplitter : WeaponNodeEntity
{
	public override float Execute( float energy, ExecuteInfo info )
	{
		ExecuteConnector( "on_one", info );
		ExecuteConnector( "on_two", info );

		return 0.0f;
	}
}
