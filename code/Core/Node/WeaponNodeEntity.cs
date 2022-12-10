using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Node.Description;
using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Core.Node;

public abstract partial class WeaponNodeEntity : Entity, IInventoryItem
{
	private class Connector
	{
		public WeaponNodeEntity WeaponNodeEntity = null;
		public readonly ConnectorAttribute ConnectorAttribute;
		public string Identifier => ConnectorAttribute.Identifier;

		public Connector( ConnectorAttribute nextAttribute )
		{
			ConnectorAttribute = nextAttribute;
		}
	}

	[Net] private IList<Connector> Connectors { get; set; } = new List<Connector>();

	public float EnergyUsage => Description?.EnergyAttribute?.Energy ?? 0;

	/// <summary>
	/// Run <see cref="Execute"/> on connected node with provided connector ID
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <param name="target">Target to pass to Execute (if any)</param>
	/// <param name="point">Position to pass to Execute (if any)</param>
	/// <exception cref="Exception"></exception>
	protected void ExecuteConnector( string identifier, Entity target, Vector3 point )
	{
		foreach ( var connector in Connectors.Where( connector => connector.Identifier == identifier ) )
		{
			connector.WeaponNodeEntity?.Execute( target, point );
			return;
		}

		// todo: write custom exception
		throw new Exception( $"Unknown connector {identifier}" );
	}

	/// <summary>
	/// Reset all connectors
	/// </summary>
	public void ResetConnections()
	{
		foreach ( var connector in Connectors )
		{
			connector.WeaponNodeEntity = null;
		}
	}

	/// <summary>
	/// Set connector link
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <param name="weaponNodeEntity">Connector linked entity</param>
	/// <exception cref="Exception"></exception>
	public void SetConnector( string identifier, WeaponNodeEntity weaponNodeEntity )
	{
		foreach ( var connector in Connectors.Where( connector => connector.Identifier == identifier ) )
		{
			connector.WeaponNodeEntity = weaponNodeEntity;
			return;
		}

		// todo: write custom exception
		throw new Exception( $"Unknown connector {identifier}" );
	}

	/// <summary>
	/// Get connector linked entity
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <exception cref="Exception"></exception>
	public WeaponNodeEntity GetConnectedNode( string identifier )
	{
		return Connectors.Where( connector => connector.Identifier == identifier )
			.Select( connector => connector.WeaponNodeEntity ).FirstOrDefault();
	}

	/// <summary>
	/// Method to be executed when this node runs.
	/// Either target or point should be non-null. *both don't have to be non-null!*
	/// </summary>
	/// <param name="target">Target (if any) entity hit</param>
	/// <param name="point">Position (if any) of hit</param>
	public abstract void Execute( Entity target, Vector3 point );

	/// <summary>
	/// Removes node from NodeExecutor and returns ownership back to the pawn
	/// </summary>
	public void RemoveFromExecutor()
	{
		if ( Owner is not NodeExecutionEntity nodeExecutor )
		{
			Log.Warning( $"RemoveFromExecutor called but Owner is unknown type {Owner.GetType()}" );
			return;
		}

		Owner = nodeExecutor.BasePlayer;
	}

	public WeaponNodeDescription Description { get; }

	protected WeaponNodeEntity()
	{
		Transmit = TransmitType.Owner;
		Description = WeaponNodeDescription.Get( GetType() );

		foreach ( var nextAttribute in Description.ConnectorAttributes )
		{
			// todo: _connectors could be incorrect after a hotload. don't do this here maybe?
			Connectors.Add( new Connector( nextAttribute ) );
		}
	}

	[Net] public Guid Uid { get; set; } = Guid.NewGuid();
}
