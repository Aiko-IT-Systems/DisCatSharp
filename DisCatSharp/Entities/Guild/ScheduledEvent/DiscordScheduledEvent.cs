// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Models;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an scheduled event.
/// </summary>
public class DiscordScheduledEvent : SnowflakeObject, IEquatable<DiscordScheduledEvent>
{
	/// <summary>
	/// Gets the guild id of the associated scheduled event.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the guild to which this scheduled event belongs.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;

	/// <summary>
	/// Gets the associated channel.
	/// </summary>
	[JsonIgnore]
	public Task<DiscordChannel> Channel
		=> this.ChannelId.HasValue ? this.Discord.ApiClient.GetChannelAsync(this.ChannelId.Value) : null;

	/// <summary>
	/// Gets id of the associated channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	/// <summary>
	/// Gets the ID of the user that created the scheduled event.
	/// </summary>
	[JsonProperty("creator_id")]
	public ulong CreatorId { get; internal set; }

	/// <summary>
	/// Gets the user that created the scheduled event.
	/// </summary>
	[JsonProperty("creator")]
	public DiscordUser Creator { get; internal set; }

	/// <summary>
	/// Gets the member that created the scheduled event.
	/// </summary>
	[JsonIgnore]
	public DiscordMember CreatorMember
		=> this.Guild.MembersInternal.TryGetValue(this.CreatorId, out var owner)
			? owner
			: this.Discord.ApiClient.GetGuildMemberAsync(this.GuildId, this.CreatorId).Result;

	/// <summary>
	/// Gets the name of the scheduled event.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the description of the scheduled event.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// Gets this event's cover hash, when applicable.
	/// </summary>
	[JsonProperty("image", NullValueHandling = NullValueHandling.Include)]
	public string CoverImageHash { get; internal set; }

	/// <summary>
	/// Gets this event's cover in url form.
	/// </summary>
	[JsonIgnore]
	public string CoverImageUrl
		=> !string.IsNullOrWhiteSpace(this.CoverImageHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Uri}{Endpoints.GUILD_EVENTS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.CoverImageHash}.png" : null;

