namespace ProjectBullet.Core;

public abstract partial class Player
{
	protected virtual float MaxHealth => 100.0f;
	protected virtual float BaseRespawn => 5.0f;
	protected virtual string OutfitJson => null;
}
