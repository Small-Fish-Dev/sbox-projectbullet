using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Nodes;

[ShopItem( 250 )]
[Energy( 3.0f )]
[Node( DisplayName = "Push", Description = "Push target away" )]
public partial class Push : WeaponNodeEntity, IGoalNode
{
	protected override float Execute( float energy, ExecuteInfo info )
	{
		if ( info.Victim is not Player.BasePlayer )
		{
			return 0.0f;
		}

		if ( !Game.IsServer )
		{
			return 0.0f;
		}

		using ( Prediction.Off() )
		{
			info.Victim.Velocity +=
				(info.Force ?? (info.Victim.Position - info.ImpactPoint).Normal * 12) * 14;
		}

		return 0.0f;
	}
}

[ShopItem( 250 )]
[Energy( 3.0f )]
[Node( DisplayName = "Pull", Description = "Pull target in" )]
public partial class Pull : WeaponNodeEntity, IGoalNode
{
	protected override float Execute( float energy, ExecuteInfo info )
	{
		if ( info.Victim is not Player.BasePlayer )
		{
			return 0.0f;
		}

		if ( !Game.IsServer )
		{
			return 0.0f;
		}

		using ( Prediction.Off() )
		{
			info.Victim.Velocity +=
				(info.Force ?? (info.ImpactPoint - info.Victim.Position).Normal * 12) * 14;
		}

		return 0.0f;
	}
}
