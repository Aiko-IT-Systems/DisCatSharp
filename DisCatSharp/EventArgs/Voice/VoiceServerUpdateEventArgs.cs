using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.VoiceServerUpdated"/> event.
/// </summary>
public class VoiceServerUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild for which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the new voice endpoint.
	/// </summary>
	public string Endpoint { get; internal set; }

	/// <summary>
	/// Gets the voice connection token.
	/// </summary>
	public string VoiceToken { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="VoiceServerUpdateEventArgs"/> class.
	/// </summary>
	internal VoiceServerUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
