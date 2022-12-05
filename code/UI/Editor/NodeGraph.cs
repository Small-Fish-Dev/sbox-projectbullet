using System.Collections.Generic;
using ProjectBullet.Items;
using ProjectBullet.Weapons;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class NodeGraph : Panel
{
	private bool _waitingForInit;

	/// <summary>
	/// Current weapon being visualised
	/// </summary>
	public Weapon Weapon { get; set; }

	public GraphableWeaponPart GraphableStartNode { get; set; } = new();

	/// <summary>
	/// List of <see cref="WeaponPart"/>s in the player's inventory but not on the weapon
	/// </summary>
	public List<WeaponPart> InactiveParts { get; set; } = new();

	/// <summary>
	/// List of <see cref="GraphableWeaponPart"/>s in the player's inventory and on the provided weapon
	/// </summary>
	public List<GraphableWeaponPart> ActiveGraphableParts { get; set; } = new();

	public NodeGraph( Weapon weapon ) => SwitchWeapon( weapon );
	public NodeGraph() => _waitingForInit = true;

	public override void Tick()
	{
		base.Tick();

		if ( _waitingForInit && Weapon != null )
		{
			SwitchWeapon( Weapon );
			_waitingForInit = false;
		}
	}

	/// <summary>
	/// Make part active
	/// </summary>
	/// <param name="part">WeaponPart</param>
	public void MovePartIntoActive( WeaponPart part )
	{
		if ( !InactiveParts.Contains( part ) )
		{
			Log.Error( "Part not contained in InactiveParts" );
			return;
		}

		// Add to active
		ActiveGraphableParts.Add( new GraphableWeaponPart( part ) );

		// Remove from inactive
		InactiveParts.Remove( part );

		// Redraw elements
		StateHasChanged();
	}

	/// <summary>
	/// Make part inactive
	/// </summary>
	/// <param name="part">GraphableWeaponPart</param>
	public void MovePartIntoInactive( GraphableWeaponPart part )
	{
		if ( !ActiveGraphableParts.Contains( part ) )
		{
			Log.Error( "Part not contained in ActiveGraphableParts" );
			return;
		}

		// Add to inactive
		InactiveParts.Add( part.WeaponPart );

		// Clean up part inputs
		part.Input.ConnectedTo = null;
		foreach ( var graphableOutput in part.Outputs )
		{
			graphableOutput.Target = null;
		}

		// Remove from active
		ActiveGraphableParts.Remove( part );

		// Remove part element
		part.Element.Delete();

		// Redraw elements
		StateHasChanged();
	}

	/// <summary>
	/// Visualise a different weapon
	/// </summary>
	/// <param name="weapon">New weapon</param>
	private void SwitchWeapon( Weapon weapon )
	{
		Log.Info( "SWITCH WEAPON!!!!!!!" );

		// Clean up first
		ActiveGraphableParts.Clear();
		InactiveParts.Clear();

		Weapon = weapon;
		Log.Info( "hi" );
		foreach ( var usedWeaponPart in WeaponPart.GetUsedWeaponParts( Weapon ) )
		{
			ActiveGraphableParts.Add( new GraphableWeaponPart( usedWeaponPart ) );
		}

		foreach ( var unusedWeaponPart in WeaponPart.GetUnusedWeaponParts( Weapon.Owner ) )
		{
			Log.Info( unusedWeaponPart );
			InactiveParts.Add( unusedWeaponPart );
		}

		StateHasChanged();
	}
}
