using System;
using System.Collections.Generic;
using ProjectBullet.UI.Editor;

namespace ProjectBullet.Weapons;

public struct SerializedPartNode
{
	public struct Output
	{
		public Guid Target { get; set; }
		public string Name { get; set; }
	}

	public Guid ItemUid { get; set; }
	public List<Output> Outputs { get; set; }
	public List<SerializedPartNode> Parts { get; set; }

	/// <summary>
	/// Create SerializedPartNode from UI editor <see cref="Node"/>
	/// </summary>
	/// <param name="editorNode">UI editor node</param>
	public SerializedPartNode( Node editorNode )
	{
		ItemUid = editorNode?.PartInventoryItem?.Uid ?? Guid.Empty;
		Parts = new();
		Outputs = new();

		foreach ( var outputHandle in editorNode.Outputs )
		{
			var nodeOutput = ((NodeOutput)outputHandle);

			if ( !nodeOutput.HasTarget )
			{
				continue;
			}

			var output = new Output();
			output.Name = nodeOutput.OutputDescription.Name;

			var targetNode = nodeOutput.Target.Node;
			output.Target = targetNode.PartInventoryItem.Uid;

			Outputs.Add( output );
			Parts.Add( new SerializedPartNode( targetNode ) );
		}
	}
}
