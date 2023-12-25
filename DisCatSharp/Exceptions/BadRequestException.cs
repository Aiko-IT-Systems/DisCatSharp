using DisCatSharp.Net;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Exceptions;

/// <summary>
/// Represents an exception thrown when a malformed request is sent.
/// </summary>
public class BadRequestException : DisCatSharpException
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
	/// Gets the error code for this exception.
	/// </summary>
	public int? Code { get; internal set; } = null;

	/// <summary>
	/// Gets the JSON message received.
	/// </summary>
	public string? JsonMessage { get; internal set; } = null;

	/// <summary>
	/// Gets the form error responses in JSON format.
	/// </summary>
	public string? Errors { get; internal set; } = null;

	/// <summary>
	/// Initializes a new instance of the <see cref="BadRequestException"/> class.
	/// </summary>
	/// <param name="request">The request.</param>
	/// <param name="response">The response.</param>
	internal BadRequestException(BaseRestRequest request, RestResponse response)
		: base("Bad request: " + response.ResponseCode)
	{
		this.WebRequest = request;
		this.WebResponse = response;

		try
		{
			var j = JObject.Parse(response.Response);

			if (j["code"] is not null)
				this.Code = (int)j["code"]!;

			if (j["message"] is not null)
				this.JsonMessage = j["message"]!.ToString();

			if (j["errors"] is not null)
				this.Errors = j["errors"]!.ToString();
		}
		catch
		{ }
	}
}
