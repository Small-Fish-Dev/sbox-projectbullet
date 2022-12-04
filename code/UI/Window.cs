using System.Diagnostics.Tracing;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI.Shop;

public partial class Window : Panel
{
	public bool ForceMouseInput { get; set; } = true;
	private string PointerEventsValue => ForceMouseInput ? "all" : "none";
	public int Width { get; set; }
	public int Height { get; set; }
	public Panel Container { get; set; }
	public Panel TitleBar { get; set; }
	public string Title { get; set; } = "Window";
	
	private bool _init;
	private bool _held = false;
	private Vector2 _holdPoint;
	
	private readonly Panel _contents;
	public Window(Panel contents) => _contents = contents;

	[Event.Hotload]
	private void OnHotload() => _init = false;

	public override void Tick()
	{
		base.Tick();

		if ( _init == false && Container != null )
		{
			if ( Container.Children.All( v => v != Container ) )
			{
				Container.AddChild( _contents );
				Log.Info( "init complete" );
			}
			_init = true;
		}
	}
	
	protected override void OnMouseDown(MousePanelEvent e)
    {
        base.OnMouseDown(e);

        if (e.Button == "mouseleft" && !_held)
        {
	        if ( e.Target.AncestorsAndSelf.Any( v => v == _contents ) )
	        {
		        return;
	        }

	        _held = true;
	        _holdPoint = Local.Hud.MousePosition - Box.Rect.TopLeft;
        }
    }

    protected override void OnMouseUp(MousePanelEvent e)
    {
        base.OnMouseUp(e);

        _held = false;
        _holdPoint = Vector2.Zero;
    }

    protected override void OnMouseMove(MousePanelEvent e)
    {
        base.OnMouseMove(e);

        if ( !_held )
        {
	        return;
        }

        var panel = this;
        panel.Style.Position = PositionMode.Absolute;
        panel.Style.Left = Length.Pixels(
	        ((Local.Hud.MousePosition.x - _holdPoint.x) * panel.ScaleFromScreen)
        );
        panel.Style.Top = Length.Pixels(
	        ((Local.Hud.MousePosition.y - _holdPoint.y) * panel.ScaleFromScreen)
        );
    }
}
