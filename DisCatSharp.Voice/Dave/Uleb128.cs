using System;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Helpers for unsigned LEB128 encoding and decoding as used in DAVE frame footer supplemental data.
/// </summary>
internal static class Uleb128
{
	/// <summary>
	///     Encodes <paramref name="value" /> into <paramref name="target" /> using unsigned LEB128 (little-endian base-128).
	/// </summary>
	/// <param name="value">The value to encode.</param>
	/// <param name="target">The buffer to write into.</param>
	/// <returns>The number of bytes written.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="target" /> is too small to hold the encoded value.</exception>
	public static int Write(uint value, Span<byte> target)
	{
		var pos = 0;
		do
		{
			if (pos >= target.Length)
				throw new ArgumentException("Target span is too small to hold the ULEB128-encoded value.", nameof(target));

			var byteVal = (byte)(value & 0x7F);
			value >>= 7;
			if (value != 0)
				byteVal |= 0x80;

			target[pos++] = byteVal;
		} while (value != 0);

		return pos;
	}

	/// <summary>
	///     Decodes a ULEB128-encoded value from <paramref name="source" />.
	/// </summary>
	/// <param name="source">The buffer to read from.</param>
	/// <param name="bytesRead">Set to the number of bytes consumed from <paramref name="source" />.</param>
	/// <returns>The decoded unsigned 32-bit value.</returns>
	/// <exception cref="InvalidOperationException">
	///     Thrown when the encoded value exceeds the range of <see cref="uint" />,
	///     or when the input ends before the value is complete.
	/// </exception>
	public static uint Read(ReadOnlySpan<byte> source, out int bytesRead)
	{
		uint result = 0;
		var shift = 0;
		bytesRead = 0;

		while (true)
		{
			if (bytesRead >= source.Length)
				throw new InvalidOperationException("Input ended before the ULEB128 value was complete.");

			if (shift >= 32)
				throw new InvalidOperationException("ULEB128 value overflows uint32.");

			var b = source[bytesRead++];
			result |= (uint)(b & 0x7F) << shift;
			shift += 7;

			if ((b & 0x80) == 0)
				break;
		}

		return result;
	}
}
