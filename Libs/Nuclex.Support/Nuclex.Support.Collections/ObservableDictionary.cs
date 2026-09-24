using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Nuclex.Support.Collections;

/// <summary>A dictionary that sneds out change notifications</summary>
/// <typeparam name="KeyType">Type of the keys used in the dictionary</typeparam>
/// <typeparam name="ValueType">Type of the values used in the dictionary</typeparam>
[Serializable]
public class ObservableDictionary<KeyType, ValueType> : ISerializable, IDeserializationCallback, IDictionary<KeyType, ValueType>, ICollection<KeyValuePair<KeyType, ValueType>>, IEnumerable<KeyValuePair<KeyType, ValueType>>, IDictionary, ICollection, IEnumerable, IObservableCollection<KeyValuePair<KeyType, ValueType>>
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

	/// <summary>Whether the directory is write-protected</summary>
	public bool IsReadOnly => typedDictionary.IsReadOnly;

	/// <summary>Number of elements contained in the Dictionary</summary>
	public int Count => typedDictionary.Count;

	/// <summary>Collection of all keys contained in the Dictionary</summary>
	public ICollection<KeyType> Keys => typedDictionary.Keys;

	/// <summary>Collection of all values contained in the Dictionary</summary>
	public ICollection<ValueType> Values => typedDictionary.Values;

	/// <summary>Accesses an item in the Dictionary by its key</summary>
	/// <param name="key">Key of the item that will be accessed</param>
	public ValueType this[KeyType key]
	{
		get
		{
			return typedDictionary[key];
		}
		set
		{
			ValueType value2;
			bool flag = typedDictionary.TryGetValue(key, out value2);
			typedDictionary[key] = value;
			if (flag)
			{
				OnRemoved(new KeyValuePair<KeyType, ValueType>(key, value2));
			}
			OnAdded(new KeyValuePair<KeyType, ValueType>(key, value));
		}
	}

	/// <summary>Whether the size of the Dictionary is fixed</summary>
	bool IDictionary.IsFixedSize => objectDictionary.IsFixedSize;

	/// <summary>Returns a collection of all keys in the Dictionary</summary>
	ICollection IDictionary.Keys => objectDictionary.Keys;

	/// <summary>Returns a collection of all values stored in the Dictionary</summary>
	ICollection IDictionary.Values => objectDictionary.Values;

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
			ValueType value2;
			bool flag = typedDictionary.TryGetValue((KeyType)key, out value2);
			objectDictionary[key] = value;
			if (flag)
			{
				OnRemoved(new KeyValuePair<KeyType, ValueType>((KeyType)key, value2));
			}
			OnAdded(new KeyValuePair<KeyType, ValueType>((KeyType)key, (ValueType)value));
		}
	}

	/// <summary>Whether the Dictionary is synchronized for multi-threaded usage</summary>
	bool ICollection.IsSynchronized => objectDictionary.IsSynchronized;

	/// <summary>Synchronization root on which the Dictionary locks</summary>
	object ICollection.SyncRoot => objectDictionary.SyncRoot;

	/// <summary>Raised when an item has been added to the dictionary</summary>
	public event EventHandler<ItemEventArgs<KeyValuePair<KeyType, ValueType>>> ItemAdded;

	/// <summary>Raised when an item is removed from the dictionary</summary>
	public event EventHandler<ItemEventArgs<KeyValuePair<KeyType, ValueType>>> ItemRemoved;

	/// <summary>Raised when the dictionary is about to be cleared</summary>
	public event EventHandler Clearing;

	/// <summary>Raised when the dictionary has been cleared</summary>
	public event EventHandler Cleared;

	/// <summary>Initializes a new observable dictionary</summary>
	public ObservableDictionary()
		: this((IDictionary<KeyType, ValueType>)new Dictionary<KeyType, ValueType>())
	{
	}

	/// <summary>Initializes a new observable Dictionary wrapper</summary>
	/// <param name="dictionary">Dictionary that will be wrapped</param>
	public ObservableDictionary(IDictionary<KeyType, ValueType> dictionary)
	{
		typedDictionary = dictionary;
		objectDictionary = typedDictionary as IDictionary;
	}

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
	protected ObservableDictionary(SerializationInfo info, StreamingContext context)
		: this((IDictionary<KeyType, ValueType>)new SerializedDictionary(info, context))
	{
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

	/// <summary>Inserts an item into the Dictionary</summary>
	/// <param name="key">Key under which to add the new item</param>
	/// <param name="value">Item that will be added to the Dictionary</param>
	public void Add(KeyType key, ValueType value)
	{
		typedDictionary.Add(key, value);
		OnAdded(new KeyValuePair<KeyType, ValueType>(key, value));
	}

	/// <summary>Removes the item with the specified key from the Dictionary</summary>
	/// <param name="key">Key of the elementes that will be removed</param>
	/// <returns>True if an item with the specified key was found and removed</returns>
	public bool Remove(KeyType key)
	{
		typedDictionary.TryGetValue(key, out var value);
		bool flag = typedDictionary.Remove(key);
		if (flag)
		{
			OnRemoved(new KeyValuePair<KeyType, ValueType>(key, value));
		}
		return flag;
	}

	/// <summary>Removes all items from the Dictionary</summary>
	public void Clear()
	{
		OnClearing();
		typedDictionary.Clear();
		OnCleared();
	}

	/// <summary>Fires the 'ItemAdded' event</summary>
	/// <param name="item">Item that has been added to the collection</param>
	protected virtual void OnAdded(KeyValuePair<KeyType, ValueType> item)
	{
		if (this.ItemAdded != null)
		{
			this.ItemAdded(this, new ItemEventArgs<KeyValuePair<KeyType, ValueType>>(item));
		}
	}

	/// <summary>Fires the 'ItemRemoved' event</summary>
	/// <param name="item">Item that has been removed from the collection</param>
	protected virtual void OnRemoved(KeyValuePair<KeyType, ValueType> item)
	{
		if (this.ItemRemoved != null)
		{
			this.ItemRemoved(this, new ItemEventArgs<KeyValuePair<KeyType, ValueType>>(item));
		}
	}

	/// <summary>Fires the 'Clearing' event</summary>
	protected virtual void OnClearing()
	{
		if (this.Clearing != null)
		{
			this.Clearing(this, EventArgs.Empty);
		}
	}

	/// <summary>Fires the 'Cleared' event</summary>
	protected virtual void OnCleared()
	{
		if (this.Cleared != null)
		{
			this.Cleared(this, EventArgs.Empty);
		}
	}

	/// <summary>Returns a new object enumerator for the Dictionary</summary>
	/// <returns>The new object enumerator</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)typedDictionary).GetEnumerator();
	}

	/// <summary>Adds an item into the Dictionary</summary>
	/// <param name="key">Key under which the item will be added</param>
	/// <param name="value">Item that will be added</param>
	void IDictionary.Add(object key, object value)
	{
		objectDictionary.Add(key, value);
		OnAdded(new KeyValuePair<KeyType, ValueType>((KeyType)key, (ValueType)value));
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
		ValueType value;
		bool flag = typedDictionary.TryGetValue((KeyType)key, out value);
		objectDictionary.Remove(key);
		if (flag)
		{
			OnRemoved(new KeyValuePair<KeyType, ValueType>((KeyType)key, value));
		}
	}

	void ICollection<KeyValuePair<KeyType, ValueType>>.Add(KeyValuePair<KeyType, ValueType> item)
	{
		typedDictionary.Add(item);
		OnAdded(item);
	}

	void ICollection<KeyValuePair<KeyType, ValueType>>.Clear()
	{
		OnClearing();
		typedDictionary.Clear();
		OnCleared();
	}

	bool ICollection<KeyValuePair<KeyType, ValueType>>.Remove(KeyValuePair<KeyType, ValueType> itemToRemove)
	{
		bool flag = typedDictionary.Remove(itemToRemove);
		if (flag)
		{
			OnRemoved(itemToRemove);
		}
		return flag;
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
