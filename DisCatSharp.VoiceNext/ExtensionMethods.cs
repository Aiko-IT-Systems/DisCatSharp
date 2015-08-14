using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.VoiceNext;

/// <summary>
/// The discord client extensions.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	/// Creates a new VoiceNext client with specified settings.
	/// </summary>
	/// <param name="client">Discord client to create VoiceNext instance for.</param>
	/// <param name="config">Configuration for the VoiceNext client.</param>
	/// <returns>VoiceNext client instance.</returns>
	public static VoiceNextExtension UseVoiceNext(this DiscordClient client, VoiceNextConfiguration? config = null)
	{
		if (client.GetExtension<VoiceNextExtension>() != null)
			throw new InvalidOperationException("VoiceNext is already enabled for that client.");

		var vnext = new VoiceNextExtension(config ?? new());
		client.AddExtension(vnext);
		return vnext;
	}

	/// <summary>
	/// Creates new VoiceNext clients on all shards in a given sharded client.
	/// </summary>
	/// <param name="client">Discord sharded client to create VoiceNext instances for.</param>
	/// <param name="config">Configuration for the VoiceNext clients.</param>
	/// <returns>A dictionary of created VoiceNext clients.</returns>
	public static async Task<IReadOnlyDictionary<int, VoiceNextExtension>> UseVoiceNextAsync(this DiscordShardedClient client, VoiceNextConfiguration? config = null)
	{
		var modules = new Dictionary<int, VoiceNextExtension>();
		await client.InitializeShardsAsync().ConfigureAwait(false);

		foreach (var shard in client.ShardClients.Select(xkvp => xkvp.Value))
		{
			var vnext = shard.GetExtension<VoiceNextExtension>();
			vnext ??= shard.UseVoiceNext(config);

			modules[shard.ShardId] = vnext;
		}

		return new ReadOnlyDictionary<int, VoiceNextExtension>(modules);
	}

	/// <summary>
	/// Gets the active instance of VoiceNext client for the DiscordClient.
	/// </summary>
	/// <param name="client">Discord client to get VoiceNext instance for.</param>
	/// <returns>VoiceNext client instance.</returns>
	public static VoiceNextExtension? GetVoiceNext(this DiscordClient client)
		=> client.GetExtension<VoiceNextExtension>();

	/// <summary>
	/// Retrieves a <see cref="VoiceNextExtension"/> instance for each shard.
	/// </summary>
	/// <param name="client">The shard client to retrieve <see cref="VoiceNextExtension"/> instances from.</param>
	/// <returns>A dictionary containing <see cref="VoiceNextExtension"/> instances for each shard.</returns>
	public static async Task<IReadOnlyDictionary<int, VoiceNextExtension?>> GetVoiceNextAsync(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		var extensions = client.ShardClients.Values.ToDictionary(shard => shard.ShardId, shard => shard.GetExtension<VoiceNextExtension>());

		return extensions.AsReadOnly();
	}

	/// <summary>
	/// Connects to this voice channel using VoiceNext.
	/// </summary>
	/// <param name="channel">Channel to connect to.</param>
	/// <returns>If successful, the VoiceNext connection.</returns>
	public static Task<VoiceNextConnection> ConnectAsync(this DiscordChannel channel)
	{
		ArgumentNullException.ThrowIfNull(channel);

		if (channel.Guild is null)
			throw new InvalidOperationException("VoiceNext can only be used with guild channels.");

		if (channel.Type is not ChannelType.Voice and not ChannelType.Stage)
			throw new InvalidOperationException("You can only connect to voice or stage channels.");

		if (channel.Discord is not DiscordClient discord || discord is null)
			throw new NullReferenceException();

		var vnext = discord.GetVoiceNext() ?? throw new InvalidOperationException("VoiceNext is not initialized for this Discord client.");
		var vnc = vnext.GetConnection(channel.Guild);
		return vnc != null
			? throw new InvalidOperationException("VoiceNext is already connected in this guild.")
			: vnext.ConnectAsync(channel);
	}
}
