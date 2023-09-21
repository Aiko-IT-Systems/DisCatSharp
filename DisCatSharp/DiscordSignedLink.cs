using System;
using System.Globalization;
using System.Web;

namespace DisCatSharp;

/// <summary>
/// Represents a <see cref="DiscordSignedLink"/> used for attachments and other things to improve security
/// and prevent bad actors from abusing Discord's CDN.
/// </summary>
public sealed class DiscordSignedLink : Uri
{
	/// <summary>
	/// When the signed link expires.
	/// </summary>
	public DateTimeOffset? ExpiresAt { get; init; }

	/// <summary>
	/// When the signed link was generated.
	/// </summary>
	public DateTimeOffset? IssuedAt { get; init; }

	/// <summary>
	/// The signature of the signed link.
	/// </summary>
	public string? Signature { get; init; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Uri"/> class with the specified URI for signed discord links.
	/// </summary>
	/// <param name="uriString">A string that identifies the resource to be represented by the <see cref="Uri"/> instance.</param>
	public DiscordSignedLink(string uriString)
		: base(uriString)
	{
		if (string.IsNullOrWhiteSpace(this.Query))
			return;

		var queries = HttpUtility.ParseQueryString(this.Query);

		if (!queries.HasKeys())
			return;

		if (queries.Get("ex") is { } expiresString && long.TryParse(expiresString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var expiresTimeStamp))
			this.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresTimeStamp);

		if (queries.Get("is") is { } issuedString && long.TryParse(issuedString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var issuedTimeStamp))
			this.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedTimeStamp);

		this.Signature = queries.Get("sg");
	}
}

