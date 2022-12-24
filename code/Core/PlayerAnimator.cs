using Sandbox;

namespace ProjectBullet.Core;

/// <summary>
/// Component to handle player animation - this is mostly from DevulTj's (sbox-template.fps) PlayerAnimator 
/// </summary>
public class PlayerAnimator : EntityComponent<Player>, ISingletonComponent
{
	public virtual void Simulate( IClient cl ) { }
	public virtual void FrameSimulate( IClient cl ) { }
}
