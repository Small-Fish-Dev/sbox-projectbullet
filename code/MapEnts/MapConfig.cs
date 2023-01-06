using Editor;
using Sandbox;

namespace ProjectBullet.MapEnts;

/// <summary>
/// Map configuration entity
/// </summary>
[Library( "pb_cfg" ), HammerEntity]
[Title( "Map Configuration" ), Category( "Gameplay" ), Icon( "place" )]
public partial class MapConfig : Entity
{
	public static MapConfig Instance;
	
	public MapConfig()
	{
		Transmit = TransmitType.Always;
		Instance = this;
	}
	
	[Net]
	[Property( Title = "Enable FFA" )]
	public bool IsFreeForAll { get; set; }

	[Net]
	[Property( Title = "Enable Workshop Anywhere" )]
	public bool EnableWorkshopAnywhere { get; set; }
}
