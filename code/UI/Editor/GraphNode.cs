using ProjectBullet.Core.Node;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphNode : Panel
{
	public SerializableGraph.WeaponNode NodeData { get; set; }
	public GraphVisualizer GraphVisualizer { get; set; }

	public GraphNode( SerializableGraph.WeaponNode nodeData, GraphVisualizer graphVisualizer )
	{
		NodeData = nodeData;
		GraphVisualizer = graphVisualizer;

		NodeData.EditorData.Element = this;
	}

	public override void Delete( bool immediate = false )
	{
		NodeData.Isolate();

		NodeData.EditorData.Element = null;

		base.Delete( immediate );
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		base.OnRightClick( e );

		Delete();
	}
}
