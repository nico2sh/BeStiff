using System;
using System.IO;

namespace Nuclex.Support.IO;

/// <summary>Specialized memory stream for ring buffers</summary>
/// <remarks>
///   This ring buffer class is specialized for binary data and tries to achieve
///   optimal efficiency when storing and retrieving chunks of several bytes
///   at once. Typical use cases include audio and network buffers where one party
///   is responsible for refilling the buffer at regular intervals while the other
///   constantly streams data out of it.
/// </remarks>
public class RingMemoryStream : Stream
{
	/// <summary>Internal stream containing the ring buffer data</summary>
	private MemoryStream ringBuffer;

	/// <summary>Start index of the data within the ring buffer</summary>
	private int startIndex;

	/// <summary>End index of the data within the ring buffer</summary>
	private int endIndex;

	/// <summary>Whether the ring buffer is empty</summary>
	/// <remarks>
	///   This field is required to differentiate between the ring buffer being
	///   filled to the limit and being totally empty, because in both cases,
	///   the start index and the end index will be the same. 
	/// </remarks>
	private bool empty;

	/// <summary>Maximum amount of data that will fit into the ring memory stream</summary>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   Thrown if the new capacity is too small for the data already contained
	///   in the ring buffer.
	/// </exception>
	public long Capacity
	{
		get
		{
			return ringBuffer.Length;
		}
		set
		{
			int num = (int)Length;
			if (value < num)
			{
				throw new ArgumentOutOfRangeException("New capacity is less than the stream's current length");
			}
			MemoryStream memoryStream = new MemoryStream((int)value);
			memoryStream.SetLength(value);
			if (num > 0)
			{
				Read(memoryStream.GetBuffer(), 0, num);
			}
			ringBuffer.Close();
			ringBuffer = memoryStream;
			startIndex = 0;
			endIndex = num;
		}
	}

	/// <summary>Whether it's possible to read from this stream</summary>
	public override bool CanRead => true;

	/// <summary>Whether this stream supports random access</summary>
	public override bool CanSeek => false;

	/// <summary>Whether it's possible to write into this stream</summary>
	public override bool CanWrite => true;

	/// <summary>Current length of the stream</summary>
	public override long Length
	{
		get
		{
			if (endIndex > startIndex || empty)
			{
				return endIndex - startIndex;
			}
			return ringBuffer.Length - startIndex + endIndex;
		}
	}

	/// <summary>Current cursor position within the stream</summary>
	/// <exception cref="T:System.NotSupportedException">Always</exception>
	public override long Position
	{
		get
		{
			throw new NotSupportedException("The ring buffer does not support seeking");
		}
		set
		{
			throw new NotSupportedException("The ring buffer does not support seeking");
		}
	}

	/// <summary>Initializes a new ring memory stream</summary>
	/// <param name="capacity">Maximum capacity of the stream</param>
	public RingMemoryStream(int capacity)
	{
		ringBuffer = new MemoryStream(capacity);
		ringBuffer.SetLength(capacity);
		empty = true;
	}

	/// <summary>Flushes the buffers and writes down unsaved data</summary>
	public override void Flush()
	{
	}

	/// <summary>Reads data from the beginning of the stream</summary>
	/// <param name="buffer">Buffer in which to store the data</param>
	/// <param name="offset">Starting index at which to begin writing the buffer</param>
	/// <param name="count">Number of bytes to read from the stream</param>
	/// <returns>Die Number of bytes actually read</returns>
	public override int Read(byte[] buffer, int offset, int count)
	{
		if (startIndex < endIndex || empty)
		{
			count = Math.Min(count, endIndex - startIndex);
			if (count > 0)
			{
				ringBuffer.Position = startIndex;
				ringBuffer.Read(buffer, offset, count);
				startIndex += count;
				if (startIndex == endIndex)
				{
					setEmpty();
				}
			}
		}
		else
		{
			int num = (int)ringBuffer.Length - startIndex;
			if (count > num)
			{
				count = Math.Min(count, num + endIndex);
				ringBuffer.Position = startIndex;
				ringBuffer.Read(buffer, offset, num);
				ringBuffer.Position = 0L;
				startIndex = count - num;
				ringBuffer.Read(buffer, offset + num, startIndex);
			}
			else
			{
				ringBuffer.Position = startIndex;
				ringBuffer.Read(buffer, offset, count);
				startIndex += count;
			}
			if (startIndex == endIndex)
			{
				setEmpty();
			}
		}
		return count;
	}

	/// <summary>Appends data to the end of the stream</summary>
	/// <param name="buffer">Buffer containing the data to append</param>
	/// <param name="offset">Starting index of the data in the buffer</param>
	/// <param name="count">Number of bytes to write to the stream</param>
	/// <exception cref="T:System.OverflowException">When the ring buffer is full</exception>
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (startIndex < endIndex || empty)
		{
			int num = (int)(ringBuffer.Length - endIndex);
			if (count > num)
			{
				if (count > num + startIndex)
				{
					throw new OverflowException("Data does not fit in buffer");
				}
				ringBuffer.Position = endIndex;
				ringBuffer.Write(buffer, offset, num);
				ringBuffer.Position = 0L;
				endIndex = count - num;
				ringBuffer.Write(buffer, offset + num, endIndex);
			}
			else
			{
				ringBuffer.Position = endIndex;
				ringBuffer.Write(buffer, offset, count);
				endIndex += count;
			}
			empty = false;
		}
		else
		{
			if (count > startIndex - endIndex)
			{
				throw new OverflowException("Data does not fit in buffer");
			}
			ringBuffer.Position = endIndex;
			ringBuffer.Write(buffer, offset, count);
			endIndex += count;
		}
	}

	/// <summary>Jumps to the specified location within the stream</summary>
	/// <param name="offset">Position to jump to</param>
	/// <param name="origin">Origin towards which to interpret the offset</param>
	/// <returns>The new offset within the stream</returns>
	/// <exception cref="T:System.NotSupportedException">Always</exception>
	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException("The ring buffer does not support seeking");
	}

	/// <summary>Changes the length of the stream</summary>
	/// <param name="value">New length to resize the stream to</param>
	/// <exception cref="T:System.NotSupportedException">Always</exception>
	public override void SetLength(long value)
	{
		throw new NotSupportedException("This operation is not supported");
	}

	/// <summary>Resets the stream to its empty state</summary>
	private void setEmpty()
	{
		empty = true;
		startIndex = 0;
		endIndex = 0;
	}
}
