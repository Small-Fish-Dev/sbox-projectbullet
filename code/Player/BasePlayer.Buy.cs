using ProjectBullet.Core;
using ProjectBullet.UI.Editor;
using Sandbox;

namespace ProjectBullet.Player;

public abstract partial class BasePlayer
{
	[Net, Change] public bool CanUseEditor { get; set; } = true;

	private GraphVisualizer _nodeGraph;

	private void OnCanUseEditorChanged()
	{
		Events.Client.Workshop.RunEditorAccessChanged( CanUseEditor );

		if ( CanUseEditor != false || _nodeGraph == null )
		{
			return;
		}

		_nodeGraph.Delete( true );
		_nodeGraph = null;
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
