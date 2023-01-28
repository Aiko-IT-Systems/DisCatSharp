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

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// The lavalink payload.
/// </summary>
internal abstract class LavalinkPayload
{
	/// <summary>
	/// Gets the operation.
	/// </summary>
	[JsonProperty("op")]
	public string Operation { get; }

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guildId", NullValueHandling = NullValueHandling.Ignore)]
	public string GuildId { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkPayload"/> class.
	/// </summary>
	/// <param name="opcode">The opcode.</param>
	internal LavalinkPayload(string opcode)
	{
		this.Operation = opcode;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkPayload"/> class.
	/// </summary>
	/// <param name="opcode">The opcode.</param>
	/// <param name="guildId">The guild id.</param>
	internal LavalinkPayload(string opcode, string guildId)
	{
		this.Operation = opcode;
		this.GuildId = guildId;
	}
}
