using ProjectBullet.Core.Gameplay;
using Sandbox;

namespace ProjectBullet.Core;

public abstract class PlayerComponent : EntityComponent<Player>
{
	public virtual void Simulate( IClient cl ) { }
	public virtual void FrameSimulate( IClient cl ) { }

	public Controller Controller => Entity.Controller;
}
