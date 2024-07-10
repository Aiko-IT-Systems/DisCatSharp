using System;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Defines the behaviour for throwing exceptions from <see cref="AsyncEvent{TSender, TArgs}.InvokeAsync(TSender, TArgs, AsyncEventExceptionMode)"/>.
/// </summary>
[Flags]
public enum AsyncEventExceptionMode
{
	/// <summary>
	/// Defines that no exceptions should be thrown. Only exception handlers will be used.
	/// </summary>
	IgnoreAll = 0,

	/// <summary>
	/// Defines that only fatal (i.e. non-<see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions
	/// should be thrown.
	/// </summary>
	ThrowFatal = 1,

	/// <summary>
	/// Defines that only non-fatal (i.e. <see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions
	/// should be thrown.
	/// </summary>
	ThrowNonFatal = 2,

	/// <summary>
	/// Defines that all exceptions should be thrown. This is equivalent to combining <see cref="ThrowFatal"/> and
	/// <see cref="ThrowNonFatal"/> flags.
	/// </summary>
	ThrowAll = ThrowFatal | ThrowNonFatal,

	/// <summary>
	/// Defines that only fatal (i.e. non-<see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions
	/// should be handled by the specified exception handler.
	/// </summary>
	HandleFatal = 4,

	/// <summary>
	/// Defines that only non-fatal (i.e. <see cref="AsyncEventTimeoutException{TSender, TArgs}"/>) exceptions
	/// should be handled by the specified exception handler.
	/// </summary>
	HandleNonFatal = 8,

	/// <summary>
	/// Defines that all exceptions should be handled by the specified exception handler. This is equivalent to
	/// combining <see cref="HandleFatal"/> and <see cref="HandleNonFatal"/> flags.
	/// </summary>
	HandleAll = HandleFatal | HandleNonFatal,

	/// <summary>
	/// Defines that all exceptions should be thrown and handled by the specified exception handler. This is
	/// equivalent to combining <see cref="HandleAll"/> and <see cref="ThrowAll"/> flags.
	/// </summary>
	ThrowAllHandleAll = ThrowAll | HandleAll,

	/// <summary>
	/// Default mode, equivalent to <see cref="HandleAll"/>.
	/// </summary>
	Default = HandleAll
}
