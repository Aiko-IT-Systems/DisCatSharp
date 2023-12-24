using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands;

/// <summary>
/// Defines various extension methods for application commands.
/// </summary>
public static class ExtensionMethods
{
	/// <summary>
	/// Enables application commands on this <see cref="DiscordClient"/>.
	/// </summary>
	/// <param name="client">Client to enable application commands for.</param>
	/// <param name="config">Configuration to use.</param>
	/// <returns>Created <see cref="ApplicationCommandsExtension"/>.</returns>
	public static ApplicationCommandsExtension UseApplicationCommands(
		this DiscordClient client,
		ApplicationCommandsConfiguration? config = null
	)
	{
		if (client.GetExtension<ApplicationCommandsExtension>() is not null)
			throw new InvalidOperationException("Application commands are already enabled for that client.");

		var scomm = new ApplicationCommandsExtension(config);
		client.AddExtension(scomm);
		return scomm;
	}

	/// <summary>
	/// Gets the application commands module for this client.
	/// </summary>
	/// <param name="client">Client to get application commands for.</param>
	/// <returns>The module, or null if not activated.</returns>
	public static ApplicationCommandsExtension GetApplicationCommands(this DiscordClient client)
		=> client.GetExtension<ApplicationCommandsExtension>();

	/// <summary>
	/// Gets the application commands from this <see cref="DiscordShardedClient"/>.
	/// </summary>
	/// <param name="client">Client to get application commands from.</param>
	/// <returns>A dictionary of current <see cref="ApplicationCommandsExtension"/> with the key being the shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, ApplicationCommandsExtension>> GetApplicationCommandsAsync(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		return client.ShardClients.Values.ToDictionary(shard => shard.ShardId, shard => shard.GetExtension<ApplicationCommandsExtension>()!);
	}

	/// <summary>
	/// Registers a command class with optional translation setup globally.
	/// </summary>
	/// <param name="extensions">Sharding extensions.</param>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public static void RegisterGlobalCommands<T>(this IReadOnlyDictionary<int, ApplicationCommandsExtension> extensions, Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : ApplicationCommandsModule
	{
		foreach (var extension in extensions.Values)
			extension.RegisterGlobalCommands<T>(translationSetup);
	}

	/// <summary>
	/// Registers a command class with optional translation setup globally.
	/// </summary>
	/// <param name="extensions">Sharding extensions.</param>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public static void RegisterGlobalCommands(this IReadOnlyDictionary<int, ApplicationCommandsExtension> extensions, Type type, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));

		foreach (var extension in extensions.Values)
			extension.RegisterGlobalCommands(type, translationSetup);
	}

	/// <summary>
	/// Registers a command class with optional translation setup for a guild.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="extensions">Sharding extensions.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public static void RegisterGuildCommands<T>(this IReadOnlyDictionary<int, ApplicationCommandsExtension> extensions, ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : ApplicationCommandsModule
	{
		foreach (var extension in extensions.Values)
			extension.RegisterGuildCommands<T>(guildId, translationSetup);
	}

	/// <summary>
	/// Registers a command class with optional translation setup for a guild.
	/// </summary>
	/// <param name="extensions">Sharding extensions.</param>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public static void RegisterGuildCommands(this IReadOnlyDictionary<int, ApplicationCommandsExtension> extensions, Type type, ulong guildId, Action<ApplicationCommandsTranslationContext> translationSetup = null)
	{
		if (!typeof(ApplicationCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from ApplicationCommandsModule", nameof(type));

		foreach (var extension in extensions.Values)
			extension.RegisterGuildCommands(type, guildId, translationSetup);
	}

	/// <summary>
	/// Enables application commands on this <see cref="DiscordShardedClient"/>.
	/// </summary>
	/// <param name="client">Client to enable application commands on.</param>
	/// <param name="config">Configuration to use.</param>
	/// <returns>A dictionary of created <see cref="ApplicationCommandsExtension"/> with the key being the shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, ApplicationCommandsExtension>> UseApplicationCommandsAsync(this DiscordShardedClient client, ApplicationCommandsConfiguration? config = null)
	{
		var modules = new Dictionary<int, ApplicationCommandsExtension>();
		await client.InitializeShardsAsync().ConfigureAwait(false);
		foreach (var shard in client.ShardClients.Values)
		{
			var scomm = shard.GetExtension<ApplicationCommandsExtension>();
			scomm ??= shard.UseApplicationCommands(config);

			modules[shard.ShardId] = scomm;
		}

		return modules;
	}

	/// <summary>
	/// Gets the name from the <see cref="ChoiceNameAttribute"/> for this enum value.
	/// </summary>
	/// <returns>The name.</returns>
	public static string GetName<T>(this T e) where T : IConvertible
	{
		if (e is not Enum)
			return null;

		var type = e.GetType();
		var values = Enum.GetValues(type);

		return (from int val in values
		        where val == e.ToInt32(CultureInfo.InvariantCulture)
		        let memInfo = type.GetMember(type.GetEnumName(val))
		        select memInfo[0]
			        .GetCustomAttributes(typeof(ChoiceNameAttribute), false)
			        .FirstOrDefault() is ChoiceNameAttribute nameAttribute
			        ? nameAttribute.Name
			        : type.GetEnumName(val)).FirstOrDefault()!;
	}
}
