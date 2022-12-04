using System.Linq;
using Sandbox;

namespace ProjectBullet.Items;

public partial class ShopItem : Entity
{
	/// <summary>
	/// String used to identify respective PurchasableItemDescription.
	/// </summary>
	[Net, Change( "OnIdentChanged" )]
	public string PurchasableItemIdent { get; private set; }

	private PurchasableItemDescription _description;
	public PurchasableItemDescription Description => _description;

	public string DisplayName { get; private set; }
	public int Price => _description.Price;

	private void UpdatePurchasableItemDescription( string ident )
	{
		var newDescription = StaticStorage.PurchasableItemDescriptions.SingleOrDefault( v =>
			v.PurchasableItemIdent == ident );

		if ( newDescription == null )
		{
			Log.Error( $"UpdatePurchasableItemDescription can't find description for {ident}, skipping" );
			return;
		}

		_description = newDescription;

		if ( _description == null )
		{
			Log.Warning( "UpdatePurchasableItemDescription still has no description!" );
			return;
		}

		DisplayName = _description.FindDisplayName();
	}

	public ShopItem( PurchasableItemDescription description )
	{
		PurchasableItemIdent = description.PurchasableItemIdent;
		UpdatePurchasableItemDescription( PurchasableItemIdent );
	}

	public ShopItem() { }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	private void OnIdentChanged( string o, string n ) => UpdatePurchasableItemDescription( n );
}
