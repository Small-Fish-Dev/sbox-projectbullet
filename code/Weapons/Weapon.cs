using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Items;
using ProjectBullet.UI.Editor;
using Sandbox;

namespace ProjectBullet.Weapons;

public partial class Weapon : BaseWeapon
{
	public IEnumerable<WeaponPart> Parts => All.OfType<WeaponPart>().Where( v => v.Owner == this );
	[Net] public WeaponPart StartPart { get; set; }
	public readonly GraphableWeaponPart ClientGraphableStartPart = new();

	public Weapon() => Transmit = TransmitType.Owner;
	
	public override void Spawn()
	{
		base.Spawn();

		StartPart = new StartWeaponPart();
	}
}
