using Editor;
using ProjectBullet.Core;
using Sandbox;

namespace ProjectBullet.MapEnts;

public class SpawnMarker : Entity
{
	public virtual PlayerTeam Team => PlayerTeam.None;
}

[Library( "pb_teamone_spawn" ), HammerEntity]
[Title( "Team One Spawn" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Spawn marker for team two" )]
public class TeamOneSpawnMarker : SpawnMarker
{
	public override PlayerTeam Team => PlayerTeam.TeamOne;
}

[Library( "pb_teamtwo_spawn" ), HammerEntity]
[Title( "Team Two Spawn" ), Category( "Gameplay" ), Icon( "place" )]
[Description( "Spawn marker for team two" )]
public class TeamTwoSpawnMarker : SpawnMarker
{
	public override PlayerTeam Team => PlayerTeam.TeamTwo;
}
