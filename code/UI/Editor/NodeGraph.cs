using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Items;
using ProjectBullet.Weapons;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class NodeGraph : Panel
{
	private bool _waitingForInit;

	public ContextMenu ContextMenu { get; set; }

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

	private void UpdateGraph()
	{
		// Request context menu redraw
		ContextMenu.StateHasChanged();

		// Request self redraw
		StateHasChanged();

		// Request redraw of all important descendants
		foreach ( var descendant in Descendants )
		{
			switch ( descendant )
			{
				case Node:
				case NodeInput:
				case NodeOutput:
					descendant.StateHasChanged();
					break;
			}
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
		var gwp = new GraphableWeaponPart( part );
		ActiveGraphableParts.Add( gwp );

		// Remove from inactive
		InactiveParts.Remove( part );

		// Add to graph
		AddChild( new Node( gwp, this ) );

		// Redraw elements
		UpdateGraph();
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
		UpdateGraph();
	}

	/// <summary>
	/// Visualise a different weapon
	/// </summary>
	/// <param name="weapon">New weapon</param>
	public void SwitchWeapon( Weapon weapon )
	{
		Log.Info( "SWITCH WEAPON!!!!!!!" );

		// Clean up first
		ActiveGraphableParts.Clear();
		InactiveParts.Clear();

		// Remove old nodes from the graph
		var cleanList = Descendants.OfType<Node>().ToList();
		foreach ( var node in cleanList )
		{
			Log.Info( $"Removing old node {node}, {node.GraphableWeaponPart.DisplayName}" );
			node.Delete( true );
		}
	
		// Set weapon
		Weapon = weapon;
		
		// Add starting node
		AddChild( new Node( GraphableStartNode ) );
		
		foreach ( var usedWeaponPart in WeaponPart.GetUsedWeaponParts( Weapon ) )
		{
			var gwp = new GraphableWeaponPart( usedWeaponPart );
			ActiveGraphableParts.Add( gwp );
		}

		foreach ( var gwp in ActiveGraphableParts )
		{
			// Resolve links
			//gwp.WeaponPart.
			
			// Add node to graph
			AddChild( new Node( gwp, this ) );
		}

		foreach ( var unusedWeaponPart in WeaponPart.GetUnusedWeaponParts( Weapon.Owner ) )
		{
			InactiveParts.Add( unusedWeaponPart );
		}

		StateHasChanged();
	}
}
