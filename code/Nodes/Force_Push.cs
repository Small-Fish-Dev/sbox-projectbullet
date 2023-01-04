using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Nodes;

// ReSharper disable once InconsistentNaming
[ShopItem( 500 )]
[Energy( 5.0f )]
[Node( DisplayName = "Force Push",
	UsageInfo = "Pushes player with activated force" )]
public class Force_Push : WeaponNode
{
	protected override float Execute( float energy, ExecuteInfo info )
	{
		switch (info.Victim)
		{
			case null:
			case WorldEntity:
				return 0.0f;
		}

		if ( !Game.IsServer )
		{
			return 0.0f;
		}

		using ( Prediction.Off() )
		{
			info.Victim.Velocity +=
				(info.Force ?? (info.Victim.Position - info.ImpactPoint).Normal * 12) * 6;
		}

		return 0.0f;
	}
}
