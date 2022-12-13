using Editor;
using Sandbox;

namespace ProjectBullet.MapEnts;

[Library( "pb_moneyarea" ), HammerEntity, Solid]
[Title( "Money Area" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Area that players can get money in" )]
public class MoneyArea : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();

		EnableTouch = true;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		Log.Info( "touching" );
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		Log.Info( "not touching" );
	}
}
