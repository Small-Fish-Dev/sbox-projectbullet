using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.Utility;

namespace ProjectBullet.UI.WorkshopElements;

public partial class GraphVisualizer : Panel
{
	private object _mouseDownTarget;
	private Vector2 _holdPoint;
	private Vector2 _holdStartPos;

	/// <summary>
	/// Check if the target panel is hovered regardless of panel state
	/// </summary>
	/// <param name="target">Target panel</param>
	/// <returns>Whether or not the mouse is over the panel</returns>
	private static bool CheckHover( Panel target ) => target.Box.Rect.IsInside( Game.RootPanel.MousePosition );

	/// <summary>
	/// Draw a node link / line from one point to another
	/// </summary>
	/// <param name="color">Line color</param>
	/// <param name="start">Start of line</param>
	/// <param name="end">End of line</param>
	private static void DrawNodeLine( Color color, Vector2 start, Vector2 end )
	{
		GraphicsX.MeshStart();

		const float step = 0.04f;
		var delta = end - start;
		var last = start;
		var thickness = Game.RootPanel.ScaleToScreen * 3;
		for ( float i = 0; i < 1.0f; i += step )
		{
			var pos = new Vector2(
				start.x + delta.x * i,
				start.y + delta.y * Easing.QuadraticInOut( i )
			);

			GraphicsX.Line( color, thickness, last, thickness, pos );
			last = pos;
		}

		GraphicsX.Line( color, thickness, last, thickness, end );

		GraphicsX.MeshEnd();
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		if ( _mouseDownTarget != null )
		{
			return;
		}

		if ( e.Button != "mouseleft" )
		{
			return;
		}

		switch ( e.Target )
		{
			case GraphNodeOut { IsConnected: false } output:
				// start linking to this GraphNodeOut!
				_mouseDownTarget = output;
				output.IsLinking = true;
				return;
			case GraphNodeIn { IsConnected: false }:
				return;
		}

		var draggable = (Draggable)e.Target.AncestorsAndSelf.SingleOrDefault( v => v is Draggable );
		if ( draggable == null || _mouseDownTarget != null )
		{
			return;
		}

		if ( !draggable.ShouldDrag( e ) )
		{
			return;
		}

		// start dragging this Draggable!
		Style.Cursor = "none";
		_mouseDownTarget = draggable;
		_holdStartPos = draggable.Box.Rect.TopLeft;
		_holdPoint = Util.Workshop.MousePosition - draggable.Box.Rect.TopLeft;
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		Style.Cursor = "inherit";

		switch ( _mouseDownTarget )
		{
			case GraphNode node when node.Data == Controller.Entry:
				// Set the location of the Entry Node (which should be this node)
				Controller.PerformAction(
					new GraphController.SetNodeLocationAction( Controller.NodeExecutor, _holdStartPos,
						node.Box.Rect.TopLeft - Box.Rect.TopLeft ),
					true );
				break;
			case GraphNode node:
				// Set the location of the held node
				Controller.PerformAction(
					new GraphController.SetNodeLocationAction( node.Data.Instance, _holdStartPos,
						node.Box.Rect.TopLeft - Box.Rect.TopLeft ),
					true );
				break;
			case GraphNodeOut output:
				foreach ( var child in e.This.Descendants )
				{
					if ( child is not GraphNodeIn input )
					{
						continue;
					}

					// if the input is currently hovered correctly...
					if ( input.IsHovered && !input.IsHoveredIncorrectly )
					{
						// and the input is connected...
						if ( input.IsConnected )
						{
							// disconnect that input first, then...
							input.NodeData.Previous?.Disconnect();
						}

						// connect to the output where we started holding the mouse down
						output.Connector.ConnectTo( input.NodeData );
						input.Node.StateHasChanged();
					}

					input.IsHovered = false;
					input.IsHoveredIncorrectly = false;
				}

				output.IsLinking = false;
				break;
		}

		_mouseDownTarget = null;
		_holdPoint = Vector2.Zero;
	}

	protected override void OnMouseMove( MousePanelEvent e )
	{
		base.OnMouseMove( e );

		if ( _mouseDownTarget is Draggable draggable )
		{
			var mousePosition = Util.Workshop.MousePosition;

			mousePosition -= Box.Rect.TopLeft;

			mousePosition -= _holdPoint;

			draggable.UpdateHold( mousePosition, _holdPoint );
		}

		foreach ( var child in e.This.Descendants )
		{
			if ( child is not GraphNodeIn input )
			{
				continue;
			}

			if ( CheckHover( child ) )
			{
				input.IsHovered = true;

				if ( _mouseDownTarget != null )
				{
					input.IsLinking = true;

					if ( _mouseDownTarget is GraphNodeOut output && input.NodeData == output.Connector.Parent )
					{
						input.IsHoveredIncorrectly = true;
					}
				}

				child.StateHasChanged();
			}
			else
			{
				if ( !input.IsHovered )
				{
					continue;
				}

				input.IsHovered = false;
				input.IsHoveredIncorrectly = false;
				input.IsLinking = false;

				child.StateHasChanged();
			}
		}
	}

	public override void DrawBackground( ref RenderState state )
	{
		// we need to draw all the line links (GraphNodeOut -> GraphNodeIn)
		foreach ( var child in Descendants )
		{
			if ( child is not GraphNodeOut output )
			{
				continue;
			}

			if ( !output.IsConnected )
			{
				continue;
			}

			// so for each connected GraphNodeOut...
			// draw a line from the GraphNodeOut center to the GraphNodeIn center
			var endpoint = output.Connector.ConnectedNode.InputElement;
			DrawNodeLine( Color.White, output.Box.ClipRect.Center, endpoint.Box.Rect.Center );
		}

		{
			if ( _mouseDownTarget is not GraphNodeOut output )
			{
				return;
			}

			// while the mouse is down:
			// draw the line from the output to the mouse location

			DrawNodeLine( Color.White, output.Box.Rect.Center,
				MousePosition + Box.Rect.TopLeft );
		}
	}
}
