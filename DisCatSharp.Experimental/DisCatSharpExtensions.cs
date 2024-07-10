using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Experimental.Entities;

namespace DisCatSharp.Experimental;

/// <summary>
/// Represents experimental extension methods for DisCatSharp.
/// </summary>
public static class DisCatSharpExtensions
{
	[Experimental("This function is being tested and might change at any time."), RequiresFeature(Features.MonetizedApplication)]
	public static async Task<string> GetUsernameAsync(this DiscordClient client, ulong id)
	{
		var user = await client.ApiClient.GetUserAsync(id);
		return user.UsernameWithDiscriminator;
	}

	/// <summary>
	/// Gets the clyde profile for the given <paramref name="profileId"/>.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="profileId">The profile id to get.</param>
	[RequiresFeature(Features.Override | Features.Experiment, "This method requires the guild and/or user to have access to clyde with treatment 5."),
	 DiscordDeprecated]
	public static async Task<ClydeProfile> GetClydeProfileAsync(this DiscordClient client, ulong profileId)
	{
		DiscordApiClientHook hook = new(client.ApiClient);
		return await hook.GetClydeProfileAsync(profileId);
	}

	/// <summary>
	/// Gets the clyde settings for the given <paramref name="guild"/>.
	/// </summary>
	/// <param name="guild">The guild to get clyde's settings for.</param>
	[RequiresFeature(Features.Override | Features.Experiment, "This method requires the guild and/or user to have access to clyde with treatment 5."),
	 DiscordDeprecated]
	public static async Task<ClydeSettings> GetClydeSettingsAsync(this DiscordGuild guild)
	{
		DiscordApiClientHook hook = new(guild.Discord.ApiClient);
		return await hook.GetClydeSettingsAsync(guild.Id);
	}

	/// <summary>
	/// Modifies the clyde settings for the given <paramref name="guild"/> by applying a <paramref name="profileId"/>.
	/// </summary>
	/// <param name="guild">The guild to modify clyde's settings for.</param>
	/// <param name="profileId">The profile id to apply.</param>
	[RequiresFeature(Features.Override | Features.Experiment, "This method requires the guild and/or user to have access to clyde with treatment 5."),
	 DiscordDeprecated]
	public static async Task<ClydeSettings> ModifyClydeSettingsAsync(this DiscordGuild guild, ulong profileId)
	{
		DiscordApiClientHook hook = new(guild.Discord.ApiClient);
		return await hook.ModifyClydeSettingsAsync(guild.Id, profileId);
	}

	/// <summary>
	/// Modifies the clyde settings for the given <paramref name="guild"/>.
	/// </summary>
	/// <param name="guild">The guild to modify clyde's settings for.</param>
	/// <param name="name">The new name.</param>
	/// <param name="personality">The new basePersonality.</param>
	/// <param name="avatar">The new avatar.</param>
	/// <param name="banner">The new banner.</param>
	/// <param name="themeColors">The new theme colors.</param>
	[RequiresFeature(Features.Override | Features.Experiment, "This method requires the guild and/or user to have access to clyde with treatment 5."),
	 DiscordDeprecated]
	public static async Task<ClydeSettings> ModifyClydeSettingsAsync(
		this DiscordGuild guild,
		Optional<string?> name,
		Optional<string> personality,
		Optional<Stream?> avatar,
		Optional<Stream?> banner,
		Optional<List<DiscordColor>?> themeColors
	)
	{
		DiscordApiClientHook hook = new(guild.Discord.ApiClient);

		return await hook.ModifyClydeSettingsAsync(guild.Id, name, personality, ImageTool.Base64FromStream(avatar), ImageTool.Base64FromStream(banner),
			themeColors.HasValue && themeColors.Value.Count != 0
				? themeColors.Value.Select(x => x.Value).ToList()
				: themeColors.HasValue
					? Optional.FromNullable<List<int>?>(null)
					: Optional.None);
	}

	/// <summary>
	/// Generates a basePersonality for clyde based on the given <paramref name="basePersonality"/>.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="basePersonality">The base base personality to generate a new one from.</param>
	[RequiresFeature(Features.Override | Features.Experiment, "This method requires the guild and/or user to have access to clyde with treatment 5."),
	 DiscordDeprecated]
	public static async Task<string> GenerateClydePersonalityAsync(this DiscordClient client, string? basePersonality = null)
	{
		DiscordApiClientHook hook = new(client.ApiClient);
		return await hook.GenerateClydePersonalityAsync(basePersonality);
	}
}
