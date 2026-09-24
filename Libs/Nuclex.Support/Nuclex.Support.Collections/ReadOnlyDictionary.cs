using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Nuclex.Support.Collections;

/// <summary>Wraps a dictionary and prevents users from modifying it</summary>
/// <typeparam name="KeyType">Type of the keys used in the dictionary</typeparam>
/// <typeparam name="ValueType">Type of the values used in the dictionary</typeparam>
[Serializable]
public class ReadOnlyDictionary<KeyType, ValueType> : ISerializable, IDeserializationCallback, IDictionary<KeyType, ValueType>, ICollection<KeyValuePair<KeyType, ValueType>>, IEnumerable<KeyValuePair<KeyType, ValueType>>, IDictionary, ICollection, IEnumerable
{
	/// <summary>
	///   Dictionary wrapped used to reconstruct a serialized read only dictionary
	/// </summary>
	private class SerializedDictionary : Dictionary<KeyType, ValueType>
	{
		/// <summary>
		///   Initializes a new instance of the System.WeakReference class, using deserialized
		///   data from the specified serialization and stream objects.
		/// </summary>
		/// <param name="info">
		///   An object that holds all the data needed to serialize or deserialize the
		///   current System.WeakReference object.
		/// </param>
		/// <param name="context">
		///   (Reserved) Describes the source and destination of the serialized stream
		///   specified by info.
		/// </param>
		/// <exception cref="T:System.ArgumentNullException">
		///   The info parameter is null.
		/// </exception>
		public SerializedDictionary(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	/// <summary>The wrapped Dictionary under its type-safe interface</summary>
	private IDictionary<KeyType, ValueType> typedDictionary;

	/// <summary>The wrapped Dictionary under its object interface</summary>
	private IDictionary objectDictionary;

	/// <summary>ReadOnly wrapper for the keys collection of the Dictionary</summary>
	private ReadOnlyCollection<KeyType> readonlyKeyCollection;

	/// <summary>ReadOnly wrapper for the values collection of the Dictionary</summary>
	private ReadOnlyCollection<ValueType> readonlyValueCollection;

	/// <summary>Whether the directory is write-protected</summary>
	public bool IsReadOnly => true;

	/// <summary>Number of elements contained in the Dictionary</summary>
	public int Count => typedDictionary.Count;

	/// <summary>Collection of all keys contained in the Dictionary</summary>
	public ICollection<KeyType> Keys
	{
		get
		{
			if (readonlyKeyCollection == null)
			{
				readonlyKeyCollection = new ReadOnlyCollection<KeyType>(typedDictionary.Keys);
			}
			return readonlyKeyCollection;
		}
	}

	/// <summary>Collection of all values contained in the Dictionary</summary>
	public ICollection<ValueType> Values
	{
		get
		{
			if (readonlyValueCollection == null)
			{
				readonlyValueCollection = new ReadOnlyCollection<ValueType>(typedDictionary.Values);
			}
			return readonlyValueCollection;
		}
	}

	/// <summary>Accesses an item in the Dictionary by its key</summary>
	/// <param name="key">Key of the item that will be accessed</param>
	public ValueType this[KeyType key] => typedDictionary[key];

	ValueType IDictionary<KeyType, ValueType>.this[KeyType key]
	{
		get
		{
			return typedDictionary[key];
		}
		set
		{
			throw new NotSupportedException("Assigning items is not supported in a read-only Dictionary");
		}
	}

	/// <summary>Whether the size of the Dictionary is fixed</summary>
	bool IDictionary.IsFixedSize => objectDictionary.IsFixedSize;

	/// <summary>Returns a collection of all keys in the Dictionary</summary>
	ICollection IDictionary.Keys
	{
		get
		{
			if (readonlyKeyCollection == null)
			{
				readonlyKeyCollection = new ReadOnlyCollection<KeyType>(typedDictionary.Keys);
			}
			return readonlyKeyCollection;
		}
	}

	/// <summary>Returns a collection of all values stored in the Dictionary</summary>
	ICollection IDictionary.Values
	{
		get
		{
			if (readonlyValueCollection == null)
			{
				readonlyValueCollection = new ReadOnlyCollection<ValueType>(typedDictionary.Values);
			}
			return readonlyValueCollection;
		}
	}

	/// <summary>Accesses an item in the Dictionary by its key</summary>
	/// <param name="key">Key of the item that will be accessed</param>
	/// <returns>The item with the specified key</returns>
	object IDictionary.this[object key]
	{
		get
		{
			return objectDictionary[key];
		}
		set
		{
			throw new NotSupportedException("Assigning items is not supported by the read-only Dictionary");
		}
	}

	/// <summary>Whether the Dictionary is synchronized for multi-threaded usage</summary>
	bool ICollection.IsSynchronized => objectDictionary.IsSynchronized;

	/// <summary>Synchronization root on which the Dictionary locks</summary>
	object ICollection.SyncRoot => objectDictionary.SyncRoot;

	/// <summary>
	///   Initializes a new instance of the System.WeakReference class, using deserialized
	///   data from the specified serialization and stream objects.
	/// </summary>
	/// <param name="info">
	///   An object that holds all the data needed to serialize or deserialize the
	///   current System.WeakReference object.
	/// </param>
	/// <param name="context">
	///   (Reserved) Describes the source and destination of the serialized stream
	///   specified by info.
	/// </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   The info parameter is null.
	/// </exception>
	protected ReadOnlyDictionary(SerializationInfo info, StreamingContext context)
		: this((IDictionary<KeyType, ValueType>)new SerializedDictionary(info, context))
	{
	}

	/// <summary>Initializes a new read-only dictionary wrapper</summary>
	/// <param name="dictionary">Dictionary that will be wrapped</param>
	public ReadOnlyDictionary(IDictionary<KeyType, ValueType> dictionary)
	{
		typedDictionary = dictionary;
		objectDictionary = typedDictionary as IDictionary;
	}

	/// <summary>
	///   Determines whether the specified KeyValuePair is contained in the Dictionary
	/// </summary>
	/// <param name="item">KeyValuePair that will be checked for</param>
	/// <returns>True if the provided KeyValuePair was contained in the Dictionary</returns>
	public bool Contains(KeyValuePair<KeyType, ValueType> item)
	{
		return typedDictionary.Contains(item);
	}

	/// <summary>Determines whether the Dictionary contains the specified key</summary>
	/// <param name="key">Key that will be checked for</param>
	/// <returns>
	///   True if an entry with the specified key was contained in the Dictionary
	/// </returns>
	public bool ContainsKey(KeyType key)
	{
		return typedDictionary.ContainsKey(key);
	}

	/// <summary>Copies the contents of the Dictionary into an array</summary>
	/// <param name="array">Array the Dictionary will be copied into</param>
	/// <param name="arrayIndex">
	///   Starting index at which to begin filling the destination array
	/// </param>
	public void CopyTo(KeyValuePair<KeyType, ValueType>[] array, int arrayIndex)
	{
		typedDictionary.CopyTo(array, arrayIndex);
	}

	/// <summary>Creates a new enumerator for the Dictionary</summary>
	/// <returns>The new Dictionary enumerator</returns>
	public IEnumerator<KeyValuePair<KeyType, ValueType>> GetEnumerator()
	{
		return typedDictionary.GetEnumerator();
	}

	/// <summary>
	///   Attempts to retrieve the item with the specified key from the Dictionary
	/// </summary>
	/// <param name="key">Key of the item to attempt to retrieve</param>
	/// <param name="value">
	///   Output parameter that will receive the key upon successful completion
	/// </param>
	/// <returns>
	///   True if the item was found and has been placed in the output parameter
	/// </returns>
	public bool TryGetValue(KeyType key, out ValueType value)
	{
		return typedDictionary.TryGetValue(key, out value);
	}

	void IDictionary<KeyType, ValueType>.Add(KeyType key, ValueType value)
	{
		throw new NotSupportedException("Adding items is not supported by the read-only Dictionary");
	}

	bool IDictionary<KeyType, ValueType>.Remove(KeyType key)
	{
		throw new NotSupportedException("Removing items is not supported by the read-only Dictionary");
	}

	/// <summary>Returns a new object enumerator for the Dictionary</summary>
	/// <returns>The new object enumerator</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)typedDictionary).GetEnumerator();
	}

