using System.Linq;
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
	public class PrimaryFireController : NodeExecutor
	{
		public override string DisplayName => "Primary Fire";
		public override float ActionDelay => 0.13f;
		public override InputButton InputButton => InputButton.PrimaryAttack;

		private float CalculateSpread()
		{
			var speed = BasePlayer.Velocity.Length;
			return speed switch
			{
				<= 50.0f => 0.0f,
				<= 200.0f => 0.3f,
				_ => speed <= 350.0f ? 1f : 1.4f
			};
		}

		protected override void PerformAction( IClient cl )
		{
			base.PerformAction( cl );

			(BasePlayer as Gunner)?.ShootEffect();

			Game.SetRandomSeed( Time.Tick );

			var ray = BasePlayer.AimRay;

			var forward = ray.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) *
			           (CalculateSpread() * 0.02f);
			forward = forward.Normal;

			var trace = Trace.Ray( ray.Position, ray.Position + forward * 5000 )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( BasePlayer );

			var result = trace.Run();

			//DebugOverlay.Circle( result.EndPosition + -ray.Forward, Rotation.LookAt( result.Normal ), 1.0f, Color.White,
			//3.0f );

			(BasePlayer as Gunner).PlaySound( "rust_pistol.shoot" );

			if ( Game.IsClient )
			{
				var pos = (BasePlayer as Gunner)?.ViewModelEntity.GetAttachment( "muzzle" );

				var system = Particles.Create( "particles/tracer.standard.vpcf" );
				system?.SetPosition( 0, pos.Value.Position );
				system?.SetPosition( 1, result.EndPosition );
			}

			ExecuteEntryNode( new ExecuteInfo()
				.UsingTraceResult( result )
				.WithAttacker( BasePlayer )
				.WithForce( 1, ray.Forward ) );
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

	public class SecondaryFireController : NodeExecutor
	{
		public override string DisplayName => "Secondary Fire";
		public override float ActionDelay => 0.01f;
		public override InputButton InputButton => InputButton.SecondaryAttack;
		public override bool AllowAutoAction => true;
		public override bool AutomaticEnergyGain => true;

		protected override void PerformAction( IClient cl )
		{
			base.PerformAction( cl );

			Log.Info( "hello from secondary fire" );
		}
	}

	public partial class UltraShiftController : NodeExecutor
	{
		public override string DisplayName => "Ultra Shift";
		public override float ActionDelay => 0.4f;
		public override InputButton InputButton => InputButton.Run;
		public override bool AutomaticEnergyGain => true;

		[Net, Predicted] public Projectile Instance { get; set; }

		public class Projectile : BasePhysics
		{
			private BasePlayer _player;
			private UltraShiftController _ctl;

			public Projectile( UltraShiftController ctl )
			{
				_player = ctl.BasePlayer;
				_ctl = ctl;

				Transmit = TransmitType.Always;
			}

			public Projectile() { }

			public override void Spawn()
			{
				base.Spawn();

				PhysicsEnabled = true;
				UsePhysicsCollision = true;
				SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Vector3.Zero, 32.0f );
			}

			[Event.Client.Frame]
			private void Frame()
			{
				DebugOverlay.Sphere( Position, 32.0f, Color.Magenta, 0.0f, true );
			}

			protected override void OnPhysicsCollision( CollisionEventData eventData )
			{
				if ( eventData.Other.Entity is BasePlayer )
				{
					return;
				}

				
				
				base.OnPhysicsCollision( eventData );
			}
		}

		[Event.Tick]
		private void HandlePhysicsTick()
		{
			if ( Instance == null )
			{
				return;
			}

			foreach ( var player in All.OfType<BasePlayer>() )
			{
				if ( !(player.Position.Distance( Position ) <= 32) )
				{
					continue;
				}

				var info = new ExecuteInfo { ImpactPoint = Instance.Position };
				ExecuteEntryNode( info
					.WithAttacker( BasePlayer )
					.WithForce( 1, BasePlayer.AimRay.Forward ) );
			}
		}

		protected override void PerformAction( IClient cl )
		{
			base.PerformAction( cl );

			if ( Instance != null )
			{
				/*var info = new ExecuteInfo { ImpactPoint = Instance.Position };
				ExecuteEntryNode( info
					.WithAttacker( BasePlayer )
					.WithForce( 1, BasePlayer.AimRay.Forward ) );*/

				if ( Game.IsClient )
				{
					return;
				}

				Instance.Delete();
			}
			else
			{
				if ( Game.IsClient )
				{
					return;
				}

				var forward = BasePlayer.AimRay.Forward;
				forward = forward.Normal;

				Instance = new Projectile( this )
				{
					Position = BasePlayer.EyePosition + forward * 32, Velocity = forward * 444
				};
			}
		}
	}

	[Net] public PrimaryFireController PrimaryFire { get; set; }
	[Net] public SecondaryFireController SecondaryFire { get; set; }
	[Net] public UltraShiftController UltraShift { get; set; }

	protected override NodeExecutor MainExecutor => PrimaryFire;

	public virtual string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public GunnerViewmodel ViewModelEntity { get; protected set; }

	public void ShootEffect()
	{
		if ( Game.IsServer )
		{
			return;
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void Spawn()
	{
		base.Spawn();

		PrimaryFire = new PrimaryFireController();
		//SecondaryFire = new SecondaryFireController();
		UltraShift = new UltraShiftController();

		RegisterNodeExecutors();
	}

	protected override void ClientRespawn()
	{
		base.ClientRespawn();

		if ( !IsLocalPawn )
		{
			return;
		}

		if ( Game.IsServer )
		{
			return;
		}

		ViewModelEntity =
			new GunnerViewmodel { Position = Position, Owner = Owner, EnableViewmodelRendering = true };
		ViewModelEntity.SetModel( ViewModelPath );
	}

	protected override void HandleDeath()
	{
		base.HandleDeath();

		if ( !IsLocalPawn )
		{
			return;
		}

		if ( Game.IsServer )
		{
			return;
		}

		Log.Info( "hi" );
		ViewModelEntity.Delete();
		ViewModelEntity = null;
	}

	protected override void CameraFrameSimulate()
	{
		base.CameraFrameSimulate();

		if ( !IsLocalPawn )
		{
			return;
		}

		ViewModelEntity?.UpdateCamera();

		Camera.Main.SetViewModelCamera( Camera.FieldOfView + 20.0f );
	}

	protected override void AnimationSimulate( CitizenAnimationHelper? providedAnimHelper = null )
	{
		var animHelper = new CitizenAnimationHelper( this );

		base.AnimationSimulate( animHelper );

		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		animHelper.Handedness = CitizenAnimationHelper.Hand.Both;
		animHelper.AimBodyWeight = 1.0f;
	}
}
