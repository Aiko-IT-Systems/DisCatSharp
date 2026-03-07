using System;
using System.Collections.Generic;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Static utility class for reading and writing DAVE per-frame encryption metadata.
///     Handles codec-specific unencrypted ranges, footer serialization, and silence detection.
/// </summary>
internal static class DaveFrameProcessor
{
	// Magic marker appended at the end of every DAVE-encrypted frame.
	private const byte MAGIC = 0xFA;

	// DAVE wire format uses an 8-byte authentication tag. This is a protocol constant
	// independent of the cipher backend (see DaveConstants.TagSize).
	private const int TAG_SIZE = DaveConstants.TagSize;

	/// <summary>
	///     Returns <c>true</c> if <paramref name="frame"/> is the Opus DTX/silence frame
	///     <c>[0xF8, 0xFF, 0xFE]</c>. These frames must bypass encryption entirely.
	/// </summary>
	public static bool IsSignalFrame(ReadOnlySpan<byte> frame)
		=> frame.Length == 3 && frame[0] == 0xF8 && frame[1] == 0xFF && frame[2] == 0xFE;

	/// <summary>
	///     Returns the byte ranges within the plaintext that must remain unencrypted
	///     according to the DAVE spec for the given codec.
	/// </summary>
	/// <remarks>
	///     For Opus the first byte (TOC byte) is always left in the clear.
	///     All other codecs currently return an empty list; RTP-level unencrypted header
	///     handling for video codecs is not implemented yet.
	/// </remarks>
	public static IReadOnlyList<(int Offset, int Length)> GetUnencryptedRanges(DaveCodec codec, ReadOnlySpan<byte> frame)
	{
		if (codec == DaveCodec.Opus && frame.Length > 0)
			return [(0, 1)];

		return [];
	}

	/// <summary>
	///     Writes a DAVE frame footer into <paramref name="dest"/> starting at offset 0.
	/// </summary>
	/// <param name="dest">Destination buffer. Must be large enough for the footer.</param>
	/// <param name="tag">8-byte AES-128-GCM authentication tag.</param>
	/// <param name="truncatedNonce">32-bit per-frame nonce counter.</param>
	/// <param name="unencryptedRanges">Unencrypted byte ranges from the plaintext.</param>
	/// <returns>Total number of bytes written (= full footer size).</returns>
	/// <remarks>
	///     Footer layout (in order):
	///     <list type="number">
	///         <item>8-byte auth tag</item>
	///         <item>ULEB128-encoded truncated nonce</item>
	///         <item>ULEB128-encoded range count, then each offset and length</item>
	///         <item>1-byte supplementalSize (tag + nonce bytes + ranges bytes)</item>
	///         <item>0xFA, 0xFA magic marker</item>
	///     </list>
	/// </remarks>
	public static int WriteFooter(
		Span<byte> dest,
		ReadOnlySpan<byte> tag,
		uint truncatedNonce,
		IReadOnlyList<(int Offset, int Length)> unencryptedRanges)
	{
		if (tag.Length != TAG_SIZE)
			throw new ArgumentException($"Tag must be exactly {TAG_SIZE} bytes.", nameof(tag));

		var pos = 0;

		// 1. Auth tag
		tag.CopyTo(dest[pos..]);
		pos += TAG_SIZE;

		// 2. ULEB128-encoded truncated nonce
		var nonceBytes = Uleb128.Write(truncatedNonce, dest[pos..]);
		pos += nonceBytes;

		// 3. ULEB128-encoded ranges descriptor: count, then (offset, length) pairs
		pos += Uleb128.Write((uint)unencryptedRanges.Count, dest[pos..]);
		foreach (var (offset, length) in unencryptedRanges)
		{
			pos += Uleb128.Write((uint)offset, dest[pos..]);
			pos += Uleb128.Write((uint)length, dest[pos..]);
		}

		// supplementalSize = total bytes written for [tag][nonce][ranges] — the parser uses this to locate the footer start
		var supplementalSize = pos;

		// 4. supplementalSize byte
		dest[pos++] = (byte)supplementalSize;

		// 5. Magic marker
		dest[pos++] = MAGIC;
		dest[pos++] = MAGIC;

		return pos;
	}

	/// <summary>
	///     Attempts to parse the DAVE footer appended to the end of <paramref name="frame"/>.
	/// </summary>
	/// <param name="frame">Full frame bytes including the footer.</param>
	/// <param name="footer">Populated on success.</param>
	/// <returns><c>true</c> if a valid DAVE footer was found; <c>false</c> otherwise.</returns>
	public static bool TryReadFooter(ReadOnlySpan<byte> frame, out DaveFrameFooter footer)
	{
		footer = default;

		// Need at least: magic(2) + supplementalSize(1) = 3 bytes
		if (frame.Length < 3)
			return false;

		// Validate magic marker
		if (frame[^2] != MAGIC || frame[^1] != MAGIC)
			return false;

		int supplementalSize = frame[^3];

		// Total footer = supplementalSize + 1 (size byte) + 2 (magic)
		var totalFooterSize = supplementalSize + 3;
		if (frame.Length < totalFooterSize)
			return false;

		// Footer content starts here
		var contentStart = frame.Length - 3 - supplementalSize;
		var content = frame.Slice(contentStart, supplementalSize);

		var cursor = 0;

		// Must have at least 8 bytes for the tag
		if (content.Length < TAG_SIZE)
			return false;

		var tagMemory = frame.Slice(contentStart, TAG_SIZE).ToArray();
		cursor += TAG_SIZE;

		// ULEB128 truncated nonce
		var truncatedNonce = Uleb128.Read(content[cursor..], out var nonceRead);
		cursor += nonceRead;

		// ULEB128 ranges count
		var rangesCount = Uleb128.Read(content[cursor..], out var countRead);
		cursor += countRead;

		var ranges = new List<(int Offset, int Length)>((int)rangesCount);
		for (uint i = 0; i < rangesCount; i++)
		{
			var rangeOffset = Uleb128.Read(content[cursor..], out var offRead);
			cursor += offRead;
			var rangeLength = Uleb128.Read(content[cursor..], out var lenRead);
			cursor += lenRead;
			ranges.Add(((int)rangeOffset, (int)rangeLength));
		}

		footer = new DaveFrameFooter
		{
			TruncatedNonce = truncatedNonce,
			Tag = tagMemory,
			UnencryptedRanges = ranges,
			FooterSize = totalFooterSize
		};

		return true;
	}
}
