using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.Utility;

namespace ProjectBullet.UI.WorkshopElements;

public partial class GraphVisualizer : Panel
{
	private object _mouseDownTarget;
	private Vector2 _holdPoint;
	private bool _makingInvalidConnection;

	private static bool CheckHover( Panel target ) => target.Box.Rect.IsInside( Game.RootPanel.MousePosition );

	private void DrawNodeLine( Color color, Vector2 start, Vector2 end )
	{
		/*GraphicsX.Line( Color.Yellow,
			ScaleToScreen * 3, start, ScaleToScreen * 3, end );*/

		GraphicsX.MeshStart();

		const float step = 0.04f;
		var delta = end - start;
		var last = start;
		var thickness = ScaleToScreen * 3;
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

		if ( ContextMenu != null )
		{
			var ctx = (ContextMenu)e.Target.AncestorsAndSelf.SingleOrDefault( v => v is ContextMenu );
			if ( ctx == null || _mouseDownTarget != null )
			{
				ContextMenu.Delete();
				ContextMenu = null;
			}
		}

		switch ( e.Target )
		{
			case GraphNodeOut { IsConnected: false } output:
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

		Style.Cursor = "none";
		_mouseDownTarget = draggable;
		_holdPoint = Util.Workshop.MousePosition - draggable.Box.Rect.TopLeft;
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		Style.Cursor = "inherit";

		if ( _mouseDownTarget is GraphNodeOut output )
		{
			foreach ( var child in e.This.Descendants )
			{
				if ( child is not GraphNodeIn input )
				{
					continue;
				}

				if ( input.IsHovered && !input.IsHoveredIncorrectly )
				{
					if ( input.IsConnected )
					{
						input.NodeData.Previous?.Disconnect();
					}

					output.Connector.ConnectTo( input.NodeData );
					input.Node.StateHasChanged();
				}

				input.IsHovered = false;
				input.IsHoveredIncorrectly = false;
			}

			output.IsLinking = false;
		}

		_makingInvalidConnection = false;
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

					/*if ( input.IsConnected )
					{
						input.IsHoveredIncorrectly = true;
					}*/

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
		foreach ( var child in Descendants )
		{
			{
				if ( child is not GraphNodeOut output )
				{
					continue;
				}

				if ( !output.IsConnected )
				{
					continue;
				}

				var endpoint = output.Connector.ConnectedNode.InputElement;
				DrawNodeLine( Color.White, output.Box.ClipRect.Center, endpoint.Box.Rect.Center );
			}
		}

		{
			if ( _mouseDownTarget is not GraphNodeOut output )
			{
				return;
			}

			DrawNodeLine( Color.White, output.Box.Rect.Center,
				MousePosition + Box.Rect.TopLeft );
		}
	}
}
