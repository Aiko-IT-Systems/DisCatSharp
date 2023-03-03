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
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an OAuth2 application.
/// </summary>
public sealed class DiscordApplication : DiscordMessageApplication, IEquatable<DiscordApplication>
{
	/// <summary>
	/// Gets the application's summary.
	/// </summary>
	public string Summary { get; internal set; }

	/// <summary>
	/// Gets the application's icon.
	/// </summary>
	public override string Icon
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.APP_ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=1024" : null;

	/// <summary>
	/// Gets the application's icon hash.
	/// </summary>
	public string IconHash { get; internal set; }

	/// <summary>
	/// Gets the application's allowed RPC origins.
	/// </summary>
	public IReadOnlyList<string> RpcOrigins { get; internal set; }

	/// <summary>
	/// Gets the application's flags.
	/// </summary>
	public ApplicationFlags Flags { get; internal set; }

	/// <summary>
	/// Gets the application's owners.
	/// </summary>
	public List<DiscordUser> Owners { get; internal set; }

	/// <summary>
	/// Gets whether this application's bot user requires code grant.
	/// </summary>
	public bool? RequiresCodeGrant { get; internal set; }

	/// <summary>
	/// Gets whether this bot application is public.
	/// </summary>
	public bool? IsPublic { get; internal set; }

	/// <summary>
	/// Gets the terms of service url of the application.
	/// </summary>
	public string TermsOfServiceUrl { get; internal set; }

	/// <summary>
	/// Gets the privacy policy url of the application.
	/// </summary>
	public string PrivacyPolicyUrl { get; internal set; }

	/// <summary>
	/// Gets the team name of the application.
	/// </summary>
	public string TeamName { get; internal set; }

	/// <summary>
	/// Gets the hash of the application's cover image.
	/// </summary>
	public string CoverImageHash { get; internal set; }

