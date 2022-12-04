using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet.Items;

public abstract partial class WeaponPart : Entity, IInventoryItem
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Owner;
	}
	
	public WeaponPartDescription Description => StaticStorage.GetWeaponPartDescription( GetType() );
	public float EnergyUsage => Description.EnergyUsage;
	public abstract void Execute( Entity target, Vector3 point, Weapon weapon );

	public int ClientSavedEditorX = 0;
	public int ClientSavedEditorY = 0;

	[Net] public Guid Uid { get; set; } = Guid.NewGuid();

	public Weapon Weapon => Parent is Weapon weapon ? weapon : null;

	public IEnumerable<WeaponPart> OutputInstances =>
		Description.Outputs.Select( v => v.PropertyDescription.GetValue( this ) ).Cast<WeaponPart>();

	public ReadOnlyCollection<PartOutputDescription> OutputDescriptions => Description.Outputs;

	private static IEnumerable<WeaponPart> GetWeaponParts( Entity owner ) =>
		Entity.All.OfType<WeaponPart>().Where( v => v.Owner == owner );

	public static IEnumerable<WeaponPart> GetUnusedWeaponParts( Entity owner ) =>
		GetWeaponParts( owner );

	public static IEnumerable<WeaponPart> GetUsedWeaponParts( Weapon weapon ) =>
		Entity.All.OfType<WeaponPart>().Where( v => v.Owner == weapon );
}
