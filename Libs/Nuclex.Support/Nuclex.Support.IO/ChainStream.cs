using System;
using System.IO;

namespace Nuclex.Support.IO;

/// <summary>Chains a series of independent streams into a single stream</summary>
/// <remarks>
///   <para>
///     This class can be used to chain multiple independent streams into a single
///     stream that acts as if its chained streams were only one combined stream.
///     It is useful to avoid creating huge memory streams or temporary files when
///     you just need to prepend or append some data to a stream or if you need to
///     read a file that was split into several parts as if it was a single file.
///   </para>
///   <para>
///     It is not recommended to change the size of any chained stream after it
///     has become part of a stream chainer, though the stream chainer will do its
///     best to cope with the changes as they occur. Increasing the length of a
///     chained stream is generally not an issue for streams that support seeking,
///     but reducing the length might invalidate the stream chainer's file pointer,
///     resulting in an IOException when Read() or Write() is next called.
///   </para>
/// </remarks>
public class ChainStream : Stream
{
	/// <summary>Streams that have been chained together</summary>
	private Stream[] streams;

	/// <summary>Current position of the overall file pointer</summary>
	private long position;

	/// <summary>Stream we're currently reading from if seeking is not supported</summary>
	/// <remarks>
	///   If seeking is not supported, the stream chainer will read from each stream
	///   until the end was reached
	///   sequentially
	/// </remarks>
	private int activeReadStreamIndex;

	/// <summary>Position in the current read stream if seeking is not supported</summary>
	/// <remarks>
	///   If there is a mix of streams supporting seeking and not supporting seeking, we
	///   need to keep track of the read index for those streams that do. If, for example,
	///   the last stream is written to and read from in succession, the file pointer
	///   of that stream would have been moved to the end by the write attempt, skipping
	///   data that should have been read in the following read attempt.
	/// </remarks>
	private long activeReadStreamPosition;

	/// <summary>Whether all of the chained streams support seeking</summary>
	private bool allStreamsCanSeek;

	/// <summary>Whether all of the chained streams support reading</summary>
	private bool allStreamsCanRead;

	/// <summary>Whether all of the chained streams support writing</summary>
	private bool allStreamsCanWrite;

	/// <summary>Whether data can be read from the stream</summary>
	public override bool CanRead => allStreamsCanRead;

	/// <summary>Whether the stream supports seeking</summary>
	public override bool CanSeek => allStreamsCanSeek;

	/// <summary>Whether data can be written into the stream</summary>
	public override bool CanWrite => allStreamsCanWrite;

	/// <summary>Length of the stream in bytes</summary>
	/// <exception cref="T:System.NotSupportedException">
	///   At least one of the chained streams does not support seeking
	/// </exception>
	public override long Length
	{
		get
		{
			if (!allStreamsCanSeek)
			{
				throw makeSeekNotSupportedException("determine length");
			}
			long num = 0L;
			for (int i = 0; i < streams.Length; i++)
			{
				num += streams[i].Length;
			}
			return num;
		}
	}

	/// <summary>Absolute position of the file pointer within the stream</summary>
	/// <exception cref="T:System.NotSupportedException">
	///   At least one of the chained streams does not support seeking
	/// </exception>
	public override long Position
	{
		get
		{
			if (!allStreamsCanSeek)
			{
				throw makeSeekNotSupportedException("seek");
			}
			return position;
		}
		set
		{
			moveFilePointer(value);
		}
	}

	/// <summary>Streams being combined by the stream chainer</summary>
	public Stream[] ChainedStreams => streams;

	/// <summary>Initializes a new stream chainer</summary>
	/// <param name="streams">Array of streams that will be chained together</param>
	public ChainStream(Stream[] streams)
	{
		this.streams = (Stream[])streams.Clone();
		determineCapabilities();
	}

