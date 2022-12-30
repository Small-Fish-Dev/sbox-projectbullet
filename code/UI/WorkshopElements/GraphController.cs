using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node;
using Sandbox;

namespace ProjectBullet.UI.WorkshopElements;

public partial class GraphController
{
	private readonly List<Node> _nodes = new();

	/// <summary>
	/// Visualizer - this could be null
	/// </summary>
	public GraphVisualizer GraphVisualizer { get; set; }

	/// <summary>
	/// Current node executor being visualised
	/// </summary>
	public NodeExecutor NodeExecutor { get; set; }

	/// <summary>
	/// Node inventory - not always up to date
	/// </summary>
	public List<WeaponNode> GraphInventory { get; set; } = new();

	/// <summary>
	/// Read-only list of created nodes
	/// </summary>
	public IEnumerable<Node> Nodes => _nodes.AsReadOnly();

	/// <summary>
	/// List of connectors - note that this uses the Nodes list so it won't work after being deserialized.
	/// </summary>
	public IEnumerable<Connector> Connectors => Nodes.SelectMany( v => v.Connectors );

	/// <summary>
	/// Find node by WeaponNode instance
	/// </summary>
	/// <param name="entity">WeaponNode</param>
	/// <returns>Node or null</returns>
	private Node GetNodeByEntity( WeaponNode entity ) => _nodes.SingleOrDefault( v => v.Instance == entity );

	public EntryNode Entry;

	public GraphController( NodeExecutor nodeExecutor, GraphVisualizer graphVisualizer )
	{
		GraphVisualizer = graphVisualizer;

		Switch( nodeExecutor );
	}

	~GraphController()
	{
		Event.Unregister( this );
	}

	private readonly List<Action> _actionHistory = new();
	private int _actionPointer = 0;

	public object PerformAction( Action action, bool addToHistory )
	{
		if ( addToHistory )
		{
			_actionHistory.Add( action );
		}

		return action.Perform( this );
	}

	public void AddToGraph( WeaponNode entity )
	{
		PerformAction( new AddNodeToGraphAction( entity ), true );
	}

	public void PerformUndo()
	{
		
	}

	public void PerformRedo()
	{
		
	}
	
	public void RemoveFromGraph( Node node )
	{
		if ( node is EntryNode )
		{
			return;
		}

		foreach ( var connector in node.Connectors )
		{
			connector.Disconnect( true );
		}

		node.Previous?.Disconnect( true );

		PerformAction( new RemoveNodeFromGraphAction( node.Instance ), true );
	}

	/// <summary>
	/// Switch to a different node executor
	/// </summary>
	public void Switch( NodeExecutor executor )
	{
		// Clean up first
		// Clean up previous node elements
		foreach ( var node in _nodes )
		{
			node.Element.Delete( true );
		}

		// Clean up previous graph inventory
		GraphInventory.Clear();

		// Clean up nodes
		_nodes.Clear();

		// Clean up previous entry node
		Entry = null;

		// Start switching
		NodeExecutor = executor;

		if ( NodeExecutor == null )
		{
			Event.Unregister( this );
			return;
		}

		Event.Register( this );

		// Create entry node
		Entry = new EntryNode( this );
		_nodes.Add( Entry );
		
		// For each item in our inventory...
		foreach (var item in Util.LocalPersistent.Items)
		{
			if ( item is not WeaponNode node )
			{
				continue;
			}

			if ( node.InUse )
			{
				// This node is in use - let's check if it's by our NodeExecutor
				if ( node.Owner != NodeExecutor )
				{
					continue;
				}

				PerformAction( new AddNodeToGraphAction( node ), false );
			}
			else
			{
				GraphInventory.Add( node );
			}
		}
	}
}
