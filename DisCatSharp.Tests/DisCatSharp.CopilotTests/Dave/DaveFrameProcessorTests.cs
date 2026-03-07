using System;
using System.Collections.Generic;

using DisCatSharp.Voice.Dave;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Dave;

public class DaveFrameProcessorTests
{
	// -------------------------------------------------------------------------
	// Silence detection
	// -------------------------------------------------------------------------

	[Fact]
	public void IsSignalFrame_SilenceFrame_ReturnsTrue()
	{
		ReadOnlySpan<byte> silence = [0xF8, 0xFF, 0xFE];
		Assert.True(DaveFrameProcessor.IsSignalFrame(silence));
	}

	[Fact]
	public void IsSignalFrame_NormalOpusFrame_ReturnsFalse()
	{
		ReadOnlySpan<byte> normal = [0x78, 0x00, 0x01, 0x02];
		Assert.False(DaveFrameProcessor.IsSignalFrame(normal));
	}

	[Fact]
	public void IsSignalFrame_EmptyFrame_ReturnsFalse()
		=> Assert.False(DaveFrameProcessor.IsSignalFrame([]));

	// -------------------------------------------------------------------------
	// Unencrypted ranges
	// -------------------------------------------------------------------------

	[Fact]
	public void GetUnencryptedRanges_Opus_ReturnsFirstByte()
	{
		ReadOnlySpan<byte> frame = [0x78, 0x00, 0x01];
		var ranges = DaveFrameProcessor.GetUnencryptedRanges(DaveCodec.Opus, frame);

		Assert.Single(ranges);
		Assert.Equal((0, 1), ranges[0]);
	}

	[Fact]
	public void GetUnencryptedRanges_Unknown_ReturnsEmpty()
	{
		ReadOnlySpan<byte> frame = [0x00, 0x01, 0x02];
		var ranges = DaveFrameProcessor.GetUnencryptedRanges(DaveCodec.Unknown, frame);
		Assert.Empty(ranges);
	}

	// -------------------------------------------------------------------------
	// Footer write / read round-trip
	// -------------------------------------------------------------------------

	[Fact]
	public void WriteAndReadFooter_RoundTrip_Succeeds()
	{
		var tag = new byte[8] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };
		const uint truncatedNonce = 0x01020304u;
		var ranges = new List<(int, int)> { (0, 1) };

		Span<byte> footerBuf = stackalloc byte[64];
		var written = DaveFrameProcessor.WriteFooter(footerBuf, tag, truncatedNonce, ranges);

		Assert.True(written > 0);

		// Verify magic marker at the end
		Assert.Equal(0xFA, footerBuf[written - 2]);
		Assert.Equal(0xFA, footerBuf[written - 1]);

		// Parse the footer back
		var frame = footerBuf[..written].ToArray();
		Assert.True(DaveFrameProcessor.TryReadFooter(frame, out var footer));

		Assert.Equal(truncatedNonce, footer.TruncatedNonce);
		Assert.Equal(written, footer.FooterSize);
		Assert.Equal(tag, footer.Tag.ToArray());
		Assert.Single(footer.UnencryptedRanges);
		Assert.Equal((0, 1), footer.UnencryptedRanges[0]);
	}

	[Fact]
	public void WriteAndReadFooter_NoRanges_RoundTrip_Succeeds()
	{
		var tag = new byte[8];
		var ranges = new List<(int, int)>();

		Span<byte> footerBuf = stackalloc byte[64];
		var written = DaveFrameProcessor.WriteFooter(footerBuf, tag, 0, ranges);

		var frame = footerBuf[..written].ToArray();
		Assert.True(DaveFrameProcessor.TryReadFooter(frame, out var footer));

		Assert.Equal(0u, footer.TruncatedNonce);
		Assert.Empty(footer.UnencryptedRanges);
		Assert.Equal(written, footer.FooterSize);
	}

	[Fact]
	public void TryReadFooter_NoMagicMarker_ReturnsFalse()
	{
		ReadOnlySpan<byte> frame = [0x01, 0x02, 0x03, 0x04];
		Assert.False(DaveFrameProcessor.TryReadFooter(frame, out _));
	}

	[Fact]
	public void TryReadFooter_TooShort_ReturnsFalse()
	{
		ReadOnlySpan<byte> frame = [0xFA, 0xFA]; // only 2 bytes
		Assert.False(DaveFrameProcessor.TryReadFooter(frame, out _));
	}

	[Fact]
	public void WriteFooter_WrongTagLength_Throws()
	{
		var buf = new byte[64];
		var badTag = new byte[7]; // wrong: DAVE protocol requires exactly 8-byte tags
		Assert.Throws<ArgumentException>(() =>
			DaveFrameProcessor.WriteFooter(buf, badTag, 0, []));
	}

	[Fact]
	public void WriteAndReadFooter_LargeNonce_RoundTrip()
	{
		var tag = new byte[8];
		const uint nonce = uint.MaxValue;

		Span<byte> buf = stackalloc byte[64];
		var written = DaveFrameProcessor.WriteFooter(buf, tag, nonce, []);

		var frame = buf[..written].ToArray();
		Assert.True(DaveFrameProcessor.TryReadFooter(frame, out var footer));
		Assert.Equal(nonce, footer.TruncatedNonce);
	}
}


