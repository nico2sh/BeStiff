using System;
using System.Reflection;

namespace Nuclex.Support;

/// <summary>Helper methods for enumerations</summary>
public static class EnumHelper
{
	/// <summary>Returns the highest value encountered in an enumeration</summary>
	/// <typeparam name="EnumType">
	///   Enumeration of which the highest value will be returned
	/// </typeparam>
	/// <returns>The highest value in the enumeration</returns>
	public static EnumType GetHighestValue<EnumType>() where EnumType : IComparable
	{
		EnumType[] values = GetValues<EnumType>();
		if (values.Length == 0)
		{
			return default(EnumType);
		}
		EnumType val = values[0];
		for (int i = 1; i < values.Length; i++)
		{
			ref readonly EnumType reference = ref values[i];
			object obj = val;
			if (reference.CompareTo(obj) > 0)
			{
				val = values[i];
			}
		}
		return val;
	}

	/// <summary>Returns the lowest value encountered in an enumeration</summary>
	/// <typeparam name="EnumType">
	///   Enumeration of which the lowest value will be returned
	/// </typeparam>
	/// <returns>The lowest value in the enumeration</returns>
	public static EnumType GetLowestValue<EnumType>() where EnumType : IComparable
	{
		EnumType[] values = GetValues<EnumType>();
		if (values.Length == 0)
		{
			return default(EnumType);
		}
		EnumType val = values[0];
		for (int i = 1; i < values.Length; i++)
		{
			ref readonly EnumType reference = ref values[i];
			object obj = val;
			if (reference.CompareTo(obj) < 0)
			{
				val = values[i];
			}
		}
		return val;
	}

	/// <summary>Retrieves a list of all values contained in an enumeration</summary>
	/// <typeparam name="EnumType">
	///   Type of the enumeration whose values will be returned
	/// </typeparam>
	/// <returns>All values contained in the specified enumeration</returns>
	/// <remarks>
	///   This method produces collectable garbage so it's best to only call it once
	///   and cache the result.
	/// </remarks>
	public static EnumType[] GetValues<EnumType>()
	{
		return (EnumType[])Enum.GetValues(typeof(EnumType));
	}

	/// <summary>Retrieves a list of all values contained in an enumeration</summary>
	/// <typeparam name="EnumType">
	///   Type of the enumeration whose values will be returned
	/// </typeparam>
	/// <returns>All values contained in the specified enumeration</returns>
	internal static EnumType[] GetValuesXbox360<EnumType>()
	{
		Type typeFromHandle = typeof(EnumType);
		if (!typeFromHandle.IsEnum)
		{
			throw new ArgumentException("The provided type needs to be an enumeration", "EnumType");
		}
		FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Static | BindingFlags.Public);
		EnumType[] array = new EnumType[fields.Length];
		for (int i = 0; i < fields.Length; i++)
		{
			array[i] = (EnumType)fields[i].GetValue(null);
		}
		return array;
	}
}
