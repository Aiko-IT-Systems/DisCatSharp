using System;
using System.Buffers;

using DisCatSharp.Voice.Dave;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Dave;

public class DaveEncryptorDecryptorTests
{
	private static readonly byte[] s_baseSecret = [
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
		0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
		0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10
	];

	private static DaveEncryptor CreateEncryptor()
	{
		var enc = new DaveEncryptor(key => new TestAeadCipher(key));
		enc.SetKeyRatchet(new HashRatchet(s_baseSecret));
		return enc;
	}

	private static DaveDecryptor CreateDecryptor()
	{
		var dec = new DaveDecryptor(key => new TestAeadCipher(key));
		dec.TransitionToKeyRatchet(new HashRatchet(s_baseSecret));
		return dec;
	}

	// -------------------------------------------------------------------------
	// Full encrypt → decrypt round-trip
	// -------------------------------------------------------------------------

	[Fact]
	public void EncryptDecrypt_OpusFrame_RoundTrip()
	{
		using var enc = CreateEncryptor();
		using var dec = CreateDecryptor();

		// 5-byte Opus frame: 1 byte TOC + 4 bytes payload
		ReadOnlySpan<byte> original = [0x78, 0x01, 0x02, 0x03, 0x04];

		Assert.True(enc.TryEncrypt(original, ssrc: 0, out var encBuf, out var encLen));
		Assert.NotNull(encBuf);
		Assert.True(encLen > original.Length); // must include footer overhead
		try
		{
			Assert.True(dec.TryDecrypt(encBuf.AsSpan(0, encLen), out var decBuf, out var decLen));
			try
			{
				Assert.NotNull(decBuf);
				Assert.Equal(original.Length, decLen);
				Assert.Equal(original.ToArray(), decBuf.AsSpan(0, decLen).ToArray());
			}
			finally { ArrayPool<byte>.Shared.Return(decBuf); }
		}
		finally { ArrayPool<byte>.Shared.Return(encBuf); }
	}

	[Fact]
	public void Encrypt_MultipleFrames_AllDecryptCorrectly()
	{
		using var enc = CreateEncryptor();
		using var dec = CreateDecryptor();

		for (var i = 0; i < 5; i++)
		{
			byte[] frame = [0x60, (byte)i, (byte)(i * 2), (byte)(i * 3)];

			Assert.True(enc.TryEncrypt(frame, ssrc: 0, out var encBuf, out var encLen), $"Failed to encrypt frame {i}");
			try
			{
				Assert.True(dec.TryDecrypt(encBuf.AsSpan(0, encLen), out var decBuf, out var decLen), $"Failed to decrypt frame {i}");
				try
				{
					Assert.Equal(frame, decBuf.AsSpan(0, decLen).ToArray());
				}
				finally { ArrayPool<byte>.Shared.Return(decBuf); }
			}
			finally { ArrayPool<byte>.Shared.Return(encBuf); }
		}
	}

	// -------------------------------------------------------------------------
	// Silence passthrough
	// -------------------------------------------------------------------------

	[Fact]
	public void Encrypt_SilenceFrame_ReturnsFalse()
	{
		using var enc = CreateEncryptor();
		ReadOnlySpan<byte> silence = [0xF8, 0xFF, 0xFE];

		var encrypted = enc.TryEncrypt(silence, ssrc: 0, out _, out _);
		Assert.False(encrypted); // silence must NOT be encrypted
	}

	// -------------------------------------------------------------------------
	// Encryptor passthrough mode
	// -------------------------------------------------------------------------

	[Fact]
	public void Encrypt_PassthroughMode_ReturnsFalse()
	{
		using var enc = new DaveEncryptor(key => new TestAeadCipher(key)); // default: passthrough = true
		ReadOnlySpan<byte> frame = [0x78, 0x01, 0x02];

		var encrypted = enc.TryEncrypt(frame, ssrc: 0, out _, out _);
		Assert.False(encrypted);
	}

	[Fact]
	public void Encrypt_SetPassthroughTrue_ReturnsFalse()
	{
		using var enc = CreateEncryptor();
		enc.SetPassthrough(true);
		ReadOnlySpan<byte> frame = [0x78, 0x01, 0x02];

		Assert.False(enc.TryEncrypt(frame, ssrc: 0, out _, out _));
	}

	// -------------------------------------------------------------------------
	// Decryptor passthrough window
	// -------------------------------------------------------------------------

	[Fact]
	public void Decrypt_UnencryptedFrameInsidePassthroughWindow_ReturnsTrue()
	{
		using var dec = new DaveDecryptor(key => new TestAeadCipher(key));
		dec.TransitionToPassthrough(TimeSpan.FromSeconds(30));

		ReadOnlySpan<byte> plainFrame = [0x78, 0x01, 0x02, 0x03];
		Assert.True(dec.TryDecrypt(plainFrame, out var result, out var len));
		try
		{
			Assert.Equal(plainFrame.ToArray(), result.AsSpan(0, len).ToArray());
		}
		finally { ArrayPool<byte>.Shared.Return(result); }
	}

	[Fact]
	public void Decrypt_UnencryptedFrameOutsidePassthroughWindow_ReturnsFalse()
	{
		using var dec = new DaveDecryptor(key => new TestAeadCipher(key)); // no ratchet, no passthrough window
		ReadOnlySpan<byte> plainFrame = [0x78, 0x01, 0x02];

		var ok = dec.TryDecrypt(plainFrame, out _, out _);
		Assert.False(ok);
	}

	// -------------------------------------------------------------------------
	// Epoch transition: old-epoch frames still decrypt during retention window
	// -------------------------------------------------------------------------

	[Fact]
	public void Decrypt_OldEpochFrame_StillSucceedsDuringRetentionWindow()
	{
		// Encrypt a frame under epoch 1 ratchet
		using var enc = new DaveEncryptor(key => new TestAeadCipher(key));
		enc.SetKeyRatchet(new HashRatchet(s_baseSecret));

		ReadOnlySpan<byte> frame = [0x78, 0xAA, 0xBB, 0xCC];
		Assert.True(enc.TryEncrypt(frame, ssrc: 0, out var encBuf, out var encLen));

		// Decryptor has epoch 1, then transitions to epoch 2
		using var dec = new DaveDecryptor(key => new TestAeadCipher(key));
		dec.TransitionToKeyRatchet(new HashRatchet(s_baseSecret));

		// Transition to a new ratchet (different secret) — epoch 1 manager is retained
		var newSecret = new byte[32];
		new Random(42).NextBytes(newSecret);
		dec.TransitionToKeyRatchet(new HashRatchet(newSecret));

		// Old epoch frame should still decrypt via retained manager
		try
		{
			Assert.True(dec.TryDecrypt(encBuf.AsSpan(0, encLen), out var decBuf, out var decLen), "Expected old-epoch frame to decrypt via retained manager");
			try
			{
				Assert.Equal(frame.ToArray(), decBuf.AsSpan(0, decLen).ToArray());
			}
			finally { ArrayPool<byte>.Shared.Return(decBuf); }
		}
		finally { ArrayPool<byte>.Shared.Return(encBuf); }
	}

	// -------------------------------------------------------------------------
	// Dispose safety
	// -------------------------------------------------------------------------

	[Fact]
	public void Dispose_CanCallMultipleTimes_NoCrash()
	{
		var enc = CreateEncryptor();
		enc.Dispose();
		enc.Dispose(); // second dispose should not throw
	}
}

