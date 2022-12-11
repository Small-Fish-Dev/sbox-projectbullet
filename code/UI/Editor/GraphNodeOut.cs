using ProjectBullet.Core.Node;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphNodeOut : Panel
{
	public PreInstanceGraph.Connector Connector { get; set; }

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
		{
			Connector.Element = this;
		}
	}

	public override void Delete( bool immediate = false )
	{
		Connector.Element = null;

		base.Delete( immediate );
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		if ( Connector.IsConnected )
		{
			Connector.Disconnect();
		}
	}

	public bool MakingLink { get; set; } = false;
	public bool IsConnected => Connector.IsConnected;
	public string RootClasses => $"{(IsConnected ? "connected" : "")} {(MakingLink ? "linking" : "")}";
}
