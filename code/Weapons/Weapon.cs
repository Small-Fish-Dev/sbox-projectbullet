using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Items;
using ProjectBullet.UI.Editor;
using Sandbox;

namespace ProjectBullet.Weapons;

public partial class Weapon : BaseWeapon
{
	public WeaponDescription Description => StaticStorage.GetWeaponDescription( GetType() );
	public IEnumerable<WeaponPart> Parts => All.OfType<WeaponPart>().Where( v => v.Owner == this );
	public string DisplayName { get; }
	[Net] public WeaponPart StartPart { get; set; }
	public readonly GraphableWeaponPart ClientGraphableStartPart = new();

	public Weapon()
	{
		Transmit = TransmitType.Owner;
		DisplayName = Description.DisplayName;
	}

	public override void Spawn()
	{
		base.Spawn();
		StartPart = new StartWeaponPart();
	}
}
