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

[KnownWeaponPart( "Test Part A" )]
[Purchasable( 300 )]
public class ShopPartOne : Items.WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

[KnownWeaponPart( "Test Part B" )]
[Purchasable( 700 )]
public class ShopPartTwo : Items.WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

[KnownWeaponPart( "Test Part C" )]
[Purchasable( 75 )]
public class ShopPartThree : Items.WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

[KnownWeaponPart( "Splitter", EnergyUsage = 30.0f )]
[Purchasable( 300 )]
public class SplitterItem : Items.WeaponPart
{
	[KnownOutput( DisplayName = "One" )] public WeaponPart One { get; set; }

	[KnownOutput( DisplayName = "Two" )] public WeaponPart Two { get; set; }

	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

[KnownWeaponPart( "Poop Fart" )]
[Purchasable( 1500 )]
public class PoopFartItem : Items.WeaponPart
{
	public override void Execute( Entity target, Vector3 point, Weapon weapon )
	{
		throw new NotImplementedException();
	}
}

public partial class Entrypoint : Sandbox.Game
{
	[Net] public ShopController Shop { get; set; }

	public Entrypoint()
	{
		if ( Host.IsClient )
		{
			Local.Hud = new RootPanel();
		}
	}

	private ShopView _shopView;
	private EditorView _editorView;

	public override void Spawn()
	{
		base.Spawn();

		Shop = new ShopController();
		Shop.AddAllPurchasableItems();
	}

	[ConCmd.Client( "pb_shop" )]
	public static void ToggleShop()
	{
		var entrypoint = Game.Current as Entrypoint;
		if ( entrypoint._shopView == null )
		{
			entrypoint._shopView = new ShopView();
			Local.Hud.AddChild( entrypoint._shopView );
		}
		else
		{
			entrypoint._shopView.Delete();
			entrypoint._shopView = null;
		}
	}


	[ConCmd.Client( "pb_editor" )]
	public static void ToggleEditor()
	{
		var entrypoint = Game.Current as Entrypoint;
		if ( entrypoint._editorView == null )
		{
			entrypoint._editorView = new EditorView();
			Local.Hud.AddChild( entrypoint._editorView );
		}
		else
		{
			(entrypoint._editorView as Panel).Delete( true );
			entrypoint._editorView = null;
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
