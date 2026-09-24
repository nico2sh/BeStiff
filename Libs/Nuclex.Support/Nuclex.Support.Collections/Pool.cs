using System.Collections.Generic;

namespace Nuclex.Support.Collections;

/// <summary>Pool that recycles objects in order to avoid garbage build-up</summary>
/// <typeparam name="ItemType">Type of objects being pooled</typeparam>
/// <remarks>
///   <para>
///     Use this class to recycle objects instead of letting them become garbage,
///     creating new instances each time. The Pool class is designed to either be
///     used on its own or as a building block for a static class that wraps it.
///   </para>
///   <para>
///     Special care has to be taken to revert the entire state of a recycled
///     object when it is returned to the pool. For example, events will need to
///     have their subscriber lists emptied to avoid sending out events to the
///     wrong subscribers and accumulating more and more subscribers each time
///     they are reused.
///   </para>
///   <para>
///     To simplify such cleanup, pooled objects can implement the IRecyclable
///     interface. When an object is returned to the pool, the pool will
///     automatically call its IRecyclable.Recycle() method.
///   </para>
/// </remarks>
public class Pool<ItemType> where ItemType : class, new()
{
	/// <summary>Default number of recyclable objects the pool will store</summary>
	public const int DefaultPoolSize = 64;

	/// <summary>Objects being retained for recycling</summary>
	private Queue<ItemType> items;

	/// <summary>Capacity of the pool</summary>
	/// <remarks>
	///   Required because the Queue class doesn't allow this value to be retrieved
	/// </remarks>
	private int capacity;

	/// <summary>Number of objects the pool can retain</summary>
	/// <remarks>
	///   Changing this value causes the pool to be emtpied. It is recommended that
	///   you only read the pool's capacity, never change it.
	/// </remarks>
	public int Capacity
	{
		get
		{
			return capacity;
		}
		set
		{
			capacity = value;
			items = new Queue<ItemType>(value);
		}
	}

	/// <summary>Initializes a new pool using the default capacity</summary>
	public Pool()
		: this(64)
	{
	}

	/// <summary>Initializes a new pool using a user-specified capacity</summary>
	/// <param name="capacity">Capacity of the pool</param>
	public Pool(int capacity)
	{
		Capacity = capacity;
	}

	/// <summary>
	///   Returns a new or recycled instance of the types managed by the pool
	/// </summary>
	/// <returns>A new or recycled instance</returns>
	public ItemType Get()
	{
		lock (this)
		{
			if (items.Count > 0)
			{
				return items.Dequeue();
			}
			return new ItemType();
		}
	}

	/// <summary>
	///   Redeems an instance that is no longer used to be recycled by the pool
	/// </summary>
	/// <param name="item">The instance that will be redeemed</param>
	public void Redeem(ItemType item)
	{
		callRecycleIfSupported(item);
		lock (this)
		{
			if (items.Count < capacity)
			{
				items.Enqueue(item);
			}
		}
	}

	/// <summary>
	///   Calls the Recycle() method on an objects if it implements
	///   the IRecyclable interface
	/// </summary>
	/// <param name="item">
	///   Object whose Recycle() method will be called if supported by the object
	/// </param>
	private static void callRecycleIfSupported(ItemType item)
	{
		if (item is IRecyclable recyclable)
		{
			recyclable.Recycle();
		}
	}
}
