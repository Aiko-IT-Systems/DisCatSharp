using System;

using DisCatSharp.Net;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Exceptions;

/// <summary>
/// Represents an exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : DisCatSharpException
{
	/// <summary>
	/// Gets the request that caused the exception.
	/// </summary>
	public BaseRestRequest WebRequest { get; internal set; }

	/// <summary>
	/// Gets the response to the request.
	/// </summary>
	public RestResponse WebResponse { get; internal set; }

	/// <summary>
	/// Gets the JSON received.
	/// </summary>
	public string? JsonMessage { get; internal set; } = null;

	/// <summary>
	/// Initializes a new instance of the <see cref="NotFoundException"/> class.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="response">The response.</param>
	internal NotFoundException(BaseRestRequest request, RestResponse response)
		: base("Not found: " + response.ResponseCode)
	{
		this.WebRequest = request;
		this.WebResponse = response;

		try
		{
			var j = JObject.Parse(response.Response);

			if (j["message"] != null)
				this.JsonMessage = j["message"]!.ToString();
		}
		catch (Exception)
		{ }
	}
}
