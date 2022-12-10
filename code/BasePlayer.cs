using System.Collections.Generic;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet;

public abstract partial class BasePlayer : Player
{
	public new Inventory Inventory => Components.Get<Inventory>();

	[Net] public IList<NodeExecutionEntity> NodeExecutors { get; private set; } = new List<NodeExecutionEntity>();

	public override void Spawn()
	{
		EnableLagCompensation = true;

		Tags.Add( "player" );

		Components.Create<Inventory>();

		base.Spawn();
	}
}
