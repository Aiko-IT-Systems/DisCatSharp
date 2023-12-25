using System;

using DisCatSharp.Net;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Exceptions;

/// <summary>
/// Represents an exception thrown when requester doesn't have necessary permissions to complete the request.
/// </summary>
public class UnauthorizedException : DisCatSharpException
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
	public string? JsonMessage { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="response">The response.</param>
	internal UnauthorizedException(BaseRestRequest request, RestResponse response)
		: base("Unauthorized: " + response.ResponseCode)
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
