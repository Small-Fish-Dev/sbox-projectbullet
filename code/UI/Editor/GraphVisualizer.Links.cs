using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphVisualizer : Panel
{
	private object _mouseDownTarget;
	private Vector2 _holdPoint;
	private bool _makingInvalidConnection;

	private const float LineStartSize = 14.0f;
	private const float LineEndSize = 14.0f;
	private const float OuterLineStartSize = 14.0f;
	private const float OuterLineEndSize = 18.0f;

	private static bool CheckHover( Panel target ) => target.Box.Rect.IsInside( Game.RootPanel.MousePosition );

	private void DrawNodeLine( Color color, Vector2 start, Vector2 end )
	{
		GraphicsX.Line( Color.White,
			ScaleToScreen * OuterLineStartSize, start, ScaleToScreen * OuterLineEndSize, end );

		//GraphicsX.Line( color,
			//ScaleToScreen * LineStartSize, start, ScaleToScreen * LineEndSize, end );
	}

	private Vector2 GetStyleMousePosition() => new Vector2(
		(Game.RootPanel.MousePosition.x - _holdPoint.x - Box.Rect.Left) * Game.RootPanel.ScaleFromScreen,
		(Game.RootPanel.MousePosition.y - _holdPoint.y - Box.Rect.Top) * Game.RootPanel.ScaleFromScreen );


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
				_mouseDownTarget = output;
				output.MakingLink = true;
				return;
			case GraphNodeIn { IsConnected: false }:
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

				if ( input.Hovered )
				{
					if ( input.IsInvalidHover )
					{
						input.NodeData.Previous?.Disconnect(  );
					}
					
					output.Connector.ConnectTo( input.GraphNode.NodeData );
					input.GraphNode.StateHasChanged();
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

			if ( node.NodeData.Instance == null )
			{
				return;
			}

			node.NodeData.Instance.LastEditorX = lmp.x;
			node.NodeData.Instance.LastEditorY = lmp.y;
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

				if ( input.IsConnected && _mouseDownTarget != null )
				{
					input.IsInvalidHover = true;
				}
				
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

	private static Color CalculateOutputColor( GraphNodeOut output )
	{
		var h = ((output.Connector.LastEstimatedEnergyOutput ?? 50.0f) * 0.01f) * 150;
		return new ColorHsv( h, 0.6f, 1 ).ToColor();
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
				DrawNodeLine( CalculateOutputColor( output ), output.Box.ClipRect.Center, endpoint.Box.Rect.Center );
			}
		}

		{
			if ( _mouseDownTarget is not GraphNodeOut output )
			{
				return;
			}

			DrawNodeLine( CalculateOutputColor( output ), output.Box.Rect.Center,
				MousePosition + Box.Rect.TopLeft );
		}
	}
}
