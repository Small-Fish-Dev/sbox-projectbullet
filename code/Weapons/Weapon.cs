using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Weapons;

[AttributeUsage( AttributeTargets.Class )]
public class KnownWeaponAttribute : Attribute
{
	public string DisplayName;
	public string DisplayDescription;
}

public class WeaponDescription
{
	private readonly KnownWeaponAttribute _attribute;
	public TypeDescription TypeDescription { get; }

	public WeaponDescription( TypeDescription typeDescription, KnownWeaponAttribute attribute )
	{
		TypeDescription = typeDescription;
		_attribute = attribute;
	}

	public string Name => TypeDescription.Name;
	public string DisplayName => _attribute == null ? Name : _attribute.DisplayName;
}

public partial class Weapon : Entity
{
	public WeaponDescription WeaponDescription => WeaponStorage.GetWeaponDescription( GetType() );

	public IEnumerable<InventoryItem> UsedItems =>
		Owner.Components.Get<Inventory>().PartItems.Where( v => v.Weapon == this );

	[Net] private List<Part> Parts { get; set; } = new();

	[ConCmd.Server]
	public static void UpdateParts( int networkIdent, string data )
	{
		var weapon = (Weapon)Entity.FindByIndex( networkIdent );

		if ( weapon == null )
		{
			Log.Info( $"Couldn't find weapon with ident {networkIdent}" );
			return;
		}

		SerializedPartNode graph;

		try
		{
			graph = Json.Deserialize<SerializedPartNode>( data );
		}
		catch ( Exception )
		{
			Log.Info( $"Failed to update parts for weapon {weapon}" );
			return;
		}

		var inventory = weapon.Owner.Components.Get<Inventory>();

		if ( inventory == null )
		{
			Log.Info( $"Couldn't find inventory for owner {weapon.Owner}" );
			return;
		}

		// Clear
		foreach ( var inventoryItem in weapon.UsedItems )
		{
			inventoryItem.Weapon = null;
		}

		// Resolve nodes
		foreach ( var node in graph.Parts.SelectMany( v => v.Parts ) )
		{
			var inventoryItem = inventory.GetItem( node.ItemUid );
			if ( inventoryItem == null )
			{
				Log.Info( $"Couldn't find inventory item {node.ItemUid} for weapon {weapon}" );
				return;
			}

			inventoryItem.Weapon = weapon;
		}
		
		// Create instances
		foreach ( var inventoryItem in weapon.UsedItems )
		{
			
		}
	}
}
