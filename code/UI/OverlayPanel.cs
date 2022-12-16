using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.UI;

public class OverlayPanel
{
	public class SceneOverlayObject : SceneCustomObject
	{
		public SceneOverlayObject(SceneWorld sceneWorld) : base(sceneWorld)
		{
		}

		public override void RenderSceneObject()
		{
			base.RenderSceneObject();
			
			
		}
	}

	public SceneOverlayObject SceneObject { get; private set; }

	public Transform Transform
	{
		get => SceneObject.Transform;
		set => SceneObject.Transform = value;
	}

	public Vector3 Position
	{
		get => Transform.Position;
		set => Transform = Transform.WithPosition( value );
	}

	public Rotation Rotation
	{
		get => Transform.Rotation;
		set => Transform = Transform.WithRotation( value );
	}

	public float WorldScale
	{
		get => Transform.Scale;
		set => Transform = Transform.WithScale( value );
	}
}
