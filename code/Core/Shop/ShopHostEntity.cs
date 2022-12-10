using System;
using System.Collections.Generic;
using ProjectBullet.Core.Node.Description;
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

	public partial class StockedItem : BaseNetworkable
	{
		public ShopItemAttribute ShopItemAttribute { get; private set; }
		[Net] private StockedItemType StockedItemType { get; set; }
		[Net, Change( "OnTypeChanged" )] private string ItemTypeName { get; set; }
		public object Description { get; private set; }

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
}
