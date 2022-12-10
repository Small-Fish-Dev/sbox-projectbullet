using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node;
using Sandbox;

namespace ProjectBullet.Core.Shop;

/// <summary>
/// Player inventory / item storage component
/// </summary>
public partial class Inventory : EntityComponent
{
	[Net] public int Money { get; private set; } = 0;
	[Net] private IList<Entity> ItemsInternal { get; set; } = new List<Entity>();

	/// <summary>
	/// Read only list of player items
	/// </summary>
	public IEnumerable<IInventoryItem> Items
	{
		get
		{
			IList<IInventoryItem> output = new List<IInventoryItem>();
			foreach ( var entity in ItemsInternal )
			{
				if ( entity is IInventoryItem item )
				{
					output.Add( item );
				}
			}

			return output.AsReadOnly();
		}
	}

	public void Add( IInventoryItem item ) => ItemsInternal.Add( item as Entity );
	public IInventoryItem Find( Guid uid ) => Items.SingleOrDefault( v => v.Uid == uid );

	/// <summary>
	/// Attempt to use some player money
	/// </summary>
	/// <param name="amount">Amount to take</param>
	/// <returns>True if money was deducted, false if not enough money</returns>
	public bool UseMoney( int amount )
	{
		if ( amount > Money )
		{
			return false;
		}

		Money -= amount;
		return true;
	}

	/// <summary>
	/// Find an inventory item in any player inventory
	/// </summary>
	/// <param name="uid">Item UID</param>
	/// <returns>Item or null</returns>
	public static IInventoryItem FindAny( Guid uid ) =>
		Entity.All.OfType<IInventoryItem>().SingleOrDefault( v => v.Uid == uid );

	[ConCmd.Admin( "pb_inventory_givemoney" )]
	public static void GiveMoney( int amount )
	{
		var inventory = ConsoleSystem.Caller.Pawn.Components.Get<Inventory>();

		if ( inventory == null )
		{
			Log.Warning(
				$"{ConsoleSystem.Caller.Name} tried to give themselves money but they have no inventory" );
			return;
		}

		inventory.Money += amount;
	}
}
