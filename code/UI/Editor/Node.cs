using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class Node : Panel
{
	public Node( GraphableWeaponPart graphableWeaponPart ) => GraphableWeaponPart = graphableWeaponPart;

	public Node( GraphableWeaponPart graphableWeaponPart, NodeGraph graph )
	{
		GraphableWeaponPart = graphableWeaponPart;
		NodeGraph = graph;
	}

	public GraphableWeaponPart GraphableWeaponPart { get; set; }
	public NodeGraph NodeGraph { get; set; }

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
		{
			return;
		}
	}

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		if ( GraphableWeaponPart != null )
		{
			GraphableWeaponPart.Element = this;

			Style.Left = Length.Pixels(
				GraphableWeaponPart.SavedX < 0 ? 0 : GraphableWeaponPart.SavedX
			);
			Style.Top = Length.Pixels(
				GraphableWeaponPart.SavedY < 0 ? 0 : GraphableWeaponPart.SavedY
			);
		}
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		NodeGraph?.MovePartIntoInactive( GraphableWeaponPart );
	}
}
