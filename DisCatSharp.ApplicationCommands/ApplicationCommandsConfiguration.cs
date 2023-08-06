// This file is part of the DisCatSharp project.
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

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.ApplicationCommands;

/// <summary>
/// A configuration for a <see cref="ApplicationCommandsExtension"/>
/// </summary>
public class ApplicationCommandsConfiguration
{
	/// <summary>
	/// <para>Sets the service provider.</para>
	/// <para>Objects in this provider are used when instantiating application command modules.</para>
	/// <para>This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to <see langword="null"/>.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	/// <para>This option enables the default help command.</para>
	/// <para>Disabling this will allow you to make your own help command.</para>
	/// <para>Defaults to <see langword="true"/>.</para>
	/// </summary>
	public bool EnableDefaultHelp { internal get; set; } = true;

	/// <summary>
	/// This option enables the localization feature.
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool EnableLocalization { internal get; set; } = false;

	/// <summary>
	/// <para>Automatically defer all responses.</para>
	/// <note type="note">If you enable this, you can't use CreateResponse. Use EditResponse instead.</note>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool AutoDefer { internal get; set; } = false;

	/// <summary>
	/// <para>This option informs the module to check through all guilds whether the
	/// <see target="_blank" alt="Application Commands Scope" href="https://discord.com/developers/docs/topics/oauth2#shared-resources-oauth2-scopes">application.commands</see> scope is set.</para>
	/// <note type="warning">This will take quite a while, when the bot is on more than 1k guilds.</note>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool CheckAllGuilds { internal get; set; } = false;

	/// <summary>
	/// <para>This option can override the default registration behavior of the module.</para>
	/// <note type="warning">
	/// <para>It can lead to unexpected behavior of the application commands module.</para>
	/// <para>Enable this option only if DisCatSharp support advises you to do so.</para>
	/// </note>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool ManualOverride { internal get; set; } = false;

	/// <summary>
	/// <para>This option increases the debug output of the module.</para>
	/// <note type="warning">
	/// <para>This is not recommended for production use.</para>
	/// <para>Enable this option only if DisCatSharp support advises you to do so.</para>
	/// </note>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool DebugStartup { internal get; set; } = false;

	/// <summary>
	/// <para>>Whether to only generate translations files and abort after that.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool GenerateTranslationFilesOnly { internal get; set; } = false;

	/// <summary>
	/// Creates a new configuration with default values.
	/// </summary>
	public ApplicationCommandsConfiguration()
	{ }

	/// <summary>
	/// Utilized via dependency injection pipeline.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public ApplicationCommandsConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	/// Creates a new instance of <see cref="ApplicationCommandsConfiguration"/>, copying the properties of another configuration.
	/// </summary>
	/// <param name="acc">Configuration the properties of which are to be copied.</param>
	public ApplicationCommandsConfiguration(ApplicationCommandsConfiguration acc)
	{
		this.EnableDefaultHelp = acc.EnableDefaultHelp;
		this.ServiceProvider = acc.ServiceProvider;
		this.DebugStartup = acc.DebugStartup;
		this.CheckAllGuilds = acc.CheckAllGuilds;
		this.ManualOverride = acc.ManualOverride;
		this.AutoDefer = acc.AutoDefer;
		this.EnableLocalization = acc.EnableLocalization;
		this.GenerateTranslationFilesOnly = acc.GenerateTranslationFilesOnly;
	}
}
