using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAuditLogChange
{
	[JsonProperty("new_value", NullValueHandling = NullValueHandling.Ignore)]
	public object NewValue { get; internal set; }

	[JsonProperty("old_value", NullValueHandling = NullValueHandling.Ignore)]
	public object OldValue { get; internal set; }

	[JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
	public string Key { get; internal set; }
}
