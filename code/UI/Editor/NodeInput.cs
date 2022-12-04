using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class NodeInput : Panel
{
	public GraphableWeaponPart.GraphableInput GraphableInput { get; set; }

	public bool BeingHovered { get; set; } = false;
	public bool BeingIncorrectlyHovered { get; set; } = false;
	public bool HasConnection => GraphableInput.ConnectedTo != null;
	public string RootClasses => $"{(HasConnection ? "has-connection" : "")} {(BeingHovered ? "being-hovered" : "")} {(BeingIncorrectlyHovered ? "being-incorrectly-hovered" : "")}";
	
	protected override void OnRightClick( MousePanelEvent e )
	{
		GraphableInput.ConnectedTo = null;
	}
	
	protected override void OnParametersSet()
	{
		base.OnParametersSet();
		
		if ( GraphableInput != null )
		{
			GraphableInput.Element = this;
		}
	}
}
