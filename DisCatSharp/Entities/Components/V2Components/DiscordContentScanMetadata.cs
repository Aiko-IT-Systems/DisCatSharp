using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents Discord's content scan result for an unfurled component media item.</summary>
public sealed class DiscordContentScanMetadata
{
	/// <summary>Gets the categories flagged by Discord's content scan.</summary>
	[JsonProperty("flags")]
	public ContentScanFlags Flags { get; internal set; }
}

/// <summary>Categories flagged by Discord's media content scanning.</summary>
[Flags]
public enum ContentScanFlags
{
	/// <summary>No documented categories were flagged.</summary>
	None = 0,
	/// <summary>The media was flagged as explicit.</summary>
	Explicit = 1 << 0,
	/// <summary>The media was flagged as gore.</summary>
	Gore = 1 << 1,
	/// <summary>The media was flagged as self-harm content.</summary>
	SelfHarm = 1 << 2
}
