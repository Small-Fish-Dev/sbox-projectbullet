﻿using System;
using System.Collections.Generic;
using ProjectBullet.Core.Node;
using Sandbox;

namespace ProjectBullet.Core.Shop;

/// <summary>
/// Shop controller entity
/// </summary>
public partial class ShopHostEntity : Entity
{
	public ShopHostEntity()
	{
		Transmit = TransmitType.Always;
		Instance = this;
	}

	public static ShopHostEntity Instance { get; private set; }

	public enum StockedItemType
	{
		WeaponNode = 0,
	}

	// todo: this shouldn't need to be an entity...
	public partial class StockedItem : Entity
	{
		public StockedItem() => Transmit = TransmitType.Always;
		public ShopItemAttribute ShopItemAttribute { get; private set; }
		[Net] private StockedItemType StockedItemType { get; set; }
		[Net, Change( "OnTypeChanged" )] private string ItemTypeName { get; set; }
		public object Description { get; private set; }
		public int Price => ShopItemAttribute.Price;

		/// <summary>
		/// Find item display name based on description type
		/// </summary>
		/// <returns>Display name or "Unknown"</returns>
		public string GetDisplayName()
		{
			if ( Description is WeaponNodeDescription { NodeAttribute.DisplayName: { } } weaponNodeDescription )
			{
				return weaponNodeDescription?.NodeAttribute?.DisplayName;
			}

			return "Unknown";
		}

		/// <summary>
		/// Set item to a WeaponNode
		/// </summary>
		/// <param name="weaponNodeDescription">Description of the WeaponNode</param>
		public void SetItem( WeaponNodeDescription weaponNodeDescription )
		{
			StockedItemType = StockedItemType.WeaponNode;
			ItemTypeName = weaponNodeDescription.TargetType.FullName;
			Description = weaponNodeDescription;
			ShopItemAttribute = weaponNodeDescription.TypeDescription.GetAttribute<ShopItemAttribute>();
		}

		private void OnTypeChanged()
		{
			var typeDescription = TypeLibrary.GetType( ItemTypeName );
			if ( typeDescription == null )
			{
				throw new Exception( $"Type {ItemTypeName} not found" );
			}

			// Get ShopItemAttribute
			ShopItemAttribute = typeDescription.GetAttribute<ShopItemAttribute>();

			if ( StockedItemType == StockedItemType.WeaponNode )
			{
				Description = WeaponNodeDescription.Get( typeDescription );
			}
		}
	}

	[Net] public IList<StockedItem> Stock { get; set; } = new List<StockedItem>();

	public void StockAllItems()
	{
		Game.AssertServer(  );

		foreach ( var pair in TypeLibrary.GetTypesWithAttribute<ShopItemAttribute>() )
		{
			var item = new StockedItem();
			item.SetItem( WeaponNodeDescription.Get( pair.Type ) );
			Stock.Add( item );
		}
	}
}
