using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Mark WeaponNodeEntity as energy user 
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class EnergyAttribute : Attribute
{
	public float Energy { get; }

	public EnergyAttribute( float energy )
	{
		Energy = energy;
	}
}
