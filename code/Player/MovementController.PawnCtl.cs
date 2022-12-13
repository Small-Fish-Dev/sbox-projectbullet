using Sandbox;

namespace ProjectBullet.Player;

/// <summary>
/// PawnController functionality
/// </summary>
public partial class MovementController
{
	public Entity Pawn { get; protected set; }
	public IClient Client { get; protected set; }
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
	public Vector3 Velocity { get; set; }
	public Rotation EyeRotation { get; set; }
	public Vector3 EyeLocalPosition { get; set; }
	public Vector3 BaseVelocity { get; set; }
	public Entity GroundEntity { get; set; }
	public Vector3 GroundNormal { get; set; }
	public Vector3 WishVelocity { get; set; }

	/// <summary>
	/// BasePlayer props. -> MovementController
	/// </summary>
	public void Start()
	{
		Position = BasePlayer.Position;
		Rotation = BasePlayer.Rotation;
		Velocity = BasePlayer.Velocity;
		EyeRotation = BasePlayer.EyeRotation;
		EyeLocalPosition = BasePlayer.EyeLocalPosition;
		BaseVelocity = BasePlayer.BaseVelocity;
		GroundEntity = BasePlayer.GroundEntity;
		WishVelocity = BasePlayer.Velocity;
	}

	/// <summary>
	/// MovementController props. -> BasePlayer
	/// </summary>
	public void End()
	{
		BasePlayer.Position = Position;
		BasePlayer.Velocity = Velocity;
		BasePlayer.Rotation = Rotation;
		BasePlayer.GroundEntity = GroundEntity;
		BasePlayer.BaseVelocity = BaseVelocity;
		BasePlayer.EyeLocalPosition = EyeLocalPosition;
		BasePlayer.EyeRotation = EyeRotation;
	}
}
