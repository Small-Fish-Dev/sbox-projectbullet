using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sandbox;

namespace ProjectBullet.Items;

[AttributeUsage( AttributeTargets.Class )]
public class KnownWeaponPartAttribute : Attribute
{
	public string DisplayName;
	public string DisplayDescription = "No description provided.";
	public float EnergyUsage;
	public bool OutputOnly;
}

public class WeaponPartDescription
{
	private readonly List<PartOutputDescription> _outputs;
	private readonly KnownWeaponPartAttribute _attribute;

	public TypeDescription TypeDescription { get; }
	public ReadOnlyCollection<PartOutputDescription> Outputs => _outputs.AsReadOnly();

	/// <summary>
	/// Value to uniquely identify the <see cref="WeaponPartDescription"/>
	/// </summary>
	public string DescriptionIdent => GetType().Name;
	
	/// <summary>
	/// Value to uniquely identify the WeaponPart type
	/// </summary>
	public string WeaponPartIdent => TypeDescription.TargetType.FullName;
	
	public string DisplayName => _attribute == null ? WeaponPartIdent : _attribute.DisplayName;
	public string DisplayDescription => _attribute == null ? "No description found." : _attribute.DisplayDescription;
	public float EnergyUsage => _attribute.EnergyUsage;
	public bool OutputOnly => _attribute.OutputOnly;

	public WeaponPartDescription( TypeDescription typeDescription, KnownWeaponPartAttribute attribute )
	{
		TypeDescription = typeDescription;

		_outputs = new List<PartOutputDescription>();

		_attribute = attribute;

		foreach ( var property in TypeDescription.Properties )
		{
			var knownOutputAttribute = property.GetCustomAttribute<KnownOutputAttribute>();
			if ( knownOutputAttribute == null )
			{
				continue;
			}

			_outputs.Add( new PartOutputDescription( property, knownOutputAttribute ) );
		}
	}
}
