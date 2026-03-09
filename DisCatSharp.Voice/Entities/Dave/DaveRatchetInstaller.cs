using DisCatSharp.Voice.Interop.Dave;

namespace DisCatSharp.Voice.Entities.Dave;

/// <summary>
///     Carries a ratchet for installation into an encryptor or decryptor.
///     Exactly one of <see cref="ManagedSecret"/> or <see cref="NativeHandle"/> is non-null.
/// </summary>
internal readonly struct DaveRatchetInstaller
{
	/// <summary>
	///     32-byte secret used by the managed <see cref="HashRatchet"/> path (tests).
	/// </summary>
	public byte[]? ManagedSecret { get; }

	/// <summary>
	///     Opaque libdave ratchet handle used by the native path (production).
	/// </summary>
	public DaveKeyRatchetSafeHandle? NativeHandle { get; }

	/// <summary>
	///     Returns <see langword="true"/> when this installer carries a native handle.
	/// </summary>
	public bool IsNative
		=> this.NativeHandle is not null;

	private DaveRatchetInstaller(byte[]? secret, DaveKeyRatchetSafeHandle? handle)
	{
		this.ManagedSecret = secret;
		this.NativeHandle = handle;
	}

	/// <summary>
	///     Creates an installer from a managed 32-byte secret (test path).
	/// </summary>
	public static DaveRatchetInstaller FromManaged(byte[] secret)
		=> new(secret, null);

	/// <summary>
	///     Creates an installer from a native libdave key ratchet handle (production path).
	/// </summary>
	public static DaveRatchetInstaller FromNative(DaveKeyRatchetSafeHandle handle)
		=> new(null, handle);
}
