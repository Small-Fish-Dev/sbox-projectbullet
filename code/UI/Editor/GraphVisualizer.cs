using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ProjectBullet.Core.Node;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI.Editor;

public partial class GraphVisualizer : Panel
{
	private bool _waitingForInit;

	/// <summary>
	/// Current node executor being visualised
	/// </summary>
	public NodeExecutionEntity NodeExecutor { get; set; }

	public BasePlayer LocalBasePlayer => Game.LocalPawn as BasePlayer;
	public SerializableGraph SerializableGraph { get; set; } = new();
	public List<WeaponNodeEntity> InactiveNodes { get; set; } = new();

	public GraphVisualizer( NodeExecutionEntity nodeExecutor ) => SwitchNodeExecutor( nodeExecutor );
	public GraphVisualizer() => _waitingForInit = true;

	public override void Tick()
	{
		base.Tick();

		if ( _waitingForInit && NodeExecutor != null )
		{
			SwitchNodeExecutor( NodeExecutor );
			_waitingForInit = false;
		}
	}

	private void UpdateGraph()
	{
	}

	public void AddToGraph( WeaponNodeEntity weaponNodeEntity )
	{
		var node = SerializableGraph.AddNode( weaponNodeEntity );

		// Add to graph
		AddChild( new GraphNode( node, this ) );

		if ( InactiveNodes.Contains( weaponNodeEntity ) )
		{
			InactiveNodes.Remove( weaponNodeEntity );
		}

		// Redraw elements
		UpdateGraph();
	}

	/// <summary>
	/// Make part inactive
	/// </summary>
	/// <param name="node">WeaponNode</param>
	public void RemoveFromGraph( SerializableGraph.WeaponNode node )
	{
		// Remove part element
		node.EditorData.Element?.Delete();

		SerializableGraph.RemoveNode( node );

		InactiveNodes.Add( node.Instance );

		// Redraw elements
		UpdateGraph();
	}

	/// <summary>
	/// Find unused inventory nodes - these are owned by the player pawn 
	/// </summary>
	/// <returns>Unused inventory nodes</returns>
	private IEnumerable<WeaponNodeEntity> FindUnusedInventoryNodeEntities() => LocalBasePlayer.Inventory.Items
		.OfType<WeaponNodeEntity>().Where( v => v.Owner == LocalBasePlayer );

	public override void OnDeleted()
	{
		base.OnDeleted();

		SendToServer();
	}

	public void SendToServer()
	{
		Log.Info( JsonSerializer.Serialize( SerializableGraph ) );

		NodeCmd.UpdateExecutor( NodeExecutor, SerializableGraph.Serialize() );
	}

	/// <summary>
	/// Visualize a different NodeExecutionEntity
	/// </summary>
	/// <param name="nodeExecutor">New NodeExecutionEntity</param>
	public void SwitchNodeExecutor( NodeExecutionEntity nodeExecutor )
	{
		Log.Info( "SWITCH WEAPON!!!!!!!" );

		// Remove old nodes from the graph
		for ( var index = SerializableGraph.Nodes.Count - 1; index >= 0; index-- )
		{
			var node = SerializableGraph.Nodes[index];
			if ( node is not SerializableGraph.EntryWeaponNode )
			{
				RemoveFromGraph( node );
			}
		}

		// Reset InactiveNodes
		InactiveNodes.Clear();

		foreach ( var weaponNodeEntity in FindUnusedInventoryNodeEntities() )
		{
			InactiveNodes.Add( weaponNodeEntity );
			Log.Info( $"adding wne {weaponNodeEntity}" );
		}

		NodeExecutor = nodeExecutor;

		SerializableGraph = SerializableGraph.CreateFromNodeExecutor( NodeExecutor );

		// Add fake entry node to graph
		AddChild( new GraphNode( SerializableGraph.EntryNode, this ) );

		InventoryMenu.StateHasChanged();
	}
}
