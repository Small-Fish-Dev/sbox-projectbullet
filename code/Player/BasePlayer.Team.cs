using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Player;

/// <summary>
/// Basic team impl
/// </summary>
public abstract partial class BasePlayer : AnimatedEntity
{
	public enum PlayerTeam
	{
		None,
		TeamOne,
		TeamTwo
	}

	private readonly List<KeyValuePair<PlayerTeam, string>> _teamTagPairs = new()
	{
		new KeyValuePair<PlayerTeam, string>( PlayerTeam.TeamOne, "team_one" ),
		new KeyValuePair<PlayerTeam, string>( PlayerTeam.TeamTwo, "team_two" )
	};

	private PlayerTeam GetTeam() =>
		(from pair in _teamTagPairs where Tags.Has( pair.Value ) select pair.Key).FirstOrDefault();

	private void SetTeam( PlayerTeam team )
	{
		foreach ( var pair in _teamTagPairs )
		{
			if ( pair.Key == team )
			{
				Tags.Add( pair.Value );
				continue;
			}

			Tags.Remove( pair.Value );
		}
	}

	public PlayerTeam Team
	{
		get => GetTeam();
		set => SetTeam( value );
	}

	public bool IsOnOppositeTeam
	{
		get
		{
			if ( IsLocalPawn )
			{
				return false;
			}

			if ( Team == PlayerTeam.None )
			{
				return false;
			}

			var localTeam = (Game.LocalPawn as BasePlayer)?.GetTeam();
			return Team != localTeam;
		}
	}
}
