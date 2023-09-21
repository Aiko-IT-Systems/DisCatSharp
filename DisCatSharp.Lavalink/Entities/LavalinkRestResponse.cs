using System.Net;
using System.Net.Http.Headers;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a response sent by the remote HTTP party.
/// </summary>
internal sealed class LavalinkRestResponse
{
	/// <summary>
	/// Gets the response code sent by the remote party.
	/// </summary>
	public HttpStatusCode ResponseCode { get; internal set; }

	/// <summary>
	/// Gets the headers of the response send by the remote party.
	/// </summary>
	public HttpResponseHeaders Headers { get; internal set; } = null!;

	/// <summary>
	/// Gets the contents of the response sent by the remote party.
	/// </summary>
	public string? Response { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkRestResponse"/> class.
	/// </summary>
	internal LavalinkRestResponse()
	{ }
}
