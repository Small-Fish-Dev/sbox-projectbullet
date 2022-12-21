using Sandbox;
using System;
using System.Linq;
using ProjectBullet.Classes;
using ProjectBullet.Core.Node;
using ProjectBullet.Core.Shop;
using ProjectBullet.MapEnts;
using ProjectBullet.Player;
using ProjectBullet.UI.HUD;
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
			Game.RootPanel.AddChild<HUD>();
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
		var players = All.OfType<BasePlayer>().ToList();
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
		if ( pawn is not BasePlayer player )
		{
			base.MoveToSpawnpoint( pawn );
			return;
		}

		// get all markers
		var markers = All.OfType<SpawnMarker>().Where( v => v.Team == player.Team ).Cast<Entity>();

		var enumerable = markers as Entity[] ?? markers.ToArray();
		if ( !enumerable.Any() )
		{
			markers = All.OfType<SpawnPoint>();
		}
		
		// choose random one...
		var marker = enumerable.MinBy( _ => Guid.NewGuid() );

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

		// Create a pawn for this client to play with
		var pawn = new Gunner();
		client.Pawn = pawn;

		// Give the pawn a team
		pawn.Team = GetDisadvantagedTeam();

		// Say which team
		Log.Info( $"Put {client.Name} on {pawn.Team}" );

		// Respawn pawn
		pawn.Respawn();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		DebugOverlay.ScreenText( "ProjectBullet - Node Design Test", Vector2.One * 20, 0, Color.Orange );
		DebugOverlay.ScreenText( "This test is meant for node balance / design. Any gameplay you see isn't final!",
			Vector2.One * 20, 1, Color.Orange );
		DebugOverlay.ScreenText( "<3 - team@snail", Vector2.One * 20, 2, Color.Cyan );

		if ( Input.Pressed( InputButton.View ) )
		{
			BasePlayer.ToggleEditor();
		}
	}
}
