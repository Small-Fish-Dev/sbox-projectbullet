using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add connector / "next node" to WeaponNodeEntity class 
/// </summary>
[System.AttributeUsage( AttributeTargets.Class )]
public class NextAttribute : System.Attribute
{
	public string DisplayName { get; }
	public string Identifier { get; }

	public NextAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
