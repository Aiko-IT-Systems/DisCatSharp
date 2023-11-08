using System.Collections.Concurrent;
using System.Collections.Generic;

using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a stage instance to which the user is invited.
/// </summary>
public class DiscordInviteStage : SnowflakeObject
{
	/// <summary>
	/// Gets the members speaking in the Stage.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordMember> Members { get; internal set; }

	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordMember> MembersInternal = new();

	/// <summary>
	/// Gets the number of users in the Stage.
	/// </summary>
	[JsonProperty("participant_count", NullValueHandling = NullValueHandling.Ignore)]
	public int ParticipantCount { get; internal set; }

	/// <summary>
	/// Gets the number of users speaking in the Stage.
	/// </summary>
	[JsonProperty("speaker_count", NullValueHandling = NullValueHandling.Ignore)]
	public int SpeakerCount { get; internal set; }

	/// <summary>
	/// Gets the topic of the Stage instance.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string Topic { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordInviteStage"/> class.
	/// </summary>
	internal DiscordInviteStage()
	{
		this.Members = new ReadOnlyConcurrentDictionary<ulong, DiscordMember>(this.MembersInternal);
	}
}
