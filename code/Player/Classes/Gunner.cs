using Sandbox;
using ProjectBullet.Core.Node;
using ProjectBullet.Player;

namespace ProjectBullet.Classes;

public class GunnerViewmodel : BaseViewModel
{
	private float _walkBob = 0;

	public void UpdateCamera()
	{
		var rotationDistance = Rotation.Distance( Camera.Rotation );

		Position = Camera.Position;
		Rotation = Rotation.Lerp( Rotation, Camera.Rotation, RealTime.Delta * rotationDistance * 1.1f );

		if ( Game.LocalPawn.LifeState == LifeState.Dead )
			return;

		var speed = Game.LocalPawn.Velocity.Length.LerpInverse( 0, 400 );
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;

		if ( Game.LocalPawn.GroundEntity != null )
		{
			_walkBob += Time.Delta * 2.0f * speed;
		}
	}
}

public partial class Gunner : BasePlayer
{
	public class PrimaryFireController : NodeExecutionEntity
	{
		public override string DisplayName => "Primary Fire";
		public override float ActionDelay => 0.15f;
		public override InputButton InputButton => InputButton.PrimaryAttack;

		protected override void PerformAction( IClient cl )
		{
			base.PerformAction( cl );

			(BasePlayer as Gunner)?.ShootEffect();

			var ray = BasePlayer.AimRay;

			var trace = Trace.Ray( ray, 8096 )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( BasePlayer );

			ExecuteEntryNode( new ExecuteInfo()
				.UsingTraceResult( trace.Run() )
				.WithAttacker( BasePlayer ) );
		}

		protected override void BeginReload()
		{
			base.BeginReload();

			if ( Game.IsServer )
			{
				return;
			}

			(BasePlayer as Gunner)?.ViewModelEntity.SetAnimParameter( "reload", true );
		}

		protected override void EndReload()
		{
			base.EndReload();

			if ( Game.IsServer )
			{
				return;
			}

			(BasePlayer as Gunner)?.ViewModelEntity.SetAnimParameter( "reload", false );
		}
	}

	public class SecondaryFireController : NodeExecutionEntity
	{
		public override string DisplayName => "Secondary Fire";
		public override float ActionDelay => 4.0f;
		public override InputButton InputButton => InputButton.SecondaryAttack;

		protected override void PerformAction( IClient cl )
		{
			base.PerformAction( cl );

			Log.Info( "hello from secondary fire" );
		}
	}

	public class UltraShiftController : NodeExecutionEntity
	{
		public override string DisplayName => "Ultra Shift";
		public override float ActionDelay => 7.0f;
		public override InputButton InputButton => InputButton.Run;

		protected override void PerformAction( IClient cl )
		{
			base.PerformAction( cl );

			Log.Info( "hello from ultra shift" );
		}
	}

	[Net] public PrimaryFireController PrimaryFire { get; set; }
	[Net] public SecondaryFireController SecondaryFire { get; set; }
	[Net] public UltraShiftController UltraShift { get; set; }

	public virtual string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public GunnerViewmodel ViewModelEntity { get; protected set; }

	public void ShootEffect()
	{
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void Spawn()
	{
		base.Spawn();

		PrimaryFire = new PrimaryFireController();
		SecondaryFire = new SecondaryFireController();
		UltraShift = new UltraShiftController();

		RegisterNodeExecutors();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		if ( !IsLocalPawn )
		{
			return;
		}

		ViewModelEntity =
			new GunnerViewmodel { Position = Position, Owner = Owner, EnableViewmodelRendering = true };
		ViewModelEntity.SetModel( ViewModelPath );
	}

	protected override void CameraFrameSimulate()
	{
		base.CameraFrameSimulate();

		if ( IsLocalPawn )
		{
			ViewModelEntity?.UpdateCamera();

			Camera.Main.SetViewModelCamera( Camera.FieldOfView + 20.0f );
		}

		var ray = AimRay;

		var trace = Trace.Ray( ray, 8096 )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "npc", "glass" )
			.Ignore( this );

		var result = trace.Run();

		DebugOverlay.Circle( result.EndPosition, Rotation.Identity, 3.0f, Color.Red );
	}
}
