using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

public sealed class WelcomeMessage : ObservableApiObject
{
	[JsonProperty("author_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> AuthorIds { get; internal set; } = new();

	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public string Message { get; internal set; }
}