using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord sticker pack.
/// </summary>
public sealed class DiscordStickerPack : SnowflakeObject
{
	/// <summary>
	/// Gets the stickers contained in this pack.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordSticker> Stickers => this.StickersInternal;

	[JsonProperty("stickers")]
	internal List<DiscordSticker> StickersInternal = [];

	/// <summary>
	/// Gets the name of this sticker pack.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the sku id.
	/// </summary>
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
	[JsonIgnore]
	public string BannerUrl => $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.APP_ASSETS}{Endpoints.STICKER_APPLICATION}{Endpoints.STORE}/{this.BannerAssetId}.png?size=4096";

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordStickerPack"/> class.
	/// </summary>
	internal DiscordStickerPack()
	{ }
}
