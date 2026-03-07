using System;
using System.Buffers;
using System.Threading;

using DisCatSharp.Voice.Dave.Interop;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     <see cref="IDaveEncryptor"/> backed by the native libdave encryptor.
///     One instance per <c>VoiceConnection</c> (outbound direction).
/// </summary>
internal sealed class LibDaveEncryptor : IDaveEncryptor
{
	/// <summary>Integer constant passed to libdave to identify audio media frames.</summary>
	private const int DaveMediaTypeAudio = 0;

	/// <summary>Codec constant for Opus audio (matches DAVECodec enum in dave.h).</summary>
	private const int DaveCodecOpus = 1;

	/// <summary>Maximum number of overhead bytes the native encryptor may add beyond the plaintext length.</summary>
	private const int MaxEncryptedOverhead = 256;

	private readonly DaveEncryptorSafeHandle _handle;

	// Native state is mutable; lock is required rather than atomic swap.
	// The _handle points to opaque libdave memory that is mutated in-place by every API call
	// (EncryptorSetKeyRatchet, EncryptorEncrypt, EncryptorSetPassthroughMode).  There is no
	// safe way to snapshot or atomically exchange this state, so lock(_sync) serialises all
	// operations on the native handle.
	private readonly Lock _sync = new();
	private bool _isActive;
	private uint _registeredSsrc;
	private bool _ssrcRegistered;

	/// <summary>
	///     Creates the native libdave encryptor handle via <see cref="DaveNative.EncryptorCreate"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when the native encryptor could not be created.</exception>
	public LibDaveEncryptor()
	{
		this._handle = DaveNative.EncryptorCreate();
		if (this._handle.IsInvalid)
			throw new InvalidOperationException("[DAVE] Failed to create native encryptor.");
	}

	/// <inheritdoc/>
	public bool IsActive => this._isActive;

	/// <inheritdoc/>
	public void InstallRatchet(DaveRatchetInstaller installer)
	{
		if (!installer.IsNative || installer.NativeHandle is null || installer.NativeHandle.IsInvalid)
			throw new ArgumentException("LibDaveEncryptor requires a valid native ratchet handle.", nameof(installer));

		lock (this._sync)
		{
			DaveNative.EncryptorSetKeyRatchet(this._handle, installer.NativeHandle);
			installer.NativeHandle!.Dispose(); // libdave copied the ratchet; release our reference
			this._isActive = true;
		}
	}

	/// <inheritdoc/>
	public void SetPassthrough(bool passthrough)
	{
		lock (this._sync)
		{
			DaveNative.EncryptorSetPassthroughMode(this._handle, passthrough);
			if (passthrough)
				this._isActive = false;
		}
	}

	/// <inheritdoc/>
	public unsafe bool TryEncrypt(ReadOnlySpan<byte> frame, uint ssrc, out byte[] result, out int resultLength)
	{
		result = null!;
		resultLength = 0;

		lock (this._sync)
		{
			if (!this._isActive || frame.IsEmpty)
				return false;

			var outCapacity = (nuint)(frame.Length + MaxEncryptedOverhead);
			var rented = ArrayPool<byte>.Shared.Rent((int)outCapacity);
			var success = false;
			try
			{
				fixed (byte* pFrame = frame)
				fixed (byte* pOut = rented)
				{
					// Register this SSRC→Opus mapping on first use so libdave applies correct frame
					// partitioning. Without this, CodecForSsrc returns Unknown and triggers an assert
					// in debug builds; the fallback behavior is identical for Opus, but explicit is safer.
					if (!this._ssrcRegistered || this._registeredSsrc != ssrc)
					{
						DaveNative.EncryptorAssignSsrcToCodec(this._handle, ssrc, DaveCodecOpus);
						this._registeredSsrc = ssrc;
						this._ssrcRegistered = true;
					}

					// ssrc is the connection's own RTP SSRC (from the READY payload).
					// libdave embeds it in the SFrame header for sender identification;
					// passing 0 would produce frames with incorrect sender metadata.
					var rc = DaveNative.EncryptorEncrypt(
						this._handle,
						DaveMediaTypeAudio,
						ssrc,
						pFrame, (nuint)frame.Length,
						pOut, outCapacity,
						out var written);

					if (rc != 0 || written == 0)
					{
						result = Array.Empty<byte>();
						resultLength = 0;
						return false;
					}

					result = rented;       // transfer ownership; caller returns to pool
					resultLength = (int)written;
					success = true;
					return true;
				}
			}
			finally
			{
				if (!success)
					ArrayPool<byte>.Shared.Return(rented);
			}
		}
	}

	/// <inheritdoc/>
	public void Dispose()
		=> this._handle.Dispose();
}
