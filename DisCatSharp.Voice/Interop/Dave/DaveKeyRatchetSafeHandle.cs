using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.Voice.Interop.Dave;

/// <summary>
///     <see cref="System.Runtime.InteropServices.SafeHandle"/> wrapping a libdave <c>DAVEKeyRatchetHandle</c>.
///     <see cref="ReleaseHandle"/> calls <see cref="DaveNative.KeyRatchetDestroy"/> to free the native resource.
/// </summary>
internal sealed class DaveKeyRatchetSafeHandle : SafeHandle
{
	/// <summary>
	///     Initialises an empty handle; libdave populates it via the <c>daveSessionGetKeyRatchet</c> return value.
	/// </summary>
	public DaveKeyRatchetSafeHandle()
		: base(IntPtr.Zero, ownsHandle: true) { }

	/// <summary>
	///     Wraps an existing raw ratchet handle, transferring ownership to this instance.
	/// </summary>
	internal DaveKeyRatchetSafeHandle(IntPtr existingHandle)
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
			DaveNative.KeyRatchetDestroy(this.handle);
		return true;
	}
}
