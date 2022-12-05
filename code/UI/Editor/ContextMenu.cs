using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class ContextMenu : Panel
{
	public NodeGraph NodeGraph { get; set; }
	public ContextMenu( NodeGraph nodeGraph ) => NodeGraph = nodeGraph;
	public ContextMenu() {}
}
