using System;
using Sandbox;

namespace ProjectBullet.Items;

[AttributeUsage( AttributeTargets.Class )]
public class PurchasableAttribute : Attribute
{
	public int Price;

	public PurchasableAttribute( int price ) => Price = price;
}

public class PurchasableItemDescription
{
	private readonly PurchasableAttribute _attribute;
	public TypeDescription TypeDescription { get; }
	public int Price => _attribute.Price;

	/// <summary>
	/// Value to uniquely identify the <see cref="PurchasableItemDescription"/>
	/// </summary>
	public string DescriptionIdent => GetType().Name;

	/// <summary>
	/// Value to uniquely identify the type of purchasable item
	/// </summary>
	public string PurchasableItemIdent => TypeDescription.TargetType.FullName;

	public PurchasableItemDescription( TypeDescription typeDescription, PurchasableAttribute attribute )
	{
		TypeDescription = typeDescription;

		_attribute = attribute;
	}

	/// <summary>
	/// Finds display name of item based on type
	/// </summary>
	/// <returns>Display name or null</returns>
	public string FindDisplayName()
	{
		
	}
}
