using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a forum channel edit model.
/// </summary>
public class ForumChannelEditModel : BaseEditModel
{
	/// <summary>
	/// Sets the channel's new name.
	/// </summary>
	public string Name { internal get; set; }

	/// <summary>
	/// Sets the channel's new position.
	/// </summary>
	public int? Position { internal get; set; }

	/// <summary>
	/// Sets the channel's new topic.
	/// </summary>
	public Optional<string> Topic { internal get; set; }

	/// <summary>
	/// Sets the channel's new template.
	/// <note type="warning">This is not yet released and won't be applied by library.</note>
	/// </summary>
	public Optional<string> Template { internal get; set; }

	/// <summary>
	/// Sets whether the channel is to be marked as NSFW.
	/// </summary>
	public bool? Nsfw { internal get; set; }

	/// <summary>
	/// Sets the available tags.
	/// </summary>
	public Optional<List<ForumPostTag>?> AvailableTags { internal get; set; }

	/// <summary>
	/// Sets the default reaction emoji.
	/// </summary>
	public Optional<ForumReactionEmoji> DefaultReactionEmoji { internal get; set; }

	/// <summary>
	/// Sets the default forum post sort order
	/// </summary>
	public Optional<ForumPostSortOrder?> DefaultSortOrder { internal get; set; }

	/// <summary>
	/// <para>Sets the parent of this channel.</para>
	/// <para>This should be channel with <see cref="DisCatSharp.Entities.DiscordChannel.Type"/> set to <see cref="ChannelType.Category"/>.</para>
	/// </summary>
	public Optional<DiscordChannel> Parent { internal get; set; }

	/// <summary>
	/// <para>Sets the voice channel's new user limit.</para>
	/// <para>Setting this to 0 will disable the user limit.</para>
	/// </summary>
	public int? UserLimit { internal get; set; }

	/// <summary>
	/// <para>Sets the channel's new slow mode timeout.</para>
	/// <para>Setting this to null or 0 will disable slow mode.</para>
	/// </summary>
	public Optional<int?> PerUserRateLimit { internal get; set; }

	/// <summary>
	/// <para>Sets the channel's new post slow mode timeout.</para>
	/// <para>Setting this to null or 0 will disable slow mode.</para>
	/// </summary>
	public Optional<int?> PostCreateUserRateLimit { internal get; set; }

	/// <summary>
	/// Sets this channel's default duration for newly created threads, in minutes, to automatically archive the thread after recent activity.
	/// </summary>
	public Optional<ThreadAutoArchiveDuration?> DefaultAutoArchiveDuration { internal get; set; }

	/// <summary>
	/// Sets the channel's permission overwrites.
	/// </summary>
	public IEnumerable<DiscordOverwriteBuilder> PermissionOverwrites { internal get; set; }

	public Optional<ChannelFlags?> Flags { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ChannelEditModel"/> class.
	/// </summary>
	internal ForumChannelEditModel()
	{ }
}
