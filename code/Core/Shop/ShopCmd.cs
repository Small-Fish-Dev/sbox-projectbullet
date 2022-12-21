using System.Linq;
using ProjectBullet.Core.Node;
using Sandbox;

namespace ProjectBullet.Core.Shop;

/// <summary>
/// Client -> server communication for the shop system
/// </summary>
public static class ShopCmd
{
	[ConCmd.Server]
	private static void BuyItem( int stockedItemNetworkIndex )
	{
		Game.AssertServer();

		var shopHost = ShopHostEntity.Instance;
		var inventory = ConsoleSystem.Caller.Pawn?.Components?.Get<Inventory>();
		var item = shopHost.Stock.SingleOrDefault( v => v.NetworkIdent == stockedItemNetworkIndex );

		if ( inventory == null )
		{
			Log.Warning(
				$"{ConsoleSystem.Caller.Name} tried to buy an item but they have no inventory" );
			return;
		}

		if ( item == null )
		{
			Log.Warning(
				$"{ConsoleSystem.Caller.Name} tried to buy an unknown item - index {stockedItemNetworkIndex}" );
			return;
		}

		if ( !inventory.UseMoney( item.ShopItemAttribute.Price ) )
		{
			Log.Info(
				$"{ConsoleSystem.Caller.Name} tried to buy {item.GetDisplayName()} but they don't have the funds" );
			return;
		}

		if ( item.Description is WeaponNodeDescription wnd )
		{
			var instance = wnd.TypeDescription.Create<WeaponNode>();
			instance.Owner = (Entity)ConsoleSystem.Caller.Pawn;
			inventory.Add( instance );
		}

		else
		{
			Log.Error( $"Unknown item description type {item.Description.GetType()} - can't give player item" );
		}
	}

	/// <summary>
	/// Send BuyItem request to the server - will buy an item if the player has funds
	/// </summary>
	/// <param name="item"><see cref="ShopHostEntity.StockedItem"/> to buy</param>
	public static void BuyItem( ShopHostEntity.StockedItem item ) => BuyItem( item.NetworkIdent );
}
