using Sandbox;

namespace ProjectBullet;

public partial class MovementController
{
	public enum FrictionLevels
	{
		Normal,
		Skate,
		Sticky,
		Floating
	}

	private BasePlayer BasePlayer;

	public MovementController( BasePlayer basePlayer ) => BasePlayer = basePlayer;

	/// <summary>
	/// Called when player dies / etc
	/// </summary>
	public virtual void Reset()
	{
	}

	/// <summary>
	/// Called on Simulate when player is alive
	/// </summary>
	public virtual void Simulate()
	{
		Start();

		End();
	}

	/// <summary>
	/// Called on FrameSimulate when player is alive
	/// </summary>
	public virtual void FrameSimulate()
	{
		Start();
		
		End();
	}
}
