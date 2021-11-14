// This file is part of the DisCatSharp project, a fork of DSharpPlus.
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
using DisCatSharp.Entities;
using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions
{
    /// <summary>
    /// The rest guild sheduled event create payload.
    /// </summary>
    internal class RestGuildSheduledEventCreatePayload
    {
        /// <summary>
        /// Gets or sets the channel id.
        /// </summary>
        [JsonProperty("channel_id")]
        public ulong? ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the entity metadata.
        /// </summary>
        [JsonProperty("entity_metadata")]
        public DiscordSheduledEventEntityMetadata EntityMetadata { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the privacy level of the scheduled event.
        /// </summary>
        [JsonProperty("privacy_level")]
        public SheduledEventPrivacyLevel PrivacyLevel { get; set; }

        /// <summary>
        /// Gets or sets the time to schedule the scheduled event.
        /// </summary>
        [JsonProperty("scheduled_start_time")]
        public DateTimeOffset SheduledStartTime { get; internal set; }

        /// <summary>
        /// Gets or sets the time when the scheduled event is scheduled to end.
        /// </summary>
        [JsonProperty("scheduled_end_time")]
        public DateTimeOffset? SheduledEndTime { get; internal set; }

        /// <summary>
        /// Gets or sets the entity type of the scheduled event.
        /// </summary>
        [JsonProperty("privacy_level")]
        public SheduledEventEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the image as base64.
        /// </summary>
        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageBase64 { get; set; }
    }

    /// <summary>
    /// The rest guild sheduled event modify payload.
    /// </summary>
    internal class RestGuildSheduledEventModifyPayload
    {
        /// <summary>
        /// Gets or sets the channel id.
        /// </summary>
        [JsonProperty("channel_id")]
        public Optional<ulong?> ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the entity metadata.
        /// </summary>
        [JsonProperty("entity_metadata")]
        public Optional<DiscordSheduledEventEntityMetadata> EntityMetadata { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonProperty("name")]
        public Optional<string> Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("description")]
        public Optional<string> Description { get; set; }

        /// <summary>
        /// Gets or sets the privacy level of the scheduled event.
        /// </summary>
        [JsonProperty("privacy_level")]
        public Optional<SheduledEventPrivacyLevel> PrivacyLevel { get; set; }

        /// <summary>
        /// Gets or sets the time to schedule the scheduled event.
        /// </summary>
        [JsonProperty("scheduled_start_time")]
        public Optional<DateTimeOffset> SheduledStartTime { get; internal set; }

        /// <summary>
        /// Gets or sets the time when the scheduled event is scheduled to end.
        /// </summary>
        [JsonProperty("scheduled_end_time")]
        public Optional<DateTimeOffset?> SheduledEndTime { get; internal set; }

        /// <summary>
        /// Gets or sets the entity type of the scheduled event.
        /// </summary>
        [JsonProperty("privacy_level")]
        public Optional<SheduledEventEntityType> EntityType { get; set; }

        /// <summary>
        /// Gets or sets the image as base64.
        /// </summary>
        [JsonProperty("image")]
        public Optional<string> ImageBase64 { get; set; }
    }
}
