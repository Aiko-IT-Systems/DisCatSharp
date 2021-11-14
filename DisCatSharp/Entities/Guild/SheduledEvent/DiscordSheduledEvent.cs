// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021 AITSYS
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
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents an scheduled event.
    /// </summary>
    public class DiscordSheduledEvent : SnowflakeObject, IEquatable<DiscordSheduledEvent>
    {
        /// <summary>
        /// Gets the guild id of the associated sheduled event.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the guild to which this sheduled event belongs.
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
        /// Gets the ID of the user that created the sheduled event.
        /// </summary>
        [JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong CreatorId { get; internal set; }

        /// <summary>
        /// Gets the member that created the sheduled event.
        /// </summary>
        [JsonIgnore]
        public DiscordMember CreatorMember
            => this.Guild._members.TryGetValue(this.CreatorId, out var owner)
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
        /* TODO|INFO: Is not available yet to users / clients / bots.
        /// <summary>
        /// Gets this channels's banner hash, when applicable.
        /// </summary>
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageHash { get; internal set; }

        /// <summary>
        /// Gets this channels's banner in url form.
        /// </summary>
        [JsonIgnore]
        public string ImageUrl
            => !string.IsNullOrWhiteSpace(this.ImageHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Uri}{Endpoints.EVENTS}/{this.GuildId.ToString(CultureInfo.InvariantCulture)}/images/{this.ImageHash}.{(this.ImageHash.StartsWith("a_") ? "gif" : "png")}" : null;
        */
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
        public SheduledEventPrivacyLevel PrivacyLevel { get; internal set; }

        /// <summary>
        /// Gets the status of the scheduled event.
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public SheduledEventStatus Status { get; internal set; }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        [JsonProperty("entity_type", NullValueHandling = NullValueHandling.Ignore)]
        public SheduledEventEntityType EntityType { get; internal set; }

        /// <summary>
        /// Gets id of the entity.
        /// </summary>
        [JsonProperty("entity_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? EntityId { get; internal set; }

        /// <summary>
        /// Gets metadata of the entity.
        /// </summary>
        [JsonProperty("entity_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSheduledEventEntityMetadata EntityMetadata { get; internal set; }

        /// <summary>
        /// Gets the total number of users subscribed to the scheduled event.
        /// </summary>
        [JsonProperty("sku_ids", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<ulong> SkuIds { get; internal set; }

        /// <summary>
        /// Gets the user that created the sheduled event.
        /// </summary>
        [JsonProperty("creator", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Creator { get; internal set; }

        // TODO: Skus

        /// <summary>
        /// Gets the total number of users subscribed to the scheduled event.
        /// </summary>
        [JsonProperty("user_count", NullValueHandling = NullValueHandling.Ignore)]
        public int UserCount { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordSheduledEvent"/> class.
        /// </summary>
        internal DiscordSheduledEvent() { }

        #region Methods
        // TODO: Later
        /// <summary>
        /// Updates a scheduled event.
        /// </summary>
        /// <param name="channel">New channel of the sheduled event.</param>
        /// <param name="metadata">New <see cref="EntityMetadata"/> of the sheduled event.</param>
        /// <param name="name">New name of the sheduled event.</param>
        /// <param name="privacy_level">New <see cref="SheduledEventPrivacyLevel"/> of the sheduled event.</param>
        /// <param name="scheduled_start_time">New DateTime when the sheduled event should start.</param>
        /// <param name="scheduled_end_time">New DateTime when the sheduled event should end.</param>
        /// <param name="description">New description of the sheduled event.</param>
        /// <param name="type">New <see cref="SheduledEventEntityType"/> of the sheduled event.</param>
        /// <param name="reason">Audit log reason</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the event does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task ModifyAsync(Optional<DiscordChannel> channel, Optional<DiscordSheduledEventEntityMetadata> metadata, Optional<string> name, Optional<StagePrivacyLevel> privacy_level, Optional<DateTime> scheduled_start_time, Optional<DateTime> scheduled_end_time, Optional<string> description, Optional<SheduledEventEntityType> type, string reason = null)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            => throw new NotImplementedException("This method is not implemented yet."); /*await this.Discord.ApiClient.ModifyStageEventAsync(this.Id, channel, name, scheduled_start_time, description, privacy_level, type, reason);*/

        /// <summary>
        /// Gets a list of users RSVP'd to the sheduled event.
        /// </summary>
        /// <param name="limit">The limit how many users to receive from the event.</param>
        /// <param name="with_members">Wether to include guild member data.</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the correct permissions.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the event does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyDictionary<ulong, DiscordUser>> GetUsersAsync(int? limit = null, bool? with_members = null)
            => await this.Discord.ApiClient.GetGuildSheduledEventRSPVUsersAsync(this.GuildId, this.Id, limit, with_members);

        /// <summary>
        /// Deletes a scheduled event.
        /// </summary>
        /// <param name="reason">Audit log reason</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the event does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task DeleteAsync(string reason = null)
            => await this.Discord.ApiClient.DeleteGuildSheduledEventAsync(this.GuildId ,this.Id, reason);

        #endregion

        /// <summary>
        /// Checks whether this <see cref="DiscordSheduledEvent"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordSheduledEvent"/>.</returns>
        public override bool Equals(object obj)
            => this.Equals(obj as DiscordSheduledEvent);

        /// <summary>
        /// Checks whether this <see cref="DiscordSheduledEvent"/> is equal to another <see cref="DiscordSheduledEvent"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordSheduledEvent"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordSheduledEvent"/> is equal to this <see cref="DiscordSheduledEvent"/>.</returns>
        public bool Equals(DiscordSheduledEvent e)
            => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordSheduledEvent"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordSheduledEvent"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordSheduledEvent"/> objects are equal.
        /// </summary>
        /// <param name="e1">First event to compare.</param>
        /// <param name="e2">Second ecent to compare.</param>
        /// <returns>Whether the two events are equal.</returns>
        public static bool operator ==(DiscordSheduledEvent e1, DiscordSheduledEvent e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordSheduledEvent"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First event to compare.</param>
        /// <param name="e2">Second event to compare.</param>
        /// <returns>Whether the two events are not equal.</returns>
        public static bool operator !=(DiscordSheduledEvent e1, DiscordSheduledEvent e2)
            => !(e1 == e2);
    }
}
