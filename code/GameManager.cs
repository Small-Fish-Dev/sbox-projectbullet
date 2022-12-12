using Sandbox;
using System;
using System.Linq;
using System.Text.Json;
using ProjectBullet.Classes;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Node.Description;
using ProjectBullet.Core.Shop;
using ProjectBullet.Nodes;
using ProjectBullet.UI.Editor;
using Sandbox.UI;

// ReSharper disable InconsistentNaming

namespace ProjectBullet;

[ShopItem( 300 )]
[Energy( 12.0f, Estimated = true )]
[Connector( "on_one", Order = 0, EnergyOutPercentage = 0.5f, DisplayName = "One" )]
[Connector( "on_two", Order = 1, EnergyOutPercentage = 0.5f, DisplayName = "Two" )]
[Node( DisplayName = "Splitter", Description = "Cheap and simple splitter" )]
public class BasicSplitter : WeaponNodeEntity
{
	public override float Execute( float energy, Entity target, Vector3 point )
	{
		ExecuteConnector( "on_one", target, point );
		ExecuteConnector( "on_two", target, point );

		return 0.0f;
	}
}

[ShopItem( 200 )]
[Energy( 15.0f, Estimated = true )]
[Connector( "on_player_hit", Order = 0, EnergyOutAmount = 8f, DisplayName = "On Player Hit" )]
[Node( DisplayName = "Explosion", Description = "Cheap and simple explosion" )]
public class CheapExplosion : WeaponNodeEntity
{
	public override float Execute( float energy, Entity target, Vector3 point )
	{
		ExecuteConnector( "on_player_hit", target, point );

		return 0.0f;
	}
}

public partial class Entrypoint : GameManager
{
	[Net] public ShopHostEntity GameShop { get; set; }

	private GraphVisualizer _nodeGraph { get; set; }

	public Entrypoint()
	{
		WeaponNodeDescription.InitAll();

		if ( Game.IsClient )
		{
			Game.RootPanel = new RootPanel();
		}
		else
		{
			GameShop = new ShopHostEntity();
			GameShop.StockAllItems();
		}
	}

	[ConCmd.Client( "pb_editor" )]
	public static void ToggleEditor()
	{
		var entrypoint = Current as Entrypoint;
		if ( entrypoint is { _nodeGraph: null } )
		{
			var nodeExecutor = (Game.LocalPawn as Gunner).NodeExecutors.First();
			entrypoint._nodeGraph = new GraphVisualizer( nodeExecutor );
			Game.RootPanel.AddChild( entrypoint._nodeGraph );
		}
		else
		{
			(entrypoint?._nodeGraph as Panel)?.Delete( true );
			if ( entrypoint != null )
			{
				entrypoint._nodeGraph = null;
			}
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Gunner();
		client.Pawn = pawn;
		pawn.Respawn();

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			//pawn.Transform = tx;
		}
	}
}
