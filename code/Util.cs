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

	private static Hud _hud;

	public static Hud Hud
	{
		get
		{
			if ( _hud != null )
			{
				return _hud;
			}

			CreateHud( Game.RootPanel );

			return _hud;
		}
	}

	public static void CreateHud( RootPanel rootPanel )
	{
		Game.AssertClient();

		if ( _hud != null )
		{
			return;
		}

		_hud = rootPanel.AddChild<Hud>();
		if ( _hud != null )
		{
			Log.Info( "Created local HUD" );
		}
	}

	public static Workshop Workshop { get; private set; }
	public static bool IsWorkshopOpen;
	public static string WorkshopOpenClass => IsWorkshopOpen ? "workshop-open" : "workshop-closed";
	public static void ToggleWorkshop( RootPanel rootPanel )
	{
		Game.AssertClient();

		if ( Workshop != null )
		{
			IsWorkshopOpen = false;
			Hud.StateHasChanged();
			foreach (var child in Hud.Children)
			{
				child.StateHasChanged();
			}
			Workshop.Delete();
			Workshop = null;
			_hud?.RemoveClass( "workshop-open" );
			return;
		}

		Workshop = rootPanel.AddChild<Workshop>();
		if ( Workshop == null )
		{
			return;
		}

		Log.Info( "Created local workshop" );
		_hud?.AddClass( "workshop-open" );
		IsWorkshopOpen = true;
		Hud.StateHasChanged();
		foreach (var child in Hud.Children)
		{
			child.StateHasChanged();
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
