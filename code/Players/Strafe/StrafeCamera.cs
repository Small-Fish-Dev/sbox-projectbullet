using Sandbox;

namespace ProjectBullet.Players.Strafe;

public class StrafeCamera : CameraMode
{
	public override void Update()
	{
		var target = Local.Pawn;

		Position = target.EyePosition;
		Rotation = target.EyeRotation;

		Viewer = target;
	}
}
