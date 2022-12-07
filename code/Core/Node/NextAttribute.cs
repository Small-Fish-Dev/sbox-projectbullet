using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add connector / "next node" to WeaponNodeEntity class 
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ConnectorAttribute : Attribute
{
	public string DisplayName { get; init; }
	public string Identifier { get; }

	public ConnectorAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
