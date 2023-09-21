using System;

namespace DisCatSharp.Exceptions;

public class DisCatSharpException : Exception
{
	/// <summary>
	/// Represents a <see cref="DisCatSharpException"/>.
	/// </summary>
	/// <param name="message">The exception message.</param>
	internal DisCatSharpException(string message)
		: base(message)
	{ }
}
