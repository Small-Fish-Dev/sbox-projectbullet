using Sandbox;
using System;
using System.Linq;
using ProjectBullet.Classes;
using ProjectBullet.Core.Node.Description;
using ProjectBullet.Core.Shop;
using ProjectBullet.MapEnts;
using ProjectBullet.Player;
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

	[Net] public ShopHostEntity GameShop { get; set; } = null;
	

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

	/// <summary>
	/// Get team for new player to join
	/// </summary>
	/// <returns>PlayerTeam</returns>
	private PlayerTeam GetDisadvantagedTeam()
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

	public override void MoveToSpawnpoint( Entity pawn )
	{
		if ( pawn is not BasePlayer player )
		{
			base.MoveToSpawnpoint( pawn );
			return;
		}

		// get all markers
		var markers = Entity.All.OfType<SpawnMarker>().Where( v => v.Team == player.Team );

		// choose random one...
		var marker = markers.MinBy( x => Guid.NewGuid() );

		// if it exists put the pawn down!
		if ( marker == null )
		{
			return;
		}

		var transform = marker.Transform;
		transform.Position += Vector3.Up * 50.0f;
		player.Transform = transform;
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
}
