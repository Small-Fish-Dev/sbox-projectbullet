using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Mark WeaponNode as a registered node
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
public sealed class NodeAttribute : Attribute
{
	public string DisplayName { get; init; }
	public string UsageInfo { get; init; }
}
