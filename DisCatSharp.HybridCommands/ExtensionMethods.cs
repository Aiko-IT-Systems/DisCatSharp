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
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;

namespace DisCatSharp.HybridCommands;
public static class ExtensionMethods
{
	/// <summary>
	/// Enables hybrid commands on this <see cref="DiscordClient"/>.
	/// <para>Do not initialize <see cref="ApplicationCommandsExtension"/> or <see cref="CommandsNextExtension"/>. This module uses it's own settings and initializes them with proper settings for you.</para>
	/// </summary>
	/// <param name="client">Client to enable hybrid commands for.</param>
	/// <param name="config">Configuration to use.</param>
	/// <returns>Created <see cref="HybridCommandsExtension"/>.</returns>
	public static HybridCommandsExtension UseHybridCommands(this DiscordClient client, HybridCommandsConfiguration? config = null)
	{
		if (client.GetExtension<HybridCommandsExtension>() != null)
			throw new InvalidOperationException("Hybrid commands are already enabled for that client.");

		var scomm = new HybridCommandsExtension(config);
		client.AddExtension(scomm);
		return scomm;
	}

	/// <summary>
	/// Gets the hybrid commands module for this client.
	/// </summary>
	/// <param name="client">Client to get hybrid commands for.</param>
	/// <returns>The module, or null if not activated.</returns>
	public static HybridCommandsExtension GetHybridCommands(this DiscordClient client)
		=> client.GetExtension<HybridCommandsExtension>();

	/// <summary>
	/// Gets the hybrid commands from this <see cref="DiscordShardedClient"/>.
	/// </summary>
	/// <param name="client">Client to get hybrid commands from.</param>
	/// <returns>A dictionary of current <see cref="HybridCommandsExtension"/> with the key being the shard id.</returns>
	public static async Task<IReadOnlyDictionary<int, HybridCommandsExtension>> GetHybridCommandsAsync(this DiscordShardedClient client)
	{
		await client.InitializeShardsAsync().ConfigureAwait(false);
		var modules = new Dictionary<int, HybridCommandsExtension>();
		foreach (var shard in client.ShardClients.Values)
			modules.Add(shard.ShardId, shard.GetExtension<HybridCommandsExtension>());
		return modules;
	}
}
