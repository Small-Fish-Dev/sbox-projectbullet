using ProjectBullet.Core.Node;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphNodeIn : Panel
{
	public GraphNode GraphNode { get; set; }

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
		{
			GraphNode.NodeData.EditorData.InputElement = this;
		}
	}

	public override void Delete( bool immediate = false )
	{
		GraphNode.NodeData.EditorData.InputElement = null;

		base.Delete( immediate );
	}

	public bool Hovered { get; set; } = false;
	public bool IsInvalidHover { get; set; } = false;
	public bool MakingLink { get; set; } = false;
	public bool Connected => GraphNode.NodeData.Previous != null;
	public string RootClasses => $"{(Connected ? "connected" : "")} {(MakingLink ? "linking" : "")}";

	protected override void OnRightClick( MousePanelEvent e )
	{
		GraphNode.NodeData.Previous?.Disconnect();
	}
}
