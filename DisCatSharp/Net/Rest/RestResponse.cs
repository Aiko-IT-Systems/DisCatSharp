using System.Collections.Generic;
using System.Net;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a response sent by the remote HTTP party.
/// </summary>
public sealed class RestResponse
{
	/// <summary>
	/// Gets the response code sent by the remote party.
	/// </summary>
	public HttpStatusCode ResponseCode { get; internal set; }

	/// <summary>
	/// Gets the headers sent by the remote party.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Headers { get; internal set; }

	/// <summary>
	/// Gets the contents of the response sent by the remote party.
	/// </summary>
	public string Response { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RestResponse"/> class.
	/// </summary>
	internal RestResponse()
	{ }
}
