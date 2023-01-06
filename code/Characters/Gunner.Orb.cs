using ProjectBullet.Core.Node;
using Sandbox;
using Player = ProjectBullet.Core.Player;

namespace ProjectBullet.Characters;

public partial class Gunner
{
	public partial class OrbExecutor : NodeExecutor
	{
		public override string DisplayName => "Slow Orb";
		public override string UsageInfo => "Shoots a slow moving orb that can be pressed again to activate";
		public override float ActionDelay => 0.5f;
		protected override bool AutomaticEnergyGain => true;
		protected override InputButton InputButton => InputButton.Use;

		public partial class Projectile : ModelEntity
		{
			[Net] public bool Complete { get; private set; }
			private TraceResult? _traceResult;
			private Entity _attacker;

			public override void Spawn()
			{
				base.Spawn();

				SetModel( "models/dev/sphere.vmdl" );

				Scale = 0.1f;
			}

			public override void Simulate( IClient cl )
			{
				base.Simulate( cl );

				if ( Complete )
				{
					return;
				}

				_attacker = Owner;

				var velocity = Rotation.Forward * 200;

				var point = Position + velocity * Time.Delta;

				var result = Trace.Ray( Position, point )
					.UseHitboxes()
					.Ignore( Owner )
					.Ignore( this )
					.Size( 2.0f )
					.Run();

				if ( result.Hit )
				{
					Complete = true;
					Position = result.EndPosition + Rotation.Forward * -1;

					_traceResult = result;
					
					// Stick to target
					SetParent( result.Entity, result.Bone );
					Owner = null;
				}
				else
				{
					Position = point;
				}
			}

			public ExecuteInfo? CreateExecuteInfo()
			{
				var info = new ExecuteInfo { ImpactPoint = Position }
					.WithAttacker( (Player)_attacker );

				if ( _traceResult == null )
				{
					return info;
				}

				info = info.WithForce( 1, Rotation.Forward * 256 );
				if ( _traceResult.Value.Entity is not { IsValid: true } )
				{
					return info;
				}

				info.Normal = _traceResult.Value.Normal;
				info.Victim = _traceResult.Value.Entity;
				info.BoneIndex = _traceResult.Value.Bone;
				info.Hitbox = _traceResult.Value.Hitbox;

				return info;
			}
		}

		[Net, Predicted] private Projectile Instance { get; set; }

		protected override void PerformAction( IClient cl )
		{
			base.PerformAction( cl );

			Game.SetRandomSeed( Time.Tick );

			if ( Game.IsClient )
			{
				return;
			}

			if ( Instance != null )
			{
				var info = Instance.CreateExecuteInfo();

				if ( info == null )
				{
					Log.Warning( "Instance ExecuteInfo == null" );
					return;
				}

				ExecuteEntryNode( info.Value );

				Instance.Delete();
				Instance = null;
			}
			else
			{
				Instance = new Projectile
				{
					Position = Player.EyePosition + (Vector3.Left * 2) + Vector3.Down,
					Rotation = Player.EyeRotation,
					Owner = Player
				};
			}
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );

			Instance?.Simulate( cl );
		}
	}
}
