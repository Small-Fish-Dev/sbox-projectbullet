using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ProjectBullet.Items;
using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet;

public static class StaticStorage
{
	private static readonly List<WeaponPartDescription> WeaponPartDescriptionsCache = new();
	private static readonly List<WeaponDescription> WeaponDescriptionsCache = new();
	private static readonly List<PurchasableItemDescription> PurchasableItemDescriptionsCache = new();

	public static ReadOnlyCollection<WeaponPartDescription> WeaponPartDescriptions
	{
		get
		{
			Init();
			return WeaponPartDescriptionsCache.AsReadOnly();
		}
	}

	public static ReadOnlyCollection<WeaponDescription> WeaponDescriptions
	{
		get
		{
			Init();
			return WeaponDescriptionsCache.AsReadOnly();
		}
	}

	public static ReadOnlyCollection<PurchasableItemDescription> PurchasableItemDescriptions
	{
		get
		{
			Init();
			return PurchasableItemDescriptionsCache.AsReadOnly();
		}
	}

	[Event.Hotload]
	private static void OnHotload()
	{
		WeaponPartDescriptionsCache.Clear();
		WeaponDescriptionsCache.Clear();
		PurchasableItemDescriptionsCache.Clear();
	}

	public static void Init()
	{
		if ( WeaponPartDescriptionsCache.Count == 0 )
		{
			foreach ( var pairing in TypeLibrary.GetTypesWithAttribute<KnownWeaponPartAttribute>() )
			{
				WeaponPartDescriptionsCache.Add( new WeaponPartDescription( pairing.Type, pairing.Attribute ) );
			}
		}

		if ( WeaponDescriptionsCache.Count == 0 )
		{
			foreach ( var pairing in TypeLibrary.GetTypesWithAttribute<KnownWeaponAttribute>() )
			{
				WeaponDescriptionsCache.Add( new WeaponDescription( pairing.Type, pairing.Attribute ) );
			}
		}

		if ( PurchasableItemDescriptionsCache.Count == 0 )
		{
			foreach ( var pairing in TypeLibrary.GetTypesWithAttribute<PurchasableAttribute>() )
			{
				Log.Info( pairing.Type.TargetType );
				PurchasableItemDescriptionsCache.Add(
					new PurchasableItemDescription( pairing.Type, pairing.Attribute ) );
			}
		}
	}

	public static WeaponPartDescription GetWeaponPartDescription<T>()
	{
		Init();
		return WeaponPartDescriptionsCache.FirstOrDefault( knownWeaponPartDescription =>
			knownWeaponPartDescription.TypeDescription.TargetType == typeof(T) );
	}

	public static WeaponPartDescription GetWeaponPartDescription( Type type )
	{
		Init();
		return WeaponPartDescriptionsCache.FirstOrDefault( knownWeaponPartDescription =>
			knownWeaponPartDescription.TypeDescription.TargetType == type );
	}

	public static WeaponPartDescription GetWeaponPartDescription( TypeDescription typeDescription )
	{
		Init();
		return WeaponPartDescriptionsCache.FirstOrDefault( knownWeaponPartDescription =>
			knownWeaponPartDescription.TypeDescription == typeDescription );
	}

	public static WeaponPartDescription GetWeaponPartDescription( string typeName )
	{
		Init();
		return WeaponPartDescriptionsCache.FirstOrDefault( knownWeaponPartDescription =>
			knownWeaponPartDescription.TypeDescription.TargetType.FullName == typeName );
	}

	public static WeaponDescription GetWeaponDescription( Type type )
	{
		Init();
		return WeaponDescriptionsCache.FirstOrDefault( knownWeaponDescription =>
			knownWeaponDescription.TypeDescription.TargetType == type );
	}
}
