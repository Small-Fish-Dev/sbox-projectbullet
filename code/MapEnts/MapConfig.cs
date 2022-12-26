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
	[Net]
	[Property( Title = "Enable FFA" )]
	public bool IsFreeForAll { get; set; }

	[Property( Title = "Enable Workshop Anywhere" )]
	public bool EnableWorkshopAnywhere { get; set; }
}
