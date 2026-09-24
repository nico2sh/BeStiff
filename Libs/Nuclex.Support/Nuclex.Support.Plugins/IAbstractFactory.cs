namespace Nuclex.Support.Plugins;

/// <summary>Abstract factory for a concrete type</summary>
/// <typeparam name="ProductType">
///   Interface or base class of the product of the factory
/// </typeparam>
public interface IAbstractFactory<ProductType>
{
	/// <summary>
	///   Creates a new instance of the type to which the factory is specialized
	/// </summary>
	/// <returns>The newly created instance</returns>
	ProductType CreateInstance();
}
/// <summary>Abstract factory for a concrete type</summary>
public interface IAbstractFactory
{
	/// <summary>
	///   Creates a new instance of the type to which the factory is specialized
	/// </summary>
	/// <returns>The newly created instance</returns>
	object CreateInstance();
}
