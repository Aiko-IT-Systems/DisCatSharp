using Newtonsoft.Json;

namespace DisCatSharp.Exceptions;

/// <summary>
///     Represents a <see cref="DiscordJsonException" />.
/// </summary>
public sealed class DiscordJsonException : DisCatSharpException
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordJsonException" /> class.
	/// </summary>
	/// <param name="jre">The JSON reader exception.</param>
	public DiscordJsonException(JsonReaderException jre)
		: base(jre.Message, jre.InnerException)
	{
		this.Path = jre.Path;
		this.LineNumber = jre.LineNumber;
		this.LinePosition = jre.LinePosition;
		this.HResult = jre.HResult;
		this.HelpLink = jre.HelpLink;
	}

	/// <summary>
	///     Gets the line number indicating where the error occurred.
	/// </summary>
	/// <value>The line number indicating where the error occurred.</value>
	public int LineNumber { get; }

	/// <summary>
	///     Gets the line position indicating where the error occurred.
	/// </summary>
	/// <value>The line position indicating where the error occurred.</value>
	public int LinePosition { get; }

	/// <summary>
	///     Gets the path to the JSON where the error occurred.
	/// </summary>
	/// <value>The path to the JSON where the error occurred.</value>
	public string? Path { get; }
}
