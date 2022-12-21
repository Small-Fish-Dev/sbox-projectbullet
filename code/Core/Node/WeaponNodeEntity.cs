using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBullet.Core.Shop;
using ProjectBullet.Player;
using Sandbox;

namespace ProjectBullet.Core.Node;

public abstract partial class WeaponNodeEntity : Entity, IInventoryItem
{
	public partial class Connector : Entity
	{
		// TODO: THIS SHOULDN'T NEED TO BE AN ENTITY!!!!!!
		[Net] public WeaponNodeEntity WeaponNodeEntity { get; set; } = null;
		public ConnectorAttribute ConnectorAttribute { get; private set; }
		[Net] public string Identifier { get; set; }

		[Net, Change( "OnTypeNameChanged" )]
		public string TypeName { get; set; } = ""; // hacky fix, we can't network types / attrs

		public Connector( ConnectorAttribute nextAttribute, WeaponNodeEntity entity )
		{
			Transmit = TransmitType.Owner;
			ConnectorAttribute = nextAttribute;
			Identifier = ConnectorAttribute.Identifier;
			TypeName = entity.GetType().FullName; // hacky
		}

		public Connector() { }

		private void OnTypeNameChanged()
		{
			// find the ConnectorAttribute ourselves!
			var type = TypeLibrary.GetType( TypeName );
			if ( type == null )
			{
				Log.Error( $"Connector entity type {TypeName} invalid" );
				return;
			}

			ConnectorAttribute = TypeLibrary.GetAttributes<ConnectorAttribute>( type.TargetType )
				.SingleOrDefault( v => v.Identifier == Identifier );
		}
	}

	/// <summary>
	/// Whether or not the node is being used in a graph
	/// </summary>
	public bool InUse => Owner is NodeExecutionEntity;

	public NodeExecutionEntity NodeExecutor => Owner as NodeExecutionEntity;

	public BasePlayer BasePlayer => InUse ? (BasePlayer)Owner.Owner : (BasePlayer)Owner;

	[Net] public IList<Connector> Connectors { get; set; } = new List<Connector>();

	[Net] private WeaponNodeEntity Previous { get; set; }
	[Net] private string PreviousConnectorId { get; set; } // this is because BaseNetworkable exchanging sucks

	public float? LastEditorX = null;
	public float? LastEditorY = null;

	public float EnergyUsage => Description?.EnergyAttribute?.Energy ?? 0;

	/// <summary>
	/// Run <see cref="Execute"/> on connected node with provided connector ID
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <param name="target">Target to pass to Execute (if any)</param>
	/// <param name="point">Position to pass to Execute (if any)</param>
	/// <exception cref="Exception"></exception>
	protected void ExecuteConnector( string identifier, ExecuteInfo info )
	{
		if ( Owner is not NodeExecutionEntity nodeExecutor )
		{
			Log.Warning( "ExecuteConnector called without NodeExecutionEntity" );
			return;
		}

		foreach ( var connector in Connectors.Where( connector => connector.Identifier == identifier ) )
		{
			var instance = connector.WeaponNodeEntity;
			if ( instance == null )
			{
				continue;
			}

			connector.WeaponNodeEntity?.PostExecuteConnector( this, connector, info );
			return;
		}

		// todo: write custom exception
		Log.Warning( $"Unknown connector {identifier}" );
	}

	/// <summary>
	/// Reset all connectors
	/// </summary>
	public void ResetConnectors()
	{
		foreach ( var connector in Connectors )
		{
			connector.WeaponNodeEntity = null;
		}
	}

	/// <summary>
	/// Get connector by identifier
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <returns>Connector or null</returns>
	private Connector GetConnector( string identifier ) =>
		Connectors.FirstOrDefault( connector => connector.Identifier == identifier );

