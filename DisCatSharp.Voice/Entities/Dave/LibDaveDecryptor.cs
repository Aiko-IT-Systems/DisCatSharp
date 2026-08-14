using System;
using System.Buffers;
using System.Threading;

using DisCatSharp.Voice.Interfaces.Dave;

using DisCatSharp.Voice.Interop.Dave;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     <see cref="IDaveDecryptor"/> backed by the native libdave decryptor.
///     One instance per remote user in the voice channel.
/// </summary>
/// <remarks>
/// <para>
/// Ratchet transitions are protected by an internal lock: a frame in progress will complete
/// with either the old or new ratchet, never a partially-transitioned state.
/// </para>
/// <para>
/// Late packet handling: libdave retains superseded encrypted ratchets for its transition
/// grace period and temporarily accepts plaintext while transitioning back to E2EE.
/// Reusing this decryptor across epochs preserves those overlap windows.
/// </para>
/// <para>
/// On success, <see cref="TryDecrypt"/> returns a rented <see cref="System.Buffers.ArrayPool{T}"/> buffer.
/// The caller is responsible for returning it via <c>ArrayPool&lt;byte&gt;.Shared.Return(result)</c>
/// after consuming <c>result[0..resultLength]</c>.
/// </para>
/// </remarks>
internal sealed class LibDaveDecryptor : IDaveDecryptor
{
	/// <summary>
	///     Integer constant passed to libdave to identify audio media frames.
	/// </summary>
	private const int DaveMediaTypeAudio = 0;

	/// <summary>
	///     Maximum number of overhead bytes the native decryptor may add beyond the ciphertext length.
	/// </summary>
	private const int MaxDecryptedOverhead = 256;

	private readonly DaveDecryptorSafeHandle _handle;

	// Native state is mutable; lock is required rather than atomic swap.
	// The _handle points to opaque libdave memory that is mutated in-place by every API call
	// (DecryptorTransitionToKeyRatchet, DecryptorDecrypt).  There is no safe way to snapshot
	// or atomically exchange this state, so lock(_sync) serialises all operations on the
	// native handle.
	// IMPORTANT: Dispose() also acquires _sync so that a concurrent TryDecrypt() on the audio
	// thread cannot call DecryptorDecrypt on a closed handle.
	private readonly Lock _sync = new();
	private bool _disposed;

	/// <summary>
	///     Creates the native libdave decryptor handle via <see cref="DaveNative.DecryptorCreate"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when the native decryptor could not be created.</exception>
	public LibDaveDecryptor()
	{
		this._handle = DaveNative.DecryptorCreate();
		if (this._handle.IsInvalid)
			throw new InvalidOperationException("[DAVE] Failed to create native decryptor.");
	}

	/// <inheritdoc/>
	public void TransitionTo(DaveRatchetInstaller? installer, bool passthrough)
	{
		if (installer is { } candidate
			&& (!candidate.IsNative || candidate.NativeHandle is null || candidate.NativeHandle.IsInvalid))
		{
			throw new ArgumentException("LibDaveDecryptor requires a valid native ratchet handle.", nameof(installer));
		}

		lock (this._sync)
		{
			DaveNative.DecryptorTransitionToPassthroughMode(this._handle, passthrough);

			if (installer is { } value)
			{
				DaveNative.DecryptorTransitionToKeyRatchet(this._handle, value.NativeHandle);
				value.NativeHandle.Dispose();
			}
			else
			{
				DaveNative.DecryptorTransitionToNoKeyRatchet(this._handle, IntPtr.Zero);
			}
		}
	}

	/// <inheritdoc/>
	public unsafe bool TryDecrypt(ReadOnlySpan<byte> frame, out byte[] result, out int resultLength)
	{
		result = null!;
		resultLength = 0;

		lock (this._sync)
		{
			// Guard against use after Dispose() — Dispose() acquires the same lock so this check
			// is always consistent with the disposed/closed handle state.
			if (this._disposed || frame.IsEmpty)
				return false;

			var outCapacity = (nuint)(frame.Length + MaxDecryptedOverhead);
			var rented = ArrayPool<byte>.Shared.Rent((int)outCapacity);
			var success = false;
			try
			{
				fixed (byte* pFrame = frame)
				fixed (byte* pOut = rented)
				{
					var rc = DaveNative.DecryptorDecrypt(
						this._handle,
						DaveMediaTypeAudio,
						pFrame, (nuint)frame.Length,
						pOut, outCapacity,
						out var written);

					if (rc != 0 || written == 0)
						return false;

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
	{
		// Acquire _sync so that any in-flight TryDecrypt() on the audio thread completes
		// before we close the native handle.  Without this, the gateway thread disposing a
		// replaced decryptor could race with the audio thread still inside lock(_sync) and
		// cause an ObjectDisposedException from SafeHandle marshaling.
		lock (this._sync)
		{
			if (this._disposed)
				return;
			this._disposed = true;
			this._handle.Dispose();
		}
	}
}
