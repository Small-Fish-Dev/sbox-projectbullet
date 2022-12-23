using Sandbox;

namespace ProjectBullet.Characters;

public partial class Gunner : Core.Player
{
	[Net] public PrimaryFireController PrimaryFire { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PrimaryFire = new PrimaryFireController();

		RegisterNodeExecutors();
	}
}
