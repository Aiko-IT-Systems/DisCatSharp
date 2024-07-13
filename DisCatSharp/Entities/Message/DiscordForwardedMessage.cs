using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

[DiscordInExperiment, Experimental("This is subject to change at any time.")]
public class DiscordForwardedMessage : ObservableApiObject
{
	internal DiscordForwardedMessage()
	{
		this._attachmentsLazy = new(() => new ReadOnlyCollection<DiscordAttachment>(this.AttachmentsInternal));
		this._embedsLazy = new(() => new ReadOnlyCollection<DiscordEmbed>(this.EmbedsInternal));
		/*this._mentionedChannelsLazy = new(() => this.MentionedChannelsInternal != null
			? new ReadOnlyCollection<DiscordChannel>(this.MentionedChannelsInternal)
			: Array.Empty<DiscordChannel>());
		this._mentionedRolesLazy = new(() => this.MentionedRolesInternal != null ? new ReadOnlyCollection<DiscordRole>(this.MentionedRolesInternal) : Array.Empty<DiscordRole>());
		this.MentionedUsersLazy = new(() => new ReadOnlyCollection<DiscordUser>(this.MentionedUsersInternal));*/
	}

	/// <summary>
	/// Gets the type of the message.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public MessageType? MessageType { get; internal set; }

	/// <summary>
	/// Gets the message's content.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public string Content { get; internal set; }

	/// <summary>
	/// Gets embeds attached to this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordEmbed> Embeds
		=> this._embedsLazy.Value;

	[JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordEmbed> EmbedsInternal = [];

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordEmbed>> _embedsLazy;

	/// <summary>
	/// Gets files attached to this message.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordAttachment> Attachments
		=> this._attachmentsLazy.Value;

	[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordAttachment> AttachmentsInternal = [];

	[JsonIgnore]
	private readonly Lazy<IReadOnlyList<DiscordAttachment>> _attachmentsLazy;

	/// <summary>
	/// Gets the message's creation timestamp.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? Timestamp
		=> !string.IsNullOrWhiteSpace(this.TimestampRaw) && DateTimeOffset.TryParse(this.TimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets the message's creation timestamp as raw string.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string TimestampRaw { get; set; }

	/// <summary>
	/// Gets the message's edit timestamp. Will be null if the message was not edited.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EditedTimestamp
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw) && DateTimeOffset.TryParse(this.EditedTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets the message's edit timestamp as raw string. Will be null if the message was not edited.
	/// </summary>
	[JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	internal string EditedTimestampRaw { get; set; }

	/// <summary>
	/// Gets whether this message was edited.
	/// </summary>
	[JsonIgnore]
	public bool IsEdited
		=> !string.IsNullOrWhiteSpace(this.EditedTimestampRaw);

	/// <summary>
	/// Gets the bitwise flags for this message.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public MessageFlags? Flags { get; internal set; }

	/// <summary>
	/// Gets whether the message mentions everyone.
	/// </summary>
	[JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
	public bool MentionEveryone { get; internal set; }

	/*
	 TODO: Implement if stable
		/// <summary>
		/// Gets users or members mentioned by this message.
		/// </summary>
		[JsonIgnore]
		public IReadOnlyList<DiscordUser> MentionedUsers
			=> this.MentionedUsersLazy.Value;
	*/

	[JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
	internal List<TransportUser> MentionedUsersInternal { get; set; } = [];

	/// <summary>
	/// Gets users mentioned by this message.
	/// </summary>
	[JsonIgnore]
	public List<DiscordUser> MentionedUsers
		=> this.MentionedUsersInternal.Count is not 0
			? this.MentionedUsersInternal.Select(x => new DiscordUser(x)
			{
				Discord = this.Discord
			}).ToList()
			: [];

	/*
	 TODO: Implement if stable

	 [JsonIgnore]
	 internal readonly Lazy<IReadOnlyList<DiscordUser>> MentionedUsersLazy;

		 /// <summary>
		 /// Gets roles mentioned by this message.
		 /// </summary>
		 [JsonIgnore]
		 public IReadOnlyList<DiscordRole> MentionedRoles
			 => this._mentionedRolesLazy.Value;

		 [JsonIgnore]
		 internal List<DiscordRole> MentionedRolesInternal;
	 */

	/// <summary>
	/// Gets role ids mentioned by this message.
	/// </summary>
	[JsonProperty("mention_roles")]
	public List<ulong> MentionedRoleIds = [];

	/*
	 TODO: Implement if stable
		[JsonIgnore]
		private readonly Lazy<IReadOnlyList<DiscordRole>> _mentionedRolesLazy;

		/// <summary>
		/// Gets channels mentioned by this message.
		/// </summary>
		[JsonIgnore]
		public IReadOnlyList<DiscordChannel> MentionedChannels
			=> this._mentionedChannelsLazy.Value;

		[JsonIgnore]
		internal List<DiscordChannel> MentionedChannelsInternal;

		[JsonIgnore]
		private readonly Lazy<IReadOnlyList<DiscordChannel>> _mentionedChannelsLazy;
	*/
}