	/// <summary>Removes all items from the Dictionary</summary>
	void IDictionary.Clear()
	{
		throw new NotSupportedException("Clearing is not supported in a read-only Dictionary");
	}

	/// <summary>Adds an item into the Dictionary</summary>
	/// <param name="key">Key under which the item will be added</param>
	/// <param name="value">Item that will be added</param>
	void IDictionary.Add(object key, object value)
	{
		throw new NotSupportedException("Adding items is not supported in a read-only Dictionary");
	}

	/// <summary>Determines whether the specified key exists in the Dictionary</summary>
	/// <param name="key">Key that will be checked for</param>
	/// <returns>True if an item with the specified key exists in the Dictionary</returns>
	bool IDictionary.Contains(object key)
	{
		return objectDictionary.Contains(key);
	}

	/// <summary>Returns a new entry enumerator for the dictionary</summary>
	/// <returns>The new entry enumerator</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return objectDictionary.GetEnumerator();
	}

	/// <summary>Removes an item from the Dictionary</summary>
	/// <param name="key">Key of the item that will be removed</param>
	void IDictionary.Remove(object key)
	{
		throw new NotSupportedException("Removing is not supported by the read-only Dictionary");
	}

	void ICollection<KeyValuePair<KeyType, ValueType>>.Add(KeyValuePair<KeyType, ValueType> item)
	{
		throw new NotSupportedException("Adding items is not supported by the read-only Dictionary");
	}

	void ICollection<KeyValuePair<KeyType, ValueType>>.Clear()
	{
		throw new NotSupportedException("Clearing is not supported in a read-only Dictionary");
	}

	bool ICollection<KeyValuePair<KeyType, ValueType>>.Remove(KeyValuePair<KeyType, ValueType> itemToRemove)
	{
		throw new NotSupportedException("Removing items is not supported in a read-only Dictionary");
	}

	/// <summary>Copies the contents of the Dictionary into an array</summary>
	/// <param name="array">Array the Dictionary contents will be copied into</param>
	/// <param name="index">
	///   Starting index at which to begin filling the destination array
	/// </param>
	void ICollection.CopyTo(Array array, int index)
	{
		objectDictionary.CopyTo(array, index);
	}

	/// <summary>Serializes the Dictionary</summary>
	/// <param name="info">
	///   Provides the container into which the Dictionary will serialize itself
	/// </param>
	/// <param name="context">
	///   Contextual informations about the serialization environment
	/// </param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		(typedDictionary as ISerializable).GetObjectData(info, context);
	}

	/// <summary>Called after all objects have been successfully deserialized</summary>
	/// <param name="sender">Nicht unterstützt</param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		(typedDictionary as IDeserializationCallback).OnDeserialization(sender);
	}
}
