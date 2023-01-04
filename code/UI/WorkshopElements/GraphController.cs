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
	public GraphVisualizer Visualizer { get; set; }

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
		Visualizer = graphVisualizer;

		Switch( nodeExecutor );
	}

	~GraphController()
	{
		Event.Unregister( this );
	}

	/// <summary>
	/// Update graph inventory
	/// </summary>
	private void UpdateInventory()
	{
		// For each item in our inventory...
		foreach ( var item in Util.LocalPersistent.Items )
		{
			if ( item is not WeaponNode node )
			{
				continue;
			}

			if ( GraphInventory.Contains( item ) )
			{
				continue;
			}

			if ( _nodes.Any( v => v.Instance == item ) )
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
				// GraphInventory.Add( node );
				// Add the item to the screen
				PerformAction( new AddNodeToGraphAction( node ), false );
			}
		}
	}

	[ClientRpc]
	public static void OnNewItem()
	{
		Util.Workshop.Controller.UpdateInventory();
	}

	private readonly List<Action> _actionHistory = new();
	private int _actionPointer = -1;

	public object PerformAction( Action action, bool addToHistory )
	{
		if ( addToHistory )
		{
			// remove everything in history that comes after the action pointer
			_actionHistory.RemoveRange( _actionPointer + 1, _actionHistory.Count - (_actionPointer + 1) );

			_actionHistory.Add( action );
			_actionPointer++;

			return action.Perform( this );
		}

		return action.Perform( this );
	}

	private void PrintActionHistory()
	{
		Log.Info( $"action pointer: {_actionPointer}" );
		for ( var i = 0; i < _actionHistory.Count; i++ )
		{
			var extra = i == _actionPointer ? "(actionptr)" : "";
			Log.Info( $"{i}: {extra} {_actionHistory[i]} " );
		}
	}

	public void PerformUndo()
	{
		PrintActionHistory();

		if ( _actionPointer == -1 )
		{
			Log.Info( "No actions left" );
			return;
		}

		var action = _actionHistory[_actionPointer];
		var opposite = action.CreateOpposite();

		PerformAction( opposite, false );

		_actionHistory[_actionPointer] = opposite;
		_actionPointer--;
	}

	public void PerformRedo()
	{
		PrintActionHistory();

		if ( _actionPointer + 1 >= _actionHistory.Count )
		{
			Log.Info( "No actions to redo" );
			return;
		}

		_actionPointer++;

		var action = _actionHistory[_actionPointer];
		var opposite = action.CreateOpposite();

		PerformAction( opposite, false );

		_actionHistory[_actionPointer] = opposite;
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

		// Add entry node to visualizer
		var graphNode = new GraphNode( Entry );
		graphNode.AddClass( "entry-node" );
		Visualizer.AddChild( graphNode );

		// Update inventory
		UpdateInventory();

		// Resolve node links
		foreach ( var node in _nodes )
		{
			foreach ( var connector in node.Connectors )
			{
				var connectedInstance = node.Instance?.GetConnectedNode( connector.Identifier );
				if ( connectedInstance != null )
				{
					PerformAction(
						new ConnectConnectorToNodeAction( node.Instance, connector.Identifier, connectedInstance ),
						false
					);
				}
			}
		}

		// Set entry node connection
		if ( NodeExecutor.EntryNode != null )
		{
			PerformAction( new ConnectEntryNodeAction( NodeExecutor.EntryNode ), false );
		}
		else
		{
			Log.Warning( $"NodeExecutor {NodeExecutor} has no entry node." );
		}
	}
}
