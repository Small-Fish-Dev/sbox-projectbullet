using ProjectBullet.Core.Node;
using Sandbox.Component;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphVisualizer : Panel
{
	private bool _waitingForInit = true;
	public NodeExecutor NodeExecutor { get; private set; }

	public PreInstanceGraph Graph { get; private set; }

	public GraphVisualizer( NodeExecutor nodeExecutor ) => NodeExecutor = nodeExecutor;

	public override void Tick()
	{
		base.Tick();

		if ( !_waitingForInit || Graph != null || NodeExecutor == null )
		{
			return;
		}

		SwitchNodeExecutor( NodeExecutor );
		_waitingForInit = false;
	}

	/// <summary>
	/// Visualize a different NodeExecutionEntity
	/// </summary>
	/// <param name="nodeExecutor">New NodeExecutionEntity</param>
	public void SwitchNodeExecutor( NodeExecutor nodeExecutor )
	{
		Log.Info( "SWITCH WEAPON!!!!!!!" );

		// Clean up UI nodes
		if ( Graph != null )
		{
			foreach ( var node in Graph.Nodes )
			{
				node.Element.Delete();
			}
		}

		Graph = new PreInstanceGraph( nodeExecutor, this );

		// Add fake entry node
		var graphNode = new GraphNode( Graph.Entry, this );
		graphNode.AddClass( "entry-node" );
		AddChild( graphNode );

		NodeExecutor = nodeExecutor;
	}
}
