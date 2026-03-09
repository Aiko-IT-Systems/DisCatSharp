using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Per-sender key ratchet based on RFC 9420 §9.1 <c>ExpandWithLabel</c> using HKDF-SHA256.
///     Each call to <see cref="Get" /> returns the AES-GCM frame key and nonce base for the requested
///     generation and advances the ratchet so that past key material can be cleared from memory.
/// </summary>
internal sealed class HashRatchet
{
	/// <summary>
	///     Initial 32-byte secret for generation 0 (padded from the 16-byte MLS exporter output).
	/// </summary>
	private readonly byte[] _baseSecret;

	/// <summary>
	///     Cached secrets keyed by generation, so we can walk forward without recomputing from scratch.
	/// </summary>
	private readonly Dictionary<uint, byte[]> _secrets = [];

	/// <summary>
	///     The lowest generation we are still allowed to derive — enforces forward secrecy.
	/// </summary>
	private uint _minGeneration;

	/// <summary>
	///     Initialises the ratchet with the 32-byte base secret provided by the MLS exporter for generation 0.
	/// </summary>
	/// <param name="baseSecret">
	///     32-byte base secret from the MLS key exporter (SHA-256 cipher suite produces exactly 32 bytes).
	/// </param>
	/// <exception cref="ArgumentException">Thrown when <paramref name="baseSecret"/> is not exactly 32 bytes.</exception>
	public HashRatchet(ReadOnlySpan<byte> baseSecret)
	{
		if (baseSecret.Length < 32)
			throw new ArgumentException(
				$"Base secret must be at least 32 bytes for HKDF-SHA256 (got {baseSecret.Length}).", nameof(baseSecret));

		this._baseSecret = baseSecret.ToArray();

		// Seed the cache with generation 0's secret so Walk() has a starting point.
		this._secrets[0] = this._baseSecret;
	}

	/// <summary>
	///     Returns the <c>(key: 16 bytes, nonceBase: 12 bytes)</c> pair for the given <paramref name="generation" />.
	///     Advances the ratchet past that generation and clears earlier key material from the cache.
	/// </summary>
	/// <param name="generation">The frame generation to derive keys for.</param>
	/// <returns>A tuple of the 16-byte AES-GCM key and the 12-byte nonce base.</returns>
	/// <exception cref="InvalidOperationException">
	///     Thrown when <paramref name="generation" /> is less than <see cref="_minGeneration" />,
	///     indicating an attempt to backtrack to already-cleared key material.
	/// </exception>
	public (byte[] Key, byte[] NonceBase) Get(uint generation)
	{
		if (generation < this._minGeneration)
			throw new InvalidOperationException(
				$"Cannot derive keys for generation {generation}; ratchet has already advanced past generation {this._minGeneration - 1}.");

		// Walk forward from the lowest cached secret up to the requested generation.
		this.WalkTo(generation);

		var secret = this._secrets[generation];
		var key = ExpandWithLabel(secret, "key", generation, 16);
		var nonceBase = ExpandWithLabel(secret, "nonce", generation, 12);

		// Derive and cache the next secret so future calls can advance cheaply.
		var nextSecret = ExpandWithLabel(secret, "secret", generation, 32);
		this._secrets[generation + 1] = nextSecret;

		// Advance the ratchet — everything before 'generation' can be forgotten.
		this._minGeneration = generation + 1;
		for (var g = generation; g > 0 && this._secrets.ContainsKey(g - 1); g--)
		{
			CryptographicOperations.ZeroMemory(this._secrets[g - 1]);
			this._secrets.Remove(g - 1);
		}

		// Also clear the current generation's secret now that we've derived from it.
		CryptographicOperations.ZeroMemory(this._secrets[generation]);
		this._secrets.Remove(generation);

		return (key, nonceBase);
	}

	/// <summary>
	///     Ensures _secrets contains an entry for every generation from the current lowest up to (and including) target.
	/// </summary>
	private void WalkTo(uint target)
	{
		// Find the highest cached generation that is <= target.
		uint cursor = 0;
		for (var g = target; ; g--)
		{
			if (this._secrets.ContainsKey(g))
			{
				cursor = g;
				break;
			}

			if (g == 0)
			{
				// Should not happen because the constructor seeds generation 0.
				throw new InvalidOperationException("Ratchet cache is unexpectedly empty.");
			}
		}

		// Walk forward from cursor to target, deriving each generation's secret.
		while (cursor < target)
		{
			var currentSecret = this._secrets[cursor];
			var nextSecret = ExpandWithLabel(currentSecret, "secret", cursor, 32);
			this._secrets[cursor + 1] = nextSecret;
			cursor++;
		}
	}

	/// <summary>
	///     Derives output material using the RFC 9420 §9.1 <c>ExpandWithLabel</c> construction.
	/// </summary>
	private static byte[] ExpandWithLabel(byte[] prk, string labelName, uint generation, int outputLength)
		=> HKDF.Expand(HashAlgorithmName.SHA256, prk, outputLength, BuildKdfLabel(labelName, generation, outputLength));

	/// <summary>
	///     Builds the TLS-serialized <c>KdfLabel</c> info buffer:
	///     <c>[outputLength: u16 BE][labelLength: u8][label bytes][contextLength: u8][context bytes]</c>
	///     where label = <c>"MLS 1.0 " + labelName</c> and context = generation as big-endian uint32.
	/// </summary>
	private static byte[] BuildKdfLabel(string labelName, uint generation, int outputLength)
	{
		var label = "MLS 1.0 " + labelName;
		var labelBytes = Encoding.ASCII.GetBytes(label);
		var contextBytes = new byte[4];
		BinaryPrimitives.WriteUInt32BigEndian(contextBytes, generation);

		// Layout: u16 + u8 + labelBytes + u8 + contextBytes
		var result = new byte[2 + 1 + labelBytes.Length + 1 + contextBytes.Length];
		var span = result.AsSpan();

		BinaryPrimitives.WriteUInt16BigEndian(span[..2], (ushort)outputLength);
		span[2] = (byte)labelBytes.Length;
		labelBytes.CopyTo(span[3..]);
		span[3 + labelBytes.Length] = (byte)contextBytes.Length;
		contextBytes.CopyTo(span[(3 + labelBytes.Length + 1)..]);

		return result;
	}
}
