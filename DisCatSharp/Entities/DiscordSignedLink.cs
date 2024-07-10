using System;
using System.Globalization;
using System.Web;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a <see cref="DiscordSignedLink"/> used for attachments and other things to improve security
/// and prevent bad actors from abusing Discord's CDN.
/// </summary>
[JsonConverter(typeof(DiscordSignedLinkJsonConverter))]
public class DiscordSignedLink : Uri
{
	private readonly object _rawValue;

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
	/// <param name="uri">An <see cref="Uri"/>.</param>
	public DiscordSignedLink(Uri uri)
		: base(uri.AbsoluteUri)
	{
		ArgumentNullException.ThrowIfNull(uri);

		this._rawValue = uri.AbsoluteUri;

		if (string.IsNullOrWhiteSpace(this.Query))
			return;

		var queries = HttpUtility.ParseQueryString(this.Query);

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
	/// Initializes a new instance of the <see cref="Uri"/> class with the specified URI for signed discord links.
	/// </summary>
	/// <param name="uriString">A string that identifies the resource to be represented by the <see cref="Uri"/> instance.</param>
	public DiscordSignedLink(string uriString)
		: base(uriString)
	{
		ArgumentNullException.ThrowIfNull(uriString);

		this._rawValue = uriString;

		if (string.IsNullOrWhiteSpace(this.Query))
			return;

		var queries = HttpUtility.ParseQueryString(this.Query);

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
	/// Represents a <see cref="DiscordSignedLinkJsonConverter"/>.
	/// </summary>
	internal sealed class DiscordSignedLinkJsonConverter : JsonConverter
	{
		/// <summary>
		/// Writes the json.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			=> writer.WriteValue((value as DiscordSignedLink)._rawValue);

		/// <summary>
		/// Reads the json.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="objectType">The object type.</param>
		/// <param name="existingValue">The existing value.</param>
		/// <param name="serializer">The serializer.</param>
		public override object ReadJson(
			JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer
		)
		{
			var val = reader.Value;
			return val == null
				? null
				: val is not string s
					? throw new JsonReaderException("DiscordSignedLink value invalid format! This is a bug in DisCatSharp. " +
					                                $"Include the type in your bug report: [[{reader.TokenType}]]")
					: new DiscordSignedLink(s);
		}

		/// <summary>
		/// Whether it can be converted.
		/// </summary>
		/// <param name="objectType">The object type.</param>
		/// <returns>A bool.</returns>
		public override bool CanConvert(Type objectType)
			=> objectType == typeof(DiscordSignedLink);
	}
}
