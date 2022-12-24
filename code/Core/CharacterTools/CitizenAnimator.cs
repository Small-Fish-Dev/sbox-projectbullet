using Sandbox;

namespace ProjectBullet.Core.CharacterTools;

/// <summary>
/// An extension of the player animator, fit for Facepunch's Citizen Anim Graph
/// </summary>
public partial class CitizenAnimator : PlayerAnimator
{
	public override void Simulate( IClient cl )
	{
		var player = Entity;
		var controller = player.Controller;
		var animHelper = new CitizenAnimationHelper( player );

		animHelper.WithWishVelocity( controller.GetWishVelocity() );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( player.EyePosition + player.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = player.EyeRotation;
		animHelper.FootShuffle = 0f;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, 1 - controller.CurrentEyeHeight.Remap( 30, 72, 0, 1 ).Clamp( 0, 1 ), Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && cl.IsValid()) ? cl.Voice.LastHeard < 0.5f ? cl.Voice.CurrentLevel: 0.0f : 0.0f;
		animHelper.IsGrounded = controller.GroundEntity != null;
		animHelper.IsSwimming = player.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		var weapon = player.HoldableWeapon;
		if ( !weapon.IsValid() )
		{
			return;
		}

		weapon.Animate( ref animHelper );
	}
}
