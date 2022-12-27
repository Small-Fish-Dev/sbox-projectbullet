namespace ProjectBullet.Core;

public abstract partial class Player
{
	public virtual float MaxHealth => 100.0f;
	protected virtual float BaseRespawnDelay => 5.0f;
	protected virtual string OutfitJson => null;
	protected virtual float NormalEyeHeight => 64.0f;
	protected virtual float DuckingEyeHeight => 40.0f;
	public virtual string DisplayTitle => "Player";
}
