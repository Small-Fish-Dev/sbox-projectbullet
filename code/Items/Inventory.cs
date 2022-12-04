using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Items;

public partial class Inventory : EntityComponent
{
	[Net] public int Money { get; set; } = 0;
	[Net] private IList<Entity> ItemsInternal { get; set; } = new List<Entity>();

	public ReadOnlyCollection<IInventoryItem> Items
	{
		get
		{
			IList<IInventoryItem> output = new List<IInventoryItem>();
			foreach ( var entity in ItemsInternal )
			{
				if ( entity is IInventoryItem item )
				{
					output.Add( item );
				}
			}

			return output.AsReadOnly();
		}
	}

	public void Add( IInventoryItem item ) => ItemsInternal.Add( (Entity) item );
	public IInventoryItem Find( Guid uid ) => Items.SingleOrDefault( v => v.Uid == uid );
}
