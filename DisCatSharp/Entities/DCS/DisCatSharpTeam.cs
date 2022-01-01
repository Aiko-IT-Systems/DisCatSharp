// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;

namespace DisCatSharp.Entities
{
	/// <summary>
	/// The DisCatSharp team.
	/// </summary>
	public sealed class DisCatSharpTeam : SnowflakeObject
	{
		/// <summary>
		/// Gets the team name.
		/// </summary>
		public string TeamName { get; internal set; }

		/// <summary>
		/// Gets the teams's icon.
		/// </summary>
		public string Icon
			=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.TEAM_ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=1024" : null;

		/// <summary>
		/// Gets the team's icon hash.
		/// </summary>
		public string IconHash { get; internal set; }

		/// <summary>
		/// Gets the teams's logo.
		/// </summary>
		public string Logo
			=> !string.IsNullOrWhiteSpace(this.LogoHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ICONS}/{this.GuildId.ToString(CultureInfo.InvariantCulture)}/{this.LogoHash}.png?size=1024" : null;

		/// <summary>
		/// Gets the team's logo hash.
		/// </summary>
		public string LogoHash { get; internal set; }

		/// <summary>
		/// Gets the teams's banner.
		/// </summary>
		public string Banner
			=> !string.IsNullOrWhiteSpace(this.BannerHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.BANNERS}/{this.GuildId.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.png?size=1024" : null;

		/// <summary>
		/// Gets the team's banner hash.
		/// </summary>
		public string BannerHash { get; internal set; }

		/// <summary>
		/// Gets the team's docs url.
		/// </summary>
		public string DocsUrl { get; internal set; }

		/// <summary>
		/// Gets the team's repo url.
		/// </summary>
		public string RepoUrl { get; internal set; }

		/// <summary>
		/// Gets the team's terms of service url.
		/// </summary>
		public string TermsOfServiceUrl { get; internal set; }

		/// <summary>
		/// Gets the team's privacy policy url.
		/// </summary>
		public string PrivacyPolicyUrl { get; internal set; }

		/// <summary>
		/// Get's the team's guild id
		/// </summary>
		public ulong GuildId { get; internal set; }

		/// <summary>
		/// Gets the team's developers.
		/// </summary>
		public IReadOnlyList<DisCatSharpTeamMember> Developers { get; internal set; }

		/// <summary>
		/// Gets the team's owner.
		/// </summary>
		public DisCatSharpTeamMember Owner { get; internal set; }

		/// <summary>
		/// Gets the team's guild.
		/// </summary>
		public DiscordGuild Guild { get; internal set; }

		/// <summary>
		/// Gets the team's support invite.
		/// </summary>
		public DiscordInvite SupportInvite { get; internal set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DisCatSharpTeam"/> class.
		/// </summary>
		internal DisCatSharpTeam()
		{ }
	}
}
