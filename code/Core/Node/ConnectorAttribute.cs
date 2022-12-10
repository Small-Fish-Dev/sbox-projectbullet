using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add connector / "next node" to WeaponNodeEntity class 
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ConnectorAttribute : Attribute
{
	public string DisplayName { get; init; }
	public int Order { get; init; }
	public float EnergyOutPercentage { get; init; } = 1.0f;
	public float EnergyOutAmount { get; set; } = 0.0f;
	public string Identifier { get; }

	public float GetEnergyAmount( float dimension )
	{
		if ( EnergyOutAmount != 0.0f )
		{
			return dimension;
		}

		if ( EnergyOutPercentage != 0.0f )
		{
			return dimension * EnergyOutPercentage;
		}

		return dimension;
	}

	public ConnectorAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
