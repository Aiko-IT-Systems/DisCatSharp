using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.VoiceNext;

/// <summary>
///     The stream extensions.
/// </summary>
public static class StreamExtensions
{
	/// <summary>
	///     Asynchronously reads the bytes from the current stream and writes them to the specified
	///     <see cref="VoiceTransmitSink" />.
	/// </summary>
	/// <param name="source">The source <see cref="Stream" /></param>
	/// <param name="destination">The target <see cref="VoiceTransmitSink" /></param>
	/// <param name="bufferSize">
	///     The size, in bytes, of the buffer. This value must be greater than zero. If
	///     <see langword="null" />, defaults to the packet size specified by <paramref name="destination" />.
	/// </param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <returns></returns>
	public static async Task CopyToAsync(this Stream source, VoiceTransmitSink destination, int? bufferSize = null, CancellationToken cancellationToken = default)
	{
		// adapted from CoreFX
		// https://source.dot.net/#System.Private.CoreLib/Stream.cs,8048a9680abdd13b

		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);
		if (bufferSize is <= 0)
			throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize, "bufferSize cannot be less than or equal to zero");

		var bufferLength = bufferSize ?? destination.SampleLength;
		var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
		try
		{
			int bytesRead;
			while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, bufferLength), cancellationToken).ConfigureAwait(false)) is not 0)
				await destination.WriteAsync(new(buffer, 0, bytesRead), cancellationToken).ConfigureAwait(false);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}
