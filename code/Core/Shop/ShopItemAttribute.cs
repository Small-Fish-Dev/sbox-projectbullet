using System;

namespace ProjectBullet.Core.Shop;

[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ShopItemAttribute : Attribute
{
	public int Price { get; }

	public ShopItemAttribute( int price )
	{
		Price = price;
	}
}
