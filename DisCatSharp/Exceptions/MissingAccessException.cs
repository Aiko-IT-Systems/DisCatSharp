using System;

using DisCatSharp.Net;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Exceptions;

/// <summary>
/// Represents an exception thrown when requester doesn't have access to an endpoint / route.
/// </summary>
public class MissingAccessException : DisCatSharpException
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
	public string JsonMessage { get; internal set; }

	/// <summary>
	/// Gets the error code received.
	/// </summary>
	public int ErrorCode { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MissingAccessException"/> class.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="response">The response.</param>
	internal MissingAccessException(BaseRestRequest request, RestResponse response)
		: base("Missing Access: " + response.ResponseCode)
	{
		this.WebRequest = request;
		this.WebResponse = response;

		try
		{
			var j = JObject.Parse(response.Response);

			if (j["message"] != null)
				this.JsonMessage = j["message"].ToString();

			if (j["code"] != null)
				this.ErrorCode = Convert.ToInt32(j["code"].ToString());
		}
		catch (Exception) { }
	}
}
