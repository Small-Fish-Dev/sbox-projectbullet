using System;
using System.Linq;
using ProjectBullet.Characters;
using ProjectBullet.Core;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.MapEnts;
using ProjectBullet.UI;
using ProjectBullet.UI.Workshop;
using Sandbox;
using Sandbox.Diagnostics;
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

	public GameManager()
	{
		WeaponNodeDescription.InitAll();

		if ( Game.IsClient )
		{
			Game.RootPanel = new RootPanel();
			Util.CreateHud( Game.RootPanel );
		}
		else
		{
			// we don't need to store this, ShopHostEntity.Instance works
			var shopHost = new ShopHostEntity();
			shopHost.StockAllItems();
		}
	}

	/// <summary>
	/// Get team for new player to join
	/// </summary>
	/// <returns>PlayerTeam</returns>
	private static PlayerTeam GetDisadvantagedTeam()
	{
		var players = All.OfType<Core.Player>().ToList();
		var teamOneCount = players.Count( v => v.Team == PlayerTeam.TeamOne );
		var teamTwoCount = players.Count( v => v.Team == PlayerTeam.TeamTwo );

		return teamOneCount >= teamTwoCount
			?
			// add to team two
			PlayerTeam.TeamTwo
			:
			// add to team one
			PlayerTeam.TeamOne;
	}


	/// <summary>
	/// Move player to their team spawnpoint
	/// </summary>
	/// <param name="pawn">Player</param>
	public override void MoveToSpawnpoint( Entity pawn )
	{
		if ( pawn is not Core.Player player )
		{
			base.MoveToSpawnpoint( pawn );
			return;
		}

		// get all markers
		var markers = All.OfType<SpawnMarker>().Where( v => v.Team == player.Team ).Cast<Entity>();

		// choose random one...
		var marker = markers.MinBy( _ => Guid.NewGuid() );

		// if it exists put the pawn down!
		var transform = marker?.Transform ?? player.Transform;
		transform.Position += Vector3.Up * 50.0f;
		player.Transform = transform;
		var test = Vector3.Random;
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Update all persistent data clients...
		PersistentData.UpdateAll();

		// Check if this client already has data...
		var persistent = PersistentData.Get( client );
		persistent ??= new PersistentData( client );

		var pawn = persistent.CreateClientPawn<Gunner>();

		// Give the pawn a team
		var config = Util.MapConfig;
		pawn.Team = config.IsFreeForAll ? PlayerTeam.TeamOne : GetDisadvantagedTeam();

		// Say which team
		Log.Info( $"Put {client.Name} on {pawn.Team}" );

		// Respawn pawn
		pawn.Respawn();
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		// Update all persistent data clients...
		PersistentData.UpdateAll();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		DebugOverlay.ScreenText( "ProjectBullet - Node Design Test", Vector2.One * 20, 0, Color.Orange );
		DebugOverlay.ScreenText( "This test is meant for node balance / design. Any gameplay you see isn't final!",
			Vector2.One * 20, 1, Color.Orange );
		DebugOverlay.ScreenText( "<3 - team@snail", Vector2.One * 20, 2, Color.Cyan );
	}

	/// <summary>
	/// An entity, which is a pawn, and has a client, has been killed.
	/// </summary>
	public override void OnKilled( IClient client, Entity pawn )
	{
		Game.AssertServer();

		OnDeathFeed( pawn, pawn.LastAttacker );
	}

	[ClientRpc]
	public virtual void OnDeathFeed( Entity victim, Entity attacker )
	{
		Util.Hud.DeathFeed.AddEntry( victim, attacker );
	}
}
