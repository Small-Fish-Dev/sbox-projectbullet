using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI.WorkshopElements;

public partial class GraphVisualizer : Panel
{
	private object _mouseDownTarget;
	private Vector2 _holdPoint;
	private bool _makingInvalidConnection;

	private static bool CheckHover( Panel target ) => target.Box.Rect.IsInside( Game.RootPanel.MousePosition );

	private void DrawNodeLine( Color color, Vector2 start, Vector2 end )
	{
		GraphicsX.Line( Color.White,
			ScaleToScreen * 6, start, ScaleToScreen * 6, end );
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		if ( _mouseDownTarget != null )
		{
			return;
		}

		if ( ContextMenu != null )
		{
			var target = e.Target.AncestorsAndSelf.SingleOrDefault( v => v is ContextMenu );
			if ( target == null )
			{
				ContextMenu.Delete();
				ContextMenu = null;
			}
		}

		if ( e.Button != "mouseleft" )
		{
			return;
		}

		switch ( e.Target )
		{
			case GraphNodeOut { IsConnected: false } output:
				_mouseDownTarget = output;
				Style.Cursor = "none";
				return;
			case GraphNodeIn { IsConnected: false }:
				Style.Cursor = "none";
				return;
		}

		var draggable = e.Target.AncestorsAndSelf.SingleOrDefault( v => v is Draggable );
		if ( draggable == null || _mouseDownTarget != null )
		{
			return;
		}

		Style.Cursor = "none";
		_mouseDownTarget = (Draggable)draggable;
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

				if ( input.IsHovered )
				{
					if ( input.IsHoveredIncorrectly )
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

				if ( input.IsConnected && _mouseDownTarget != null )
				{
					input.IsHoveredIncorrectly = true;
				}

				child.StateHasChanged();
			}
			else
			{
				if ( !input.IsHovered )
				{
					continue;
				}

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
