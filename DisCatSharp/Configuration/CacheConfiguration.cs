using System;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
///     Configuration for Discord client caching behavior.
/// </summary>
public sealed class CacheConfiguration
{
	/// <summary>
	///     Creates a new cache configuration with default values.
	/// </summary>
	public CacheConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another cache configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public CacheConfiguration(CacheConfiguration other)
	{
		this.MessageCacheSize = other.MessageCacheSize;
		this.PresenceCacheSize = other.PresenceCacheSize;
		this.AlwaysCacheMembers = other.AlwaysCacheMembers;
		this.AutoRefreshChannelCache = other.AutoRefreshChannelCache;
		this.AutoFetchApplicationEmojis = other.AutoFetchApplicationEmojis;
		this.AutoFetchSkuIds = other.AutoFetchSkuIds;
		this.SkuId = other.SkuId;
	}

	/// <summary>
	///     <para>Sets the size of the global message cache.</para>
	///     <para>Setting this to 0 will disable message caching entirely.</para>
	///     <para>Defaults to 1024.</para>
	/// </summary>
	public int MessageCacheSize { internal get; set; } = 1024;

	/// <summary>
	///     <para>Sets the maximum number of cached presence entries.</para>
	///     <para>
	///         This affects <see cref="DiscordClient.GetPresences" /> and <see cref="Entities.DiscordGuild.Presences" /> because both read from the centralized presence store.
	///     </para>
	///     <para>When the cap is exceeded, the oldest cached presence entries are evicted. Set to 0 to disable the cap. Defaults to 0.</para>
	/// </summary>
	public int PresenceCacheSize
	{
		internal get;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "Presence cache size cannot be negative.");

			field = value;
		}
	} = 0;

	/// <summary>
	///     Sets whether the client should attempt to cache members if exclusively using unprivileged intents.
	///     <para>
	///         This will only take effect if there are no <see cref="DiscordIntents.GuildMembers" /> or
	///         <see cref="DiscordIntents.GuildPresences" />
	///         intents specified. Otherwise, this will always be overwritten to true.
	///     </para>
	///     <para>Defaults to <see langword="true" />.</para>
	/// </summary>
	public bool AlwaysCacheMembers { internal get; set; } = true;

	/// <summary>
	///     <para>Refresh full guild channel cache.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool AutoRefreshChannelCache { internal get; set; } = false;

	/// <summary>
	///     <para>Defines that the client should attempt to fetch application emojis on startup.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool AutoFetchApplicationEmojis { internal get; set; } = false;

	/// <summary>
	///     Whether to autofetch the sku ids.
	///     <para>Mutually exclusive to <see cref="SkuId" />.</para>
	/// </summary>
	[RequiresFeature(Features.MonetizedApplication)]
	public bool AutoFetchSkuIds { internal get; set; } = false;

	/// <summary>
	///     The applications sku id for premium apps.
	///     <para>Mutually exclusive to <see cref="AutoFetchSkuIds" />.</para>
	/// </summary>
	[RequiresFeature(Features.MonetizedApplication)]
	public ulong? SkuId { internal get; set; } = null;
}
