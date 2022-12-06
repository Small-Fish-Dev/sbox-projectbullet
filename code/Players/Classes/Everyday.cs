using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet.Players.Classes;

/// <summary>
/// Everyday, as in... an everyday guy
/// </summary>
public partial class Everyday : ClassBase
{
	[KnownWeapon( DisplayName = "Pistol" )]
	public class WorldPistol : Weapon
	{
	}
	
	[KnownWeapon( DisplayName = "Club" )]
	public class Club : Weapon
	{
	}

	public override void Spawn()
	{
		base.Spawn();
		Weapons.Add( new WorldPistol() { Owner = this } );
		Weapons.Add( new Club() { Owner = this } );
	}
}
