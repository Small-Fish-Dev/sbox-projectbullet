using System.Linq;
using ProjectBullet.Classes;
using ProjectBullet.UI.Editor;
using Sandbox;
using Sandbox.UI;

namespace ProjectBullet.Player;

public abstract partial class BasePlayer
{
	[Net, Change] public bool CanUseEditor { get; set; } = false;

	private GraphVisualizer _nodeGraph;

	public void OnCanUseEditorChanged()
	{
		if ( CanUseEditor == false && _nodeGraph != null )
		{
			_nodeGraph.Delete( true );
			_nodeGraph = null;
		}
	}

	[ConCmd.Client( "pb_editor" )]
	public static void ToggleEditor()
	{
		if ( Game.LocalPawn is not BasePlayer localPlayer )
		{
			return;
		}

		if ( localPlayer._nodeGraph == null )
		{
			if ( !localPlayer.CanUseEditor )
			{
				Log.Info( "Can't open editor outside of buy zone" );
				return;
			}

			localPlayer._nodeGraph = new GraphVisualizer( localPlayer.MainExecutor );
			Game.RootPanel.AddChild( localPlayer._nodeGraph );
		}
		else
		{
			localPlayer._nodeGraph.Delete( true );
			localPlayer._nodeGraph = null;
		}
	}
}
