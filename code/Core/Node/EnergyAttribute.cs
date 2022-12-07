using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Mark WeaponNodeEntity as energy user 
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class EnergyAttribute : System.Attribute
{
	public float Energy { get; }
	public bool Estimated { get; init; }

	public EnergyAttribute( float energy )
	{
		Energy = energy;
	}
}
