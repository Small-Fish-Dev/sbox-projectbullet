using Editor;
using ProjectBullet.Core.Effects;
using Sandbox;

namespace ProjectBullet.MapEnts;

[Library( "pb_money_area" ), HammerEntity, Solid]
[Title( "Money Area" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Area that players can get money in" )]
public partial class MoneyArea : BaseTrigger
{
	[Net]
	[Property( Title = "Money Per Second" )]
	public int MoneyPerSecond { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public override void OnTouchStart( Entity entity )
	{
		base.OnTouchStart( entity );

		if ( Game.IsServer )
		{
			entity.Components.Add( new MoneyGainEffect( this ) );
		}
	}

	public override void OnTouchEnd( Entity entity )
	{
		base.OnTouchEnd( entity );

		if ( Game.IsClient )
		{
			return;
		}

		foreach ( var moneyAreaParticipant in entity.Components.GetAll<MoneyGainEffect>() )
		{
			if ( moneyAreaParticipant.Creator == this )
			{
				moneyAreaParticipant.Remove();
			}
		}
	}
}
