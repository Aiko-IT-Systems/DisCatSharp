using System;

using DisCatSharp.Net;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Exceptions;

/// <summary>
/// Represents an exception thrown when an elastisearch endpoint isn't finished with indexing yet.
/// </summary>
public sealed class NotIndexedException : Exception
{
	/// <summary>
	/// Gets the response to the request.
	/// </summary>
	public RestResponse WebResponse { get; internal set; }

	/// <summary>
	/// Gets the error code for this exception.
	/// </summary>
	public int? Code { get; internal set; } = null;

	/// <summary>
	/// Gets the json message.
	/// </summary>
	public string? JsonMessage { get; internal set; } = null;

	/// <summary>
	/// Gets the indexed documents count.
	/// </summary>
	public long? DocumentsIndexed { get; internal set; } = null;

	/// <summary>
	/// Gets when to retry the request.
	/// </summary>
	public long? RetryAfter { get; internal set; } = null;

	/// <summary>
	/// Initializes a new instance of the <see cref="NotIndexedException"/> class.
	/// </summary>
	/// <param name="response">The response.</param>
	internal NotIndexedException(RestResponse response)
		: base("Not indexed: " + response.ResponseCode)
	{
		this.WebResponse = response;

		try
		{
			var j = JObject.Parse(response.Response);

			if (j["code"] is not null)
				this.Code = (int)j["code"]!;

			if (j["message"] is not null)
				this.JsonMessage = j["message"]!.ToString();

			if (j["documents_indexed"] is not null)
				this.DocumentsIndexed = (long)j["documents_indexed"]!;

			if (j["retry_after"] is not null)
				this.RetryAfter = (long)j["retry_after"]!;
		}
		catch
		{ }
	}
}