	/// <summary>
	///   Clears all buffers for this stream and causes any buffered data to be written
	///   to the underlying device.
	/// </summary>
	public override void Flush()
	{
		for (int i = 0; i < streams.Length; i++)
		{
			streams[i].Flush();
		}
	}

	/// <summary>
	///   Reads a sequence of bytes from the stream and advances the position of
	///   the file pointer by the number of bytes read.
	/// </summary>
	/// <param name="buffer">Buffer that will receive the data read from the stream</param>
	/// <param name="offset">
	///   Offset in the buffer at which the stream will place the data read
	/// </param>
	/// <param name="count">Maximum number of bytes that will be read</param>
	/// <returns>
	///   The number of bytes that were actually read from the stream and written into
	///   the provided buffer
	/// </returns>
	/// <exception cref="T:System.NotSupportedException">
	///   The chained stream at the current position does not support reading
	/// </exception>
	public override int Read(byte[] buffer, int offset, int count)
	{
		if (!allStreamsCanRead)
		{
			throw new NotSupportedException("Can't read: at least one of the chained streams doesn't support reading");
		}
		int num = 0;
		int num2 = streams.Length - 1;
		if (CanSeek)
		{
			findStreamIndexAndOffset(position, out var streamIndex, out var streamPosition);
			while (count > 0)
			{
				Stream stream = streams[streamIndex];
				long num3 = Math.Min(count, stream.Length - streamPosition);
				stream.Position = streamPosition;
				int num4 = stream.Read(buffer, offset, (int)num3);
				num += num4;
				if (num4 < num3 || streamIndex == num2)
				{
					break;
				}
				streamIndex++;
				streamPosition = 0L;
				count -= num4;
				offset += num4;
			}
			position += num;
		}
		else
		{
			while (activeReadStreamIndex <= num2)
			{
				Stream stream2 = streams[activeReadStreamIndex];
				if (stream2.CanSeek)
				{
					stream2.Position = activeReadStreamPosition;
				}
				num = stream2.Read(buffer, offset, count);
				if (num != 0)
				{
					break;
				}
				activeReadStreamPosition = 0L;
				activeReadStreamIndex++;
			}
			activeReadStreamPosition += num;
		}
		return num;
	}

	/// <summary>Changes the position of the file pointer</summary>
	/// <param name="offset">
	///   Offset to move the file pointer by, relative to the position indicated by
	///   the <paramref name="origin" /> parameter.
	/// </param>
	/// <param name="origin">
	///   Reference point relative to which the file pointer is placed
	/// </param>
	/// <returns>The new absolute position within the stream</returns>
	public override long Seek(long offset, SeekOrigin origin)
	{
		return origin switch
		{
			SeekOrigin.Begin => Position = offset, 
			SeekOrigin.Current => Position += offset, 
			SeekOrigin.End => Position = Length + offset, 
			_ => throw new ArgumentException("Invalid seek origin", "origin"), 
		};
	}

	/// <summary>Changes the length of the stream</summary>
	/// <param name="value">New length the stream shall have</param>
	/// <exception cref="T:System.NotSupportedException">
	///   Always, the stream chainer does not support the SetLength() operation
	/// </exception>
	public override void SetLength(long value)
	{
		throw new NotSupportedException("Resizing chained streams is not supported");
	}

