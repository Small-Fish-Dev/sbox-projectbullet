﻿using Sandbox;
using ProjectBullet.Core.Node;

namespace ProjectBullet.Classes;

public class PrimaryFireController : NodeExecutionEntity
{
	public override string DisplayName => "Primary Fire";
	public override float ActionDelay => 4.0f;
	public override InputButton InputButton => InputButton.PrimaryAttack;

	protected override void PerformAction( IClient cl )
	{
		base.PerformAction( cl );

		Log.Info( "hello from primary fire" );

		var ray = BasePlayer.AimRay;

		var trace = Trace.Ray( ray, 8096 )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "npc", "glass" )
			.Ignore( BasePlayer );

		var result = trace.Run();

		Log.Info( result.EndPosition );

		DebugOverlay.Circle( result.EndPosition, Rotation.Identity, 20.0f, Color.Cyan, 3.0f );

		ExecuteEntryNode( result.Entity, result.EndPosition );
	}
}

public class SecondaryFireController : NodeExecutionEntity
{
	public override string DisplayName => "Secondary Fire";
	public override float ActionDelay => 4.0f;
	public override InputButton InputButton => InputButton.SecondaryAttack;

	protected override void PerformAction( IClient cl )
	{
		base.PerformAction( cl );

		Log.Info( "hello from secondary fire" );
	}
}

public class UltraShiftController : NodeExecutionEntity
{
	public override string DisplayName => "Ultra Shift";
	public override float ActionDelay => 7.0f;
	public override InputButton InputButton => InputButton.Run;

	protected override void PerformAction( IClient cl )
	{
		base.PerformAction( cl );

		Log.Info( "hello from ultra shift" );
	}
}

public partial class Gunner : BasePlayer
{
	[Net] public PrimaryFireController PrimaryFire { get; set; }
	[Net] public SecondaryFireController SecondaryFire { get; set; }
	[Net] public UltraShiftController UltraShift { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PrimaryFire = new PrimaryFireController { Owner = this };
		SecondaryFire = new SecondaryFireController { Owner = this };
		UltraShift = new UltraShiftController { Owner = this };

		NodeExecutors.Add( PrimaryFire );
		NodeExecutors.Add( SecondaryFire );
		NodeExecutors.Add( UltraShift );
	}
}
