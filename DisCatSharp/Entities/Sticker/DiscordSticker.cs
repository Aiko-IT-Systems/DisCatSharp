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
using System.Threading.Tasks;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a Discord Sticker.
    /// </summary>
    public class DiscordSticker : SnowflakeObject, IEquatable<DiscordSticker>
    {
        /// <summary>
        /// Gets the Pack ID of this sticker.
        /// </summary>
        [JsonProperty("pack_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? PackId { get; internal set; }

        /// <summary>
        /// Gets the Name of the sticker.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the Description of the sticker.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the type of sticker.
        /// </summary>
        [JsonProperty("type")]
        public StickerType Type { get; internal set; }

        /// <summary>
        /// For guild stickers, gets the user that made the sticker.
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the guild associated with this sticker, if any.
        /// </summary>
        public DiscordGuild Guild => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

        /// <summary>
        /// Gets the guild id the sticker belongs too.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? GuildId { get; internal set; }

        /// <summary>
        /// Gets whether this sticker is available. Only applicable to guild stickers.
        /// </summary>
        [JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
        public bool Available { get; internal set; }

        /// <summary>
        /// Gets the sticker's sort order, if it's in a pack.
        /// </summary>
        [JsonProperty("sort_value", NullValueHandling = NullValueHandling.Ignore)]
        public int? SortValue { get; internal set; }

        /// <summary>
        /// Gets the list of tags for the sticker.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string> Tags
            => this.InternalTags != null ? this.InternalTags.Split(',') : Array.Empty<string>();

        /// <summary>
        /// Gets the asset hash of the sticker.
        /// </summary>
        [JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
        public string Asset { get; internal set; }

        /// <summary>
        /// Gets the preview asset hash of the sticker.
        /// </summary>
        [JsonProperty("preview_asset", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviewAsset { get; internal set; }

        /// <summary>
        /// Gets the Format type of the sticker.
        /// </summary>
        [JsonProperty("format_type")]
        public StickerFormat FormatType { get; internal set; }

        /// <summary>
        /// Gets the tags of the sticker.
        /// </summary>
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        internal string InternalTags { get; set; }

        /// <summary>
        /// Gets the url of the sticker.
        /// </summary>
        [JsonIgnore]
        public string Url => $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Stickers}/{this.Id}.{(this.FormatType == StickerFormat.Lottie ? "json" : "png")}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordSticker"/> class.
        /// </summary>
        internal DiscordSticker() { }

        /// <summary>
        /// Whether to stickers are equal.
        /// </summary>
        /// <param name="Other">DiscordSticker</param>
        /// <returns></returns>
        public bool Equals(DiscordSticker Other) => this.Id == Other.Id;

        /// <summary>
        /// Gets the sticker in readable format.
        /// </summary>
        public override string ToString() => $"Sticker {this.Id}; {this.Name}; {this.FormatType}";

        /// <summary>
        /// Modifies the sticker
        /// </summary>
        /// <param name="Name">The name of the sticker</param>
        /// <param name="Description">The description of the sticker</param>
        /// <param name="Tags">The name of a unicode emoji representing the sticker's expression</param>
        /// <param name="Reason">Audit log reason</param>
        /// <returns>A sticker object</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the sticker could not be found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.ArgumentException">Sticker does not belong to a guild.</exception>
        public Task<DiscordSticker> Modify(Optional<string> Name, Optional<string> Description, Optional<string> Tags, string Reason = null)
        {
            return !this.GuildId.HasValue
                ? throw new ArgumentException("This sticker does not belong to a guild.")
                : Name.HasValue && (Name.Value.Length < 2 || Name.Value.Length > 30)
                ? throw new ArgumentException("Sticker name needs to be between 2 and 30 characters long.")
                : Description.HasValue && (Description.Value.Length < 1 || Description.Value.Length > 100)
                ? throw new ArgumentException("Sticker description needs to be between 1 and 100 characters long.")
                : Tags.HasValue && !DiscordEmoji.TryFromUnicode(this.Discord, Tags.Value, out var emoji)
                ? throw new ArgumentException("Sticker tags needs to be a unicode emoji.")
                : this.Discord.ApiClient.ModifyGuildStickerAsync(this.GuildId.Value, this.Id, Name, Description, Tags, Reason);
        }

        /// <summary>
        /// Deletes the sticker
        /// </summary>
        /// <param name="Reason">Audit log reason</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the sticker could not be found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.ArgumentException">Sticker does not belong to a guild.</exception>
        public Task Delete(string Reason = null)
            => this.GuildId.HasValue ? this.Discord.ApiClient.DeleteGuildStickerAsync(this.GuildId.Value, this.Id, Reason) : throw new ArgumentException("The requested sticker is no guild sticker.");
    }

    /// <summary>
    /// The sticker type
    /// </summary>
    public enum StickerType : long
    {
        /// <summary>
        /// Standard nitro sticker
        /// </summary>
        Standard = 1,
        /// <summary>
        /// Custom guild sticker
        /// </summary>
        Guild = 2
    }

    /// <summary>
    /// The sticker type
    /// </summary>
    public enum StickerFormat : long
    {
        /// <summary>
        /// Sticker is a png
        /// </summary>
        Png = 1,
        /// <summary>
        /// Sticker is a animated png
        /// </summary>
        Apng = 2,
        /// <summary>
        /// Sticker is lottie
        /// </summary>
        Lottie = 3
    }
}
