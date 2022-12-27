using System.Linq;
using ProjectBullet.MapEnts;
using Sandbox;

namespace ProjectBullet;

public static class Util
{
	// note(lotuspar): maybe AssertClient here?
	public static Core.Player LocalPlayer => Game.LocalPawn as Core.Player;
	public static Core.PersistentData LocalPersistent => LocalPlayer.Persistent;
	public static Core.PlayerTeam LocalTeam => LocalPlayer.Team;

	private static MapConfig _mapConfigCache;

	public static MapConfig MapConfig
	{
		get
		{
			_mapConfigCache ??= Entity.All.OfType<MapConfig>().SingleOrDefault();
			return _mapConfigCache;
		}
	}
}
