using System;

namespace ProjectBullet.Core.Node;

/// <summary>
/// Add value to WeaponNode class 
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
public sealed class ValueAttribute : Attribute
{
	public string Identifier { get; init; }

	public bool IsPercentage => Percentage != 0;
	public bool IsAbsolute => Absolute != 0;
	public float Percentage { get; init; } = 0.0f;
	public float Absolute { get; init; } = 0.0f;

	public float GetValue( float? dimension ) =>
		IsPercentage switch
		{
			true when dimension == null => throw new Exception(
				"GetValue used on percentage value but dimension not provided" ),
			true => dimension.Value * Percentage,
			_ => IsAbsolute ? Absolute : 0.0f
		};

	public ValueAttribute( string identifier )
	{
		Identifier = identifier;
	}
}
