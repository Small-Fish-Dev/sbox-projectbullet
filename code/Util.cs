namespace ProjectBullet;

public static class Util
{
	// note(lotuspar): maybe AssertClient here?
	public static Core.Player LocalPlayer => Sandbox.Game.LocalPawn as Core.Player;
	public static Core.Shop.Inventory LocalInventory => LocalPlayer.Inventory;
	public static Core.PlayerTeam LocalTeam => LocalPlayer.Team;
}
