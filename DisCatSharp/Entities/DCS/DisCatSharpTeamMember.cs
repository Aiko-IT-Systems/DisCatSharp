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

using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a DisCatSharp team member.
/// </summary>
public sealed class DisCatSharpTeamMember : SnowflakeObject
{
	/// <summary>
	/// Gets this user's username.
	/// </summary>
	public string Username { get; internal set; }

	/// <summary>
	/// Gets the user's 4-digit discriminator.
	/// </summary>
	public string Discriminator { get; internal set; }

	/// <summary>
	/// Gets the discriminator integer.
	/// </summary>
	internal int DiscriminatorInt
		=> int.Parse(this.Discriminator, NumberStyles.Integer, CultureInfo.InvariantCulture);

	/// <summary>
	/// Gets the user's banner color, if set. Mutually exclusive with <see cref="BannerHash"/>.
	/// </summary>
	public DiscordColor? BannerColor
		=> !this.BannerColorInternal.HasValue ? null : new DiscordColor(this.BannerColorInternal.Value);

	internal int? BannerColorInternal;

	/// <summary>
	/// Gets the user's banner url
	/// </summary>
	public string BannerUrl
		=> string.IsNullOrWhiteSpace(this.BannerHash) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.BANNERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_") ? "gif" : "png")}?size=4096";

	/// <summary>
	/// Gets the user's profile banner hash. Mutually exclusive with <see cref="BannerColor"/>.
	/// </summary>
	public string BannerHash { get; internal set; }

	/// <summary>
	/// Gets the user's avatar hash.
	/// </summary>
	public string AvatarHash { get; internal set; }

	/// <summary>
	/// Gets the user's avatar URL.
	/// </summary>
	public string AvatarUrl
		=> string.IsNullOrWhiteSpace(this.AvatarHash) ? this.DefaultAvatarUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHash}.{(this.AvatarHash.StartsWith("a_") ? "gif" : "png")}?size=1024";

	/// <summary>
	/// Gets the URL of default avatar for this user.
	/// </summary>
	public string DefaultAvatarUrl
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.EMBED}{Endpoints.AVATARS}/{(this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture)}.png?size=1024";

	/// <summary>
	/// Initializes a new instance of the <see cref="DisCatSharpTeamMember"/> class.
	/// </summary>
	internal DisCatSharpTeamMember()
	{ }
}
