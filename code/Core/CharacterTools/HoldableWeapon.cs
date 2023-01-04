using Sandbox;

namespace ProjectBullet.Core.CharacterTools;

public partial class HoldableWeapon : AnimatedEntity
{
	protected virtual string ModelPath => null;
	protected virtual string ViewModelPath => null;

	public class ExtendedViewModel : BaseViewModel
	{
		public override void PlaceViewmodel() { }
	}

	public ExtendedViewModel ViewModelEntity { get; set; }

	[Net] private Player Player { get; set; }

	protected HoldableWeapon( Player player )
	{
		Player = player;
	}

	protected HoldableWeapon() => Game.AssertClient();

	public override void Spawn()
	{
		base.Spawn();

		if ( !string.IsNullOrEmpty( ModelPath ) )
		{
			SetModel( ModelPath );
		}

		SetParent( Player, true );
		Owner = Player;

		EnableAllCollisions = false;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		SetParent( Player, true );
		Owner = Player;

		CreateViewModel();
	}

	/// <summary>
	/// Create the viewmodel. You can override this in your base classes if you want
	/// to create a certain viewmodel entity.
	/// </summary>
	protected virtual void CreateViewModel()
	{
		Game.AssertClient();

		if ( !IsLocalPawn )
		{
			return;
		}

		if ( string.IsNullOrEmpty( ViewModelPath ) )
		{
			return;
		}

		ViewModelEntity = new ExtendedViewModel { Owner = Owner, EnableViewmodelRendering = true };
		ViewModelEntity.SetModel( ViewModelPath );
	}

	/// <summary>
	/// We're done with the viewmodel - delete it
	/// </summary>
	protected virtual void DestroyViewModel()
	{
		if ( !IsLocalPawn )
		{
			return;
		}

		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Game.IsClient )
		{
			DestroyViewModel();
		}
	}

	/// <summary>
	/// Just update the viewmodel positioning
	/// </summary>
	public virtual void UpdateViewModel()
	{
		if ( !IsLocalPawn )
		{
			return;
		}

		ViewModelEntity.Position = Camera.Position;
		ViewModelEntity.Rotation = Camera.Rotation;
	}

	public virtual void Animate( ref CitizenAnimationHelper animationHelper )
	{
		Player.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
		Player.SetAnimParameter( "holdtype_handedness", (int)CitizenAnimationHelper.Hand.Both );
		Player.SetAnimParameter( "aim_headaim_body_weight_weight", 1.0f );
	}

	/// <summary>
	/// Utility - return the entity we should be spawning particles from etc
	/// </summary>
	public virtual ModelEntity EffectEntity =>
		ViewModelEntity.IsValid() && IsFirstPersonMode ? ViewModelEntity : this;
}
