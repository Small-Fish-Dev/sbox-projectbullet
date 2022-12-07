using System;

namespace ProjectBullet.Core.Shop;

[System.AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ShopItemAttribute : System.Attribute
{
	public int Price { get; init; }

	public ShopItemAttribute( int price )
	{
		Price = price;
	}
}
