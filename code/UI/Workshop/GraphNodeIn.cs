using ProjectBullet.Core.Node;
using Sandbox.UI;

namespace ProjectBullet.UI.Workshop;

public partial class GraphNodeIn : Panel
{
	public GraphNode GraphNode { get; set; }
	public PreInstanceGraph.Node NodeData => GraphNode.NodeData;
	public PreInstanceGraph Graph => GraphNode.NodeData.Root;

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
		{
			GraphNode.NodeData.InputElement = this;
		}
	}

	public override void Delete( bool immediate = false )
	{
		GraphNode.NodeData.InputElement = null;

		base.Delete( immediate );
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		if ( NodeData.IsConnected )
		{
			NodeData.Previous.Disconnect();
		}

		e.StopPropagation();
	}

	public bool Hovered { get; set; } = false;
	public bool IsInvalidHover { get; set; } = false;

	public bool InvalidEstimatedEnergy
	{
		get
		{
			var estimated = NodeData.Previous?.LastEstimatedEnergyOutput;
			if ( estimated == null )
			{
				return false;
			}

			var required = NodeData.Instance?.Description?.EnergyAttribute?.Energy;

			return required > estimated;
		}
	}

	public bool IsConnected => GraphNode.NodeData.IsConnected;

	public string RootClasses =>
		$"{(InvalidEstimatedEnergy ? "estimation-low" : "")} {(IsConnected ? "connected" : "")} {(IsInvalidHover ? "invalid-link" : "")}";
}
