using ProjectBullet.Core.Shop;
using Sandbox;

namespace ProjectBullet.Core;

public static partial class Events
{
	private const string ClientNodeConnectorChangedEvent = "clientconnectorchanged";
	private const string ClientWorkshopMoneyChangedEvent = "clientmoneychanged";
	private const string ClientWorkshopNewItemEvent = "clientnewitem";
	private const string ClientWorkshopEditorAccessChangedEvent = "clienteditoraccesschanged";
	private const string ServerNodeConnectorChangedEvent = "serverconnectorchanged";

	public static class Client
	{
		public static class Node
		{
			public class ConnectorChangedAttribute : EventAttribute
			{
				public ConnectorChangedAttribute() : base( ClientNodeConnectorChangedEvent ) { }
			}

			public static void RunConnectorChanged()
			{
				Game.AssertClient();
				Event.Run( ClientNodeConnectorChangedEvent );
			}
		}

		public static class Workshop
		{
			public class MoneyChangedAttribute : EventAttribute
			{
				public MoneyChangedAttribute() : base( ClientWorkshopMoneyChangedEvent ) { }
			}

			public class NewItemAttribute : EventAttribute
			{
				public NewItemAttribute() : base( ClientWorkshopNewItemEvent ) { }
			}

			public class EditorAccessChangedAttribute : EventAttribute
			{
				public EditorAccessChangedAttribute() : base( ClientWorkshopEditorAccessChangedEvent ) { }
			}

			public static void RunMoneyChanged( int value )
			{
				Game.AssertClient();
				Event.Run( ClientWorkshopMoneyChangedEvent, value );
			}

			public static void RunNewItem( IInventoryItem item )
			{
				Game.AssertClient();
				Event.Run( ClientWorkshopNewItemEvent, item );
			}

			public static void RunEditorAccessChanged( bool value )
			{
				Game.AssertClient();
				Event.Run( ClientWorkshopEditorAccessChangedEvent, value );
			}
		}
	}

	public static class Server
	{
		public static class Node
		{
			public class ConnectorChangedAttribute : EventAttribute
			{
				public ConnectorChangedAttribute() : base( ServerNodeConnectorChangedEvent ) { }
			}

			public static void RunConnectorChanged( Entity entity )
			{
				Game.AssertServer();
				Event.Run( ServerNodeConnectorChangedEvent, entity );
			}
		}
	}

	public static partial class Shared
	{
		public static class Node
		{
			public static void RunConnectorChanged( Entity entity )
			{
				Game.AssertServer();
				Server.Node.RunConnectorChanged( entity );
				RpcRunConnectorChanged( To.Single( entity ) );
			}
		}

		public static class Workshop
		{
			public static void RunNewItem( To to, Entity entity )
			{
				Game.AssertServer();
				RpcRunNewItem( to, entity );
			}
		}
	}

	[ClientRpc]
	public static void RpcRunConnectorChanged()
	{
		Client.Node.RunConnectorChanged();
	}

	[ClientRpc]
	public static void RpcRunNewItem( Entity entity )
	{
		Client.Workshop.RunNewItem( (IInventoryItem)entity );
	}
}
