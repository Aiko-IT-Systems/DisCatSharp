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

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// The transport team.
/// </summary>
internal sealed class TransportTeam : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the icon hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
	public string IconHash { get; set; }

	/// <summary>
	/// Gets or sets the owner id.
	/// </summary>
	[JsonProperty("owner_user_id")]
	public ulong OwnerId { get; set; }

	/// <summary>
	/// Gets or sets the members.
	/// </summary>
	[JsonProperty("members", NullValueHandling = NullValueHandling.Include)]
	public List<TransportTeamMember> Members { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TransportTeam"/> class.
	/// </summary>
	internal TransportTeam() { }
}

/// <summary>
/// The transport team member.
/// </summary>
internal sealed class TransportTeamMember
{
	/// <summary>
	/// Gets or sets the membership state.
	/// </summary>
	[JsonProperty("membership_state")]
	public int MembershipState { get; set; }

	/// <summary>
	/// Gets or sets the permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Include)]
	public List<string> Permissions { get; set; }

	/// <summary>
	/// Gets or sets the team id.
	/// </summary>
	[JsonProperty("team_id")]
	public ulong TeamId { get; set; }

	/// <summary>
	/// Gets or sets the user.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Include)]
	public TransportUser User { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TransportTeamMember"/> class.
	/// </summary>
	internal TransportTeamMember() { }
}
