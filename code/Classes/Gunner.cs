using Sandbox;
using ProjectBullet.Core.Node;

namespace ProjectBullet.Classes;

public partial class Gunner : BasePlayer
{
	private class PrimaryFireController : NodeExecutionEntity
	{
		public override float ActionDelay => 4.0f;
		public override InputButton InputButton => InputButton.PrimaryAttack;

		protected override void PerformAction( Client cl )
		{
			base.PerformAction( cl );

			Log.Info( "hello from primary fire" );
		}
	}

	[Net] private PrimaryFireController PrimaryFire { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PrimaryFire = new PrimaryFireController();
		PrimaryFire.Owner = this;
		
		ActiveNodeExecutors.Add( PrimaryFire );
	}
}
