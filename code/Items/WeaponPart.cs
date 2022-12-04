using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet.Items;

public abstract partial class WeaponPart : Entity
{
	public WeaponPartDescription Description => StaticStorage.GetWeaponPartDescription( GetType() );
	public float EnergyUsage => Description.EnergyUsage;
	public abstract void Execute( Entity target, Vector3 point, Weapon weapon );
}
