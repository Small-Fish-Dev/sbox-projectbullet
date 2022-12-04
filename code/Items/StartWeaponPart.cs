using System;
using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet.Items;

[KnownWeaponPart( "Start" )]
public class StartWeaponPart : WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}
