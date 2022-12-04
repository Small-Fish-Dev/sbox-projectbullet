using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet.Players.Classes;

/// <summary>
/// Everyday, as in... an everyday guy
/// </summary>
public partial class Everyday : Player
{
	[KnownWeapon(DisplayName = "Primary")]
	public class PrimaryFire : Weapon
	{
	}

	[Net] public Weapon Primary { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Primary = new();
		Primary.Owner = this;
		Weapons.Add( Primary );
	}
}
