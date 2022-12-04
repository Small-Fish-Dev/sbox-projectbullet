using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class NodeOutput : Panel
{
	public GraphableWeaponPart.GraphableOutput GraphableOutput { get; set; }

	public bool MakingLink { get; set; } = false;
	public bool HasTarget => GraphableOutput.Target != null;
	public string RootClasses => $"{(HasTarget ? "has-target" : "")} {(MakingLink ? "making-link" : "")}";

	protected override void OnRightClick( MousePanelEvent e )
	{
		GraphableOutput.Target = null;
	}

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		if ( GraphableOutput != null )
		{
			GraphableOutput.Element = this;
		}
	}
}
