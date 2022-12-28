using ProjectBullet.Core.Node;
using Sandbox.Component;
using Sandbox.UI;

namespace ProjectBullet.UI.Workshop;

public partial class GraphVisualizer : Panel
{
	private bool _waitingForInit = true;
	public NodeExecutor NodeExecutor { get; private set; }

	public GraphController GraphController { get; private set; }

	public GraphVisualizer( NodeExecutor nodeExecutor ) => NodeExecutor = nodeExecutor;

	public override void Tick()
	{
		base.Tick();

		if ( !_waitingForInit || GraphController != null || NodeExecutor == null )
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
		if ( GraphController != null )
		{
			foreach ( var node in GraphController.Nodes )
			{
				node.Element.Delete();
			}
		}

		GraphController = new GraphController( nodeExecutor, this );

		// Add fake entry node
		var graphNode = new GraphNode( GraphController.Entry, this );
		graphNode.AddClass( "entry-node" );
		AddChild( graphNode );

		NodeExecutor = nodeExecutor;
	}
}
