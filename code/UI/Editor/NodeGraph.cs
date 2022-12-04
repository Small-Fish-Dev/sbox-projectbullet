using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Items;
using ProjectBullet.Weapons;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class NodeGraph : Panel
{
	/// <summary>
	/// Current weapon being visualised
	/// </summary>
	public Weapon Weapon { get; set; }

	private bool _waitingForInit;

	public GraphableWeaponPart GraphableStartNode => Weapon.ClientGraphableStartPart;
	public List<GraphableWeaponPart> DisplayedParts { get; set; } = new();

	public NodeGraph( Weapon weapon ) => SwitchWeapon( weapon );

	public NodeGraph() => _waitingForInit = true;

	public override void Tick()
	{
		base.Tick();

		if ( _waitingForInit && Weapon == null )
		{
			SwitchWeapon( Weapon );
			_waitingForInit = false;
		}
	}

	public void RemovePart( GraphableWeaponPart part )
	{
		part.Element.Delete();

		Log.Info( part.Input );
		Log.Info( part.Input.ConnectedTo );
		part.Input.ConnectedTo = null;

		foreach ( var graphableOutput in part.Outputs )
		{
			graphableOutput.Target = null;
		}

		DisplayedParts.Remove( part );

		Weapon.RemoveWeaponPart( Weapon.NetworkIdent, part.WeaponPart.Uid.ToString() );
		
		StateHasChanged();
	}

	public void AddPart( WeaponPart part )
	{
		DisplayedParts.Add( new GraphableWeaponPart( part ) );
		
		StateHasChanged();
	}

	/// <summary>
	/// Visualise a different weapon
	/// </summary>
	/// <param name="weapon">New weapon</param>
	public void SwitchWeapon( Weapon weapon )
	{
		if ( weapon == Weapon )
		{
			Log.Info( "Tried to use SwitchWeapon to go to same weapon" );
			return;
		}

		Weapon = weapon;
		foreach ( var usedWeaponPart in WeaponPart.GetUsedWeaponParts( Weapon ) )
		{
			DisplayedParts.Add( new GraphableWeaponPart( usedWeaponPart ) );
		}
	}
}
