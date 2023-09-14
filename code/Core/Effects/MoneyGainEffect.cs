using ProjectBullet.MapEnts;
using Sandbox;

namespace ProjectBullet.Core.Effects;

public partial class MoneyGainEffect : BaseEffect
{
	[Net] private int MoneyPerSecond { get; set; }
	[Net, Predicted] private TimeUntil TimeUntilGive { get; set; }

	public MoneyGainEffect( MoneyArea moneyArea ) : base( moneyArea )
	{
		MoneyPerSecond = moneyArea.MoneyPerSecond;
		TimeUntilGive = 1;
	}

	public MoneyGainEffect( int mps )
	{
		MoneyPerSecond = mps;
		TimeUntilGive = 1;
	}

	public MoneyGainEffect() => Game.AssertClient();

	[Event.Tick]
	private void Tick()
	{
		if ( TimeUntilGive )
		{
			TimeUntilGive = 1;

			if ( Game.IsServer )
			{
				Entity.Persistent.GiveMoney( MoneyPerSecond );
			}
		}
	}
}