	/// <summary>
	/// Gets this application's cover image URL.
	/// </summary>
	public override string CoverImageUrl
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.APP_ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.CoverImageHash}.png?size=1024";

	/// <summary>
	/// Gets the team which owns this application.
	/// </summary>
	public DiscordTeam Team { get; internal set; }

	/// <summary>
	/// Gets the hex encoded key for verification in interactions and the GameSDK's GetTicket
	/// </summary>
	public string VerifyKey { get; internal set; }

	/// <summary>
	/// If this application is a game sold on Discord, this field will be the guild to which it has been linked
	/// </summary>
	public ulong? GuildId { get; internal set; }

	/// <summary>
	/// Gets the partial guild if set as support server.
	/// </summary>
	public DiscordGuild? Guild { get; internal set; }

	/// <summary>
	/// If this application is a game sold on Discord, this field will be the id of the "Game SKU" that is created, if exists
	/// </summary>
	public ulong? PrimarySkuId { get; internal set; }

	/// <summary>
	/// If this application is a game sold on Discord, this field will be the URL slug that links to the store page
	/// </summary>
	public string Slug { get; internal set; }

	/// <summary>
	/// Gets or sets a list of <see cref="DiscordApplicationAsset"/>.
	/// </summary>
	private IReadOnlyList<DiscordApplicationAsset> _assets;

	/// <summary>
	/// A custom url for the Add To Server button.
	/// </summary>
	public string CustomInstallUrl { get; internal set; }

	/// <summary>
	/// Install parameters for adding the application to a guild.
	/// </summary>
	public DiscordApplicationInstallParams InstallParams { get; internal set; }

	/// <summary>
	/// The application's role connection verification entry point,
	/// which when configured will render the app as a verification method in the guild role verification configuration.
	/// </summary>
	public string RoleConnectionsVerificationUrl { get; internal set; }

	/// <summary>
	/// The application tags.
	/// Not used atm.
	/// </summary>
	public IReadOnlyList<string> Tags { get; internal set; }

	/// <summary>
	/// Whether the application is hooked.
	/// </summary>
	public bool IsHook { get; internal set; }

	/// <summary>
	/// Gets the application type.
	/// Mostly null.
	/// </summary>
	public string Type { get; internal set; }

	/// <summary>
	/// Gets the approximate guild count
	/// </summary>
	public int? ApproximateGuildCount { get; internal set; }

	/// <summary>
	/// Gets the interactions endpoint url.
	/// </summary>
	public string InteractionsEndpointUrl { get; set; }

	/// <summary>
	/// Gets the redirect uris.
	/// </summary>
	public List<string> RedirectUris { get; set; } = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApplication"/> class.
	/// </summary>
	internal DiscordApplication()
	{ }

	/// <summary>
	/// Gets the application's cover image URL, in requested format and size.
	/// </summary>
	/// <param name="fmt">Format of the image to get.</param>
	/// <param name="size">Maximum size of the cover image. Must be a power of two, minimum 16, maximum 2048.</param>
	/// <returns>URL of the application's cover image.</returns>
	public string GetAvatarUrl(ImageFormat fmt, ushort size = 1024)
	{
		if (fmt == ImageFormat.Unknown)
			throw new ArgumentException("You must specify valid image format.", nameof(fmt));

		if (size < 16 || size > 2048)
			throw new ArgumentOutOfRangeException(nameof(size));

		var log = Math.Log(size, 2);
		if (log < 4 || log > 11 || log % 1 != 0)
			throw new ArgumentOutOfRangeException(nameof(size));

		var sfmt = "";
		sfmt = fmt switch
		{
			ImageFormat.Gif => "gif",
			ImageFormat.Jpeg => "jpg",
			ImageFormat.Auto or ImageFormat.Png => "png",
			ImageFormat.WebP => "webp",
			_ => throw new ArgumentOutOfRangeException(nameof(fmt)),
		};
		var ssize = size.ToString(CultureInfo.InvariantCulture);
		return !string.IsNullOrWhiteSpace(this.CoverImageHash)
			? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.{sfmt}?size={ssize}"
			: null;
	}

	/// <summary>
	/// Retrieves this application's assets.
	/// </summary>
	/// <returns>This application's assets.</returns>
	public async Task<IReadOnlyList<DiscordApplicationAsset>> GetAssetsAsync()
	{
		this._assets ??= await this.Discord.ApiClient.GetApplicationAssetsAsync(this).ConfigureAwait(false);

		return this._assets;
	}

	/// <summary>
	/// Generates an oauth url for the application.
	/// </summary>
	/// <param name="permissions">The permissions.</param>
	/// <returns>OAuth Url</returns>
	public string GenerateBotOAuth(Permissions permissions = Permissions.None)
	{
		permissions &= PermissionMethods.FullPerms;
		// hey look, it's not all annoying and blue :P
		return new QueryUriBuilder($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.OAUTH2}{Endpoints.AUTHORIZE}")
			.AddParameter("client_id", this.Id.ToString(CultureInfo.InvariantCulture))
			.AddParameter("scope", "bot")
			.AddParameter("permissions", ((long)permissions).ToString(CultureInfo.InvariantCulture))
			.ToString();
	}

	/// <summary>
	/// Checks whether this <see cref="DiscordApplication"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordApplication"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordApplication);

	/// <summary>
	/// Checks whether this <see cref="DiscordApplication"/> is equal to another <see cref="DiscordApplication"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordApplication"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordApplication"/> is equal to this <see cref="DiscordApplication"/>.</returns>
	public bool Equals(DiscordApplication e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordApplication"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordApplication"/> objects are equal.
	/// </summary>
	/// <param name="e1">First application to compare.</param>
	/// <param name="e2">Second application to compare.</param>
	/// <returns>Whether the two applications are equal.</returns>
	public static bool operator ==(DiscordApplication e1, DiscordApplication e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordApplication"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First application to compare.</param>
	/// <param name="e2">Second application to compare.</param>
	/// <returns>Whether the two applications are not equal.</returns>
	public static bool operator !=(DiscordApplication e1, DiscordApplication e2)
		=> !(e1 == e2);
}

/// <summary>
/// Represents an discord asset.
/// </summary>
public abstract class DiscordAsset
{
	/// <summary>
	/// Gets the ID of this asset.
	/// </summary>
	public virtual string Id { get; set; }

	/// <summary>
	/// Gets the URL of this asset.
	/// </summary>
	public abstract Uri Url { get; }
}

/// <summary>
/// Represents an asset for an OAuth2 application.
/// </summary>
public sealed class DiscordApplicationAsset : DiscordAsset, IEquatable<DiscordApplicationAsset>
{
	/// <summary>
	/// Gets the Discord client instance for this asset.
	/// </summary>
	internal BaseDiscordClient Discord { get; set; }

	/// <summary>
	/// Gets the asset's name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the asset's type.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationAssetType Type { get; internal set; }

	/// <summary>
	/// Gets the application this asset belongs to.
	/// </summary>
	public DiscordApplication Application { get; internal set; }

	/// <summary>
	/// Gets the Url of this asset.
	/// </summary>
	public override Uri Url
		=> new($"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.APP_ASSETS}/{this.Application.Id.ToString(CultureInfo.InvariantCulture)}/{this.Id}.png");

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApplicationAsset"/> class.
	/// </summary>
	internal DiscordApplicationAsset()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApplicationAsset"/> class.
	/// </summary>
	/// <param name="app">The app.</param>
	internal DiscordApplicationAsset(DiscordApplication app)
	{
		this.Discord = app.Discord;
	}

	/// <summary>
	/// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordApplicationAsset);

	/// <summary>
	/// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another <see cref="DiscordApplicationAsset"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordApplicationAsset"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordApplicationAsset"/> is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
	public bool Equals(DiscordApplicationAsset e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordApplication"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are equal.
	/// </summary>
	/// <param name="e1">First application asset to compare.</param>
	/// <param name="e2">Second application asset to compare.</param>
	/// <returns>Whether the two application assets not equal.</returns>
	public static bool operator ==(DiscordApplicationAsset e1, DiscordApplicationAsset e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First application asset to compare.</param>
	/// <param name="e2">Second application asset to compare.</param>
	/// <returns>Whether the two application assets are not equal.</returns>
	public static bool operator !=(DiscordApplicationAsset e1, DiscordApplicationAsset e2)
		=> !(e1 == e2);
}

/// <summary>
/// Represents an spotify asset.
/// </summary>
public sealed class DiscordSpotifyAsset : DiscordAsset
{
	/// <summary>
	/// Gets the URL of this asset.
	/// </summary>
	public override Uri Url
		=> this._url.Value;

	private readonly Lazy<Uri> _url;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordSpotifyAsset"/> class.
	/// </summary>
	public DiscordSpotifyAsset()
	{
		this._url = new Lazy<Uri>(() =>
		{
			var ids = this.Id.Split(':');
			var id = ids[1];
			return new Uri($"https://i.scdn.co/image/{id}");
		});
	}
}

/// <summary>
/// Determines the type of the asset attached to the application.
/// </summary>
public enum ApplicationAssetType : int
{
	/// <summary>
	/// Unknown type. This indicates something went terribly wrong.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// This asset can be used as small image for rich presences.
	/// </summary>
	SmallImage = 1,

	/// <summary>
	/// This asset can be used as large image for rich presences.
	/// </summary>
	LargeImage = 2
}
