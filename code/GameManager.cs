using Sandbox;
using System;
using System.Linq;
using ProjectBullet.Classes;
using ProjectBullet.Core.Node.Description;
using ProjectBullet.Core.Shop;
using ProjectBullet.UI.Editor;
using Sandbox.UI;

namespace ProjectBullet;

public partial class GameManager : Sandbox.GameManager
{
	public new static GameManager Current => (GameManager)Sandbox.GameManager.Current;
	
	/// <summary>
	/// Whether or not the game should have team damage
	/// </summary>
	[Net]
	public bool AllowTeamDamage { get; set; } = false;
	
	[Net] public ShopHostEntity GameShop { get; set; }

	private GraphVisualizer _nodeGraph { get; set; }

	public GameManager()
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
		var entrypoint = Current as GameManager;
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
