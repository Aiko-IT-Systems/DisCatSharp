// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets a sticker.
	/// </summary>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetStickerAsync(ulong stickerId)
	{
		var route = $"{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Gets the sticker packs.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
	{
		var route = $"{Endpoints.STICKERPACKS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response)["sticker_packs"] as JArray;
		var ret = json.ToDiscordObject<DiscordStickerPack[]>();

		return ret.ToList();
	}
}
