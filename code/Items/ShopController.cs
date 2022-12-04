using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Items;

public partial class ShopController : Entity
{
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	[Net] public IList<ShopItem> Stock { get; set; } = new List<ShopItem>();

	public void AddAllPurchasableItems()
	{
		foreach ( var purchasableItemDescription in StaticStorage.PurchasableItemDescriptions )
		{
			Stock.Add( new ShopItem( purchasableItemDescription ) );
		}
	}

	[ConCmd.Admin( "pb_debug_setmoney" )]
	public static void SetMoneyCCmd( int money )
	{
		var inventory = ConsoleSystem.Caller.Pawn?.Components.Get<Inventory>();
		if ( inventory == null )
		{
			Log.Info( $"{ConsoleSystem.Caller.Name} has no inventory" );
			return;
		}

		inventory.Money = money;
		Log.Info( $"Set money for {ConsoleSystem.Caller.Name} to {money}" );
	}

	[ConCmd.Server( "pb_internal_buy" )]
	public static void BuyItemCCmd( string purchasableItemIdent )
	{
		var shop = All.OfType<ShopController>().Single();
		var stockedItem = shop.Stock.SingleOrDefault( v => v.PurchasableItemIdent == purchasableItemIdent );

		if ( stockedItem == null )
		{
			Log.Info( $"{ConsoleSystem.Caller.Name} tried to buy unknown / unstocked item {purchasableItemIdent}" );
			return;
		}

		var inventory = ConsoleSystem.Caller.Pawn?.Components.Get<Inventory>();
		if ( inventory == null )
		{
			Log.Info( $"{ConsoleSystem.Caller.Name} tried to buy an item without an inventory" );
			return;
		}

		if ( inventory.Money < stockedItem.Price )
		{
			Log.Info(
				$"{ConsoleSystem.Caller.Name} tried to buy item {purchasableItemIdent} but didn't have the funds" );
			return;
		}

		inventory.Money -= stockedItem.Price;
		var instance = stockedItem.Description.CreateInventoryItemInstance( Guid.NewGuid() );
		if ( instance is Entity entity )
		{
			entity.Owner = ConsoleSystem.Caller.Pawn;
		}
		inventory.Add( instance );
	}
}
