using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

internal sealed class RestAutomodRuleModifyPayload : ObservableApiObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Name { get; set; }

	[JsonProperty("event_type", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<AutomodEventType> EventType { get; set; }

	[JsonProperty("trigger_type", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<AutomodTriggerType> TriggerType { get; set; }

	[JsonProperty("trigger_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<AutomodTriggerMetadata> TriggerMetadata { get; set; }

	[JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<AutomodAction>> Actions { get; set; }

	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Enabled { get; set; }

	[JsonProperty("exempt_roles", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<ulong>> ExemptRoles { get; set; }

	[JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<ulong>> ExemptChannels { get; set; }
}
