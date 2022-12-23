using Editor;
using Sandbox;

namespace ProjectBullet.MapEnts;

[Library( "pb_workshop_area" ), HammerEntity, Solid]
[Title( "Workshop Area" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Area that players can open the workshop in" )]
public class WorkshopArea : BaseTrigger
{
	public override void OnTouchStart( Entity entity )
	{
		base.OnTouchStart( entity );

		entity.Tags.Add( "can_workshop" );
	}

	public override void OnTouchEnd( Entity entity )
	{
		base.OnTouchEnd( entity );

		entity.Tags.Remove( "can_workshop" );
	}
}
