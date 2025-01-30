using System.Collections.Generic;
using System.Net;

namespace DisCatSharp.Net.V2;

/// <summary>
///     Represents a response sent by the remote HTTP party.
/// </summary>
public sealed class RestResponseV2
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RestResponseV2" /> class.
	/// </summary>
	internal RestResponseV2()
	{ }

	/// <summary>
	///     Gets the response code sent by the remote party.
	/// </summary>
	public HttpStatusCode ResponseCode { get; internal set; }

	/// <summary>
	///     Gets the headers sent by the remote party.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Headers { get; internal set; }

	/// <summary>
	///     Gets the contents of the response sent by the remote party.
	/// </summary>
	public string Response { get; internal set; }

	public bool IsFaulted { get; internal set; }
}
