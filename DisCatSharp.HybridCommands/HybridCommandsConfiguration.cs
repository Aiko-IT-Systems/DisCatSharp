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
using System.Collections.Generic;

using DisCatSharp.CommandsNext;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.HybridCommands;

/// <summary>
/// Represents a configuration for <see cref="HybridCommandsExtension"/>.
/// </summary>
public sealed class HybridCommandsConfiguration
{
	/// <summary>
	/// <para>Whether to enable debug logs.</para>
	/// <para>Defaults to false.</para>
	/// </summary>
	public bool DebugEnabled { internal get; set; } = false;

	/// <summary>
	/// <para>Sets the string prefixes used for prefix commands.</para>
	/// <para>Defaults to no value (disabled).</para>
	/// </summary>
	public List<string>? StringPrefixes { internal get; set; }

	/// <summary>
	/// <para>Sets the custom prefix resolver used for prefix commands.</para>
	/// <para>Defaults to none (disabled).</para>
	/// </summary>
	public PrefixResolverDelegate? PrefixResolver { internal get; set; }

	/// <summary>
	/// <para>Sets whether to allow mentioning the bot to be used as command prefix.</para>
	/// <para>Defaults to true.</para>
	/// </summary>
	public bool EnableMentionPrefix { internal get; set; } = true;

	/// <summary>
	/// <para>Sets whether to enable default help command.</para>
	/// <para>Disabling this will allow you to make your own help command.</para>
	/// <para>Defaults to true.</para>
	/// </summary>
	public bool EnableDefaultHelp { internal get; set; } = true;

	/// <summary>
	/// <para>Sets the service provider for this <see cref="HybridCommandsExtension"/> instance.</para>
	/// <para>Objects in this provider are used when instantiating command modules. This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider? ServiceProvider { internal get; set; }

	/// <summary>
	/// This option enables the localization feature.
	/// <para>Defaults to false.</para>
	/// </summary>
	public bool EnableLocalization { internal get; set; } = false;

	/// <summary>
	/// Whether to entirely disable usage of cached command assemblies.
	/// <para>This will cause commands to be recompiled on every startup. This will take significant amount of time if a lot of commands are registered.</para>
	/// <para>Defaults to false.</para>
	/// </summary>
	public bool DisableCompilationCache { internal get; set; } = false;

	/// <summary>
	/// Whether the generated classes should be output to a sub-directory. Used for debugging-purposes.
	/// <para>Defaults to false.</para>
	/// </summary>
	public bool OutputGeneratedClasses { internal get; set; } = false;

	/// <summary>
	/// Creates a new instance of <see cref="HybridCommandsConfiguration"/>.
	/// </summary>
	public HybridCommandsConfiguration() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="HybridCommandsConfiguration"/> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public HybridCommandsConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	/// Creates a new instance of <see cref="HybridCommandsConfiguration"/>, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public HybridCommandsConfiguration(HybridCommandsConfiguration other)
	{
		this.EnableDefaultHelp = other.EnableDefaultHelp;
		this.EnableLocalization = other.EnableLocalization;
		this.EnableMentionPrefix = other.EnableMentionPrefix;
		this.PrefixResolver = other.PrefixResolver;
		this.ServiceProvider = other.ServiceProvider;
		this.StringPrefixes = other.StringPrefixes;
	}
}
