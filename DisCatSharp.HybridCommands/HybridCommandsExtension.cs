// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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

namespace DisCatSharp.HybridCommands;
public sealed class HybridCommandsExtension : BaseExtension
{
	internal static HybridCommandsConfiguration? Configuration;

	internal static ILogger? Logger { get; set; }

	public static bool DebugEnabled 
		=> Configuration?.DebugEnabled ?? false;

	internal protected override void Setup(DiscordClient client)
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
			EnableDefaultHelp = Configuration.EnableDefaultHelp,
			EnableLocalization = Configuration.EnableLocalization,
			ServiceProvider = Configuration.ServiceProvider ?? null,
		});


		client.UseCommandsNext(new CommandsNextConfiguration
		{
			CaseSensitive = Configuration.CaseSensitive,
			EnableDefaultHelp = Configuration.EnableDefaultHelp,
			EnableMentionPrefix = Configuration.EnableMentionPrefix,
			PrefixResolver = Configuration.PrefixResolver ?? null,
			ServiceProvider = Configuration.ServiceProvider ?? null,
			StringPrefixes = Configuration.StringPrefixes ?? null,
		});
#pragma warning restore CS8601 // Possible null reference assignment.
	}

	/// <summary>
	/// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
	/// </summary>
	/// <param name="assembly">Assembly to register commands from.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	public void RegisterGuildCommands(Assembly assembly, ulong guildId)
	{
		var types = assembly.GetTypes().Where(xt =>
		{
			var xti = xt.GetTypeInfo();
			return xti.IsModuleCandidateType() && !xti.IsNested;
		});
		foreach (var xt in types)
			this.RegisterGuildCommands(xt, guildId, null);
	}

	/// <summary>
	/// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
	/// </summary>
	/// <param name="assembly">Assembly to register commands from.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGlobalCommands(Assembly assembly, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
		=> throw new NotImplementedException();

	/// <summary>
	/// Registers a command class with optional translation setup for a guild.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGuildCommands<T>(ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : HybridCommandsModule
		=> throw new NotImplementedException();

	/// <summary>
	/// Registers a command class with optional translation setup for a guild.
	/// </summary>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="guildId">The guild id to register it on.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGuildCommands(Type type, ulong guildId, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		if (!typeof(HybridCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from HybridCommandsModule", nameof(type));
		throw new NotImplementedException();
	}

	/// <summary>
	/// Registers a command class with optional translation setup globally.
	/// </summary>
	/// <typeparam name="T">The command class to register.</typeparam>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGlobalCommands<T>(Action<ApplicationCommandsTranslationContext>? translationSetup = null) where T : HybridCommandsModule
		=> throw new NotImplementedException();

	/// <summary>
	/// Registers a command class with optional translation setup globally.
	/// </summary>
	/// <param name="type">The <see cref="System.Type"/> of the command class to register.</param>
	/// <param name="translationSetup">A callback to setup translations with.</param>
	public void RegisterGlobalCommands(Type type, Action<ApplicationCommandsTranslationContext>? translationSetup = null)
	{
		if (!typeof(HybridCommandsModule).IsAssignableFrom(type))
			throw new ArgumentException("Command classes have to inherit from HybridCommandsModule", nameof(type));
		throw new NotImplementedException();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="HybridCommandsExtension"/>.
	/// </summary>
	/// <param name="configuration">The configuration.</param>
	internal HybridCommandsExtension(HybridCommandsConfiguration? configuration = null)
	{
		configuration ??= new HybridCommandsConfiguration();
		Configuration = new HybridCommandsConfiguration(configuration);
	}

	private HybridCommandsExtension() { }
}
