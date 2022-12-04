using System;
using Sandbox;

namespace ProjectBullet.Weapons;

[AttributeUsage( AttributeTargets.Class )]
public class KnownWeaponAttribute : Attribute
{
	public string DisplayName;
	public string DisplayDescription;
}

public class WeaponDescription
{
	private readonly KnownWeaponAttribute _attribute;
	public TypeDescription TypeDescription { get; }

	public WeaponDescription( TypeDescription typeDescription, KnownWeaponAttribute attribute )
	{
		TypeDescription = typeDescription;
		_attribute = attribute;
	}

	/// <summary>
	/// Value to identify the <see cref="WeaponDescription"/>
	/// </summary>
	public string DescriptionIdent => GetType().Name;
	
	/// <summary>
	/// Value to uniquely identify the Weapon type
	/// </summary>
	public string WeaponIdent => TypeDescription.TargetType.FullName;
	
	public string DisplayName => _attribute == null ? WeaponIdent : _attribute.DisplayName;
}
