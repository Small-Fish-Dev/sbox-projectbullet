using Sandbox;

namespace ProjectBullet.Player;

public abstract partial class BasePlayer
{
	protected virtual void CameraFrameSimulate()
	{
		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = 103.0f;
		Camera.Position = EyePosition;
		Camera.FirstPersonViewer = this;
		Camera.ZNear = 1f;
		Camera.ZFar = 5000.0f;
	}
}
