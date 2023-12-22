using System;
using System.Net;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Exceptions;

/// <summary>
/// Represents a lavalink rest exception.
/// </summary>
public sealed class LavalinkRestException : Exception
{
	/// <summary>
	/// Gets the datetime offset when the exception was thrown.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Timestamp => Utilities.GetDateTimeOffsetFromMilliseconds(this._timestamp);

	[JsonProperty("timestamp")]
	private readonly long _timestamp;

	/// <summary>
	/// Gets the response http status code.
	/// </summary>
	[JsonProperty("status")]
	public HttpStatusCode Status { get; internal set; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	[JsonProperty("error")]
	public string Error { get; internal set; }

	/// <summary>
	/// Gets the exception message.
	/// </summary>
	[JsonProperty("message")]
	public new string Message { get; internal set; }

	/// <summary>
	/// Gets the path where the exception was thrown.
	/// </summary>
	[JsonProperty("path")]
	public string Path { get; internal set; }

	/// <summary>
	/// Gets the trace information to this exception.
	/// <para>Requires <see cref="LavalinkConfiguration.EnableTrace"/> to set to <see langword="true"/>.</para>
	/// </summary>
	[JsonProperty("trace")]
	public string? Trace { get; internal set; }

	/// <summary>
	/// Gets the response header.
	/// </summary>
	[JsonIgnore]
	public HttpResponseHeaders Headers { get; internal set; }

	/// <summary>
	/// Gets the response json.
	/// </summary>
	[JsonIgnore]
	public string Json { get; internal set; }
}
