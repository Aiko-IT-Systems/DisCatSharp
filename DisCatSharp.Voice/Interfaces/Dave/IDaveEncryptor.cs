using System;
using DisCatSharp.Voice.Entities.Dave;

namespace DisCatSharp.Voice.Interfaces.Dave;

/// <summary>
///     Abstraction over the outbound DAVE frame encryptor.
///     Managed implementation: <see cref="DaveEncryptor"/>.
///     Native implementation: <c>LibDaveEncryptor</c>.
/// </summary>
internal interface IDaveEncryptor : IDisposable
{
	/// <summary>
	///     Gets whether a ratchet has been installed and encryption is active.
	/// </summary>
	bool IsActive { get; }

	/// <summary>
	///     Encrypts an Opus frame. Returns <see langword="false"/> in passthrough mode.
	/// </summary>
	/// <param name="frame">The raw Opus frame to encrypt.</param>
	/// <param name="ssrc">
	///     The RTP SSRC of the local sender (from the voice READY payload).
	///     Passed directly to <c>daveEncryptorEncrypt</c> so the native encryptor can embed
	///     the correct sender SSRC in the SFrame header.  The managed implementation ignores
	///     this value (it does not use SFrame headers), but the parameter is required for
	///     interface uniformity.
	/// </param>
	/// <param name="result">On success, a buffer rented from <see cref="System.Buffers.ArrayPool{T}.Shared"/> containing the encrypted frame bytes.</param>
	/// <param name="resultLength">Number of valid bytes in <paramref name="result"/>.</param>
	/// <remarks>
	///     On success, <paramref name="result"/> is a buffer rented from
	///     <see cref="System.Buffers.ArrayPool{T}.Shared"/>.
	///     The caller MUST return it via <c>ArrayPool&lt;byte&gt;.Shared.Return(result)</c>
	///     after consuming <c>result[0..resultLength]</c>.
	///     On failure, <paramref name="result"/> is <see cref="Array.Empty{T}"/> and must NOT be returned.
	/// </remarks>
	bool TryEncrypt(ReadOnlySpan<byte> frame, uint ssrc, out byte[] result, out int resultLength);

	/// <summary>
	///     Controls passthrough mode.
	/// </summary>
	void SetPassthrough(bool passthrough);

	/// <summary>
	///     Installs a ratchet. For managed implementations, reads <see cref="DaveRatchetInstaller.ManagedSecret"/>.
	///     For native implementations, passes <see cref="DaveRatchetInstaller.NativeHandle"/> to libdave.
	/// </summary>
	void InstallRatchet(DaveRatchetInstaller installer);
}
