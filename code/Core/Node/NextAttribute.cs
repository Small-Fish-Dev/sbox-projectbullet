using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add connector / "next node" to WeaponNodeEntity class 
/// </summary>
[System.AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class NextAttribute : System.Attribute
{
	public string DisplayName { get; init; }
	public string Identifier { get; init; }

	public NextAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
