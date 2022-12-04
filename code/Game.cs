using Sandbox;
using System;
using System.Linq;
using ProjectBullet.Items;
using Sandbox.UI;
using ProjectBullet.Players.Classes;
using ProjectBullet.UI.Editor;
using ProjectBullet.UI.Shop;
using ProjectBullet.Weapons;

namespace ProjectBullet;

public class TestClass
{
	public string Hello { get; set; }
}

[KnownWeaponPart("Shop Part One")]
[Purchasable(300)]
public class ShopPartOne : Items.WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

[KnownWeaponPart("Shop Part Two")]
[Purchasable(700)]
public class ShopPartTwo : Items.WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

[KnownWeaponPart("Shop Part Three")]
[Purchasable(75)]
public class ShopPartThree : Items.WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

public partial class Entrypoint : Sandbox.Game
{
	[Net] public ShopController Shop { get; set; }

	private Window _shopWindow = null;
	private Window _editorWindow = null;
	
	public Entrypoint()
	{
		if ( Host.IsClient )
		{
			Local.Hud = new RootPanel();
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		
		Shop = new ShopController();
		Shop.AddAllPurchasableItems();
	}

	[ConCmd.Client("pb_shop")]
	public static void ToggleShop()
	{
		var entrypoint = Game.Current as Entrypoint;
		if ( entrypoint._shopWindow == null )
		{
			entrypoint._shopWindow = new Window( new ShopView() );
			entrypoint._shopWindow.Title = "Shop";
			entrypoint._shopWindow.Width = 1200;
			entrypoint._shopWindow.Height = 840;
			Local.Hud.AddChild( entrypoint._shopWindow );
		}
		else
		{
			entrypoint._shopWindow.Delete(  );
			entrypoint._shopWindow = null;
		}
	}
	
	[ConCmd.Client("pb_editor")]
	public static void ToggleEditor()
	{
		var entrypoint = Game.Current as Entrypoint;
		if ( entrypoint._editorWindow == null )
		{
			entrypoint._editorWindow = new Window( new EditorView() );
			entrypoint._editorWindow.Title = "Editor";
			entrypoint._editorWindow.Width = 1400;
			entrypoint._editorWindow.Height = 900;
			Local.Hud.AddChild( entrypoint._editorWindow );
		}
		else
		{
			entrypoint._editorWindow.Delete(  );
			entrypoint._editorWindow = null;
		}
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