	/// <summary>
	/// Set connector link
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <param name="weaponNodeEntity">Connector linked entity</param>
	/// <exception cref="Exception"></exception>
	public void SetConnector( string identifier, WeaponNodeEntity weaponNodeEntity )
	{
		if ( weaponNodeEntity == null )
		{
			DisconnectConnector( identifier );
			return;
		}

		if ( Game.IsClient )
		{
			NodeCmd.SetConnector( this, identifier, weaponNodeEntity );
		}

		var connector = GetConnector( identifier );

		if ( connector == null )
		{
			// todo: write custom exception
			throw new Exception( $"Unknown connector {identifier}" );
		}

		connector.WeaponNodeEntity = weaponNodeEntity;
		connector.WeaponNodeEntity.Previous = this;
		connector.WeaponNodeEntity.PreviousConnectorId = identifier;
	}

	/// <summary>
	/// Disconnect connector link
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	public void DisconnectConnector( string identifier )
	{
		if ( Game.IsClient )
		{
			NodeCmd.DisconnectConnector( this, identifier );
		}

		var connector = GetConnector( identifier );

		if ( connector == null )
		{
			// todo: write custom exception
			throw new Exception( $"Unknown connector {identifier}" );
		}

		if ( connector.WeaponNodeEntity == null )
		{
			return;
		}

		connector.WeaponNodeEntity.Previous = null;
		connector.WeaponNodeEntity = null;
	}

	/// <summary>
	/// Get connector that will execute this node
	/// </summary>
	/// <returns>Connector or null</returns>
	private Connector GetPreviousConnector() => Previous?.GetConnector( PreviousConnectorId );

	/// <summary>
	/// Get connector linked entity
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <exception cref="Exception"></exception>
	public WeaponNodeEntity GetConnectedNode( string identifier ) => GetConnector( identifier )?.WeaponNodeEntity;

	/// <summary>
	/// Method to be executed when this node runs.
	/// Either target or point should be non-null. *both don't have to be non-null!*
	/// </summary>
	/// <param name="energy">Amount of energy provided to the node</param>
	/// <param name="info">ExecuteInfo</param>
	/// <returns>Amount of energy to be taken away from the node executor</returns>
	public abstract float Execute( float energy, ExecuteInfo info );

	public void PreExecute( float energy, ExecuteInfo info )
	{
		if ( Description != null )
		{
			if ( !NodeExecutor.UseEnergy( Description.EnergyAttribute.Energy ) )
			{
				Log.Info( $"{GetType().Name}: Not enough real energy to run Execute" );
				return;
			}
		}

		var inputEnergy = Math.Min( energy, NodeExecutor.Energy );
		Log.Info( $"Executing {GetType().Name} with {inputEnergy} energy" );

		var outputEnergy = Execute( Math.Min( energy, NodeExecutor.Energy ), info );
		if ( NodeExecutor.Energy - outputEnergy <= 0 )
		{
			NodeExecutor.Energy = 0;
		}
		else
		{
			NodeExecutor.Energy -= outputEnergy;
		}
	}

	protected void PostExecuteConnector( WeaponNodeEntity previous, Connector connector, ExecuteInfo info )
	{
		var estimatedEnergy = previous.EstimateConnectorOutput( connector.Identifier ) ??
		                      NodeExecutor.Energy;

		if ( estimatedEnergy <= 0 )
		{
			Log.Info( $"{GetType().Name}: No more estimated energy, can't run Execute" );
			return;
		}

		PreExecute( estimatedEnergy, info );
	}

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

