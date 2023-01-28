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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for application command permissions update events.
/// </summary>
public sealed class ApplicationCommandPermissionsUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the application command permissions.
	/// </summary>
	public List<DiscordApplicationCommandPermission> Permissions { get; internal set; }

	/// <summary>
	/// Gets the application command.
	/// </summary>
	public DiscordApplicationCommand Command { get; internal set; }

	/// <summary>
	/// Gets the application id.
	/// </summary>
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationCommandPermissionsUpdateEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public ApplicationCommandPermissionsUpdateEventArgs(IServiceProvider provider) : base(provider)
	{ }
}
