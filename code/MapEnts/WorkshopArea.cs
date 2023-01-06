using Editor;
using Sandbox;
using Player = ProjectBullet.Core.Player;

namespace ProjectBullet.MapEnts;

[Library( "pb_workshop_area" ), HammerEntity, Solid]
[Title( "Workshop Area" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Area that players can open the workshop in" )]
public class WorkshopArea : BaseTrigger
{
	public override void OnTouchStart( Entity entity )
	{
		base.OnTouchStart( entity );

		if ( entity is Player player )
		{
			player.CanUseWorkshop = true;
		}
	}

	public override void OnTouchEnd( Entity entity )
	{
		base.OnTouchEnd( entity );

		if ( entity is Player player )
		{
			player.CanUseWorkshop = false;
		}
	}
}
