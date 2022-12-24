using ProjectBullet.Core.Node;
using Sandbox.UI;

namespace ProjectBullet.UI.Workshop;

public partial class GraphNode : Panel
{
	public PreInstanceGraph.Node NodeData { get; set; }
	public GraphVisualizer GraphVisualizer { get; set; }
	private bool _waitingForInit = true;

	public GraphNode( PreInstanceGraph.Node nodeData, GraphVisualizer graphVisualizer )
	{
		NodeData = nodeData;
		GraphVisualizer = graphVisualizer;
		NodeData.Element = this;
	}

	public override void Tick()
	{
		base.Tick();

		if ( !_waitingForInit || NodeData.Instance == null )
		{
			return;
		}

		if ( NodeData.Instance.LastEditorX != null )
		{
			Style.Left = Length.Pixels(
				NodeData.Instance.LastEditorX.Value
			);
		}

		if ( NodeData.Instance.LastEditorY != null )
		{
			Style.Top = Length.Pixels(
				NodeData.Instance.LastEditorY.Value
			);
		}

		_waitingForInit = false;
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		NodeData.Root.RemoveFromGraph( NodeData );
	}
}
