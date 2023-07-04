// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using Microsoft.Extensions.Logging;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;
using System.Reflection;
using System.Linq;
using DisCatSharp.ApplicationCommands.Context;
using System.Threading.Tasks;
using DisCatSharp.HybridCommands.Context;
using DisCatSharp.HybridCommands.Attributes;
using System.Collections.Generic;
using DisCatSharp.Enums;
using DisCatSharp.Entities;
using System.Diagnostics;
using DisCatSharp.EventArgs;

namespace DisCatSharp.HybridCommands;
public sealed class HybridCommandsExtension : BaseExtension
{
	internal static HybridCommandsConfiguration? Configuration;

	internal static ILogger? Logger { get; set; }

	internal static string? ExecutionDirectory { get; set; }

	internal List<HybridCommandAttribute> registeredCommands { get; set; } = new();

	public static bool DebugEnabled 
		=> Configuration?.DebugEnabled ?? false;

	internal protected override async void Setup(DiscordClient client)
	{
		if (this.Client != null)
			throw new InvalidOperationException("What did I tell you?");

		if (Configuration is null)
			throw new InvalidOperationException("Configuration was not initialized.");

		this.Client = client;
		Logger = client.Logger;

#pragma warning disable CS8601 // Possible null reference assignment.
		client.UseApplicationCommands(new ApplicationCommandsConfiguration
		{
			EnableLocalization = Configuration.EnableLocalization,
			ServiceProvider = Configuration.ServiceProvider ?? null,
			EnableDefaultHelp = false,
			AutoDefer = true
		});


		client.UseCommandsNext(new CommandsNextConfiguration
		{
			EnableMentionPrefix = Configuration.EnableMentionPrefix,
			PrefixResolver = Configuration.PrefixResolver ?? null,
			ServiceProvider = Configuration.ServiceProvider ?? null,
			StringPrefixes = Configuration.StringPrefixes ?? null,
			EnableDefaultHelp = false
		});
#pragma warning restore CS8601 // Possible null reference assignment.

		if (HybridCommandsExtension.Configuration.EnableDefaultHelp)
			await this.RegisterGlobalCommands<DefaultHybridHelp>();
	}

	/// <summary>
	/// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
	/// </summary>
	/// <param name="assembly">Assembly to register commands from.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup app command translations with.</param>
	public async Task RegisterGuildCommands(Assembly assembly, ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		var types = assembly.GetTypes().Where(xt =>
		{
			var xti = xt.GetTypeInfo();
			return xti.IsModuleCandidateType() && !xti.IsNested;
		});
		foreach (var xt in types)
			await this.RegisterGuildCommands(xt, guildId, translationSetup);
	}

	/// <summary>
	/// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
	/// </summary>
	/// <param name="assembly">Assembly to register commands from.</param>
	/// <param name="translationSetup">A callback to setup app command translations with.</param>
	public async Task RegisterGlobalCommands(Assembly assembly, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		var types = assembly.GetTypes().Where(xt =>
		{
			var xti = xt.GetTypeInfo();
			return xti.IsModuleCandidateType() && !xti.IsNested;
		});
		foreach (var xt in types)
			await this.RegisterGlobalCommands(xt, translationSetup);
	}

	/// <summary>
	/// Registers a hybrid command class with optional translation setup globally.
	/// </summary>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="translationSetup">A callback to setup app command translations with.</param>
	public async Task RegisterGlobalCommands(Type type, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		if (!type.IsModuleCandidateType())
			throw new ArgumentException("Command Class is not a valid module candidate.");

		foreach (var assembly in await type.CompileAndLoadCommands())
		{
			var commandsNextModule = assembly.DefinedTypes.FirstOrDefault(x => typeof(BaseCommandModule).IsAssignableTo(x), null);
			if (commandsNextModule is not null)
			{
				this.Client.GetCommandsNext().RegisterCommands(commandsNextModule);
			}

			var applicationCommandsModule = assembly.DefinedTypes.FirstOrDefault(x => typeof(ApplicationCommandsModule).IsAssignableTo(x), null);
			if (applicationCommandsModule is not null)
			{
#pragma warning disable CS8604 // Possible null reference argument.
				this.Client.GetApplicationCommands().RegisterGlobalCommands(applicationCommandsModule, translationSetup);
#pragma warning restore CS8604 // Possible null reference argument.
			}
		}
	}

