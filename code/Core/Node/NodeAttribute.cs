﻿using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Mark WeaponNodeEntity as a registered node
/// </summary>
[System.AttributeUsage( AttributeTargets.Class )]
public class NodeAttribute : System.Attribute
{
	public string DisplayName { get; }
	public string Description { get; }
}
