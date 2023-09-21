namespace DisCatSharp.Exceptions;

public sealed class ModerationException : DisCatSharpException
{
	/// <summary>
	/// Represents a <see cref="ModerationException"/>.
	/// </summary>
	/// <param name="message">The exception message.</param>
	internal ModerationException(string message)
		: base(message)
	{
	}
}
