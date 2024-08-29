using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordRole
{
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Id { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
	public int Color { get; internal set; }

	[JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
	public bool Hoist { get; internal set; }

	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string? Icon { get; internal set; }

	[JsonProperty("unicode_emoji", NullValueHandling = NullValueHandling.Ignore)]
	public string? UnicodeEmoji { get; internal set; }

	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; internal set; }

	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions Permissions { get; internal set; }

	[JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
	public bool Managed { get; internal set; }

	[JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
	public bool Mentionable { get; internal set; }
}
