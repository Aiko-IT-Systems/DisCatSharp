using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

using DisCatSharp.Voice.Entities.Dave;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Dave;

public class HashRatchetTests
{
	// A deterministic 32-byte base secret for reproducible test vectors (SHA-256 requires >= 32-byte PRK).
	private static readonly byte[] s_baseSecret = [
		0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
		0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
		0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
		0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
	];

	/// <summary>Re-derives the same key/nonce as HashRatchet.Get using raw HKDF so tests are self-contained.</summary>
	private static byte[] ReferenceDeriveKey(byte[] secret, string labelName, uint generation, int length)
	{
		var label = "MLS 1.0 " + labelName;
		var labelBytes = Encoding.ASCII.GetBytes(label);
		var contextBytes = new byte[4];
		BinaryPrimitives.WriteUInt32BigEndian(contextBytes, generation);

		// KdfLabel: [outputLength: u16 BE][labelLength: u8][labelBytes][contextLength: u8][contextBytes]
		var info = new byte[2 + 1 + labelBytes.Length + 1 + contextBytes.Length];
		BinaryPrimitives.WriteUInt16BigEndian(info.AsSpan(0, 2), (ushort)length);
		info[2] = (byte)labelBytes.Length;
		labelBytes.CopyTo(info, 3);
		info[3 + labelBytes.Length] = (byte)contextBytes.Length;
		contextBytes.CopyTo(info, 3 + labelBytes.Length + 1);

		return HKDF.Expand(HashAlgorithmName.SHA256, secret, length, info);
	}

	[Fact]
	public void Get_Generation0_ReturnsExpectedKeyAndNonce()
	{
		var ratchet = new HashRatchet(s_baseSecret);
		var (key, nonceBase) = ratchet.Get(0);

		var expectedKey = ReferenceDeriveKey(s_baseSecret, "key", 0, 16);
		var expectedNonce = ReferenceDeriveKey(s_baseSecret, "nonce", 0, 12);

		Assert.Equal(expectedKey, key);
		Assert.Equal(expectedNonce, nonceBase);
	}

	[Fact]
	public void Get_KeyIs16Bytes_NonceIs12Bytes()
	{
		var ratchet = new HashRatchet(s_baseSecret);
		var (key, nonceBase) = ratchet.Get(0);

		Assert.Equal(16, key.Length);
		Assert.Equal(12, nonceBase.Length);
	}

	[Fact]
	public void Get_DifferentGenerations_ProduceDifferentKeys()
	{
		// Use two separate ratchets since Get() is forward-only
		var ratchet0 = new HashRatchet(s_baseSecret);
		var ratchet1 = new HashRatchet(s_baseSecret);

		var (key0, _) = ratchet0.Get(0);
		var (key1, _) = ratchet1.Get(1);

		Assert.NotEqual(key0, key1);
	}

	[Fact]
	public void Get_CanAdvanceToHigherGeneration()
	{
		var ratchet = new HashRatchet(s_baseSecret);
		var (key2, nonce2) = ratchet.Get(2);

		Assert.Equal(16, key2.Length);
		Assert.Equal(12, nonce2.Length);
	}

	[Fact]
	public void Get_Backtrack_Throws()
	{
		var ratchet = new HashRatchet(s_baseSecret);
		ratchet.Get(1); // advance ratchet to minGeneration = 2
		Assert.Throws<InvalidOperationException>(() => ratchet.Get(0));
	}

	[Fact]
	public void Get_SameGenerationTwice_Throws()
	{
		var ratchet = new HashRatchet(s_baseSecret);
		ratchet.Get(0); // minGeneration becomes 1
		Assert.Throws<InvalidOperationException>(() => ratchet.Get(0));
	}

	[Fact]
	public void Get_Generation1_MatchesManualDerivation()
	{
		// Manually derive secret for generation 1 from base secret, then derive key from that
		var secret1 = ReferenceDeriveKey(s_baseSecret, "secret", 0, 32);
		var expectedKey1 = ReferenceDeriveKey(secret1, "key", 1, 16);

		var ratchet2 = new HashRatchet(s_baseSecret);
		var (key1, _) = ratchet2.Get(1);

		Assert.Equal(expectedKey1, key1);
	}
}

