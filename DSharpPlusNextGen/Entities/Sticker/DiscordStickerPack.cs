// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlusNextGen.Enums.Discord;
using DSharpPlusNextGen.Net;
using Newtonsoft.Json;

namespace DSharpPlusNextGen.Entities
{
    /// <summary>
    /// Represents a Discord sticker pack.
    /// </summary>
    public sealed class DiscordStickerPack : SnowflakeObject
    {
        /// <summary>
        /// Gets the stickers contained in this pack.
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordSticker> Stickers => this._stickers;

        [JsonProperty("stickers")]
        internal Dictionary<ulong, DiscordSticker> _stickers = new();

        /// <summary>
        /// Gets the name of this sticker pack.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("sku_id")]
        public ulong SkuId { get; internal set; }

        /// <summary>
        /// Gets the Id of this pack's cover sticker.
        /// </summary>
        [JsonProperty("cover_sticker_id")]
        public ulong CoverStickerId { get; internal set; }

        /// <summary>
        /// Gets the pack's cover sticker.
        /// </summary>
        public Task<DiscordSticker> CoverSticker => this.Discord.ApiClient.GetStickerAsync(this.CoverStickerId);

        /// <summary>
        /// Gets the Id of this pack's banner.
        /// </summary>
        [JsonProperty("banner_asset_id")]
        public ulong BannerAssetId { get; internal set; }

        /// <summary>
        /// Gets the pack's banner url.
        /// </summary>
        public string BannerUrl => $"https://cdn.{DiscordDomain.GetDomain(CoreDomain.DiscordAppOld).Domain}/{Endpoints.APP_ASSETS}/710982414301790216/{Endpoints.STORE}/{this.BannerAssetId}.png?size=4096";

        internal DiscordStickerPack() { }
    }
}
