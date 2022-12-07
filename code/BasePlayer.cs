using System.Collections.Generic;
using ProjectBullet.Core.Node;
using Sandbox;

namespace ProjectBullet;

public abstract partial class BasePlayer : Player
{
	[Net] public int Money { get; set; } = 0;

	[Net] protected IList<NodeExecutionEntity> ActiveNodeExecutors { get; set; } = new List<NodeExecutionEntity>();
}
