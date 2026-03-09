using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.Voice.Interop.Dave;

/// <summary>
///     <see cref="System.Runtime.InteropServices.SafeHandle"/> wrapping a libdave <c>DAVEEncryptorHandle</c>.
///     <see cref="ReleaseHandle"/> calls <see cref="DaveNative.EncryptorDestroy"/> to free the native resource.
/// </summary>
internal sealed class DaveEncryptorSafeHandle : SafeHandle
{
	/// <summary>
	///     Initialises an empty handle; libdave populates it via the <c>daveEncryptorCreate</c> return value.
	/// </summary>
	public DaveEncryptorSafeHandle()
		: base(IntPtr.Zero, ownsHandle: true) { }

	/// <summary>
	///     Wraps an existing raw encryptor handle, transferring ownership to this instance.
	/// </summary>
	internal DaveEncryptorSafeHandle(IntPtr existingHandle)
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
			DaveNative.EncryptorDestroy(this.handle);
		return true;
	}
}
