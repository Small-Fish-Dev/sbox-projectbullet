using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Nodes;

[Connector( "on_start", Order = 0, EnergyOutPercentage = 1.0f )]
[Node( DisplayName = "Start", Description = "The start of your weapon code" )]
public class StartNode : WeaponNodeEntity
{
	public override void Execute( Entity target, Vector3 point )
	{
		Log.Warning( "StartNode has been executed. This shouldn't be done." );
		ExecuteConnector( "on_start", target, point );
	}
}
