using Editor;
using Sandbox;

namespace ProjectBullet.MapEnts;

[Library( "pb_money_area" ), HammerEntity, Solid]
[Title( "Money Area" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Area that players can get money in" )]
public class MoneyArea : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public override void OnTouchStart( Entity entity )
	{
		base.OnTouchStart( entity );

		entity.Tags.Add( "in_money_area" );
	}

	public override void OnTouchEnd( Entity entity )
	{
		base.OnTouchEnd( entity );

		entity.Tags.Remove( "in_money_area" );
	}
}
