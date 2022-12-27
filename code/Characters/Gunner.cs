using ProjectBullet.Core.CharacterTools;
using Sandbox;
using Player = ProjectBullet.Core.Player;

namespace ProjectBullet.Characters;

public partial class Gunner : Core.Player
{
	[Net] public PrimaryFireController PrimaryFire { get; set; }

	protected override string OutfitJson =>
		"[{\"id\":-1293797531},{\"id\":853300482},{\"id\":-691668871},{\"id\":-2069474809},{\"id\":-1696290982},{\"id\":1240466157},{\"id\":105676996},{\"id\":-100519886}]";

	public override string DisplayTitle => "Gunner";

	public class PistolWeapon : HoldableWeapon
	{
		protected override string ModelPath => "weapons/rust_pistol/rust_pistol.vmdl";
		protected override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public PistolWeapon( Player player ) : base( player )
		{
		}

		public PistolWeapon() { }
	}

	public override void Spawn()
	{
		base.Spawn();

		PrimaryFire = new PrimaryFireController();

		RegisterNodeExecutors();
	}

	protected override void CreateHoldableWeapon()
	{
		base.CreateHoldableWeapon();

		HoldableWeapon = new PistolWeapon( this );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		HoldableWeapon?.UpdateViewModel();

		Camera.Main.SetViewModelCamera( Camera.FieldOfView + 8.0f );
	}
}
