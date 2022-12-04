using Sandbox;
using System;
using System.Linq;
using ProjectBullet.UI;
using ProjectBullet.UI.Editor;
using Sandbox.UI;
using Player = ProjectBullet.Players.Player;
using ProjectBullet.Players.Classes;

namespace ProjectBullet;

public class TestClass
{
	public string Hello { get; set; }
}

public partial class Entrypoint : Sandbox.Game
{
	public Entrypoint()
	{
		if ( Host.IsClient )
		{
			Local.Hud = new RootPanel();
			Local.Hud.AddChild( new PlayerHud() );
		}
	}

	[ConCmd.Client( "pb_test" )]
	public static void Test()
	{
		Local.Hud.AddChild<MainView>();
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Everyday();
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
			pawn.Transform = tx;
		}
	}
}
