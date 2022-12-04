using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Items;

public partial class Inventory : EntityComponent
{
	[Net] public List<InventoryItem> Items { get; private set; } = new();
	public InventoryItem Find( Guid uid ) => Items.SingleOrDefault( v => v.Uid == uid );
}
