using System.Linq;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Node.Description;
using Sandbox;

namespace ProjectBullet.Core.Shop;

public static class ShopCmd
{
	[ConCmd.Server( "pb_buyitem" )]
	public static void BuyItem( int stockedItemNetworkIndex )
	{
		var shopHost = ShopHostEntity.Instance;
		var inventory = ConsoleSystem.Caller.Pawn.Components.Get<Inventory>();
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
			var instance = wnd.TypeDescription.Create<WeaponNodeEntity>();
			instance.Owner = (Entity)ConsoleSystem.Caller.Pawn;
			inventory.Add( instance );
		}

		else
		{
			Log.Error( $"Unknown item description type {item.Description.GetType()} - can't give player item" );
		}
	}

	public static void BuyItem( ShopHostEntity.StockedItem item ) => BuyItem( item.NetworkIdent );
}
