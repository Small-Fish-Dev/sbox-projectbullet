using Sandbox;

namespace ProjectBullet.Core;

public abstract partial class Player
{
	private ModelEntity _lastRagdoll;

	[ClientRpc]
	private void CreateRagdoll( Vector3 velocity, Vector3 forcePos, Vector3 force, int bone, bool bullet, bool blast )
	{
		var ent = new ModelEntity
		{
			Position = Position,
			Rotation = Rotation,
			Scale = Scale,
			UsePhysicsCollision = true,
			EnableAllCollisions = true,
			SurroundingBoundsMode = SurroundingBoundsType.Physics,
			RenderColor = RenderColor,
			PhysicsGroup = { Velocity = velocity },
			PhysicsEnabled = true
		};

		ent.Tags.Add( "ragdoll", "solid", "debris" );

		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.CopyMaterialOverrides( this );
		ent.TakeDecalsFrom( this );

		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) )
			{
				continue;
			}

			if ( child is not ModelEntity e )
			{
				continue;
			}

			var clothing = new ModelEntity { RenderColor = e.RenderColor };

			clothing.SetModel( e.GetModelName() );
			clothing.SetParent( ent, true );

			clothing.CopyBodyGroups( e );
			clothing.CopyMaterialGroup( e );
		}

		if ( bullet )
		{
			var body = bone > 0 ? ent.GetBonePhysicsBody( bone ) : null;

			if ( body != null )
			{
				body.ApplyImpulseAt( forcePos, force * body.Mass );
			}
			else
			{
				ent.PhysicsGroup.ApplyImpulse( force );
			}
		}

		if ( blast )
		{
			if ( ent.PhysicsGroup != null )
			{
				ent.PhysicsGroup.AddVelocity( (Position - (forcePos + Vector3.Down * 100.0f)).Normal *
				                              (force.Length * 0.2f) );
				var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
				ent.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
			}
		}

		_lastRagdoll = ent;
		ent.DeleteAsync( BaseRespawnDelay * 1.5f );
	}
}
