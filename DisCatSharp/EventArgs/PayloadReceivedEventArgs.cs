using System;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents a gateway payload.
/// </summary>
public class PayloadReceivedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// The JSON from this payload event.
	/// </summary>
	public string Json
	{
		get
		{
			if (string.IsNullOrWhiteSpace(this._json))
				this._json = this.PayloadObject.ToString();
			return this._json;
		}
	}

	private string _json;

	/// <summary>
	/// Gets or sets the payload object.
	/// </summary>
	internal JObject PayloadObject { get; set; }

	/// <summary>
	/// The name of this event.
	/// </summary>
	public string EventName { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PayloadReceivedEventArgs"/> class.
	/// </summary>
	internal PayloadReceivedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
