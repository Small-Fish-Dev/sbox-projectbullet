using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Mark WeaponNode as a registered node
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class NodeAttribute : Attribute
{
	/// <summary>
	/// Display name for this node
	/// </summary>
	public string DisplayName { get; init; }
	
	/// <summary>
	/// Usage info / display description for this connector
	/// </summary>
	public string UsageInfo { get; init; }
}
