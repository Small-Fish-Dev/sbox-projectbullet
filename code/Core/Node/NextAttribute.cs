using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add connector / "next node" to WeaponNodeEntity class 
/// </summary>
[System.AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ConnectorAttribute : System.Attribute
{
	public string DisplayName { get; init; }
	public string Identifier { get; init; }

	public ConnectorAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
