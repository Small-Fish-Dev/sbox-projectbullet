using System;
using Sandbox;

namespace ProjectBullet.Items;

[AttributeUsage( AttributeTargets.Property )]
public class KnownOutputAttribute : Attribute
{
	public string DisplayName;
}

/// <summary>
/// Wrapper for a <see cref="PropertyDescription"/> from a <see cref="WeaponPart"/>
/// </summary>
public class PartOutputDescription
{
	private readonly KnownOutputAttribute _attribute;
	public PropertyDescription PropertyDescription { get; }

	public PartOutputDescription( PropertyDescription propertyDescription, KnownOutputAttribute attribute )
	{
		PropertyDescription = propertyDescription;
		_attribute = attribute;
	}

	/// <summary>
	/// Value to uniquely identify the <see cref="PartOutputDescription"/>
	/// </summary>
	public string DescriptionIdent => GetType().Name;

	/// <summary>
	/// Value to uniquely identify the property
	/// </summary>
	public string PartOutputIdent => PropertyDescription.Name;

	public string DisplayName => _attribute == null ? PartOutputIdent : _attribute.DisplayName;
}
