using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI.HUD;

public partial class Hitmarker : Panel
{
	private readonly TimeUntil _timeUntilDeleted;

	public Hitmarker()
	{
		_timeUntilDeleted = 2;
	}

	public override void Tick()
	{
		base.Tick();

		if ( _timeUntilDeleted <= 0 )
		{
			Delete();
		}
	}
}
