using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add connector / "next node" to WeaponNode class 
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ConnectorAttribute : Attribute
{
	public string DisplayName { get; init; }
	public int Order { get; init; }
	public float EnergyOutPercentage { get; init; } = 0.0f;
	public float EnergyOutAmount { get; set; } = 0.0f;
	public string Identifier { get; }

	public ConnectorAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