	/// <summary>
	///   Writes a sequence of bytes to the stream and advances the position of
	///   the file pointer by the number of bytes written.
	/// </summary>
	/// <param name="buffer">
	///   Buffer containing the data that will be written to the stream
	/// </param>
	/// <param name="offset">
	///   Offset in the buffer at which the data to be written starts
	/// </param>
	/// <param name="count">Number of bytes that will be written into the stream</param>
	/// <remarks>
	///   The behavior of this method is as follows: If one or more chained streams
	///   do not support seeking, all data is appended to the final stream in the
	///   chain. Otherwise, writing will begin with the stream the current file pointer
	///   offset falls into. If the end of that stream is reached, writing continues
	///   in the next stream. On the last stream, writing more data into the stream
	///   that it current size allows will enlarge the stream.
	/// </remarks>
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (!allStreamsCanWrite)
		{
			throw new NotSupportedException("Can't write: at least one of the chained streams doesn't support writing");
		}
		int num = count;
		if (allStreamsCanSeek)
		{
			findStreamIndexAndOffset(position, out var streamIndex, out var streamPosition);
			int num2 = streams.Length - 1;
			while (num > 0)
			{
				Stream stream = streams[streamIndex];
				if (streamIndex == num2)
				{
					stream.Position = streamPosition;
					stream.Write(buffer, offset, num);
					num = 0;
					continue;
				}
				long val = stream.Length - streamPosition;
				int num3 = (int)Math.Min(num, val);
				stream.Position = streamPosition;
				stream.Write(buffer, offset, num3);
				offset += num3;
				num -= num3;
				streamPosition = 0L;
				streamIndex++;
			}
		}
		else
		{
			Stream stream2 = streams[streams.Length - 1];
			if (stream2.CanSeek)
			{
				stream2.Seek(0L, SeekOrigin.End);
			}
			stream2.Write(buffer, offset, num);
		}
		position += count;
	}

	/// <summary>Moves the file pointer</summary>
	/// <param name="position">New position the file pointer will be moved to</param>
	private void moveFilePointer(long position)
	{
		if (!allStreamsCanSeek)
		{
			throw makeSeekNotSupportedException("seek");
		}
		this.position = position;
	}

	/// <summary>
	///   Finds the stream index and local offset for an absolute position within
	///   the combined streams.
	/// </summary>
	/// <param name="overallPosition">Absolute position within the combined streams</param>
	/// <param name="streamIndex">
	///   Index of the stream the overall position falls into
	/// </param>
	/// <param name="streamPosition">
	///   Local position within the stream indicated by <paramref name="streamIndex" />
	/// </param>
	private void findStreamIndexAndOffset(long overallPosition, out int streamIndex, out long streamPosition)
	{
		streamIndex = streams.Length - 1;
		for (int i = 0; i < streams.Length; i++)
		{
			long length = streams[i].Length;
			if (overallPosition < length)
			{
				streamIndex = i;
				break;
			}
			overallPosition -= length;
		}
		streamPosition = overallPosition;
	}

	/// <summary>Determines the capabilities of the chained streams</summary>
	/// <remarks>
	///   <para>
	///     Theoretically, it would be possible to create a stream chainer that supported
	///     writing only when the file pointer was on a chained stream with write support,
	///     that could seek within the beginning of the stream until the first chained
	///     stream with no seek capability was encountered and so on.
	///   </para>
	///   <para>
	///     However, the interface of the Stream class requires us to make a definitive
	///     statement as to whether the Stream supports seeking, reading and writing.
	///     We can't return "maybe" or "mostly" in CanSeek, so the only sane choice that
	///     doesn't violate the Stream interface is to implement these capabilities as
	///     all or nothing - either all streams support a feature, or the stream chainer
	///     will report the feature as unsupported.
	///   </para>
	/// </remarks>
	private void determineCapabilities()
	{
		allStreamsCanSeek = true;
		allStreamsCanRead = true;
		allStreamsCanWrite = true;
		for (int i = 0; i < streams.Length; i++)
		{
			allStreamsCanSeek &= streams[i].CanSeek;
			allStreamsCanRead &= streams[i].CanRead;
			allStreamsCanWrite &= streams[i].CanWrite;
		}
	}

	/// <summary>
	///   Constructs a NotSupportException for an error caused by one of the chained
	///   streams having no seek support
	/// </summary>
	/// <param name="action">Action that was tried to perform</param>
	/// <returns>The newly constructed NotSupportedException</returns>
	private static NotSupportedException makeSeekNotSupportedException(string action)
	{
		return new NotSupportedException($"Can't {action}: at least one of the chained streams does not support seeking");
	}
}
