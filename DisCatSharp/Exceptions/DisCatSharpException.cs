using System;

namespace DisCatSharp.Exceptions;

/// <summary>
///     Represents a generic exception thrown by DisCatSharp.
/// </summary>
public class DisCatSharpException : Exception
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DisCatSharpException" /> class.
	/// </summary>
	/// <param name="message">The error message.</param>
	internal DisCatSharpException(string? message)
		: base(message)
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DisCatSharpException" /> class.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The inner exception.</param>
	internal DisCatSharpException(string? message, Exception? innerException)
		: base(message, innerException)
	{ }
}
