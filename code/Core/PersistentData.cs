using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Core;

public partial class PersistentData : Entity
{
	public PersistentData( IClient client )
	{
		Transmit = TransmitType.Always;

		SetClient( client );
	}

	public PersistentData() => Game.AssertClient();

	/// <summary>
	/// Set client 
	/// </summary>
	/// <param name="client"></param>
	/// <exception cref="ArgumentException"></exception>
	private void SetClient( IClient client )
	{
		if ( client is not Entity entity )
		{
			throw new ArgumentException( "Provided client isn't an entity" );
		}

		SteamId = client.SteamId;
		Owner = entity;
	}

	/// <summary>
	/// Update client using SteamId
	/// </summary>
	private void Update()
	{
		Game.AssertServer();

		if ( IsConnected )
		{
			Log.Info( $"Skipping update for {SteamId}: they are already connected" );
			return;
		}

		foreach ( var client in Game.Clients )
		{
			if ( client.SteamId != SteamId )
			{
				continue;
			}

			SetClient( client );
			return;
		}

		Log.Info( $"No client found for {SteamId}" );
	}

	public bool IsConnected => Client.IsValid && Client.SteamId == SteamId;

	[Net] private long SteamId { get; set; }
	[Net] private string PawnType { get; set; }
	[Net] public int Money { get; private set; } = 3000;
	[Net] private IList<Entity> ItemsInternal { get; set; } = new List<Entity>();

	private void SetPawnType( Type type )
	{
		Game.AssertServer();
		if ( !type.IsSubclassOf( typeof(Player) ) )
		{
			throw new ArgumentException( $"Unknown pawn type {type.FullName}" );
		}

		PawnType = type.FullName;
	}

	/// <summary>
	/// Create a pawn for the client from the saved type
	/// </summary>
	private Player CreateClientPawn( Type type = null )
	{
		if ( !IsConnected )
		{
			return null;
		}

		if ( type != null )
		{
			SetPawnType( type );
		}

		Game.AssertServer();

		var typeDescription = TypeLibrary.GetType( type );
		if ( typeDescription == null )
		{
			throw new Exception( "TypeLibrary failure" );
		}

		var instance = typeDescription.Create<Player>();
		Client.Pawn = instance;

		Log.Info( $"Set pawn for client {Client.Name}" );

		return instance;
	}

	/// <summary>
	/// Create a pawn for the client from the saved type
	/// </summary>
	public T CreateClientPawn<T>() where T : Player => (T)CreateClientPawn( typeof(T) );

	/// <summary>
	/// Read only list of player items
	/// </summary>
	public IEnumerable<IInventoryItem> Items
	{
		get
		{
			IList<IInventoryItem> output = new List<IInventoryItem>();
			foreach ( var entity in ItemsInternal )
			{
				if ( entity is IInventoryItem item )
				{
					output.Add( item );
				}
			}

			return output.AsReadOnly();
		}
	}

	public void AddItem( IInventoryItem item )
	{
		ItemsInternal.Add( item as Entity );

		if ( IsConnected )
		{
			// Events.Shared.Workshop.RunNewItem( To.Single( Client ), item as Entity );
		}
	}

	public IInventoryItem FindItem( Guid uid ) => Items.SingleOrDefault( v => v.Uid == uid );

	/// <summary>
	/// Attempt to use some player money
	/// </summary>
	/// <param name="amount">Amount to take</param>
	/// <returns>True if money was deducted, false if not enough money</returns>
	public bool UseMoney( int amount )
	{
		if ( amount > Money )
		{
			return false;
		}

		Money -= amount;
		return true;
	}

	/// <summary>
	/// Get client persistent data
	/// </summary>
	/// <param name="client"></param>
	/// <returns></returns>
	public static PersistentData Get( IClient client ) =>
		All.OfType<PersistentData>().SingleOrDefault( v => v.Client == client );

	/// <summary>
	/// Update all persistent data clients
	/// </summary>
	public static void UpdateAll()
	{
		foreach ( var persistentData in All.OfType<PersistentData>() )
		{
			persistentData.Update();
		}
	}
}
