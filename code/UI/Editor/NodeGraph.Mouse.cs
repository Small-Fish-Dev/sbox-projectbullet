using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Items;
using ProjectBullet.Weapons;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class NodeGraph : Panel
{
	private ContextMenu _contextMenu;

	private object _mouseDownTarget;
	private Vector2 _holdPoint;
	private bool _makingInvalidConnection;

	private const float LineStartSize = 23.0f;
	private const float LineEndSize = 23.0f;
	private static bool CheckHover( Panel target ) => target.Box.Rect.IsInside( Local.Hud.MousePosition );

	private void DrawNodeLine( Vector2 start, Vector2 end ) => GraphicsX.Line( Color.White,
		ScaleToScreen * LineStartSize, start, ScaleToScreen * LineEndSize, end );

	private Vector2 GetStyleMousePosition() => new Vector2(
		(Local.Hud.MousePosition.x - _holdPoint.x - Box.Rect.Left) * Local.Hud.ScaleFromScreen,
		(Local.Hud.MousePosition.y - _holdPoint.y - Box.Rect.Top) * Local.Hud.ScaleFromScreen );

	private void DrawPlaceholderNodeLine( Vector2 start, Vector2 end )
	{
		GraphicsX.Line( _makingInvalidConnection ? Color.Red : Color.White, ScaleToScreen * LineStartSize, start,
			ScaleToScreen * LineEndSize, end );
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		if ( e.Target is NodeGraph && _contextMenu == null )
		{
			_contextMenu = new ContextMenu(this);
			AddChild( _contextMenu );
			
			var panel = (_contextMenu as Panel);
			var lmp = GetStyleMousePosition();
			
			panel.Style.Left = Length.Pixels(
				lmp.x
			);
			panel.Style.Top = Length.Pixels(
				lmp.y
			);
		}
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		if ( _contextMenu != null )
		{
			var ctx = e.Target.AncestorsAndSelf.SingleOrDefault( v => v is ContextMenu );
			if ( ctx == null )
			{
				_contextMenu.Delete();
				_contextMenu = null;
			}
		}
		
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
			case NodeOutput { HasTarget: false } output:
				_mouseDownTarget = output;
				output.MakingLink = true;
				return;
			case NodeInput:
				return;
		}

		var node = e.Target.AncestorsAndSelf.SingleOrDefault( v => v is Node );
		if ( node == null || _mouseDownTarget != null )
		{
			return;
		}

		_mouseDownTarget = (Node)node;
		_holdPoint = Local.Hud.MousePosition - node.Box.Rect.TopLeft;
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		if ( _mouseDownTarget is NodeOutput output )
		{
			foreach ( var child in e.This.Descendants )
			{
				if ( child is not NodeInput input )
				{
					continue;
				}

				if ( input.BeingHovered && !input.BeingIncorrectlyHovered )
				{
					output.GraphableOutput.Target = input.GraphableInput;
				}

				input.BeingHovered = false;
				input.BeingIncorrectlyHovered = false;
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

		if ( _mouseDownTarget is Node node )
		{
			node.Style.Position = PositionMode.Absolute;

			var lmp = GetStyleMousePosition();

			node.GraphableWeaponPart.SavedX = (int)lmp.x;
			node.GraphableWeaponPart.SavedY = (int)lmp.y;

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
			if ( child is not NodeInput input )
			{
				continue;
			}

			if ( CheckHover( child ) )
			{
				input.BeingHovered = true;

				child.StateHasChanged();
			}
			else
			{
				if ( !input.BeingHovered )
				{
					continue;
				}

				input.BeingHovered = false;
				input.BeingIncorrectlyHovered = false;
				child.StateHasChanged();
			}
		}
	}

	public override void DrawBackground( ref RenderState state )
	{
		foreach ( var child in (this as Panel).Descendants )
		{
			{
				if ( child is not NodeOutput output )
				{
					continue;
				}

				if ( !output.HasTarget )
				{
					continue;
				}

				DrawNodeLine( output.Box.ClipRect.Center, output.GraphableOutput.Target.Element.Box.Rect.Center );
			}
		}

		{
			if ( _mouseDownTarget is not NodeOutput output )
			{
				return;
			}

			DrawPlaceholderNodeLine( output.Box.Rect.Center, MousePosition + this.Box.Rect.TopLeft );
		}
	}
}
