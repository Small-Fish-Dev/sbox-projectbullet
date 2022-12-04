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
	private TypeDescription _typeDescription;
	private readonly Type _type;

	public TypeDescription TypeDescription
	{
		get
		{
			if ( _typeDescription?.TargetType != null )
			{
				return _typeDescription;
			}

			if ( _type != null )
			{
				_typeDescription = TypeLibrary.GetDescription( _type );
			}

			return _typeDescription;
		}
	}

	public Type TargetType => _type;
	
	public int Price => _attribute.Price;

	/// <summary>
	/// Value to uniquely identify the <see cref="PurchasableItemDescription"/>
	/// </summary>
	public string DescriptionIdent => GetType().Name;

	/// <summary>
	/// Value to uniquely identify the type of purchasable item
	/// </summary>
	public string PurchasableItemIdent => _typeDescription.TargetType.FullName;

	public PurchasableItemDescription( TypeDescription typeDescription, PurchasableAttribute attribute )
	{
		_typeDescription = typeDescription;
		_type = typeDescription.TargetType;
		_attribute = attribute;
	}

	public bool IsWeaponPart => _typeDescription.TargetType.IsSubclassOf( typeof(WeaponPart) );

	public WeaponPartDescription GetWeaponPartDescription() =>
		StaticStorage.GetWeaponPartDescription( _typeDescription.TargetType );

	/// <summary>
	/// Finds display name of item based on type
	/// </summary>
	/// <returns>Display name or null</returns>
	public string FindDisplayName()
	{
		if ( IsWeaponPart )
		{
			return GetWeaponPartDescription().DisplayName;
		}

		return "";
	}

	/// <summary>
	/// Create instance of actual item type
	/// </summary>
	/// <returns>Entity</returns>
	public IInventoryItem CreateInventoryItemInstance( Guid uid )
	{
		Log.Info( _typeDescription );
		if ( IsWeaponPart )
		{
			var part = GetWeaponPartDescription().Create();
			var item = (IInventoryItem)part;
			item.Uid = uid;
			return part;
		}

		throw new InvalidOperationException( "CreateInventoryItemInstance unknown type" );
	}
}