	/// <summary>
	/// Gets the scheduled start time of the scheduled event.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? ScheduledStartTime
		=> !string.IsNullOrWhiteSpace(this.ScheduledStartTimeRaw) && DateTimeOffset.TryParse(this.ScheduledStartTimeRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
			dto : null;

	/// <summary>
	/// Gets the scheduled start time of the scheduled event as raw string.
	/// </summary>
	[JsonProperty("scheduled_start_time", NullValueHandling = NullValueHandling.Ignore)]
	internal string ScheduledStartTimeRaw { get; set; }

	/// <summary>
	/// Gets the scheduled end time of the scheduled event.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? ScheduledEndTime
		=> !string.IsNullOrWhiteSpace(this.ScheduledEndTimeRaw) && DateTimeOffset.TryParse(this.ScheduledEndTimeRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
			dto : null;

	/// <summary>
	/// Gets the scheduled end time of the scheduled event as raw string.
	/// </summary>
	[JsonProperty("scheduled_end_time", NullValueHandling = NullValueHandling.Ignore)]
	internal string ScheduledEndTimeRaw { get; set; }

	/// <summary>
	/// Gets the privacy level of the scheduled event.
	/// </summary>
	[JsonProperty("privacy_level", NullValueHandling = NullValueHandling.Ignore)]
	internal ScheduledEventPrivacyLevel PrivacyLevel { get; set; }

	/// <summary>
	/// Gets the status of the scheduled event.
	/// </summary>
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public ScheduledEventStatus Status { get; internal set; }

	/// <summary>
	/// Gets the entity type.
	/// </summary>
	[JsonProperty("entity_type", NullValueHandling = NullValueHandling.Ignore)]
	public ScheduledEventEntityType EntityType { get; internal set; }

	/// <summary>
	/// Gets id of the entity.
	/// </summary>
	[JsonProperty("entity_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EntityId { get; internal set; }

	/// <summary>
	/// Gets metadata of the entity.
	/// </summary>
	[JsonProperty("entity_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordScheduledEventEntityMetadata EntityMetadata { get; internal set; }

	/* This isn't used.
         * See https://github.com/discord/discord-api-docs/pull/3586#issuecomment-969066061.
         * Was originally for paid stages.
        /// <summary>
        /// Gets the sku ids of the scheduled event.
        /// </summary>
        [JsonProperty("sku_ids", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<ulong> SkuIds { get; internal set; }*/

	/// <summary>
	/// Gets the total number of users subscribed to the scheduled event.
	/// </summary>
	[JsonProperty("user_count", NullValueHandling = NullValueHandling.Ignore)]
	public int UserCount { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordScheduledEvent"/> class.
	/// </summary>
	internal DiscordScheduledEvent()
	{ }

	#region Methods


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Modifies the current scheduled event.
	/// </summary>
	/// <param name="action">Action to perform on this thread</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the event does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task ModifyAsync(Action<ScheduledEventEditModel> action)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
	{
		var mdl = new ScheduledEventEditModel();
		action(mdl);

		Optional<ulong?> channelId = null;
		if (this.EntityType == ScheduledEventEntityType.External || mdl.EntityType != ScheduledEventEntityType.External)
			channelId = mdl.Channel
				.MapOrNull<ulong?>(c => c.Type != ChannelType.Voice && c.Type != ChannelType.Stage
					? throw new ArgumentException("Channel needs to be a voice or stage channel.")
					: c.Id);

		var coverb64 = ImageTool.Base64FromStream(mdl.CoverImage);

		var scheduledEndTime = Optional<DateTimeOffset>.None;
		if (mdl.ScheduledEndTime.HasValue && mdl.EntityType.HasValue ? mdl.EntityType == ScheduledEventEntityType.External : this.EntityType == ScheduledEventEntityType.External)
			scheduledEndTime = mdl.ScheduledEndTime.Value;

		await this.Discord.ApiClient.ModifyGuildScheduledEventAsync(this.GuildId, this.Id, channelId, this.EntityType == ScheduledEventEntityType.External ? new DiscordScheduledEventEntityMetadata(mdl.Location.Value) : null, mdl.Name, mdl.ScheduledStartTime, scheduledEndTime, mdl.Description, mdl.EntityType, mdl.Status, coverb64, mdl.AuditLogReason);
	}


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Starts the current scheduled event.
	/// </summary>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the event does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent> StartAsync(string reason = null)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
		=> this.Status == ScheduledEventStatus.Scheduled ? await this.Discord.ApiClient.ModifyGuildScheduledEventStatusAsync(this.GuildId, this.Id, ScheduledEventStatus.Active, reason) : throw new InvalidOperationException("You can only start scheduled events");


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Cancels the current scheduled event.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the event does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent> CancelAsync(string reason = null)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
		=> this.Status == ScheduledEventStatus.Scheduled ? await this.Discord.ApiClient.ModifyGuildScheduledEventStatusAsync(this.GuildId, this.Id, ScheduledEventStatus.Canceled, reason) : throw new InvalidOperationException("You can only cancel scheduled events");


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Ends the current scheduled event.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the event does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent> EndAsync(string reason = null)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
		=> this.Status == ScheduledEventStatus.Active ? await this.Discord.ApiClient.ModifyGuildScheduledEventStatusAsync(this.GuildId, this.Id, ScheduledEventStatus.Completed, reason) : throw new InvalidOperationException("You can only stop active events");


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Gets a list of users RSVP'd to the scheduled event.
	/// </summary>
	/// <param name="limit">The limit how many users to receive from the event. Defaults to 100. Max 100.</param>
	/// <param name="before">Get results of <see cref="DiscordScheduledEventUser"/> before the given snowflake.</param>
	/// <param name="after">Get results of <see cref="DiscordScheduledEventUser"/> after the given snowflake.</param>
	/// <param name="withMember">Whether to include guild member data.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the correct permissions.</exception>
	/// <exception cref="NotFoundException">Thrown when the event does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyDictionary<ulong, DiscordScheduledEventUser>> GetUsersAsync(int? limit = null, ulong? before = null, ulong? after = null, bool? withMember = null)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
		=> await this.Discord.ApiClient.GetGuildScheduledEventRspvUsersAsync(this.GuildId, this.Id, limit, before, after, withMember);


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Deletes a scheduled event.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the event does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task DeleteAsync(string reason = null)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
		=> await this.Discord.ApiClient.DeleteGuildScheduledEventAsync(this.GuildId, this.Id, reason);

	#endregion

	/// <summary>
	/// Checks whether this <see cref="DiscordScheduledEvent"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordScheduledEvent"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordScheduledEvent);

	/// <summary>
	/// Checks whether this <see cref="DiscordScheduledEvent"/> is equal to another <see cref="DiscordScheduledEvent"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordScheduledEvent"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordScheduledEvent"/> is equal to this <see cref="DiscordScheduledEvent"/>.</returns>
	public bool Equals(DiscordScheduledEvent e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordScheduledEvent"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordScheduledEvent"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordScheduledEvent"/> objects are equal.
	/// </summary>
	/// <param name="e1">First event to compare.</param>
	/// <param name="e2">Second event to compare.</param>
	/// <returns>Whether the two events are equal.</returns>
	public static bool operator ==(DiscordScheduledEvent e1, DiscordScheduledEvent e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordScheduledEvent"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First event to compare.</param>
	/// <param name="e2">Second event to compare.</param>
	/// <returns>Whether the two events are not equal.</returns>
	public static bool operator !=(DiscordScheduledEvent e1, DiscordScheduledEvent e2)
		=> !(e1 == e2);
}
