using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net;

using Microsoft.Extensions.Logging;

using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

using Octokit;
using Octokit.Internal;

using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Repository = NuGet.Protocol.Core.Types.Repository;

namespace DisCatSharp;

/// <summary>
/// Various Discord-related utilities.
/// </summary>
public static class Utilities
{
	/// <summary>
	/// Gets the version of the library.
	/// </summary>
	internal static string VersionHeader { get; set; }

	/// <summary>
	/// Gets or sets the permission strings.
	/// </summary>
	internal static Dictionary<Permissions, string> PermissionStrings { get; set; }

	/// <summary>
	/// Gets the utf8 encoding
	/// </summary>
	// ReSharper disable once InconsistentNaming
	internal static UTF8Encoding UTF8 { get; } = new(false);

	/// <summary>
	/// Initializes a new instance of the <see cref="Utilities"/> class.
	/// </summary>
	static Utilities()
	{
		PermissionStrings = new();
		var t = typeof(Permissions);
		var ti = t.GetTypeInfo();
		var vals = Enum.GetValues(t).Cast<Permissions>();

		foreach (var xv in vals)
		{
			var xsv = xv.ToString();
			var xmv = ti.DeclaredMembers.FirstOrDefault(xm => xm.Name == xsv);
			var xav = xmv!.GetCustomAttribute<PermissionStringAttribute>()!;

			PermissionStrings[xv] = xav.String;
		}

		var a = typeof(DiscordClient).GetTypeInfo().Assembly;

		var vs = "";
		var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (iv != null)
			vs = iv.InformationalVersion;
		else
		{
			var v = a.GetName().Version;
			vs = v?.ToString(3);
		}

		VersionHeader = $"DiscordBot (https://github.com/Aiko-IT-Systems/DisCatSharp, v{vs})";
	}

	/// <summary>
	/// Gets the api base uri.
	/// </summary>
	/// <param name="config">The config</param>
	/// <returns>A string.</returns>
	internal static string GetApiBaseUri(DiscordConfiguration? config = null)
		=> (config?.ApiChannel ?? ApiChannel.Stable) switch
		{
			ApiChannel.Stable => Endpoints.BASE_URI,
			ApiChannel.Canary => Endpoints.CANARY_URI,
			ApiChannel.Ptb => Endpoints.PTB_URI,
			ApiChannel.Staging => Endpoints.STAGING_URI,
			_ => Endpoints.BASE_URI
		} + (config?.ApiVersion ?? "10");

	/// <summary>
	/// Gets the api uri for.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="config">The config</param>
	/// <returns>An Uri.</returns>
	internal static Uri GetApiUriFor(string path, DiscordConfiguration? config = null)
		=> new($"{GetApiBaseUri(config)}{path}");

	/// <summary>
	/// Gets the api uri for.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="queryString">The query string.</param>
	/// <param name="config">The config</param>
	/// <returns>An Uri.</returns>
	internal static Uri GetApiUriFor(string path, string queryString, DiscordConfiguration? config = null)
		=> new($"{GetApiBaseUri(config)}{path}{queryString}");

	/// <summary>
	/// Gets the api uri builder for.
	/// </summary>
	/// <param name="path">The path.</param>
	/// <param name="config">The config</param>
	/// <returns>A QueryUriBuilder.</returns>
	internal static QueryUriBuilder GetApiUriBuilderFor(string path, DiscordConfiguration? config = null)
		=> new($"{GetApiBaseUri(config)}{path}");

	/// <summary>
	/// Gets the formatted token.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <returns>A string.</returns>
	internal static string GetFormattedToken(BaseDiscordClient client)
		=> GetFormattedToken(client.Configuration);

	/// <summary>
	/// Gets the formatted token.
	/// </summary>
	/// <param name="config">The config.</param>
	/// <returns>A string.</returns>
	internal static string GetFormattedToken(DiscordConfiguration config)
		=> config.TokenType switch
		{
			TokenType.Bearer => $"Bearer {config.Token}",
			TokenType.Bot => $"Bot {config.Token}",
			_ => throw new ArgumentException("Invalid token type specified.", nameof(config))
		};

