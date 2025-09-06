using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the base class for all Discord channel types.
/// </summary>
public abstract class BaseDiscordChannel : SnowflakeObject, IEquatable<BaseDiscordChannel>
{

	/// <summary>
	/// Gets the channel type.
	/// </summary>
	[JsonProperty("type")]
	public ChannelType Type { get; internal set; }

	/// <summary>
	/// Checks whether this <see cref="BaseDiscordChannel" /> is equal to another <see cref="BaseDiscordChannel" />.
	/// </summary>
	public bool Equals(BaseDiscordChannel other)
		=> other is not null && (ReferenceEquals(this, other) || this.Id == other.Id);

	public override bool Equals(object obj)
		=> this.Equals(obj as BaseDiscordChannel);

	public override int GetHashCode()
		=> this.Id.GetHashCode();

	public static bool operator ==(BaseDiscordChannel e1, BaseDiscordChannel e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;
		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	public static bool operator !=(BaseDiscordChannel e1, BaseDiscordChannel e2)
		=> !(e1 == e2);
}
