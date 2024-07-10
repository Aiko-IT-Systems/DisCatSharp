using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a <see cref="LavalinkSessionConfiguration"/>.
/// </summary>
public sealed class LavalinkSessionConfiguration
{
	/// <summary>
	/// Whether resuming is enabled for this session or not.
	/// </summary>
	[JsonProperty("resuming", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Resuming { get; internal set; }

	/// <summary>
	/// The timeout in seconds (default is 60s)
	/// </summary>
	[JsonProperty("timeout", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int> TimeoutSeconds { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="LavalinkSessionConfiguration"/>.
	/// </summary>
	/// <param name="resuming">Whether resuming is enabled for this session or not.</param>
	/// <param name="timeoutSeconds">The timeout in seconds.</param>
	public LavalinkSessionConfiguration(Optional<bool> resuming, Optional<int> timeoutSeconds)
	{
		this.Resuming = resuming;
		this.TimeoutSeconds = timeoutSeconds;
	}

	/// <summary>
	/// Constructs a new <see cref="LavalinkSessionConfiguration"/>.
	/// </summary>
	internal LavalinkSessionConfiguration()
	{ }
}
