using System;
using System.Globalization;
using System.Web;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a <see cref="DiscordSignedLink" /> used for attachments and other things to improve security
///     and prevent bad actors from abusing Discord's CDN.
/// </summary>
[JsonConverter(typeof(DiscordSignedLinkJsonConverter))]
public sealed class DiscordSignedLink : DiscordUri
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordSignedLink" /> class with the specified URI for signed discord
	///     links.
	/// </summary>
	/// <param name="uri">An <see cref="Uri" />.</param>
	public DiscordSignedLink(Uri uri)
		: base(uri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		if (string.IsNullOrWhiteSpace(uri.Query))
			return;

		var queries = HttpUtility.ParseQueryString(uri.Query);

		if (!queries.HasKeys())
			return;

		if (queries.Get("ex") is { } expiresString && long.TryParse(expiresString, NumberStyles.HexNumber,
			CultureInfo.InvariantCulture,
			out var expiresTimeStamp))
			this.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresTimeStamp);

		if (queries.Get("is") is { } issuedString &&
		    long.TryParse(issuedString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var issuedTimeStamp))
			this.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedTimeStamp);

		this.Signature = queries.Get("hm");
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordSignedLink" /> class with the specified URI for signed discord
	///     links.
	/// </summary>
	/// <param name="uriString">
	///     A string that identifies the resource to be represented by the <see cref="DiscordSignedLink" />
	///     instance.
	/// </param>
	public DiscordSignedLink(string uriString)
		: base(uriString)
	{
		ArgumentNullException.ThrowIfNull(uriString);

		var uri = this.ToUri() ?? throw new UriFormatException("Invalid URI format.");

		if (string.IsNullOrWhiteSpace(uri.Query))
			return;

		var queries = HttpUtility.ParseQueryString(uri.Query);

		if (!queries.HasKeys())
			return;

		if (queries.Get("ex") is { } expiresString && long.TryParse(expiresString, NumberStyles.HexNumber,
			CultureInfo.InvariantCulture,
			out var expiresTimeStamp))
			this.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresTimeStamp);

		if (queries.Get("is") is { } issuedString &&
		    long.TryParse(issuedString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var issuedTimeStamp))
			this.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedTimeStamp);

		this.Signature = queries.Get("hm");
	}

	/// <summary>
	///     Gets whether the url is signed.
	/// </summary>
	public bool IsSigned
		=> this.ExpiresAt.HasValue && this.IssuedAt.HasValue && !string.IsNullOrWhiteSpace(this.Signature);

	/// <summary>
	///     Gets when the signed link expires.
	/// </summary>
	public DateTimeOffset? ExpiresAt { get; init; }

	/// <summary>
	///     Gets when the signed link was generated.
	/// </summary>
	public DateTimeOffset? IssuedAt { get; init; }

	/// <summary>
	///     Gets the signature of the signed link.
	/// </summary>
	public string? Signature { get; init; }

	/// <summary>
	///     Returns a string representation of this <see cref="DiscordSignedLink"/>.
	/// </summary>
	/// <returns>This <see cref="DiscordSignedLink"/>, as a string.</returns>
	public override string? ToString()
		=> this._value?.ToString();

	/// <summary>
	///    Converts a <see cref="DiscordSignedLink" /> to a <see cref="Uri" /> implicitly, if possible.
	/// </summary>
	/// <param name="duri">The <see cref="DiscordSignedLink" /> to convert.</param>
	public static implicit operator Uri?(DiscordSignedLink duri)
		=> duri.ToUri();

	/// <summary>
	///     Represents a <see cref="DiscordSignedLinkJsonConverter" />.
	/// </summary>
	internal sealed class DiscordSignedLinkJsonConverter : JsonConverter
	{
		/// <summary>
		///     Writes the json.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The serializer.</param>
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
			=> writer.WriteValue((value as DiscordSignedLink)?._value);

		/// <summary>
		///     Reads the json.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="objectType">The object type.</param>
		/// <param name="existingValue">The existing value.</param>
		/// <param name="serializer">The serializer.</param>
		public override object? ReadJson(
			JsonReader reader,
			Type objectType,
			object? existingValue,
			JsonSerializer serializer
		)
		{
			var val = reader.Value;
			return val is null
				? null
				: val is not string s
					? throw new JsonReaderException("DiscordSignedLink value invalid format! This is a bug in DisCatSharp. " +
					                                $"Include the type in your bug report: [[{reader.TokenType}]]")
					: new DiscordSignedLink(s);
		}

		/// <summary>
		///     Whether it can be converted.
		/// </summary>
		/// <param name="objectType">The object type.</param>
		/// <returns>A bool.</returns>
		public override bool CanConvert(Type objectType)
			=> objectType == typeof(DiscordSignedLink);
	}
}
