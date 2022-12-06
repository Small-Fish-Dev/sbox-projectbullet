using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Mark WeaponNodeEntity as energy user 
/// </summary>
[System.AttributeUsage( AttributeTargets.Class )]
public class EnergyAttribute : System.Attribute
{
	public float Energy { get; }
	public bool Estimated { get; }

	public EnergyAttribute( float energy )
	{
		Energy = energy;
	}
}
