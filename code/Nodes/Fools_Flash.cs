using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Nodes;

// ReSharper disable once InconsistentNaming
[ShopItem( 1300 )]
[Energy( 5.0f )]
[Value( "health_taken", Percentage = 0.75f )]
[Node( DisplayName = "Fool's Flash",
	UsageInfo = "Teleports you to the destination but takes <health_taken> of your max health" )]
public class Fools_Flash : WeaponNode
{
	protected override float Execute( float energy, ExecuteInfo info )
	{
		var damage = GetValue( "health_taken", Player.MaxHealth ) ?? 75.0f;
		
		// Make the player take damage...
		var damageInfo = new DamageInfo().WithAttacker( this );
		damageInfo.Damage = damage;
		Player.TakeDamage( damageInfo );
		
		// Make the player teleport...
		Player.Position = info.ImpactPoint;

		return 0.0f;
	}
}
