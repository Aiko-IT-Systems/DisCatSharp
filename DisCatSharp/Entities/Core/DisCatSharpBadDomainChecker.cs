using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;
using Nager.PublicSuffix.RuleProviders.CacheProviders;

namespace DisCatSharp.Entities.Core;

/// <summary>
/// Represents a bad domain checker for DisCatSharp based on discord official bad domain hashes.
/// </summary>
public static class DisCatSharpBadDomainChecker
{
	/// <summary>
	/// Gets the cache provider.
	/// </summary>
	private static LocalFileSystemCacheProvider? s_cacheProvider { get; set; }

	/// <summary>
	/// Gets the domain parser.
	/// </summary>
	private static DomainParser? s_parser { get; set; }

	/// <summary>
	/// Gets the set of bad domain hashes.
	/// </summary>
	public static HashSet<string> BadHashes { get; internal set; } = [];

	/// <summary>
	/// Gets the Discord configuration.
	/// </summary>
	private static DiscordConfiguration? s_config { get; set; }

	/// <summary>
	/// Loads the bad domain hashes from Discord API and initializes the domain parser.
	/// </summary>
	/// <param name="client">The Discord client instance.</param>
	/// <exception cref="InvalidOperationException">Thrown if the Discord configuration is null.</exception>
	internal static async Task LoadAndInitBadDomainHashesAsync(BaseDiscordClient client)
	{
		s_config = client.Configuration ?? throw new InvalidOperationException("DiscordConfiguration is null.");
		if (s_config.EnableBadDomainCheckerSupport)
		{
			client.Logger.LogDebug("Loading bad domain hashes from Discord API...");
			BadHashes = await client.ApiClient.GetBadDomainHashesAsync().ConfigureAwait(false);
			s_cacheProvider ??= new LocalFileSystemCacheProvider();

			if (s_parser is null)
			{
				client.Logger.LogDebug("Initializing domain parser for bad domain checking...");
				var ruleProvider = new CachedHttpRuleProvider(s_cacheProvider, client.RestClient);
				client.Logger.LogDebug("Downloading public suffix list...");
				await ruleProvider.BuildAsync().ConfigureAwait(false);
				s_parser = new DomainParser(ruleProvider);
				client.Logger.LogDebug("Domain parser initialized.");
			}
			client.Logger.LogDebug("Loaded {count} bad domain hashes.", BadHashes.Count);
		}
		else
			client.Logger.LogDebug("DisCatSharp bad domain checker support is disabled in the configuration.");
	}

	/// <summary>
	/// Checks if the input is a bad domain.
	/// </summary>
	/// <param name="input">The input domain or URL to check.</param>
	/// <returns><see langword="true"/> if the input is a bad domain and the bad domain checker was enabled; otherwise, <see langword="false"/>.</returns>
	public static bool IsBadDomain(string input)
	{
		if (s_parser is null || s_config is null || !s_config.EnableBadDomainCheckerSupport || BadHashes.Count is 0)
			return false;

		if (string.IsNullOrWhiteSpace(input))
			return false;

		var host = NormalizeToHost(input);

		using var sha = SHA256.Create();

		if (ComputeAndCheck(host, sha))
			return true;

		var info = s_parser.Parse(host);
		if (!string.IsNullOrEmpty(info.RegistrableDomain))
		{
			if (ComputeAndCheck(info.RegistrableDomain, sha))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Computes the SHA256 hash of the value and checks if it exists in the bad hashes set.
	/// </summary>
	/// <param name="value">The value to compute the hash for.</param>
	/// <param name="sha">The SHA256 instance to use for hashing.</param>
	/// <returns><see langword="true"/> if the computed hash exists in the bad hashes set; otherwise, <see langword="false"/>.</returns>
	private static bool ComputeAndCheck(
		string value,
		SHA256 sha)
	{
		Span<byte> hash = stackalloc byte[32];
		sha.TryComputeHash(
			Encoding.UTF8.GetBytes(value),
			hash,
			out _);

		Span<char> hex = stackalloc char[64];
		ToHexLower(hash, hex);

		return BadHashes.Contains(new string(hex));
	}

	/// <summary>
	/// Converts the byte span to a lowercase hexadecimal character span.
	/// </summary>
	/// <param name="bytes">The byte span to convert.</param>
	/// <param name="chars">The character span to write the hexadecimal representation to.</param>
	private static void ToHexLower(
		ReadOnlySpan<byte> bytes,
		Span<char> chars)
	{
		const string hex = "0123456789abcdef";

		for (var i = 0; i < bytes.Length; i++)
		{
			var b = bytes[i];
			chars[i * 2] = hex[b >> 4];
			chars[(i * 2) + 1] = hex[b & 0xF];
		}
	}

	/// <summary>
	/// Normalizes the input to extract the host.
	/// </summary>
	/// <param name="input">The input domain or URL.</param>
	/// <returns>The normalized host.</returns>
	private static string NormalizeToHost(string input)
	{
		input = input.Trim().ToLowerInvariant();

		return input.StartsWith("http://") || input.StartsWith("https://") ? new Uri(input).Host : input;
	}
}
