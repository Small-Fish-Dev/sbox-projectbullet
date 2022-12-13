using Editor;
using ProjectBullet.Player;
using Sandbox;

namespace ProjectBullet.MapEnts;

[Library( "pb_buyarea" ), HammerEntity, Solid]
[Title( "Buy Area" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Area that players can edit their nodes in" )]
public class BuyArea : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();

		EnableTouch = true;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is BasePlayer player )
		{
			player.CanUseEditor = true;
		}
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is BasePlayer player )
		{
			player.CanUseEditor = false;
		}
	}
}
