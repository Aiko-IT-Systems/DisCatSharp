using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.Voice.Interop.Dave;

/// <summary>
///     <see cref="System.Runtime.InteropServices.SafeHandle"/> wrapping a libdave <c>DAVEDecryptorHandle</c>.
///     <see cref="ReleaseHandle"/> calls <see cref="DaveNative.DecryptorDestroy"/> to free the native resource.
/// </summary>
internal sealed class DaveDecryptorSafeHandle : SafeHandle
{
	/// <summary>
	///     Initialises an empty handle; libdave populates it via the <c>daveDecryptorCreate</c> return value.
	/// </summary>
	public DaveDecryptorSafeHandle()
		: base(IntPtr.Zero, ownsHandle: true) { }

	/// <summary>
	///     Wraps an existing raw decryptor handle, transferring ownership to this instance.
	/// </summary>
	internal DaveDecryptorSafeHandle(IntPtr existingHandle)
		: base(IntPtr.Zero, ownsHandle: true)
	{
		this.SetHandle(existingHandle);
	}

	/// <inheritdoc/>
	public override bool IsInvalid
		=> this.handle == IntPtr.Zero;

	/// <inheritdoc/>
	protected override bool ReleaseHandle()
	{
		if (this.handle != IntPtr.Zero)
			DaveNative.DecryptorDestroy(this.handle);
		return true;
	}
}
