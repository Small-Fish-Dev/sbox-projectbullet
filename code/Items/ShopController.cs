using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Weapons;
using Sandbox;

namespace ProjectBullet.Items;

public enum ShopItemType
{
	WeaponPart = 0,
}

public partial class ShopItem : BaseNetworkable
{
	/// <summary>
	/// String used to identify respective PurchasableItemDescription.
	/// </summary>
	[Net, Change( "HandleNewDescriptionIdent" )]
	public string DescriptionIdent { get; private set; }

	private PurchasableItemDescription _description;

	public string DisplayName { get; private set; }
	public int Price => _description.Price;

	public ShopItem( PurchasableItemDescription description )
	{
		DescriptionIdent = description.DescriptionIdent;
		HandleNewDescriptionIdent();
	}

	public ShopItem() => HandleNewDescriptionIdent();

	private void HandleNewDescriptionIdent()
	{
		_description =
			StaticStorage.PurchasableItemDescriptions.SingleOrDefault( v => v.DescriptionIdent == DescriptionIdent );
	}
}

public partial class ShopController : Entity
{
	[Net] public List<ShopItem> Stock { get; set; } = new();

	public void AddAllPurchasableItems()
	{
		foreach ( var purchasableItemDescription in StaticStorage.PurchasableItemDescriptions )
		{
			Stock.Add( new ShopItem( purchasableItemDescription ) );
		}
	}
}
