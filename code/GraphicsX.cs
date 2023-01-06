using System;
using System.Collections.Generic;
using Sandbox;

namespace ProjectBullet;

/// <summary>
/// Graphics Utils - made by xezno (https://github.com/xezno)
/// https://gist.github.com/xezno/c890c479e32fb314f0c0cd7739afd6ea
/// </summary>
public static class GraphicsX
{
	private static List<Vertex> VertexList { get; } = new();

	private static void AddVertex( in Vertex position ) => VertexList.Add( position );

	public static void AddVertex( in Vector2 position, in Color color )
	{
		var vertex = new Vertex { Position = position, Color = color };

		AddVertex( in vertex );
	}

	private static void AddVertex( in Vector2 position, in Color color, in Vector2 uv )
	{
		var vertex = new Vertex { Position = position, Color = color, TexCoord0 = uv };

		AddVertex( in vertex );
	}

	public static void MeshStart()
	{
		VertexList.Clear();
	}

	public static void MeshEnd( RenderAttributes attr = null )
	{
		var attributes = attr ?? new RenderAttributes();

		attributes.Set( "Texture", Texture.White );

		Graphics.Draw( VertexList.ToArray(), VertexList.Count, Material.UI.Basic, attributes );
		VertexList.Clear();
	}

	public static void Circle( in Vector2 center, in Color color, float radius, int points = 32 ) =>
		Circle( in center, radius, 0f, in color, points );

	public static void Ring( in Vector2 center, float radius, in Color color, float holeRadius, int points = 32 ) =>
		Circle( in center, radius, holeRadius, in color, points );

	public static void Circle( in Vector2 center, float outerRadius, float innerRadius, in Color color,
		int pointCount = 32, float startAngle = 0f, float endAngle = 360f, float uv = 0f )
	{
		MeshStart();

		const float twoPi = MathF.PI * 2f;
		startAngle = startAngle.NormalizeDegrees().DegreeToRadian();

		for ( endAngle = endAngle.NormalizeDegrees().DegreeToRadian(); endAngle <= startAngle; endAngle += twoPi ) ;

		if ( endAngle <= startAngle )
			return;

		var angleStep = twoPi / pointCount;

		for ( var currentAngle = startAngle; currentAngle < endAngle + angleStep; currentAngle += angleStep )
		{
			var startRadians = currentAngle;
			var endRadians = currentAngle + angleStep;

			if ( endRadians > endAngle )
			{
				endRadians = endAngle;
			}

			startRadians += MathF.PI;
			endRadians += MathF.PI;

			var startVector = new Vector2( MathF.Sin( -startRadians ), MathF.Cos( -startRadians ) );
			var endVector = new Vector2( MathF.Sin( -endRadians ), MathF.Cos( -endRadians ) );
			var startUv = startVector / 2f + 0.5f;
			var endUv = endVector / 2f + 0.5f;
			var innerStartUv = startVector * innerRadius / outerRadius / 2f + 0.5f;
			var innerEndUv = endVector * innerRadius / outerRadius / 2f + 0.5f;

			if ( uv > 0f )
			{
				startUv = new Vector2( (startRadians - MathF.PI - startAngle) * uv / twoPi, 0f );
				endUv = new Vector2( (endRadians - MathF.PI - startAngle) * uv / twoPi, 0f );
				innerStartUv = startUv.WithY( 1f );
				innerEndUv = endUv.WithY( 1f );
			}

			var v = center + startVector * outerRadius;
			AddVertex( in v, in color, in startUv );

			v = center + endVector * outerRadius;
			AddVertex( in v, in color, in endUv );

			v = center + startVector * innerRadius;
			AddVertex( in v, in color, in innerStartUv );

			if ( !(innerRadius > 0f) )
			{
				continue;
			}

			v = center + endVector * outerRadius;
			AddVertex( in v, in color, in endUv );

			v = center + endVector * innerRadius;
			AddVertex( in v, in color, in innerEndUv );

			v = center + startVector * innerRadius;
			AddVertex( in v, in color, in innerStartUv );
		}

		MeshEnd();
	}

	public static void Line( in Color color, in float startThickness, in Vector2 startPosition, in float endThickness,
		in Vector2 endPosition, in bool handleMesh = true )
	{
		if ( handleMesh )
		{
			MeshStart();
		}

		var directionVector = endPosition - startPosition;
		var perpendicularVector = directionVector.Perpendicular.Normal * -0.5f;

		var startCorner = startPosition + perpendicularVector * startThickness;
		var endCorner = startPosition + directionVector + perpendicularVector * endThickness;
		var endCorner2 = startPosition + directionVector - perpendicularVector * endThickness;
		var startCorner2 = startPosition - perpendicularVector * startThickness;

		var uv = new Vector2( 0f, 0f );
		AddVertex( in startCorner, in color, in uv );

		uv = new Vector2( 1f, 0f );
		AddVertex( in endCorner, in color, in uv );

		uv = new Vector2( 0f, 1f );
		AddVertex( in startCorner2, in color, in uv );

		uv = new Vector2( 1f, 0f );
		AddVertex( in endCorner, in color, in uv );

		uv = new Vector2( 1f, 1f );
		AddVertex( in endCorner2, in color, in uv );

		uv = new Vector2( 0f, 1f );
		AddVertex( in startCorner2, in color, in uv );

		if ( handleMesh )
		{
			MeshEnd();
		}
	}

	public static void Line( in Color color, in float thickness, in Vector2 startPosition, in Vector2 endPosition ) =>
		Line( in color, in thickness, in startPosition, in thickness, in endPosition );

	public static void CornerBorder( in Color color, in Rect rect, in float thickness, in float size )
	{
		// Draw top left border
		Line( color, thickness, rect.TopLeft, new Vector2( rect.Left, rect.Top + size ) );
		Line( color, thickness, rect.TopLeft, new Vector2( rect.Left + size, rect.Top ) );

		// Draw bottom left border
		Line( color, thickness, rect.BottomLeft, new Vector2( rect.Left, rect.Bottom - size ) );
		Line( color, thickness, rect.BottomLeft, new Vector2( rect.Left + size, rect.Bottom ) );

		// Draw top right border
		Line( color, thickness, rect.TopRight, new Vector2( rect.Right, rect.Top + size ) );
		Line( color, thickness, rect.TopRight, new Vector2( rect.Right - size, rect.Top ) );

		// Draw bottom right border
		Line( color, thickness, rect.BottomRight, new Vector2( rect.Right, rect.Bottom - size ) );
		Line( color, thickness, rect.BottomRight, new Vector2( rect.Right - size, rect.Bottom ) );
	}
}
