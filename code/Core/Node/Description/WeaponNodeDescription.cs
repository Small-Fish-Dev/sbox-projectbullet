using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Core.Node.Description;

public class WeaponNodeDescription : IStaticDescription
{
	private static readonly List<WeaponNodeDescription> Instances = new();
	public static ReadOnlyCollection<WeaponNodeDescription> All => Instances.AsReadOnly();

	private string _name;

	public NodeAttribute NodeAttribute { get; private set; }
	public EnergyAttribute EnergyAttribute { get; private set; }
	public IEnumerable<ConnectorAttribute> ConnectorAttributes { get; private set; }

	public TypeDescription TypeDescription { get; private set; }
	public Type TargetType => TypeDescription.TargetType;

	public WeaponNodeDescription( Type type )
	{
		// Find TypeDescription for provided type
		var typeDescription = TypeLibrary.GetType( type );
		if ( typeDescription == null )
		{
			// todo: write custom exception
			throw new Exception( $"No type description found for {type.Name}, {type}" );
		}

		UseNewTypeDescription( typeDescription );

		Event.Register( this );
	}

	public WeaponNodeDescription( TypeDescription typeDescription )
	{
		UseNewTypeDescription( typeDescription );

		Event.Register( this );
	}

	~WeaponNodeDescription() => Event.Unregister( this );

	/// <summary>
	/// Register all WeaponNodeEntities
	/// </summary>
	public static void InitAll()
	{
		foreach ( var typeDescription in TypeLibrary.GetTypes<WeaponNodeEntity>() )
		{
			Instances.Add( new WeaponNodeDescription( typeDescription ) );
		}
	}

	private void OnHotload() => UseNewTypeDescription
		( TypeLibrary.GetType( _name ) );

	private void UseNewTypeDescription( TypeDescription typeDescription )
	{
		TypeDescription = typeDescription;
		_name = TypeDescription.TargetType.FullName;

		NodeAttribute = TypeLibrary.GetAttribute<NodeAttribute>( TargetType );
		EnergyAttribute = TypeLibrary.GetAttribute<EnergyAttribute>( TargetType );
		ConnectorAttributes = TypeLibrary.GetAttributes<ConnectorAttribute>( TargetType );
	}

	public static WeaponNodeDescription Get( Type type ) => Instances.SingleOrDefault( v => v.TargetType == type ) ??
	                                                        new WeaponNodeDescription( type );

	public static WeaponNodeDescription Get( TypeDescription typeDescription ) =>
		Instances.SingleOrDefault( v => v.TypeDescription == typeDescription ) ??
		new WeaponNodeDescription( typeDescription );

	public static WeaponNodeDescription Get( string typeName )
	{
		var typeDescription = TypeLibrary.GetType( typeName );
		return Instances.SingleOrDefault( v => v.TypeDescription == typeDescription ) ??
		       new WeaponNodeDescription( typeDescription );
	}
}
