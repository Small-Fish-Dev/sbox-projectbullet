using Sandbox;

namespace ProjectBullet.Core.Effects;

public abstract partial class BaseEffect : EntityComponent<Player>
{
	/// <summary>
	/// The entity that created this component - used to identify instances
	/// </summary>
	[Net]
	public Entity Creator { get; private set; }

	/// <summary>
	/// Time until component is removed - 0 will keep the component alive forever
	/// </summary>
	protected virtual float Lifespan => 0;

	private readonly TimeUntil? _timeUntilComplete;

	protected BaseEffect( Entity creator )
	{
		Creator = creator;
		if ( Lifespan != 0 )
		{
			_timeUntilComplete = Lifespan;
		}
	}

	protected BaseEffect() => Game.AssertClient();
	
	[Event.Tick]
	private void OnTickLifespan()
	{
		if ( _timeUntilComplete != null && _timeUntilComplete.Value )
		{
			Remove();
		}
	}
}
