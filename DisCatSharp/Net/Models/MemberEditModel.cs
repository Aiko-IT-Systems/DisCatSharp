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

using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a member edit model.
/// </summary>
public class MemberEditModel : BaseEditModel
{
	/// <summary>
	/// New nickname
	/// </summary>
	public Optional<string> Nickname { internal get; set; }

	/// <summary>
	/// New roles
	/// </summary>
	public Optional<List<DiscordRole>> Roles { internal get; set; }

	/// <summary>
	/// Whether this user should be muted
	/// </summary>
	public Optional<bool> Muted { internal get; set; }

	/// <summary>
	/// Whether this user should be deafened
	/// </summary>
	public Optional<bool> Deafened { internal get; set; }

	/// <summary>
	/// Voice channel to move this user to, set to null to kick
	/// </summary>
	public Optional<DiscordChannel> VoiceChannel { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MemberEditModel"/> class.
	/// </summary>
	internal MemberEditModel()
	{ }
}
