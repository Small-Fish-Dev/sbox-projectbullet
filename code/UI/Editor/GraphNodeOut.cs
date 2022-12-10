using ProjectBullet.Core.Node;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphNodeOut : Panel
{
	public SerializableGraph.Connector Connector { get; set; }

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
		{
			Connector.EditorData.Element = this;
		}
	}

	public override void Delete( bool immediate = false )
	{
		Connector.EditorData.Element = null;

		base.Delete( immediate );
	}

	public bool MakingLink { get; set; } = false;
	public bool Connected => Connector.Connected;
	public string RootClasses => $"{(Connected ? "connected" : "")} {(MakingLink ? "linking" : "")}";

	protected override void OnRightClick( MousePanelEvent e )
	{
		Connector.Disconnect();
	}
}
