using Sandbox;

namespace ProjectBullet.Core;

public abstract partial class Player
{
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		if ( Input.StopProcessing )
		{
			return;
		}

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch is > 90f or < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles + look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
	}
}
