namespace DisCatSharp.Hosting;

/// <summary>
/// Contract required for <see cref="DiscordClient"/> to work in a web hosting environment
/// </summary>
public interface IDiscordHostedService : Microsoft.Extensions.Hosting.IHostedService
{
	/// <summary>
	/// Reference to connected client
	/// </summary>
	DiscordClient Client { get; }
}

/// <summary>
/// Contract required for <see cref="DiscordShardedClient"/> to work in a web hosting environment
/// </summary>
public interface IDiscordHostedShardService : Microsoft.Extensions.Hosting.IHostedService
{
	DiscordShardedClient ShardedClient { get; }
}
