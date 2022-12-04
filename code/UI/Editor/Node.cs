using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class Node : Panel
{
	public GraphableWeaponPart GraphableWeaponPart { get; set; }
	public NodeGraph NodeGraph { get; set; }

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

		if ( NodeGraph == null )
		{
			return;
		}

		NodeGraph.RemovePart( GraphableWeaponPart );

		// Remove this node
		Delete();
	}
}
