using System;
using System.Buffers;
using System.Threading;

using DisCatSharp.Voice.Dave.Interop;

namespace DisCatSharp.Voice.Dave;

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
/// Late packet handling: libdave tracks ratchet generations internally. Packets encrypted
/// with a superseded ratchet generation will fail authentication and be discarded.
/// The caller should not rely on cross-epoch packet recovery at this layer.
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
	public void InstallRatchet(DaveRatchetInstaller installer)
	{
		if (!installer.IsNative || installer.NativeHandle is null || installer.NativeHandle.IsInvalid)
			throw new ArgumentException("LibDaveDecryptor requires a valid native ratchet handle.", nameof(installer));

		lock (this._sync)
		{
			DaveNative.DecryptorTransitionToKeyRatchet(this._handle, installer.NativeHandle);
			installer.NativeHandle!.Dispose(); // libdave copied the ratchet; release our reference
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
