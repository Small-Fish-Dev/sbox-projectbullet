﻿using ProjectBullet.Core.Node;
using Sandbox;

namespace ProjectBullet.Nodes;

[Connector( "on_start", Order = 0, EnergyOutPercentage = 1.0f )]
[Node( DisplayName = "Start", Description = "The start of your weapon code" )]
public class PlaceholderNode : WeaponNodeEntity
{
	public override float Execute( float energy, Entity target, Vector3 point )
	{
		Log.Warning( "PlaceholderNode has been executed. This shouldn't be done." );
		ExecuteConnector( "on_start", target, point );
		return energy;
	}
}