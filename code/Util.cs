using System.Collections.Generic;
using System.Linq;
using ProjectBullet.MapEnts;
using ProjectBullet.UI;
using Sandbox;
using Sandbox.UI;
using Player = ProjectBullet.Core.Player;

namespace ProjectBullet;

public static class Util
{
	// note(lotuspar): maybe AssertClient for these?
	public static Core.Player LocalPlayer => Game.LocalPawn as Core.Player;
	public static Core.PersistentData LocalPersistent => LocalPlayer.Persistent;
	public static Core.PlayerTeam LocalTeam => LocalPlayer.Team;

	public static MapConfig MapConfig => MapConfig.Instance;
	
	private static Hud _hud;
	public static Workshop Workshop { get; private set; }
	public static bool IsWorkshopOpen { get; private set; }
	public static string WorkshopOpenClass => IsWorkshopOpen ? "workshop-open" : "workshop-closed";
	
	/* HUD setup */
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
	
	/* Workshop setup */
	public static void ToggleWorkshop( RootPanel rootPanel )
	{
		Game.AssertClient();
		
		// Check if the workshop is already open ->
		if ( Workshop != null )
		{
			// ... the workshop is open, we need to close it
			IsWorkshopOpen = false;
			
			// Update the HUD
			Hud.StateHasChanged();
			
			// Update all HUD children
			foreach ( var child in Hud.Children )
			{
				child.StateHasChanged();
			}

			// Delete the workshop
			Workshop.Delete();
			Workshop = null;
			
			// Return so we don't create a new Workshop
			return;
		}

		// Attempt to create & add a new Workshop
		Workshop = rootPanel.AddChild<Workshop>();
		if ( Workshop == null )
		{
			return;
		}

		// The workshop is now open, set IsWorkshopOpen
		IsWorkshopOpen = true;
		
		// Update the HUD
		Hud.StateHasChanged();
			
		// Update all HUD children
		foreach ( var child in Hud.Children )
		{
			child.StateHasChanged();
		}
	}
}
