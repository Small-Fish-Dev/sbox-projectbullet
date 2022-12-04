using System;
using Sandbox;

namespace ProjectBullet.Items;

public abstract partial class InventoryItem : BaseNetworkable
{
	[Net] public Guid Uid { get; protected set; } = Guid.NewGuid();
}
