using System.Linq;
using ProjectBullet.MapEnts;
using ProjectBullet.UI;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet;

public static class Util
{
	// note(lotuspar): maybe AssertClient here?
	public static Core.Player LocalPlayer => Game.LocalPawn as Core.Player;
	public static Core.PersistentData LocalPersistent => LocalPlayer.Persistent;
	public static Core.PlayerTeam LocalTeam => LocalPlayer.Team;

	private static HudView _hudView;

	public static HudView Hud
	{
		get
		{
			if ( _hudView != null )
			{
				return _hudView;
			}

			CreateHud( Game.RootPanel );

			return _hudView;
		}
	}

	public static void CreateHud( RootPanel rootPanel )
	{
		Game.AssertClient();

		if ( _hudView != null )
		{
			return;
		}

		_hudView = rootPanel.AddChild<HudView>();
		if ( _hudView != null )
		{
			Log.Info( "Created local HUD" );
		}
	}

	private static MapConfig _mapConfigCache;

	public static MapConfig MapConfig
	{
		get
		{
			_mapConfigCache ??= Entity.All.OfType<MapConfig>().SingleOrDefault();
			return _mapConfigCache;
		}
	}
}