	/// <summary>
	/// Gets the base headers.
	/// </summary>
	/// <returns>A Dictionary.</returns>
	internal static Dictionary<string, string> GetBaseHeaders()
		=> new();

	/// <summary>
	/// Gets the user agent.
	/// </summary>
	/// <returns>A string.</returns>
	internal static string GetUserAgent()
		=> VersionHeader;

	/// <summary>
	/// Contains the user mentions.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A bool.</returns>
	internal static bool ContainsUserMentions(string message)
	{
		var pattern = @"<@(\d+)>";
		var regex = new Regex(pattern, RegexOptions.ECMAScript);
		return regex.IsMatch(message);
	}

	/// <summary>
	/// Contains the nickname mentions.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A bool.</returns>
	internal static bool ContainsNicknameMentions(string message)
	{
		var pattern = @"<@!(\d+)>";
		var regex = new Regex(pattern, RegexOptions.ECMAScript);
		return regex.IsMatch(message);
	}

	/// <summary>
	/// Contains the channel mentions.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A bool.</returns>
	internal static bool ContainsChannelMentions(string message)
	{
		var pattern = @"<#(\d+)>";
		var regex = new Regex(pattern, RegexOptions.ECMAScript);
		return regex.IsMatch(message);
	}

	/// <summary>
	/// Contains the role mentions.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A bool.</returns>
	internal static bool ContainsRoleMentions(string message)
	{
		var pattern = @"<@&(\d+)>";
		var regex = new Regex(pattern, RegexOptions.ECMAScript);
		return regex.IsMatch(message);
	}

	/// <summary>
	/// Contains the emojis.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A bool.</returns>
	internal static bool ContainsEmojis(string message)
	{
		var pattern = @"<a?:(.*):(\d+)>";
		var regex = new Regex(pattern, RegexOptions.ECMAScript);
		return regex.IsMatch(message);
	}

