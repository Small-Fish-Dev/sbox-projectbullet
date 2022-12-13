using System;
using System.Collections.Generic;
using Sandbox;
using ProjectBullet.Player;

namespace ProjectBullet.Core.Node;

public struct ExecuteInfo
{
	public ExecuteInfo()
	{
		ImpactPoint = default;
		Hitbox = default;
		BoneIndex = 0;
		Victim = null;
		Force = default;
		Attacker = null;
		Tags = null;
	}

	/// <summary>
	/// The position of impact
	/// </summary>
	public Vector3 ImpactPoint { get; set; }

	/// <summary>The hitbox (if any) that was hit</summary>
	public Hitbox? Hitbox { get; set; }

	/// <summary>The bone index that the hitbox was attached to</summary>
	public int? BoneIndex { get; set; }

	/// <summary>Multiplier for damage</summary>
	public float DamageMultiplier { get; set; } = 1.0f;

	/// <summary>
	/// The player or NPC or exploding barrel (etc)1 that is being attacked
	/// </summary>
	public Entity Victim { get; set; }

	/// <summary>
	/// The force of the damage - for moving physics etc. This would be the trajectory
	/// of the bullet multiplied by the speed and mass.
	/// </summary>
	public Vector3? Force { get; set; }

	/// <summary>
	/// The BasePlayer that is attacking
	/// </summary>
	public BasePlayer Attacker { get; set; }

	/// <summary>Damage tags, extra information about this attack</summary>
	public HashSet<string> Tags { get; set; }

	public ExecuteInfo WithTag( string tag )
	{
		Tags ??= new HashSet<string>( StringComparer.OrdinalIgnoreCase );
		Tags.Add( tag );
		return this;
	}

	/// <summary>
	/// Set the damage
	/// </summary>
	/// <param name="damageMult">New damage multiplier</param>
	/// <returns>Self</returns>
	public ExecuteInfo WithDamageMultiplier( float damageMult )
	{
		DamageMultiplier = damageMult;
		return this;
	}

	/// <summary>
	/// Set the attacker
	/// </summary>
	/// <param name="attacker">New attacker</param>
	/// <returns>Self</returns>
	public ExecuteInfo WithAttacker( BasePlayer attacker )
	{
		Attacker = attacker;
		return this;
	}

	/// <summary>
	/// Fills in Victim, BoneIndex & Hitbox
	/// </summary>
	public ExecuteInfo UsingTraceResult( TraceResult result )
	{
		Victim = result.Entity;
		BoneIndex = result.Bone;
		Hitbox = result.Hitbox;
		ImpactPoint = result.EndPosition;
		return this;
	}

	public DamageInfo ToDamageInfo( float damage )
	{
		var result = new DamageInfo
		{
			Attacker = Attacker, Position = ImpactPoint, Damage = damage * DamageMultiplier, Tags = Tags
		};

		if ( Hitbox != null )
		{
			result.Hitbox = Hitbox.Value;
		}

		if ( Force != null )
		{
			result.Force = Force.Value;
		}

		if ( BoneIndex != null )
		{
			result.BoneIndex = BoneIndex.Value;
		}

		return result;
	}
}
