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
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a Stage Event.
    /// </summary>
    public class DiscordStageEvent : SnowflakeObject, IEquatable<DiscordStageEvent>
    {
        /// <summary>
        /// Gets id of the associated Stage Channel.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Stage
            => this.Discord.Channels.TryGetValue(this.ChannelId, out var guild) ? guild : null;
            
        /// <summary>
        /// Gets id of the associated Stage Channel id.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the name of the Stage Event.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the description of the Stage Event.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the entity type of the Stage Event.
        /// Research.
        /// </summary>
        [JsonProperty("entity_type", NullValueHandling = NullValueHandling.Ignore)]
        public int? EntityType { get; internal set; }
        
        /// <summary>
        /// Gets the scheduled start time of the Stage Event.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? ScheduledStartTime
            => !string.IsNullOrWhiteSpace(this.ScheduledStartTimeRaw) && DateTimeOffset.TryParse(this.ScheduledStartTimeRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
                dto : null;

        /// <summary>
        /// Gets the scheduled start time of the Stage Event as raw string.
        /// </summary>
        [JsonProperty("scheduled_start_time", NullValueHandling = NullValueHandling.Ignore)]
        internal string ScheduledStartTimeRaw { get; set; }
        
        /// <summary>
        /// Gets the topic of the Stage Event.
        /// </summary>
        [JsonProperty("privacy_level", NullValueHandling = NullValueHandling.Ignore)]
        public StagePrivacyLevel PrivacyLevel { get; internal set; }
        
        internal DiscordStageEvent() { }
        
        #region Methods

        /// <summary>
        /// Creates a Stage Event.
        /// </summary>
        /// <param name="topic">Topic of the stage instance.</param>
        /// <param name="privacy_level">Privacy Level of the stage instance.</param>
        /// <param name="reason">Audit log reason</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the stage channel does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /*public async Task CreateAsync(Optional<string> topic, Optional<StagePrivacyLevel> privacy_level, string reason = null)
            => await this.Discord.ApiClient.CreateStageEventAsync(this.Id, topic, privacy_level, reason);*/

        /// <summary>
        /// Updates a Stage Event.
        /// </summary>
        /// <param name="topic">Topic of the stage instance.</param>
        /// <param name="privacy_level">Privacy Level of the stage instance.</param>
        /// <param name="reason">Audit log reason</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the event does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /**/public async Task ModifyAsync(Optional<string> topic, Optional<StagePrivacyLevel> privacy_level, string reason = null)
            => await this.Discord.ApiClient.ModifyStageEventAsync(this.Id, topic, privacy_level, reason);*/

        /// <summary>
        /// Deletes a Stage Event.
        /// </summary>
        /// <param name="reason">Audit log reason</param>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEvents"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the event does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /*public async Task DeleteAsync(string reason = null)
            => await this.Discord.ApiClient.DeleteStageEventAsync(this.Id, reason);*/

        #endregion

        /// <summary>
        /// Checks whether this <see cref="DiscordStageEvent"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordStageEvent"/>.</returns>
        public override bool Equals(object obj)
            => this.Equals(obj as DiscordStageEvent);

        /// <summary>
        /// Checks whether this <see cref="DiscordStageEvent"/> is equal to another <see cref="DiscordStageEvent"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordStageEvent"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordStageEvent"/> is equal to this <see cref="DiscordStageEvent"/>.</returns>
        public bool Equals(DiscordStageEvent e)
            => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordStageEvent"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordStageEvent"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordStageEvent"/> objects are equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are equal.</returns>
        public static bool operator ==(DiscordStageEvent e1, DiscordStageEvent e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordStageEvent"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are not equal.</returns>
        public static bool operator !=(DiscordStageEvent e1, DiscordStageEvent e2)
            => !(e1 == e2);
    }
}
