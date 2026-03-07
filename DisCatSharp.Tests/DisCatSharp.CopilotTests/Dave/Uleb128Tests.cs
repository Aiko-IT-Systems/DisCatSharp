using System;

using DisCatSharp.Voice.Dave;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Dave;

public class Uleb128Tests
{
	// Round-trip helper
	private static uint RoundTrip(uint value, out int written, out int read)
	{
		Span<byte> buf = stackalloc byte[8];
		written = Uleb128.Write(value, buf);
		var result = Uleb128.Read(buf[..written], out read);
		return result;
	}

	[Theory]
	[InlineData(0u, 1)]
	[InlineData(1u, 1)]
	[InlineData(127u, 1)]
	[InlineData(128u, 2)]
	[InlineData(16383u, 2)]
	[InlineData(16384u, 3)]
	[InlineData(uint.MaxValue, 5)]
	public void Write_ProducesExpectedByteCount(uint value, int expectedBytes)
	{
		Span<byte> buf = stackalloc byte[8];
		var written = Uleb128.Write(value, buf);
		Assert.Equal(expectedBytes, written);
	}

	[Theory]
	[InlineData(0u)]
	[InlineData(1u)]
	[InlineData(127u)]
	[InlineData(128u)]
	[InlineData(16383u)]
	[InlineData(16384u)]
	[InlineData(uint.MaxValue)]
	public void RoundTrip_PreservesValue(uint value)
	{
		var decoded = RoundTrip(value, out var written, out var read);
		Assert.Equal(value, decoded);
		Assert.Equal(written, read);
	}

	[Fact]
	public void Write_Zero_ProducesSingleZeroByte()
	{
		Span<byte> buf = stackalloc byte[4];
		var written = Uleb128.Write(0, buf);
		Assert.Equal(1, written);
		Assert.Equal(0, buf[0]);
	}

	[Fact]
	public void Write_127_ProducesSingleByte_NoMSB()
	{
		Span<byte> buf = stackalloc byte[4];
		Uleb128.Write(127, buf);
		Assert.Equal(0x7F, buf[0]);
	}

	[Fact]
	public void Write_128_ProducesTwoBytes_FirstHasMSB()
	{
		Span<byte> buf = stackalloc byte[4];
		Uleb128.Write(128, buf);
		Assert.Equal(0x80, buf[0]); // 128 & 0x7F = 0, with continuation bit
		Assert.Equal(0x01, buf[1]); // high 7 bits = 1
	}

	[Fact]
	public void Read_ConsumesOnlyEncodedBytes()
	{
		// Write two values back-to-back and verify read stops at the right place
		Span<byte> buf = stackalloc byte[16];
		var w1 = Uleb128.Write(300, buf);
		var w2 = Uleb128.Write(42, buf[w1..]);

		var v1 = Uleb128.Read(buf, out var r1);
		Assert.Equal(300u, v1);
		Assert.Equal(w1, r1);

		var v2 = Uleb128.Read(buf[r1..], out var r2);
		Assert.Equal(42u, v2);
		Assert.Equal(w2, r2);
	}

	[Fact]
	public void Read_TruncatedInput_Throws()
	{
		// A byte with continuation bit set but no following byte
		byte[] bad = [0x80];
		Assert.Throws<InvalidOperationException>(() => Uleb128.Read(bad, out _));
	}

	[Fact]
	public void Write_BufferTooSmall_Throws()
	{
		byte[] buf = new byte[1];
		Assert.Throws<ArgumentException>(() => Uleb128.Write(128, buf));
	}
}


