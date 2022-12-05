using System.Collections.Generic;
using ProjectBullet.Items;
using ProjectBullet.Weapons;

namespace ProjectBullet.UI.Editor;

public class GraphableWeaponPart
{
	public class GraphableInput
	{
		public GraphableInput( GraphableWeaponPart parent ) => Parent = parent;

		private GraphableOutput _connectedTo;

		public GraphableOutput ConnectedTo
		{
			get => _connectedTo;
			set
			{
				if ( value != null )
				{
					value.Target = this;
				}
				else
				{
					if ( _connectedTo != null )
					{
						_connectedTo.Target = null;
					}
				}
			}
		}

		public void SetRawConnectedTo( GraphableOutput output ) => _connectedTo = output;

		public NodeInput Element { get; set; }
		public GraphableWeaponPart Parent { get; }
	}

	public class GraphableOutput
	{
		public GraphableOutput( GraphableWeaponPart parent, PartOutputDescription partOutputDescription )
		{
			PartOutputDescription = partOutputDescription;
			PartOutputIdent = PartOutputDescription.PartOutputIdent;
			DisplayName = PartOutputDescription.DisplayName;
			Parent = parent;
		}

		public GraphableOutput( GraphableWeaponPart parent )
		{
			PartOutputIdent = "Start";
			DisplayName = "On Start";
			Parent = parent;
		}

		public GraphableWeaponPart Parent { get; init; }
		public string PartOutputIdent { get; init; }
		public string DisplayName { get; init; }

		private GraphableInput _target;

		public GraphableInput Target
		{
			get => _target;
			set
			{
				if ( value == null )
				{
					if ( _target?.ConnectedTo is { } )
					{
						_target.SetRawConnectedTo( null );
						_target = null;
					}
				}
				else
				{
					_target = value;
					_target.SetRawConnectedTo( this );
				}

				Element?.StateHasChanged();
			}
		}

		public NodeOutput Element { get; set; }
		public PartOutputDescription PartOutputDescription { get; set; }
	}

	public GraphableWeaponPart( WeaponPart weaponPart )
	{
		WeaponPart = weaponPart;

		var description = WeaponPart.Description;
		DisplayName = description.DisplayName;
		EnergyUsage = description.EnergyUsage;
		OutputOnly = description.OutputOnly;
		Input = new GraphableInput( this );

		foreach ( var partOutputDescription in description.Outputs )
		{
			Outputs.Add( new GraphableOutput( this, partOutputDescription ) );
		}
	}

	public GraphableWeaponPart()
	{
		DisplayName = "Start";
		EnergyUsage = 0;
		OutputOnly = true;
		Input = new GraphableInput( this );

		var output = new GraphableOutput( this );
		Outputs.Add( output );
	}

	public List<GraphableOutput> Outputs { get; } = new();
	public GraphableInput Input { get; set; }

	public bool OutputOnly { get; }
	public string DisplayName { get; }
	public float EnergyUsage { get; }

	private static int _partlessSavedX = 0;
	private static int _partlessSavedY = 0;

	public int SavedX
	{
		get => WeaponPart?.ClientSavedEditorX ?? _partlessSavedX;
		set
		{
			if ( WeaponPart == null )
			{
				_partlessSavedX = value;
			}
			else
			{
				WeaponPart.ClientSavedEditorX = value;
			}
		}
	}

	public int SavedY
	{
		get => WeaponPart?.ClientSavedEditorY ?? _partlessSavedY;
		set
		{
			if ( WeaponPart == null )
			{
				_partlessSavedY = value;
			}
			else
			{
				WeaponPart.ClientSavedEditorY = value;
			}
		}
	}

	public Node Element { get; set; }
	public WeaponPart WeaponPart { get; set; }
}
