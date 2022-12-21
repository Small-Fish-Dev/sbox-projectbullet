using System;
using Sandbox;

namespace ProjectBullet.Player.Strafe;

public class StrafeDuck : Duck
{
	public StrafeDuck( BasePlayerController controller ) : base( controller )
	{
	}

	public override void PreTick()
	{
		var wants = Input.Down( InputButton.Duck );

		if ( wants != IsActive )
		{
			if ( wants ) TryDuck();
			else TryUnDuck();
		}

		if ( IsActive )
		{
			Controller.SetTag( "ducked" );
		}
	}

	protected override void TryDuck()
	{
		var wasActive = IsActive;
		base.TryDuck();

		if ( !wasActive && IsActive )
		{
			Controller.Position += Vector3.Up * 14;
		}
	}

	protected override void TryUnDuck()
	{
		var wasActive = IsActive;
		base.TryUnDuck();

		if ( !wasActive || IsActive || Controller.GroundEntity != null )
		{
			return;
		}

		var distToGround = Controller.TraceBBox( Controller.Position, Controller.Position + Vector3.Down * 1000 )
			.Distance;
		var shift = MathF.Min( 14, distToGround );
		Controller.Position += Vector3.Down * shift;
	}

	public override float GetWishSpeed() => !IsActive ? -1 : 88;
}
