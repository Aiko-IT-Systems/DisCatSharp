using System;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents a gateway payload.
/// </summary>
public class PayloadReceivedEventArgs : DiscordEventArgs
{

	/// <summary>
	///     Initializes a new instance of the <see cref="PayloadReceivedEventArgs" /> class.
	/// </summary>
	internal PayloadReceivedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     The JSON from this payload event.
	/// </summary>
	public string Json
	{
		get
		{
			if (string.IsNullOrWhiteSpace(field))
				field = this.PayloadObject.ToString();
			return field;
		}

		private set;
	}

	/// <summary>
	///     Gets or sets the payload object.
	/// </summary>
	internal JObject PayloadObject { get; set; }

	/// <summary>
	///     The name of this event.
	/// </summary>
	public string EventName { get; internal set; }
}