	/// <summary>
	/// Estimate the output energy of the provided connector
	/// </summary>
	/// <param name="providedConnector">Connector</param>
	/// <param name="otherNodeExecutor">Provided NodeExecutionEntity if the owner is not one</param>
	/// <param name="useMaxEnergy">Whether or not to estimate using the NodeExecutor MaxEnergy</param>
	/// <returns>Output energy (or null for unknown identifier / inestimable energy)</returns>
	public float? EstimateConnectorOutput( Connector providedConnector, NodeExecutionEntity otherNodeExecutor = null,
		bool useMaxEnergy = false )
	{
		NodeExecutionEntity nodeExecutor = otherNodeExecutor;

		if ( otherNodeExecutor == null )
		{
			if ( Owner is not NodeExecutionEntity ownerNodeExecutor )
			{
				Log.Error( "Can't estimate WeaponNodeEntity that's not owned by a NodeExecutionEntity" );
				return null;
			}

			nodeExecutor = ownerNodeExecutor;
		}

		// this is the entry node
		float? previousOutput;
		if ( nodeExecutor?.EntryNode != null && nodeExecutor.EntryNode == this )
		{
			previousOutput = useMaxEnergy ? nodeExecutor.MaxEnergy : nodeExecutor.Energy;
		}
		else
		{
			previousOutput = Previous?.EstimateConnectorOutput( PreviousConnectorId );
		}

		if ( previousOutput == null )
		{
			// The main way of estimating won't work!
			// Just return the absolute output...
			var absolute = providedConnector?.ConnectorAttribute?.EnergyOutAmount;
			if ( absolute != null && absolute.Value != 0.0f )
			{
				return absolute.Value;
			}

			return null;
		}

		var max = previousOutput.Value - Description.EnergyAttribute.Energy;
		var result = max;

		foreach ( var connector in Connectors.OrderBy( v => v?.ConnectorAttribute?.Order ?? 99 ) )
		{
			// We need to find out how much energy this connector would take for itself
			float usage;

			// If the connector doesn't use energy...
			if ( connector.ConnectorAttribute.EnergyOutAmount == 0.0f &&
			     connector.ConnectorAttribute.EnergyOutPercentage == 0.0f )
			{
				// If that connector is the one we're looking for...
				if ( connector == providedConnector )
				{
					return null; // ... just return null.
				}

				continue; // ... just skip.
			}

			// If the connector uses percentage based energy...
			if ( connector.ConnectorAttribute.EnergyOutPercentage != 0.0f )
			{
				usage = max * connector.ConnectorAttribute.EnergyOutPercentage;
			}
			else
			{
				// If the connector uses absolute energy...
				usage = MathF.Min( result, connector.ConnectorAttribute.EnergyOutAmount );
			}

			if ( connector == providedConnector )
			{
				if ( usage > max )
				{
					usage = max;
				}

				if ( usage < 0 )
				{
					usage = 0;
				}

				return usage;
			}

			result -= usage;
		}

		return null;
	}

	/// <summary>
	/// Estimate the output energy of the provided connector
	/// </summary>
	/// <param name="identifier">Connector ID</param>
	/// <param name="otherNodeExecutor">Provided NodeExecutionEntity if the owner is not one</param>
	/// <param name="useMaxEnergy">Whether or not to estimate using the NodeExecutor MaxEnergy</param>
	/// <returns>Output energy (or null for unknown identifier / inestimable energy)</returns>
	public float? EstimateConnectorOutput( string identifier, NodeExecutionEntity otherNodeExecutor = null,
		bool useMaxEnergy = false ) =>
		EstimateConnectorOutput( GetConnector( identifier ), otherNodeExecutor, useMaxEnergy );

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Game.IsClient )
		{
			return;
		}

		foreach ( var connector in Connectors )
		{
			connector.Delete();
		}
	}

	public WeaponNodeDescription Description
	{
		get;
	}

	protected WeaponNodeEntity()
	{
		Transmit = TransmitType.Owner;
		Description = WeaponNodeDescription.Get( GetType() );

		if ( Game.IsClient )
		{
			return;
		}

		foreach ( var nextAttribute in Description.ConnectorAttributes )
		{
			// todo: _connectors could be incorrect after a hotload. don't do this here maybe?
			Log.Info( $"Adding connector {nextAttribute.Identifier} to {this}" );
			Connectors.Add( new Connector( nextAttribute, this ) { Owner = this, Parent = this } );
		}
	}

	[Net]
	public Guid Uid

	{
		get;
		set;
	} = Guid.NewGuid();
}
