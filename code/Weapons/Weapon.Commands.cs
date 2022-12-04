using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Items;
using ProjectBullet.UI.Editor;
using Sandbox;

namespace ProjectBullet.Weapons;

public partial class Weapon : BaseWeapon
{
	/// <summary>
	/// Get weapon by network ident and make sure it's from the ConsoleSystem Caller
	/// </summary>
	/// <returns>Weapon or null</returns>
	private static Weapon GetCallerWeapon( int networkIdent )
	{
		var ent = Entity.FindByIndex( networkIdent );
		if ( ent == null )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} tried to get unknown weapon - ident {networkIdent}" );
			return null;
		}

		if ( ent is not Weapon weapon )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} tried to get entity that's not a weapon - ident {networkIdent}" );
			return null;
		}

		if ( ent.Owner != ConsoleSystem.Caller.Pawn )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} tried to get weapon that's not theirs" );
			return null;
		}

		return weapon;
	}

	private static WeaponPart GetCallerWeaponPart( int weaponIdent, Guid weaponPartUid )
	{
		var weapon = GetCallerWeapon( weaponIdent );
		if ( weapon == null )
		{
			Log.Error( "GetCallerWeaponPart failed" );
			return null;
		}

		var part = weapon.Parts.SingleOrDefault( v => v.Uid == weaponPartUid );
		if ( part == null )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} tried to get unknown weapon part - guid {weaponPartUid}" );
			return null;
		}

		return part;
	}

	private static WeaponPart GetCallerWeaponPart( int weaponIdent, string weaponPartUid )
	{
		if ( !Guid.TryParse( weaponPartUid, out Guid uid ) )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} unreadable GUID for weapon part" );
			return null;
		}

		return GetCallerWeaponPart( weaponIdent, uid );
	}

	/// <summary>
	/// Set output property to null on provided weapon part
	/// </summary>
	/// <param name="weaponIdent">Weapon identifier</param>
	/// <param name="weaponPartUid">Weapon part identifier</param>
	/// <param name="partOutputIdent">Weapon part output identifier</param>
	[ConCmd.Server]
	public static void ClearWeaponPartOutputTarget( int weaponIdent, string weaponPartUid, string partOutputIdent )
	{
		var part = GetCallerWeaponPart( weaponIdent, weaponPartUid );
		if ( part == null )
		{
			Log.Error( "ClearWeaponPartOutputTarget failed" );
			return;
		}

		var outputDescription = part.OutputDescriptions.SingleOrDefault( v => v.PartOutputIdent == partOutputIdent );
		if ( outputDescription == null )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} tried to get unknown output {partOutputIdent}" );
			return;
		}

		outputDescription.PropertyDescription.SetValue( part, null );
	}

	/// <summary>
	/// Add weapon part to weapon
	/// </summary>
	/// <param name="weaponIdent">Weapon identifier</param>
	/// <param name="weaponPartUid">Weapon part identifier</param>
	[ConCmd.Server]
	public static void AddWeaponPart( int weaponIdent, string weaponPartUid )
	{
		var weapon = GetCallerWeapon( weaponIdent );
		Log.Info( weaponIdent );
		Log.Info( weaponPartUid );
		if ( weapon == null )
		{
			Log.Error( "AddWeaponPart failed" );
			return;
		}
		
		var parts = WeaponPart.GetUnusedWeaponParts( ConsoleSystem.Caller.Pawn );
		if ( parts == null )
		{
			Log.Error( "Failed to get unused parts for player {ConsoleSystem.Caller.Name}" );
			return;
		}
		
		if ( !Guid.TryParse( weaponPartUid, out Guid uid ) )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} unreadable GUID for weapon part" );
			return;
		}

		var part = parts.SingleOrDefault( v => v.Uid == uid );
		if ( part == null )
		{
			Log.Error( $"{ConsoleSystem.Caller.Name} tried to get unknown weapon part - guid {weaponPartUid}" );
			return;
		}
		
		part.Owner = weapon;
	}
	
	/// <summary>
	/// Remove weapon part from weapon
	/// </summary>
	/// <param name="weaponIdent">Weapon identifier</param>
	/// <param name="weaponPartUid">Weapon part identifier</param>
	[ConCmd.Server]
	public static void RemoveWeaponPart( int weaponIdent, string weaponPartUid )
	{
		var part = GetCallerWeaponPart( weaponIdent, weaponPartUid );
		if ( part == null )
		{
			Log.Error( "RemoveWeaponPart failed" );
			return;
		}

		var player = part.Owner.Owner;
		part.Owner = player;
	}
}
