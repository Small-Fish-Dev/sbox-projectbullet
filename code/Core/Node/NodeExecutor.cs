using Sandbox;

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

	/// <summary>
	/// Display description of this execution entity
	/// todo: should this be done with reflection?
	/// </summary>
	public virtual string UsageInfo => "Unknown Description";

	/// <summary>
	/// <see cref="InputButton"/> used to activate this executor
	/// </summary>
	protected virtual InputButton InputButton => InputButton.PrimaryAttack;

	public Player Player => Owner as Player;

	/// <summary>
	/// First node to run / execute when this executor activates
	/// </summary>
	[Net]
	public WeaponNode EntryNode { get; set; }

	/// <summary>
	/// Current energy value of this executor
	/// </summary>
	[Net, Predicted]
	public float Energy { get; set; }

	/// <summary>
	/// Minimum energy needed for this executor to execute
	/// </summary>
	[Net]
	private float MinimumEnergy { get; set; }

	/// <summary>
	/// If this is false the user needs to reload to gain energy
	/// </summary>
	protected virtual bool AutomaticEnergyGain => false;

	protected virtual float EnergyGain => 38f;

	/// <summary>
	/// Time until this executor can execute again
	/// </summary>
	[Net, Predicted]
	private TimeUntil TimeUntilAction { get; set; }

	/// <summary>
	/// Whether or not this executor is reloading
	/// </summary>
	[Net, Predicted]
	private bool IsReloading { get; set; }

	public Vector2? LastEditorPos = null;

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
			if ( !(Energy < MinimumEnergy) || AutomaticEnergyGain || IsReloading )
			{
				return;
			}

			IsReloading = true;
			BeginReloadShared();

			return;
		}

		using ( LagCompensation() )
		{
			TimeUntilAction = ActionDelay;
			PerformAction( cl );
		}
	}

	private void BeginReloadShared()
	{
		BeginReloadClient();
		BeginReload();
	}

	[ClientRpc]
	public void BeginReloadClient()
	{
		BeginReload();
	}

	private void EndReloadShared()
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

	protected void ExecuteEntryNode( ExecuteInfo info ) => EntryNode?.PreExecute( Energy, info );

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
	private float? EstimateMinimumEnergy()
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

			var savedResult = result;

			foreach ( var connector in node.Connectors )
			{
				if ( connector.WeaponNode == null )
				{
					continue;
				}

				hasPopulatedConnector = true;
				CalcPath( connector.WeaponNode, output );

				if ( savedResult == null || result > savedResult )
				{
					savedResult = result; // hack: we NEED the worst case scenario (order etc.)
					// actually, i'm not even sure if we need the hack....
				}
			}

			result = savedResult;

			if ( hasPopulatedConnector || node is not IGoalNode )
			{
				return;
			}

			// This node has no connectors and it's a goal node!
			if ( result > output || result == null )
			{
				result = output;
			}
		}

		CalcPath( EntryNode, 0.0f );

		return result;
	}

	/// <summary>
	/// Update & estimate minimum energy for this executor
	/// </summary>
	public void UpdateMinimumEnergy() => MinimumEnergy = EstimateMinimumEnergy() ?? 0.0f;
}
