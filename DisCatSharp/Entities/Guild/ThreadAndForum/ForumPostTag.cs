using System;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Net.Models;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord forum post tag.
/// </summary>
public class ForumPostTag : NullableSnowflakeObject, IEquatable<ForumPostTag>
{
	/// <summary>
	/// Gets the channel id this tag belongs to.
	/// </summary>
	[JsonIgnore]
	internal ulong ChannelId;

	/// <summary>
	/// Gets the channel this tag belongs to.
	/// </summary>
	[JsonIgnore]
	internal DiscordChannel Channel;

	/// <summary>
	/// Gets the name of this forum post tag.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the emoji id of the forum post tag.
	/// </summary>
	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EmojiId { get; internal set; }

	/// <summary>
	/// Gets the unicode emoji of the forum post tag.
	/// </summary>
	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Include)]
	public string? UnicodeEmojiString { get; internal set; }

	/// <summary>
	/// Gets whether the tag can only be used by moderators.
	/// </summary>
	[JsonProperty("moderated", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Moderated { get; internal set; }

	/// <summary>
	/// Gets the emoji.
	/// </summary>
	[JsonIgnore]
	public DiscordEmoji Emoji
		=> this.UnicodeEmojiString != null ? DiscordEmoji.FromName(this.Discord, $":{this.UnicodeEmojiString}:", false) : DiscordEmoji.FromGuildEmote(this.Discord, this.EmojiId.Value);

	/// <summary>
	/// Initializes a new instance of the <see cref="ForumPostTag"/> class.
	/// </summary>
	internal ForumPostTag()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="ForumPostTag"/> class.
	/// </summary>
	/// <param name="name">The tags name.</param>
	/// <param name="emojiId">The tags emoji id. Defaults to <see langword="null"/>.</param>
	/// <param name="emojiName">The tags emoji name (unicode emoji). Defaults to <see langword="null"/>.</param>
	/// <param name="moderated">Whether this tag can only be applied by moderators. Defaults to <see langword="false"/>.</param>
	public ForumPostTag(string name, ulong? emojiId = null, string? emojiName = null, bool moderated = false)
	{
		this.Id = null;
		this.Name = name;
		this.EmojiId = emojiId;
		this.UnicodeEmojiString = emojiName;
		this.Moderated = moderated;
	}

	/// <summary>
	/// Modifies the tag.
	/// </summary>
	/// <exception cref="NotImplementedException">This method is currently not implemented.</exception>
	public async Task<ForumPostTag> ModifyAsync(Action<ForumPostTagEditModel> action)
	{
		var mdl = new ForumPostTagEditModel();
		action(mdl);
		var res = await this.Discord.ApiClient.ModifyForumChannelAsync(this.ChannelId, null, null, null, null, null, null, this.Channel.InternalAvailableTags.Where(x => x.Id != this.Id).ToList().Append(new()
		{
			Id = this.Id,
			Discord = this.Discord,
			ChannelId = this.ChannelId,
			Channel = this.Channel,
			EmojiId = mdl.Emoji.HasValue ? mdl.Emoji.Value.Id : this.EmojiId,
			Moderated = mdl.Moderated.HasValue ? mdl.Moderated.Value : this.Moderated,
			Name = mdl.Name.HasValue ? mdl.Name.Value : this.Name,
			UnicodeEmojiString = mdl.Emoji.HasValue ? mdl.Emoji.Value.Name : this.UnicodeEmojiString
		}).ToList(), null, null, null, null, null, null, null, mdl.AuditLogReason);
		return res.InternalAvailableTags.First(x => x.Id == this.Id);
	}

	/// <summary>
	/// Deletes the tag.
	/// </summary>
	/// <exception cref="NotImplementedException">This method is currently not implemented.</exception>
	public Task DeleteAsync(string reason = null)
		=> this.Discord.ApiClient.ModifyForumChannelAsync(this.ChannelId, null, null, Optional.None, Optional.None, null, Optional.None, this.Channel.InternalAvailableTags.Where(x => x.Id != this.Id).ToList(), Optional.None, Optional.None, Optional.None, Optional.None, Optional.None, null, Optional.None, reason);

	/// <summary>
	/// Checks whether this <see cref="ForumPostTag"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="ForumPostTag"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as ForumPostTag);

	/// <summary>
	/// Checks whether this <see cref="ForumPostTag"/> is equal to another <see cref="ForumPostTag"/>.
	/// </summary>
	/// <param name="e"><see cref="ForumPostTag"/> to compare to.</param>
	/// <returns>Whether the <see cref="ForumPostTag"/> is equal to this <see cref="ForumPostTag"/>.</returns>
	public bool Equals(ForumPostTag e)
		=> e is not null && (ReferenceEquals(this, e) || (this.Id == e.Id && this.Name == e.Name));

	/// <summary>
	/// Gets the hash code for this <see cref="ForumPostTag"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="ForumPostTag"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="ForumPostTag"/> objects are equal.
	/// </summary>
	/// <param name="e1">First forum post tag to compare.</param>
	/// <param name="e2">Second forum post tag to compare.</param>
	/// <returns>Whether the two forum post tags are equal.</returns>
	public static bool operator ==(ForumPostTag e1, ForumPostTag e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordEmoji"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First forum post tag to compare.</param>
	/// <param name="e2">Second forum post tag to compare.</param>
	/// <returns>Whether the two forum post tags are not equal.</returns>
	public static bool operator !=(ForumPostTag e1, ForumPostTag e2)
		=> !(e1 == e2);
}
