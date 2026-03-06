using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Voice;

/// <summary>
///     The discord client extensions.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	///     Creates a new Voice client with specified settings.
	/// </summary>
	/// <param name="client">Discord client to create Voice instance for.</param>
	/// <param name="config">Configuration for the Voice client.</param>
	/// <returns>Voice client instance.</returns>
	public static VoiceExtension UseVoice(this DiscordClient client, VoiceConfiguration? config = null)
	{
		if (client.GetExtension<VoiceExtension>() != null)
			throw new InvalidOperationException("Voice is already enabled for that client.");

		var vnext = new VoiceExtension(config ?? new());
		client.AddExtension(vnext);
		return vnext;
	}

	/// <summary>
	///     Creates new Voice clients on all shards in a given sharded client.
	/// </summary>
	/// <param name="client">Discord sharded client to create Voice instances for.</param>
	/// <param name="config">Configuration for the Voice clients.</param>
	/// <returns>A dictionary of created Voice clients.</returns>
	public static async Task<IReadOnlyDictionary<int, VoiceExtension>> UseVoiceAsync(this DiscordShardedClient client, VoiceConfiguration? config = null)
	{
		var modules = new Dictionary<int, VoiceExtension>();
		await client.InitializeShardsAsync().ConfigureAwait(false);

		foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
		{
			var vnext = shard.GetExtension<VoiceExtension>();
			vnext ??= shard.UseVoice(config);

			modules[shard.ShardId] = vnext;
		}

		return new ReadOnlyDictionary<int, VoiceExtension>(modules);
	}

	/// <summary>
	///     Gets the active instance of Voice client for the DiscordClient.
	/// </summary>
	/// <param name="client">Discord client to get Voice instance for.</param>
	/// <returns>Voice client instance.</returns>
	public static VoiceExtension? GetVoice(this DiscordClient client)
		=> client.GetExtension<VoiceExtension>();

	/// <summary>
	///     Retrieves a <see cref="VoiceExtension" /> instance for each shard.
	/// </summary>
	/// <param name="client">The shard client to retrieve <see cref="VoiceExtension" /> instances from.</param>
	/// <returns>A dictionary containing <see cref="VoiceExtension" /> instances for each shard.</returns>
	public static async Task<IReadOnlyDictionary<int, VoiceExtension?>> GetVoiceAsync(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		var extensions = client.ShardClients.Values.ToDictionary(shard => shard.ShardId, shard => shard.GetExtension<VoiceExtension>());

		return extensions.AsReadOnly();
	}

	/// <summary>
	///     Connects to this voice channel using Voice.
	/// </summary>
	/// <param name="channel">Channel to connect to.</param>
	/// <returns>If successful, the Voice connection.</returns>
	public static Task<VoiceConnection> ConnectAsync(this DiscordChannel channel)
	{
		ArgumentNullException.ThrowIfNull(channel);

		if (channel.Guild is null)
			throw new InvalidOperationException("Voice can only be used with guild channels.");

		if (channel.Type is not ChannelType.Voice and not ChannelType.Stage)
			throw new InvalidOperationException("You can only connect to voice or stage channels.");

		if (channel.Discord is not DiscordClient discord || discord is null)
			throw new NullReferenceException();

		var vnext = discord.GetVoice() ?? throw new InvalidOperationException("Voice is not initialized for this Discord client.");
		var vnc = vnext.GetConnection(channel.Guild);
		return vnc != null
			? throw new InvalidOperationException("Voice is already connected in this guild.")
			: vnext.ConnectAsync(channel);
	}
}
