using System;

using DisCatSharp.Voice.Entities.Dave;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Dave;

public class CryptorManagerTests
{
	private static readonly byte[] s_baseSecret = [
		0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x11, 0x22,
		0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00,
		0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x11, 0x22,
		0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0x00
	];

	private static CryptorManager CreateManager()
		=> new(new HashRatchet(s_baseSecret), key => new TestAeadCipher(key));

	// -------------------------------------------------------------------------
	// Encrypt / Decrypt round-trip
	// -------------------------------------------------------------------------

	[Fact]
	public void EncryptDecrypt_RoundTrip_Succeeds()
	{
		using var encManager = CreateManager();
		using var decManager = CreateManager();

		ReadOnlySpan<byte> plaintext = [0x01, 0x02, 0x03, 0x04, 0x05];
		ReadOnlySpan<byte> aad = [0xAA];
		const uint generation = 0;
		const uint nonce = 42;

		Span<byte> ciphertext = stackalloc byte[plaintext.Length];
		Span<byte> tag = stackalloc byte[8];
		encManager.Encrypt(generation, plaintext, ciphertext, tag, nonce, aad);

		Span<byte> recovered = stackalloc byte[plaintext.Length];
		var ok = decManager.TryDecrypt(generation, ciphertext, tag, recovered, nonce, aad);

		Assert.True(ok);
		Assert.Equal(plaintext.ToArray(), recovered.ToArray());
	}

	[Fact]
	public void TryDecrypt_WrongTag_ReturnsFalse()
	{
		using var encManager = CreateManager();
		using var decManager = CreateManager();

		ReadOnlySpan<byte> plaintext = [0xDE, 0xAD, 0xBE, 0xEF];
		Span<byte> ciphertext = stackalloc byte[plaintext.Length];
		Span<byte> tag = stackalloc byte[8];
		encManager.Encrypt(0, plaintext, ciphertext, tag, 1, []);

		// Corrupt the tag
		tag[0] ^= 0xFF;

		Span<byte> recovered = stackalloc byte[plaintext.Length];
		var ok = decManager.TryDecrypt(0, ciphertext, tag, recovered, 1, []);
		Assert.False(ok);
	}

	[Fact]
	public void TryDecrypt_WrongNonce_ReturnsFalse()
	{
		using var encManager = CreateManager();
		using var decManager = CreateManager();

		ReadOnlySpan<byte> plaintext = [0x01, 0x02];
		Span<byte> ciphertext = stackalloc byte[plaintext.Length];
		Span<byte> tag = stackalloc byte[8];
		encManager.Encrypt(0, plaintext, ciphertext, tag, 100, []);

		Span<byte> recovered = stackalloc byte[plaintext.Length];
		// Use wrong nonce for decryption
		var ok = decManager.TryDecrypt(0, ciphertext, tag, recovered, 999, []);
		Assert.False(ok);
	}

	// -------------------------------------------------------------------------
	// Generation gap enforcement
	// -------------------------------------------------------------------------

	[Fact]
	public void Encrypt_TooFarAheadGeneration_Throws()
	{
		using var manager = CreateManager();

		// First frame at generation 0 establishes the baseline
		var ct = new byte[1];
		var tag = new byte[8];
		manager.Encrypt(0, [0x01], ct, tag, 0, []);

		// Jump more than kMaxGenerationGap ahead
		var tooFarGeneration = (uint)(0 + CryptorManager.KMaxGenerationGap + 1);
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			manager.Encrypt(tooFarGeneration, [0x02], ct, tag, 1, []));
	}

	// -------------------------------------------------------------------------
	// Expiry
	// -------------------------------------------------------------------------

	[Fact]
	public void IsExpired_NoExpirySet_ReturnsFalse()
	{
		using var manager = CreateManager();
		Assert.False(manager.IsExpired);
	}

	[Fact]
	public void IsExpired_FutureExpiry_ReturnsFalse()
	{
		using var manager = CreateManager();
		manager.ExpiresAt = DateTime.UtcNow.AddSeconds(60);
		Assert.False(manager.IsExpired);
	}

	[Fact]
	public void IsExpired_PastExpiry_ReturnsTrue()
	{
		using var manager = CreateManager();
		manager.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
		Assert.True(manager.IsExpired);
	}

	// -------------------------------------------------------------------------
	// Dispose
	// -------------------------------------------------------------------------

	[Fact]
	public void Dispose_ThenEncrypt_Throws()
	{
		var manager = CreateManager();
		manager.Dispose();

		var ct = new byte[1];
		var tag = new byte[8];
		Assert.Throws<ObjectDisposedException>(() =>
			manager.Encrypt(0, [0x01], ct, tag, 0, []));
	}
}


