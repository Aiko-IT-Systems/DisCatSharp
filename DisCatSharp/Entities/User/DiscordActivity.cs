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
using System.Collections.Generic;
using System.Globalization;
using DisCatSharp.Net.Abstractions;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents user status.
    /// </summary>
    [JsonConverter(typeof(UserStatusConverter))]
    public enum UserStatus
    {
        /// <summary>
        /// User is offline.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// User is online.
        /// </summary>
        Online = 1,

        /// <summary>
        /// User is idle.
        /// </summary>
        Idle = 2,

        /// <summary>
        /// User asked not to be disturbed.
        /// </summary>
        DoNotDisturb = 4,

        /// <summary>
        /// User is invisible. They will appear as Offline to anyone but themselves.
        /// </summary>
        Invisible = 5,

        /// <summary>
        /// User is streaming.
        /// </summary>
        Streaming = 6
    }

    /// <summary>
    /// Represents a user status converter.
    /// </summary>
    internal sealed class UserStatusConverter : JsonConverter
    {
        /// <summary>
        /// Writes the json.
        /// </summary>
        /// <param name="Writer">The writer.</param>
        /// <param name="Value">The value.</param>
        /// <param name="Serializer">The serializer.</param>
        public override void WriteJson(JsonWriter Writer, object Value, JsonSerializer Serializer)
        {
            if (Value is UserStatus status)
            {
                switch (status) // reader.Value can be a string, DateTime or DateTimeOffset (yes, it's weird)
                {
                    case UserStatus.Online:
                        Writer.WriteValue("online");
                        return;

                    case UserStatus.Idle:
                        Writer.WriteValue("idle");
                        return;

                    case UserStatus.DoNotDisturb:
                        Writer.WriteValue("dnd");
                        return;

                    case UserStatus.Invisible:
                        Writer.WriteValue("invisible");
                        return;

                    case UserStatus.Streaming:
                        Writer.WriteValue("streaming");
                        return;

                    case UserStatus.Offline:
                    default:
                        Writer.WriteValue("offline");
                        return;
                }
            }
        }

        /// <summary>
        /// Reads the json.
        /// </summary>
        /// <param name="Reader">The reader.</param>
        /// <param name="ObjectType">The object type.</param>
        /// <param name="ExistingValue">The existing value.</param>
        /// <param name="Serializer">The serializer.</param>
        public override object ReadJson(JsonReader Reader, Type ObjectType, object ExistingValue, JsonSerializer Serializer)
        {
            // Active sessions are indicated with an "online", "idle", or "dnd" string per platform. If a user is
            // offline or invisible, the corresponding field is not present.
            return (Reader.Value?.ToString().ToLowerInvariant()) switch // reader.Value can be a string, DateTime or DateTimeOffset (yes, it's weird)
            {
                "online" => UserStatus.Online,
                "idle" => UserStatus.Idle,
                "dnd" => UserStatus.DoNotDisturb,
                "invisible" => UserStatus.Invisible,
                "streaming" => UserStatus.Streaming,
                _ => UserStatus.Offline,
            };
        }

        /// <summary>
        /// Whether this user5 status can be converted.
        /// </summary>
        /// <param name="ObjectType">The object type.</param>
        /// <returns>A bool.</returns>
        public override bool CanConvert(Type ObjectType) => ObjectType == typeof(UserStatus);
    }

    /// <summary>
    /// Represents a game that a user is playing.
    /// </summary>
    public sealed class DiscordActivity
    {
        /// <summary>
        /// Gets or sets the id of user's activity.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of user's activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stream URL, if applicable.
        /// </summary>
        public string StreamUrl { get; set; }

        /// <summary>
        /// Gets or sets platform in this rich presence.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets sync_id in this rich presence.
        /// </summary>
        public string SyncId { get; set; }

        /// <summary>
        /// Gets or sets session_id in this rich presence.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the activity type.
        /// </summary>
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets the rich presence details, if present.
        /// </summary>
        public DiscordRichPresence RichPresence { get; internal set; }

        /// <summary>
        /// Gets the custom status of this activity, if present.
        /// </summary>
        public DiscordCustomStatus CustomStatus { get; internal set; }

        /// <summary>
        /// Creates a new, empty instance of a <see cref="DiscordActivity"/>.
        /// </summary>
        public DiscordActivity()
        {
            this.ActivityType = ActivityType.Playing;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordActivity"/> with specified name.
        /// </summary>
        /// <param name="Name">Name of the activity.</param>
        public DiscordActivity(string Name)
        {
            this.Name = Name;
            this.ActivityType = ActivityType.Playing;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordActivity"/> with specified name.
        /// </summary>
        /// <param name="Name">Name of the activity.</param>
        /// <param name="Type">Type of the activity.</param>
        public DiscordActivity(string Name, ActivityType Type)
        {
            if (Type == ActivityType.Custom)
                throw new InvalidOperationException("Bots cannot use a custom status.");

            this.Name = Name;
            this.ActivityType = Type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordActivity"/> class.
        /// </summary>
        /// <param name="RawActivity">The raw activity.</param>
        internal DiscordActivity(TransportActivity RawActivity)
        {
            this.UpdateWith(RawActivity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordActivity"/> class.
        /// </summary>
        /// <param name="Other">The other.</param>
        internal DiscordActivity(DiscordActivity Other)
        {
            this.Name = Other.Name;
            this.ActivityType = Other.ActivityType;
            this.StreamUrl = Other.StreamUrl;
            this.SessionId = Other.SessionId;
            this.SyncId = Other.SyncId;
            this.Platform = Other.Platform;
            this.RichPresence = new DiscordRichPresence(Other.RichPresence);
            this.CustomStatus = new DiscordCustomStatus(Other.CustomStatus);
        }

        /// <summary>
        /// Updates a activity with an transport activity.
        /// </summary>
        /// <param name="RawActivity">The raw activity.</param>
        internal void UpdateWith(TransportActivity RawActivity)
        {
            this.Name = RawActivity?.Name;
            this.ActivityType = RawActivity != null ? RawActivity.ActivityType : ActivityType.Playing;
            this.StreamUrl = RawActivity?.StreamUrl;
            this.SessionId = RawActivity?.SessionId;
            this.SyncId = RawActivity?.SyncId;
            this.Platform = RawActivity?.Platform;

            if (RawActivity?.IsRichPresence() == true && this.RichPresence != null)
                this.RichPresence.UpdateWith(RawActivity);
            else this.RichPresence = RawActivity?.IsRichPresence() == true ? new DiscordRichPresence(RawActivity) : null;

            if (RawActivity?.IsCustomStatus() == true && this.CustomStatus != null)
                this.CustomStatus.UpdateWith(RawActivity.State, RawActivity.Emoji);
            else this.CustomStatus = RawActivity?.IsCustomStatus() == true
                ? new DiscordCustomStatus
                {
                    Name = RawActivity.State,
                    Emoji = RawActivity.Emoji
                }
                : null;
        }
    }

    /// <summary>
    /// Represents details for a custom status activity, attached to a <see cref="DiscordActivity"/>.
    /// </summary>
    public sealed class DiscordCustomStatus
    {
        /// <summary>
        /// Gets the name of this custom status.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the emoji of this custom status, if any.
        /// </summary>
        public DiscordEmoji Emoji { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordCustomStatus"/> class.
        /// </summary>
        internal DiscordCustomStatus() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordCustomStatus"/> class.
        /// </summary>
        /// <param name="Other">The other.</param>
        internal DiscordCustomStatus(DiscordCustomStatus Other)
        {
            this.Name = Other.Name;
            this.Emoji = Other.Emoji;
        }

        /// <summary>
        /// Updates a discord status.
        /// </summary>
        /// <param name="State">The state.</param>
        /// <param name="Emoji">The emoji.</param>
        internal void UpdateWith(string State, DiscordEmoji Emoji)
        {
            this.Name = State;
            this.Emoji = Emoji;
        }
    }

    /// <summary>
    /// Represents details for Discord rich presence, attached to a <see cref="DiscordActivity"/>.
    /// </summary>
    public sealed class DiscordRichPresence
    {
        /// <summary>
        /// Gets the details of this presence.
        /// </summary>
        public string Details { get; internal set; }

        /// <summary>
        /// Gets the game state.
        /// </summary>
        public string State { get; internal set; }

        /// <summary>
        /// Gets the application for which the rich presence is for.
        /// </summary>
        public DiscordApplication Application { get; internal set; }

        /// <summary>
        /// Gets the instance status.
        /// </summary>
        public bool? Instance { get; internal set; }

        /// <summary>
        /// Gets the large image for the rich presence.
        /// </summary>
        public DiscordAsset LargeImage { get; internal set; }

        /// <summary>
        /// Gets the hovertext for large image.
        /// </summary>
        public string LargeImageText { get; internal set; }

        /// <summary>
        /// Gets the small image for the rich presence.
        /// </summary>
        public DiscordAsset SmallImage { get; internal set; }

        /// <summary>
        /// Gets the hovertext for small image.
        /// </summary>
        public string SmallImageText { get; internal set; }

        /// <summary>
        /// Gets the current party size.
        /// </summary>
        public long? CurrentPartySize { get; internal set; }

        /// <summary>
        /// Gets the maximum party size.
        /// </summary>
        public long? MaximumPartySize { get; internal set; }

        /// <summary>
        /// Gets the party ID.
        /// </summary>
        public ulong? PartyId { get; internal set; }

        /// <summary>
        /// Gets the buttons.
        /// </summary>
        public IReadOnlyList<string> Buttons { get; internal set; }

        /// <summary>
        /// Gets the game start timestamp.
        /// </summary>
        public DateTimeOffset? StartTimestamp { get; internal set; }

        /// <summary>
        /// Gets the game end timestamp.
        /// </summary>
        public DateTimeOffset? EndTimestamp { get; internal set; }

        /// <summary>
        /// Gets the secret value enabling users to join your game.
        /// </summary>
        public string JoinSecret { get; internal set; }

        /// <summary>
        /// Gets the secret value enabling users to receive notifications whenever your game state changes.
        /// </summary>
        public string MatchSecret { get; internal set; }

        /// <summary>
        /// Gets the secret value enabling users to spectate your game.
        /// </summary>
        public string SpectateSecret { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRichPresence"/> class.
        /// </summary>
        internal DiscordRichPresence() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRichPresence"/> class.
        /// </summary>
        /// <param name="RawGame">The raw game.</param>
        internal DiscordRichPresence(TransportActivity RawGame)
        {
            this.UpdateWith(RawGame);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRichPresence"/> class.
        /// </summary>
        /// <param name="Other">The other.</param>
        internal DiscordRichPresence(DiscordRichPresence Other)
        {
            this.Details = Other.Details;
            this.State = Other.State;
            this.Application = Other.Application;
            this.Instance = Other.Instance;
            this.LargeImageText = Other.LargeImageText;
            this.SmallImageText = Other.SmallImageText;
            this.LargeImage = Other.LargeImage;
            this.SmallImage = Other.SmallImage;
            this.CurrentPartySize = Other.CurrentPartySize;
            this.MaximumPartySize = Other.MaximumPartySize;
            this.PartyId = Other.PartyId;
            this.Buttons = Other.Buttons;
            this.StartTimestamp = Other.StartTimestamp;
            this.EndTimestamp = Other.EndTimestamp;
            this.JoinSecret = Other.JoinSecret;
            this.MatchSecret = Other.MatchSecret;
            this.SpectateSecret = Other.SpectateSecret;
        }

        /// <summary>
        /// Updates a game activity with an transport activity.
        /// </summary>
        /// <param name="RawGame">The raw game.</param>
        internal void UpdateWith(TransportActivity RawGame)
        {
            this.Details = RawGame?.Details;
            this.State = RawGame?.State;
            this.Application = RawGame?.ApplicationId != null ? new DiscordApplication { Id = RawGame.ApplicationId.Value } : null;
            this.Instance = RawGame?.Instance;
            this.LargeImageText = RawGame?.Assets?.LargeImageText;
            this.SmallImageText = RawGame?.Assets?.SmallImageText;
            this.CurrentPartySize = RawGame?.Party?.Size?.Current;
            this.MaximumPartySize = RawGame?.Party?.Size?.Maximum;
            if (RawGame?.Party != null && ulong.TryParse(RawGame.Party.Id, NumberStyles.Number, CultureInfo.InvariantCulture, out var partyId))
                this.PartyId = partyId;
            this.Buttons = RawGame?.Buttons;
            this.StartTimestamp = RawGame?.Timestamps?.Start;
            this.EndTimestamp = RawGame?.Timestamps?.End;
            this.JoinSecret = RawGame?.Secrets?.Join;
            this.MatchSecret = RawGame?.Secrets?.Match;
            this.SpectateSecret = RawGame?.Secrets?.Spectate;

            var lid = RawGame?.Assets?.LargeImage;
            if (lid != null)
            {
                if (lid.StartsWith("spotify:"))
                    this.LargeImage = new DiscordSpotifyAsset { Id = lid };
                else if (ulong.TryParse(lid, NumberStyles.Number, CultureInfo.InvariantCulture, out var ulid))
                    this.LargeImage = new DiscordApplicationAsset { Id = lid, Application = this.Application, Type = ApplicationAssetType.LargeImage };
            }

            var sid = RawGame?.Assets?.SmallImage;
            if (sid != null)
            {
                if (sid.StartsWith("spotify:"))
                    this.SmallImage = new DiscordSpotifyAsset { Id = sid };
                else if (ulong.TryParse(sid, NumberStyles.Number, CultureInfo.InvariantCulture, out var usid))
                    this.SmallImage = new DiscordApplicationAsset { Id = sid, Application = this.Application, Type = ApplicationAssetType.LargeImage };
            }
        }
    }

    /// <summary>
    /// Determines the type of a user activity.
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// Indicates the user is playing a game.
        /// </summary>
        Playing = 0,

        /// <summary>
        /// Indicates the user is streaming a game.
        /// </summary>
        Streaming = 1,

        /// <summary>
        /// Indicates the user is listening to something.
        /// </summary>
        ListeningTo = 2,

        /// <summary>
        /// Indicates the user is watching something.
        /// </summary>
        Watching = 3,

        /// <summary>
        /// Indicates the current activity is a custom status.
        /// </summary>
        Custom = 4,

        /// <summary>
        /// Indicates the user is competing in something.
        /// </summary>
        Competing = 5
    }
}
