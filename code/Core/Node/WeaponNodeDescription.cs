using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace ProjectBullet.Core.Node;

public class WeaponNodeDescription
{
	private static readonly List<WeaponNodeDescription> Instances = new();

	private string _name;

	public NodeAttribute NodeAttribute { get; private set; }
	public EnergyAttribute EnergyAttribute { get; private set; }
	public List<ConnectorAttribute> ConnectorAttributes { get; private set; }

	public TypeDescription TypeDescription { get; private set; }
	public Type TargetType => TypeDescription.TargetType;

	private WeaponNodeDescription( Type type )
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

	private WeaponNodeDescription( TypeDescription typeDescription )
	{
		UseNewTypeDescription( typeDescription );

		Event.Register( this );
	}

	~WeaponNodeDescription() => Event.Unregister( this );

	/// <summary>
	/// Register all WeaponNodes
	/// </summary>
	public static void InitAll()
	{
		foreach ( var typeDescription in TypeLibrary.GetTypes<WeaponNode>() )
		{
			Instances.Add( new WeaponNodeDescription( typeDescription ) );
		}
	}

	// ReSharper disable once UnusedMember.Local
	private void OnHotload() => UseNewTypeDescription
		( TypeLibrary.GetType( _name ) );

	private void UseNewTypeDescription( TypeDescription typeDescription )
	{
		TypeDescription = typeDescription;
		_name = TypeDescription.TargetType.FullName;

		NodeAttribute = TypeLibrary.GetAttribute<NodeAttribute>( TargetType );
		EnergyAttribute = TypeLibrary.GetAttribute<EnergyAttribute>( TargetType );
		ConnectorAttributes = TypeLibrary.GetAttributes<ConnectorAttribute>( TargetType ).ToList();
	}

	/// <summary>
	/// Get stored (or newly created) WeaponNodeDescription by type
	/// </summary>
	/// <param name="type">Type / key</param>
	/// <returns>WeaponNodeDescription</returns>
	public static WeaponNodeDescription Get( Type type )
	{
		ArgumentNullException.ThrowIfNull( type );
		return Instances.SingleOrDefault( v => v.TargetType == type ) ??
		       new WeaponNodeDescription( type );
	}

	/// <summary>
	/// Get stored (or newly created) WeaponNodeDescription by TypeDescription
	/// </summary>
	/// <param name="typeDescription">TypeDescription / key</param>
	/// <returns>WeaponNodeDescription</returns>
	public static WeaponNodeDescription Get( TypeDescription typeDescription ) =>
		Instances.SingleOrDefault( v => v.TypeDescription == typeDescription ) ??
		new WeaponNodeDescription( typeDescription );

	/// <summary>
	/// Get stored (or newly created) WeaponNodeDescription by type name
	/// </summary>
	/// <param name="typeName">Type name / key</param>
	/// <returns>WeaponNodeDescription</returns>
	public static WeaponNodeDescription Get( string typeName )
	{
		var typeDescription = TypeLibrary.GetType( typeName );
		return Instances.SingleOrDefault( v => v.TypeDescription == typeDescription ) ??
		       new WeaponNodeDescription( typeDescription );
	}
}
