using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphVisualizer : Panel
{
	private object _mouseDownTarget;
	private Vector2 _holdPoint;
	private bool _makingInvalidConnection;

	private const float LineStartSize = 13.0f;
	private const float LineEndSize = 13.0f;
	private static bool CheckHover( Panel target ) => target.Box.Rect.IsInside( Game.RootPanel.MousePosition );

	private void DrawNodeLine( Vector2 start, Vector2 end ) => GraphicsX.Line( Color.White,
		ScaleToScreen * LineStartSize, start, ScaleToScreen * LineEndSize, end );

	private Vector2 GetStyleMousePosition() => new Vector2(
		(Game.RootPanel.MousePosition.x - _holdPoint.x - Box.Rect.Left) * Game.RootPanel.ScaleFromScreen,
		(Game.RootPanel.MousePosition.y - _holdPoint.y - Box.Rect.Top) * Game.RootPanel.ScaleFromScreen );

	private void DrawPlaceholderNodeLine( Vector2 start, Vector2 end )
	{
		GraphicsX.Line( _makingInvalidConnection ? Color.Red : Color.White, ScaleToScreen * LineStartSize, start,
			ScaleToScreen * LineEndSize, end );
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
			case GraphNodeOut { Connected: false } output:
				_mouseDownTarget = output;
				output.MakingLink = true;
				return;
			case GraphNodeIn { Connected: false }:
				return;
		}

		var node = e.Target.AncestorsAndSelf.SingleOrDefault( v => v is GraphNode );
		if ( node == null || _mouseDownTarget != null )
		{
			return;
		}

		_mouseDownTarget = (GraphNode)node;
		_holdPoint = Game.RootPanel.MousePosition - node.Box.Rect.TopLeft;
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		if ( _mouseDownTarget is GraphNodeOut output )
		{
			foreach ( var child in e.This.Descendants )
			{
				if ( child is not GraphNodeIn input )
				{
					continue;
				}

				if ( input.Hovered && !input.IsInvalidHover )
				{
					output.Connector.Connect( input.GraphNode.NodeData );
				}

				input.Hovered = false;
				input.IsInvalidHover = false;
			}

			output.MakingLink = false;
		}

		_makingInvalidConnection = false;
		_mouseDownTarget = null;
		_holdPoint = Vector2.Zero;
	}

	protected override void OnMouseMove( MousePanelEvent e )
	{
		base.OnMouseMove( e );

		if ( _mouseDownTarget is GraphNode node )
		{
			node.Style.Position = PositionMode.Absolute;

			var lmp = GetStyleMousePosition();

			node.NodeData.EditorData.LastX = (int)lmp.x;
			node.NodeData.EditorData.LastY = (int)lmp.y;

			if ( lmp.x < 0 )
			{
				lmp.x = 0;
			}

			if ( lmp.y < 0 )
			{
				lmp.y = 0;
			}

			node.Style.Left = Length.Pixels(
				lmp.x
			);
			node.Style.Top = Length.Pixels(
				lmp.y
			);

			return;
		}

		foreach ( var child in e.This.Descendants )
		{
			if ( child is not GraphNodeIn input )
			{
				continue;
			}

			if ( CheckHover( child ) )
			{
				input.Hovered = true;

				child.StateHasChanged();
			}
			else
			{
				if ( !input.Hovered )
				{
					continue;
				}

				input.Hovered = false;
				input.IsInvalidHover = false;
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

				if ( !output.Connected )
				{
					continue;
				}
				
				var endpoint = output.Connector.ConnectedNode.EditorData.InputElement;
				DrawNodeLine( output.Box.ClipRect.Center, endpoint.Box.Rect.Center );
			}
		}

		{
			if ( _mouseDownTarget is not GraphNodeOut output )
			{
				return;
			}

			DrawPlaceholderNodeLine( output.Box.Rect.Center, MousePosition + this.Box.Rect.TopLeft );
		}
	}
}