	/// <summary>
	/// Gets the user mentions.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A list of ulong.</returns>
	internal static IEnumerable<ulong> GetUserMentions(DiscordMessage message)
	{
		var regex = new Regex(@"<@!?(\d+)>", RegexOptions.ECMAScript | RegexOptions.Compiled);
		var matches = regex.Matches(message.Content);
		return from Match match in matches
		       select ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Gets the role mentions.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A list of ulong.</returns>
	internal static IEnumerable<ulong> GetRoleMentions(DiscordMessage message)
	{
		var regex = new Regex(@"<@&(\d+)>", RegexOptions.ECMAScript);
		var matches = regex.Matches(message.Content);
		return from Match match in matches
		       select ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Gets the channel mentions.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A list of ulong.</returns>
	internal static IEnumerable<ulong> GetChannelMentions(DiscordMessage message)
	{
		var regex = new Regex(@"<#(\d+)>", RegexOptions.ECMAScript);
		var matches = regex.Matches(message.Content);
		return from Match match in matches
		       select ulong.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Gets the emojis.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <returns>A list of ulong.</returns>
	internal static IEnumerable<ulong> GetEmojis(DiscordMessage message)
	{
		var regex = new Regex(@"<a?:([a-zA-Z0-9_]+):(\d+)>", RegexOptions.ECMAScript);
		var matches = regex.Matches(message.Content);
		return from Match match in matches
		       select ulong.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Are the valid slash command name.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <returns>A bool.</returns>
	internal static bool IsValidSlashCommandName(string name)
	{
		var regex = new Regex(@"^[\w-]{1,32}$");
		return regex.IsMatch(name);
	}

	/// <summary>
	/// Checks the thread auto archive duration feature.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="taad">The taad.</param>
	/// <returns>A bool.</returns>
	[DiscordDeprecated, Deprecated, System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
	internal static bool CheckThreadAutoArchiveDurationFeature(DiscordGuild guild, ThreadAutoArchiveDuration taad)
		=> true;

	/// <summary>
	/// Checks the thread private feature.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <returns>A bool.</returns>
	[DiscordDeprecated, Deprecated, System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
	internal static bool CheckThreadPrivateFeature(DiscordGuild guild)
		=> true;

	/// <summary>
	/// Have the message intents.
	/// </summary>
	/// <param name="intents">The intents.</param>
	/// <returns>A bool.</returns>
	internal static bool HasMessageIntents(DiscordIntents intents)
		=> intents.HasIntent(DiscordIntents.GuildMessages) || intents.HasIntent(DiscordIntents.DirectMessages);

	/// <summary>
	/// Have the message intents.
	/// </summary>
	/// <param name="intents">The intents.</param>
	/// <returns>A bool.</returns>
	internal static bool HasMessageContentIntents(DiscordIntents intents)
		=> intents.HasIntent(DiscordIntents.MessageContent);

	/// <summary>
	/// Have the reaction intents.
	/// </summary>
	/// <param name="intents">The intents.</param>
	/// <returns>A bool.</returns>
	internal static bool HasReactionIntents(DiscordIntents intents)
		=> intents.HasIntent(DiscordIntents.GuildMessageReactions) || intents.HasIntent(DiscordIntents.DirectMessageReactions);

	/// <summary>
	/// Have the typing intents.
	/// </summary>
	/// <param name="intents">The intents.</param>
	/// <returns>A bool.</returns>
	internal static bool HasTypingIntents(DiscordIntents intents)
		=> intents.HasIntent(DiscordIntents.GuildMessageTyping) || intents.HasIntent(DiscordIntents.DirectMessageTyping);

	// https://discord.com/developers/docs/topics/gateway#sharding-sharding-formula
	/// <summary>
	/// Gets a shard id from a guild id and total shard count.
	/// </summary>
	/// <param name="guildId">The guild id the shard is on.</param>
	/// <param name="shardCount">The total amount of shards.</param>
	/// <returns>The shard id.</returns>
	public static int GetShardId(ulong guildId, int shardCount)
		=> (int)(guildId >> 22) % shardCount;

	/// <summary>
	/// Helper method to create a <see cref="DateTimeOffset"/> from Unix time seconds for targets that do not support this natively.
	/// </summary>
	/// <param name="unixTime">Unix time seconds to convert.</param>
	/// <param name="shouldThrow">Whether the method should throw on failure. Defaults to true.</param>
	/// <returns>Calculated <see cref="DateTimeOffset"/>.</returns>
	public static DateTimeOffset GetDateTimeOffset(long unixTime, bool shouldThrow = true)
	{
		try
		{
			return DateTimeOffset.FromUnixTimeSeconds(unixTime);
		}
		catch (Exception)
		{
			if (shouldThrow)
				throw;

			return DateTimeOffset.MinValue;
		}
	}

	/// <summary>
	/// Helper method to create a <see cref="DateTimeOffset"/> from Unix time milliseconds for targets that do not support this natively.
	/// </summary>
	/// <param name="unixTime">Unix time milliseconds to convert.</param>
	/// <param name="shouldThrow">Whether the method should throw on failure. Defaults to true.</param>
	/// <returns>Calculated <see cref="DateTimeOffset"/>.</returns>
	public static DateTimeOffset GetDateTimeOffsetFromMilliseconds(long unixTime, bool shouldThrow = true)
	{
		try
		{
			return DateTimeOffset.FromUnixTimeMilliseconds(unixTime);
		}
		catch (Exception)
		{
			if (shouldThrow)
				throw;

			return DateTimeOffset.MinValue;
		}
	}

	/// <summary>
	/// Helper method to calculate Unix time seconds from a <see cref="DateTimeOffset"/> for targets that do not support this natively.
	/// </summary>
	/// <param name="dto"><see cref="DateTimeOffset"/> to calculate Unix time for.</param>
	/// <returns>Calculated Unix time.</returns>
	public static long GetUnixTime(DateTimeOffset dto)
		=> dto.ToUnixTimeMilliseconds();

	/// <summary>
	/// Computes a timestamp from a given snowflake.
	/// </summary>
	/// <param name="snowflake">Snowflake to compute a timestamp from.</param>
	/// <returns>Computed timestamp.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DateTimeOffset GetSnowflakeTime(this ulong snowflake)
		=> DiscordClient.DiscordEpoch.AddMilliseconds(snowflake >> 22);

	/// <summary>
	/// Computes a timestamp from a given snowflake.
	/// </summary>
	/// <param name="snowflake">Snowflake to compute a timestamp from.</param>
	/// <returns>Computed timestamp.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DateTimeOffset? GetSnowflakeTime(this ulong? snowflake)
		=> snowflake is not null ? DiscordClient.DiscordEpoch.AddMilliseconds(snowflake.Value >> 22) : null;

	/// <summary>
	/// Converts this <see cref="Permissions"/> into human-readable format.
	/// </summary>
	/// <param name="perm">Permissions enumeration to convert.</param>
	/// <param name="useNewline">Whether to seperate permissions by newline. Defaults to <see langword="false"/>.</param>
	/// <param name="sortAscending">Whether to sort permissions from a to z. Defaults to <see langword="true"/>.</param>
	/// <param name="includeValue">Whether to include the permissions value. Defaults to <see langword="false"/>.</param>
	/// <param name="shortIfAll">Whether to show <c>All Permissions</c>, if the member has all permissions. Defaults to &lt;see langword="false"/&gt;.</param>
	/// <returns>Human-readable permissions.</returns>
	public static string ToPermissionString(this Permissions perm, bool useNewline = false, bool sortAscending = true, bool includeValue = false, bool shortIfAll = false)
	{
		if (perm == Permissions.None)
			return PermissionStrings[perm];

		if (shortIfAll && perm.HasPermission(Permissions.All))
			return PermissionStrings[Permissions.All];

		perm &= PermissionMethods.FullPerms;

		var strs = PermissionStrings
			.Where(xkvp => xkvp.Key != Permissions.None && xkvp.Key != Permissions.All && (perm & xkvp.Key) == xkvp.Key)
			.Select(xkvp => includeValue ? $"{xkvp.Value} ({(long)xkvp.Key})" : xkvp.Value);

		return string.Join(useNewline ? "\n" : ", ", sortAscending ? strs.OrderBy(xs => xs) : strs);
	}

	/// <summary>
	/// Checks whether this string contains given characters.
	/// </summary>
	/// <param name="str">String to check.</param>
	/// <param name="characters">Characters to check for.</param>
	/// <returns>Whether the string contained these characters.</returns>
	public static bool Contains(this string str, params char[] characters)
		=> str.Any(characters.Contains);

	/// <summary>
	/// Logs the task fault.
	/// </summary>
	/// <param name="task">The task.</param>
	/// <param name="logger">The logger.</param>
	/// <param name="level">The level.</param>
	/// <param name="eventId">The event id.</param>
	/// <param name="message">The message.</param>
	internal static void LogTaskFault(this Task task, ILogger logger, LogLevel level, EventId eventId, string message)
	{
		if (task == null)
			throw new ArgumentNullException(nameof(task));

		if (logger == null)
			return;

		task.ContinueWith(t => logger.Log(level, eventId, t.Exception, message), TaskContinuationOptions.OnlyOnFaulted);
	}

	/// <summary>
	/// Deconstructs the.
	/// </summary>
	/// <param name="kvp">The kvp.</param>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	internal static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
	{
		key = kvp.Key;
		value = kvp.Value;
	}

	/// <summary>
	/// Gets whether the version check has finished.
	/// </summary>
	internal static Dictionary<string, bool> VersionCheckFinishedFor { get; } = [];

	/// <summary>
	/// Perfoms a version check against github releases.
	/// </summary>
	/// <param name="client">The base discord client.</param>
	/// <param name="owner">The owner of the target github <paramref name="repository"/>.</param>
	/// <param name="repository">The target github repository.</param>
	/// <param name="productName">The name of the product.</param>
	/// <param name="manualVersion">The manual version string to check.</param>
	/// <param name="githubToken">The token to use for private repositories.</param>
	public static Task CheckGitHubVersionAsync(DiscordClient client, string owner = "Aiko-IT-Systems", string repository = "DisCatSharp", string productName = "DisCatSharp", string? manualVersion = null, string? githubToken = null)
		=> CheckGitHubVersionAsync(client, false, false, owner, repository, productName, manualVersion, githubToken);

	/// <summary>
	/// Perfoms a version check against github releases.
	/// </summary>
	/// <param name="client">The base discord client.</param>
	/// <param name="startupCheck">Whether this is called on startup.</param>
	/// <param name="fromShard">Whether this method got called from a sharded client.</param>
	/// <param name="owner">The owner of the target github <paramref name="repository"/>.</param>
	/// <param name="repository">The target github repository.</param>
	/// <param name="productName">The name of the product.</param>
	/// <param name="manualVersion">The manual version string to check.</param>
	/// <param name="githubToken">The token to use for private repositories.</param>
	internal static Task CheckVersionAsync(BaseDiscordClient client, bool startupCheck, bool fromShard = false, string owner = "Aiko-IT-Systems", string repository = "DisCatSharp", string productName = "DisCatSharp", string? manualVersion = null, string? githubToken = null)
		=> CheckGitHubVersionAsync(client, startupCheck, fromShard, owner, repository, productName, manualVersion, githubToken);

	/// <summary>
	/// Perfoms a version check against github releases.
	/// </summary>
	/// <param name="client">The base discord client.</param>
	/// <param name="startupCheck">Whether this is called on startup.</param>
	/// <param name="fromShard">Whether this method got called from a sharded client.</param>
	/// <param name="owner">The owner of the target github <paramref name="repository"/>.</param>
	/// <param name="repository">The target github repository.</param>
	/// <param name="productName">The name of the product.</param>
	/// <param name="manualVersion">The manual version string to check.</param>
	/// <param name="githubToken">The token to use for private repositories.</param>
	internal static async Task CheckGitHubVersionAsync(BaseDiscordClient client, bool startupCheck, bool fromShard = false, string owner = "Aiko-IT-Systems", string repository = "DisCatSharp", string productName = "DisCatSharp", string? manualVersion = null, string? githubToken = null)
	{
		if (startupCheck && VersionCheckFinishedFor.TryGetValue(productName, out var val) && val && manualVersion is null)
			return;

		try
		{
			var version = manualVersion ?? client.VersionString;
			var currentVersion = version.Split('-')[0]!.Split('+')[0]!;
			var splitVersion = currentVersion.Split('.');
			var api = Convert.ToInt32(splitVersion[0]);
			var major = Convert.ToInt32(splitVersion[1]);
			var minor = Convert.ToInt32(splitVersion[2]);

			ApiConnection apiConnection = githubToken is not null ? new(new Connection(new($"{client.BotLibrary}", client.VersionString), new InMemoryCredentialStore(new(githubToken)))) : new(new Connection(new($"{client.BotLibrary}", client.VersionString)));
			ReleasesClient releaseClient = new(apiConnection);
			var latest = await releaseClient.GetLatest(owner, repository);

			if (latest is null)
			{
				client.Logger.LogWarning("[{Type}] Failed to check for updates. Could not determine remote version", fromShard ? "ShardedClient" : "Client");
				return;
			}

			var lastGitHubRelease = latest.TagName.Replace("v", string.Empty, StringComparison.InvariantCultureIgnoreCase);
			var githubSplitVersion = lastGitHubRelease.Split('.');
			var githubApi = Convert.ToInt32(githubSplitVersion[0]);
			var githubMajor = Convert.ToInt32(githubSplitVersion[1]);
			var githubMinor = Convert.ToInt32(githubSplitVersion[2]);

			if (api < githubApi || (api == githubApi && major < githubMajor) || (api == githubApi && major == githubMajor && minor < githubMinor))
				client.Logger.LogCritical("[{Type}] Your version of {Product} is outdated!\n\tCurrent version: v{CurrentVersion}\n\tLatest version: v{LastGitHubRelease}", fromShard ? "ShardedClient" : "Client", productName, version, lastGitHubRelease);
			else if (githubApi < api || (githubApi == api && githubMajor < major) || (githubApi == api && githubMajor == major && githubMinor < minor))
				client.Logger.LogWarning("[{Type}] Your version of {Product} is newer than the latest release!\n\tPre-release are not recommended for production.\n\tCurrent version: v{CurrentVersion}\n\tLatest version: v{LastGitHubRelease}", fromShard ? "ShardedClient" : "Client", productName, version, lastGitHubRelease);
			else
				client.Logger.LogInformation("[{Type}] Your version of {Product} is up to date!\n\tCurrent version: v{CurrentVersion}", fromShard ? "ShardedClient" : "Client", productName, version);
		}
		catch (Exception ex)
		{
			client.Logger.LogWarning("[{Type}] Failed to check for updates for {Product}. Error: {Exception}", fromShard ? "ShardedClient" : "Client", productName, ex);
		}
		finally
		{
			if (startupCheck)
				if (!VersionCheckFinishedFor.TryAdd(productName, true) && VersionCheckFinishedFor.TryGetValue(productName, out _))
					VersionCheckFinishedFor[productName] = true;
		}
	}

	/// <summary>
	/// Performs a version check against nuget.
	/// </summary>
	/// <param name="client">The base discord client.</param>
	/// <param name="packageId">The id of the package.</param>
	/// <param name="manualVersion">The manual version string to check.</param>
	/// <returns></returns>
	public static async Task CheckNuGetVersionAsync(BaseDiscordClient client, string packageId = "DisCatSharp", string? manualVersion = null, bool includePrerelease = true)
	{
		var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
		var resource = await repository.GetResourceAsync<MetadataResource>();
		var sourceCache = new SourceCacheContext()
		{
			RefreshMemoryCache = true,
			IgnoreFailedSources = true,
			NoCache = true
		};
		var latestVersions = (await resource.GetLatestVersions(new List<string>()
		{
			packageId.ToLowerInvariant()
		}, includePrerelease, false, sourceCache, new NullLogger(), CancellationToken.None))?.ToList() ?? throw new("Could not get latest versions");

		var latestPackageVersion = latestVersions.First(x => string.Equals(x.Key, packageId, StringComparison.InvariantCultureIgnoreCase)).Value;
		var version = manualVersion ?? client.VersionString;
		var gitLessVersion = version.Split('+')[0];

		NuGetVersion currentPackageVersion = new(gitLessVersion);
		if (latestPackageVersion > currentPackageVersion)
			client.Logger.LogCritical("Your version of {Product} is outdated!\n\tCurrent version: v{CurrentVersion}\n\tLatest version: v{LastGitHubRelease}", packageId, currentPackageVersion.OriginalVersion, latestPackageVersion.OriginalVersion);
		else if (latestPackageVersion < currentPackageVersion)
			client.Logger.LogWarning("Your version of {Product} is newer than the latest release!\n\tPre-release are not recommended for production.\n\tCurrent version: v{CurrentVersion}\n\tLatest version: v{LastGitHubRelease}", packageId, currentPackageVersion.OriginalVersion, latestPackageVersion.OriginalVersion);
		else
			client.Logger.LogInformation("Your version of {Product} is up to date!\n\tCurrent version: v{CurrentVersion}", packageId, currentPackageVersion.OriginalVersion);
	}
}
