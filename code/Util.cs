using ProjectBullet.UI;
using Sandbox;
using Sandbox.UI;
using Player = ProjectBullet.Core.Player;

namespace ProjectBullet;

/// <summary>
/// Static utility / helper class
/// </summary>
public static class Util
{
	// note(lotuspar): maybe AssertClient for these?
	/// <summary>
	/// The local player's pawn as a ProjectBullet <see cref="Player"/>. Client-only.
	/// </summary>
	public static Player LocalPlayer => Game.LocalPawn as Player;

	/// <summary>
	/// <see cref="Core.PersistentData"/> for local player. Client-only.
	/// </summary>
	public static Core.PersistentData LocalPersistent => LocalPlayer.Persistent;

	/// <summary>
	/// <see cref="Core.PlayerTeam"/> the local player is on. Client-only.
	/// </summary>
	public static Core.PlayerTeam LocalTeam => LocalPlayer.Team;
	public static Workshop Workshop { get; private set; }

	/// <summary>
	/// Whether or not the <see cref="Workshop"/> is open and ready for use.
	/// </summary>
	public static bool IsWorkshopOpen { get; private set; }

	/// <summary>
	/// Returns "workshop-open" or "workshop-closed" depending on <see cref="IsWorkshopOpen"/>. Used for CSS element classes.
	/// </summary>
	public static string WorkshopOpenClass => IsWorkshopOpen ? "workshop-open" : "workshop-closed";

	/* HUD setup */
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

	/// <summary>
	/// Create the <see cref="Hud"/> if it doesn't exist
	/// </summary>
	/// <param name="rootPanel"><see cref="RootPanel"/> to add new <see cref="Hud"/> to if needed</param>
	public static void CreateHud( RootPanel rootPanel )
	{
		Game.AssertClient();

		if ( _hud != null )
		{
			return;
		}

		_hud = rootPanel.AddChild<Hud>();
	}

	/* Workshop setup */
	/// <summary>
	/// Toggle the <see cref="Workshop"/> visibility / status
	/// </summary>
	/// <param name="rootPanel"><see cref="RootPanel"/> to add new <see cref="Workshop"/> to if needed</param>
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
