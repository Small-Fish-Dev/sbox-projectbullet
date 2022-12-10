using Sandbox;

namespace ProjectBullet.Core.Node;

public partial class NodeExecutionEntity : Entity
{
	public NodeExecutionEntity() => Transmit = TransmitType.Owner;
	public virtual float MaxEnergy => 100.0f;
	public virtual float ActionDelay => 2.0f;
	public virtual bool AllowAutoAction => false;

	/// <summary>
	/// Display name of this execution entity
	/// todo: should this be done with reflection?
	/// </summary>
	public virtual string DisplayName => "Unknown Executor";

	public virtual InputButton InputButton => InputButton.PrimaryAttack;

	public BasePlayer BasePlayer => Owner as BasePlayer;

	/// <summary>
	/// First node to run
	/// </summary>
	[Net]
	public WeaponNodeEntity EntryNode { get; set; }

	[Net, Predicted] public float Energy { get; private set; } = 0.0f;
	[Net, Predicted] public float MinimumEnergy { get; private set; } = 0.0f;
	[Net, Predicted] private TimeSince TimeSinceAction { get; set; }

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

		if ( !CanPerformAction() )
		{
			return;
		}

		using ( LagCompensation() )
		{
			TimeSinceAction = 0;
			PerformAction( cl );
		}
	}

	protected void ExecuteEntryNode( Entity target, Vector3 point )
	{
		if ( EntryNode == null )
		{
			Log.Warning( "ExecuteEntryNode used with no entry node" );
			return;
		}

		EntryNode.Execute( target, point );
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

		return TimeSinceAction > (1 / delay);
	}
}