	/// <summary>
	/// Registers a hybrid command class with optional translation setup for a guild.
	/// </summary>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup app command translations with.</param>
	public async Task RegisterGuildCommands(Type type, ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		if (!type.IsModuleCandidateType())
			throw new ArgumentException("Command Class is not a valid module candidate.");

		foreach (var assembly in await type.CompileAndLoadCommands(guildId))
		{
			var commandsNextModule = assembly.DefinedTypes.FirstOrDefault(x => typeof(BaseCommandModule).IsAssignableFrom(x), null);
			if (commandsNextModule is not null)
			{
				this.Client.GetCommandsNext().RegisterCommands(commandsNextModule);
			}

			var applicationCommandsModule = assembly.DefinedTypes.FirstOrDefault(x => typeof(ApplicationCommandsModule).IsAssignableFrom(x), null);
			if (applicationCommandsModule is not null)
			{
#pragma warning disable CS8604 // Possible null reference argument.
				this.Client.GetApplicationCommands().RegisterGuildCommands(applicationCommandsModule, guildId, translationSetup);
#pragma warning restore CS8604 // Possible null reference argument.
			}
		}
	}

	/// <inheritdoc cref="RegisterGlobalCommands(Type, Action{ApplicationCommandsTranslationContext}?)"/>
	/// <typeparam name="T">The command class to register.</typeparam>
	public async Task RegisterGlobalCommands<T>(Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : HybridCommandsModule
		=> await this.RegisterGlobalCommands(typeof(T), translationSetup);

	/// <inheritdoc cref="RegisterGuildCommands(Type, ulong, Action{ApplicationCommandsTranslationContext}?)"/>
	/// <typeparam name="T">The command class to register.</typeparam>
	public async Task RegisterGuildCommands<T>(ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : HybridCommandsModule
		=> await this.RegisterGuildCommands(typeof(T), guildId, translationSetup);

	internal HybridCommandsExtension(HybridCommandsConfiguration? configuration = null)
	{
		configuration ??= new HybridCommandsConfiguration();
		Configuration = new HybridCommandsConfiguration(configuration);
	}

	private HybridCommandsExtension() { }
}

public class DefaultHybridHelp : HybridCommandsModule
{
	[HybridCommand("help", "Displays all commands, their usage and description", true, false)]
	public async Task HelpAsync(HybridCommandContext ctx)
	{
		var hybridModule = ctx.Client.GetHybridCommands();
		var commandDescriptions = new List<string>();

		foreach (var command in hybridModule.registeredCommands)
			commandDescriptions.Add($"`{command.Name}` - _{command.Description}_{(command.IsNsfw ? " (**NSFW**)" : "")}");

		var splitDescriptions = new List<string>();

		var currentBuild = "";
		foreach (var description in commandDescriptions)
		{
			var add = $"{description}\n";

			if (add.Length + currentBuild.Length > 2048)
			{
				splitDescriptions.Add(currentBuild);
				currentBuild = "";
			}

			currentBuild += add;
		}

		if (currentBuild.Length > 0)
		{
			splitDescriptions.Add(currentBuild);
			currentBuild = "";
		}

		var embeds = splitDescriptions.Select(x => new DiscordEmbedBuilder
		{
			Description = x,
			Title = "Command list",
			Timestamp = DateTime.UtcNow
		}).ToList();

		var PrevPage = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Previous page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("◀")));
		var NextPage = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("▶")));

		Task EditMessage()
			=> ctx.RespondOrEditAsync(new DiscordMessageBuilder()
			.WithEmbed(embeds![0])
			.AddComponents(PrevPage!.Disable(), NextPage!));

		await EditMessage();

		uint currentIndex = 0;
		var sw = Stopwatch.StartNew();
		ctx.Client.ComponentInteractionCreated += RunInteraction;

		async Task RunInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
		{
			if (e.Id == PrevPage.CustomId)
				currentIndex--;
			else if (e.Id == NextPage.CustomId)
				currentIndex++;

			PrevPage!.SetState(currentIndex <= 0);
			NextPage.SetState(currentIndex >= embeds.Count);

			await EditMessage();
		}

		while (sw.Elapsed < TimeSpan.FromSeconds(60))
		{
			await Task.Delay(1000);
		}

		ctx.Client.ComponentInteractionCreated -= RunInteraction;
		await ctx.RespondOrEditAsync(embeds[(int)currentIndex].WithFooter("Interaction timed out"));
	}
}
