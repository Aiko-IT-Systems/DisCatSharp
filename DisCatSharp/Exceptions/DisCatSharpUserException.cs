
using System;

namespace DisCatSharp.Exceptions;

/// <summary>
/// Represents a generic exception thrown by DisCatSharp because of a user error.
/// </summary>
public class DisCatSharpUserException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DisCatSharpUserException"/> class.
	/// </summary>
	/// <param name="message">The error message.</param>
	internal DisCatSharpUserException(string? message)
		: base(message)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="DisCatSharpUserException"/> class.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The inner exception.</param>
	internal DisCatSharpUserException(string? message, Exception? innerException)
		: base(message, innerException)
	{ }
}
