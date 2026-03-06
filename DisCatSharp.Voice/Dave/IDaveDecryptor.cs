using System;

namespace DisCatSharp.Voice.Dave;

/// <summary>
///     Abstraction over an inbound per-user DAVE frame decryptor.
///     Managed implementation: <see cref="DaveDecryptor"/>.
///     Native implementation: <c>LibDaveDecryptor</c>.
/// </summary>
internal interface IDaveDecryptor : IDisposable
{
	/// <summary>
	///     Attempts to decrypt a DAVE-encrypted frame.
	/// </summary>
	/// <param name="frame">The raw received frame (may or may not carry a DAVE footer).</param>
	/// <param name="result">On success, a buffer rented from <see cref="System.Buffers.ArrayPool{T}.Shared"/> containing the decrypted payload.</param>
	/// <param name="resultLength">Number of valid bytes in <paramref name="result"/>.</param>
	/// <remarks>
	///     On success, <paramref name="result"/> is a buffer rented from
	///     <see cref="System.Buffers.ArrayPool{T}.Shared"/>.
	///     The caller MUST return it via <c>ArrayPool&lt;byte&gt;.Shared.Return(result)</c>
	///     after consuming <c>result[0..resultLength]</c>.
	///     On failure, <paramref name="result"/> is <see langword="null"/> and must NOT be returned.
	/// </remarks>
	bool TryDecrypt(ReadOnlySpan<byte> frame, out byte[] result, out int resultLength);

	/// <summary>
	///     Installs a ratchet for the current epoch.
	/// </summary>
	void InstallRatchet(DaveRatchetInstaller installer);
}
