using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add connector / "next node" to WeaponNode class 
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ConnectorAttribute : Attribute
{
	/// <summary>
	/// Display name for this connector
	/// </summary>
	public string DisplayName { get; init; }

	public int Order { get; init; }

	/// <summary>
	/// Energy output as a percentage (0-1)
	/// </summary>
	public float EnergyOutPercentage { get; init; } = 0.0f;

	/// <summary>
	/// Energy output as an absolute value - this will take precedence over the percentage
	/// </summary>
	public float EnergyOutAbsolute { get; init; } = 0.0f;

	public string Identifier { get; }

	public ConnectorAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
