using System;
using System.Runtime.InteropServices;

namespace DisCatSharp.Voice.Interop.Dave;

/// <summary>
///     <see cref="System.Runtime.InteropServices.SafeHandle"/> wrapping a libdave <c>DAVESessionHandle</c>.
///     <see cref="ReleaseHandle"/> calls <see cref="DaveNative.SessionDestroy"/> to free the native resource.
/// </summary>
internal sealed class DaveSessionSafeHandle : SafeHandle
{
	/// <summary>
	///     Initialises an empty handle; libdave populates it via the <c>daveSessionCreate</c> return value.
	/// </summary>
	public DaveSessionSafeHandle()
		: base(IntPtr.Zero, ownsHandle: true) { }

	/// <summary>
	///     Wraps an existing raw session handle, transferring ownership to this instance.
	/// </summary>
	internal DaveSessionSafeHandle(IntPtr existingHandle)
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
			DaveNative.SessionDestroy(this.handle);
		return true;
	}
}
