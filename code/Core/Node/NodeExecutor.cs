﻿using Sandbox;

namespace ProjectBullet.Core.Node;

public partial class NodeExecutor : Entity
{
	public NodeExecutor() => Transmit = TransmitType.Owner;
	public virtual float MaxEnergy => 100.0f;
	public virtual float ActionDelay => 2.0f;
	public virtual bool AllowAutoAction => false;

	/// <summary>
	/// Display name of this execution entity
	/// todo: should this be done with reflection?
	/// </summary>
	public virtual string DisplayName => "Unknown Executor";

	public virtual InputButton InputButton => InputButton.PrimaryAttack;

	public Player Player => Owner as Player;

	/// <summary>
	/// First node to run
	/// </summary>
	[Net]
	public WeaponNode EntryNode { get; set; }

	[Net, Predicted] public float Energy { get; set; } = 0.0f;
	[Net] public float MinimumEnergy { get; private set; } = 0.0f;

	/// <summary>
	/// If this is false the user needs to reload to gain energy
	/// </summary>
	public virtual bool AutomaticEnergyGain => false;

	public virtual float EnergyGain => 38f;
	[Net, Predicted] public bool EnergyGainEnabled { get; set; }
	[Net, Predicted] public TimeUntil TimeUntilAction { get; private set; }
	[Net, Predicted] public bool IsReloading { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Energy = MaxEnergy;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( !Owner.IsValid )
		{
			return;
		}

		if ( Input.Pressed( InputButton.Reload ) && !IsReloading && !AutomaticEnergyGain && Energy != MaxEnergy )
		{
			IsReloading = true;
			BeginReloadShared();
		}

		if ( IsReloading || AutomaticEnergyGain )
		{
			Energy += EnergyGain * Time.Delta;
			if ( Energy > MaxEnergy )
			{
				EndReloadShared();
				IsReloading = false;
				Energy = MaxEnergy;
			}
		}

		if ( !CanPerformAction() )
		{
			if ( Energy < MinimumEnergy && !AutomaticEnergyGain && !IsReloading )
			{
				IsReloading = true;
				BeginReloadShared();
			}

			return;
		}

		using ( LagCompensation() )
		{
			TimeUntilAction = ActionDelay;
			PerformAction( cl );
		}
	}

	public void BeginReloadShared()
	{
		BeginReloadClient();
		BeginReload();
	}
	
	[ClientRpc]
	public void BeginReloadClient()
	{
		BeginReload();
	}
	
	public void EndReloadShared()
	{
		EndReloadClient();
		EndReload();
	}
	
	[ClientRpc]
	public void EndReloadClient()
	{
		EndReload();
	}
	
	protected virtual void BeginReload()
	{
		Energy = 0;
		
		Player.SetAnimParameter( "b_reload", true );
	}

	protected virtual void EndReload()
	{
		Player.SetAnimParameter( "b_reload", false );
	}

	protected void ExecuteEntryNode( ExecuteInfo info )
	{
		if ( EntryNode == null )
		{
			// Log.Warning( "ExecuteEntryNode used with no entry node" );
			return;
		}

		EntryNode.PreExecute( Energy, info );
	}

	protected virtual void PerformAction( IClient cl )
	{
	}

	protected virtual bool CanPerformAction()
	{
		if ( AllowAutoAction )
		{
			if ( !Input.Down( InputButton ) )
			{
				return false;
			}
		}
		else
		{
			if ( !Input.Pressed( InputButton ) )
			{
				return false;
			}
		}

		if ( IsReloading )
		{
			return false;
		}

		if ( !Owner.IsValid() )
		{
			return false;
		}

		if ( Energy < MinimumEnergy )
		{
			return false;
		}

		var delay = ActionDelay;
		if ( delay <= 0 )
		{
			return true;
		}

		return TimeUntilAction <= 0;
	}

	/// <summary>
	/// Attempt to use some energy
	/// </summary>
	/// <param name="amount">Amount to take</param>
	/// <returns>True if energy was deducted, false if not enough energy</returns>
	public bool UseEnergy( float amount )
	{
		if ( amount > Energy )
		{
			return false;
		}

		Energy -= amount;
		return true;
	}

	/// <summary>
	/// Estimate minimum energy required to use this executor.
	/// note: this is slow!! use sparingly
	/// </summary>
	/// <returns>Minimum energy or null</returns>
	public float? EstimateMinimumEnergy()
	{
		if ( EntryNode == null )
		{
			return null;
		}

		float? result = null;

		void CalcPath( WeaponNode node, float input )
		{
			var hasPopulatedConnector = false;
			var output = input;

			output += node.Description?.EnergyAttribute?.Energy ?? 0.0f;

			Log.Info( $"CalcPath ({node.GetType().Name}) - input {input}, output {output}" );

			float? savedResult = result;
			
			foreach ( var connector in node.Connectors )
			{
				if ( connector.WeaponNode != null )
				{
					hasPopulatedConnector = true;
					CalcPath( connector.WeaponNode, output );

					if ( savedResult == null || result > savedResult )
					{
						savedResult = result; // hack: we NEED the worst case scenario (order etc.)
						// actually, i'm not even sure if we need the hack....
					}
				}
			}

			result = savedResult;
			
			if ( !hasPopulatedConnector && node is IGoalNode )
			{
				// This node has no connectors and it's a goal node!
				if ( result > output || result == null )
				{
					result = output;
				}
			}
		}

		CalcPath( EntryNode, 0.0f );

		return result;
	}

	[Events.Server.Node.ConnectorChanged]
	public void OnConnectorChanged( Entity player )
	{
		if ( player == Player )
		{
			Log.Info( "estimating" );
			MinimumEnergy = EstimateMinimumEnergy() ?? 0.0f;
		}
	}
}