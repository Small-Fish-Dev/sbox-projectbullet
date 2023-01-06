using System;

namespace ProjectBullet.Core.Node;

public class UnknownConnectorException : Exception
{
	public UnknownConnectorException() { }

	public UnknownConnectorException( string identifier )
		: base( $"Unknown connector {identifier}" )
	{
	}

	public UnknownConnectorException( string identifier, Exception inner )
		: base( $"Unknown connector {identifier}", inner )
	{
	}
}
