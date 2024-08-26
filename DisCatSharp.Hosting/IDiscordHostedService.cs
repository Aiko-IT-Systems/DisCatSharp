using Microsoft.Extensions.Hosting;

namespace DisCatSharp.Hosting;

/// <summary>
///     Contract required for <see cref="DiscordClient" /> to work in a web hosting environment
/// </summary>
public interface IDiscordHostedService : IHostedService
{
	/// <summary>
	///     Reference to connected client
	/// </summary>
	DiscordClient Client { get; }
}

/// <summary>
///     Contract required for <see cref="DiscordShardedClient" /> to work in a web hosting environment
/// </summary>
public interface IDiscordHostedShardService : IHostedService
{
	DiscordShardedClient ShardedClient { get; }
}
