using System;
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
		=> string.IsNullOrWhiteSpace(this.BannerHash) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.BANNERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=4096";

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
		=> string.IsNullOrWhiteSpace(this.AvatarHash) ? this.DefaultAvatarUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHash}.{(this.AvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

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
