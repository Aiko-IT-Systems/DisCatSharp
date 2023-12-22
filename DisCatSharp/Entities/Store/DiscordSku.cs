using System;
using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a <see cref="DiscordSku"/>.
/// </summary>
public class DiscordSku : SnowflakeObject, IEquatable<DiscordSku>
{
	/// <summary>
	/// Gets the sku type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public SkuType Type { get; internal set; }

	/// <summary>
	/// Gets the dependent (parent) sku id.
	/// </summary>
	[JsonProperty("dependent_sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? DependentSkuId { get; internal set; }

	/// <summary>
	/// Gets the application id the sku belongs to.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	/// Gets the manifest labels.
	/// </summary>
	[JsonProperty("manifest_labels", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> ManifestLabels { get; internal set; } = [];

	/// <summary>
	/// Gets the locales.
	/// </summary>
	[JsonProperty("locales", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Locales { get; internal set; } = [];

	/// <summary>
	/// Gets the access type.
	/// </summary>
	[JsonProperty("access_type", NullValueHandling = NullValueHandling.Ignore)]
	public SkuAccessType AccessType { get; internal set; }

	/// <summary>
	/// Gets the sku name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the skus features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public List<int> Features { get; internal set; } = [];

	/// <summary>
	/// Gets the skus genres.
	/// </summary>
	[JsonProperty("genres", NullValueHandling = NullValueHandling.Ignore)]
	public List<int> Genres { get; internal set; } = [];

	/// <summary>
	/// Gets the skus release date.
	/// </summary>
	[JsonProperty("release_date", NullValueHandling = NullValueHandling.Ignore)]
	public string ReleaseDate { get; internal set; }

	/// <summary>
	/// Gets the skus legal notice.
	/// </summary>
	[JsonProperty("legal_notice", NullValueHandling = NullValueHandling.Ignore)]
	public string? LegalNotice { get; internal set; }

	/// <summary>
	/// Gets whether the sku is premium.
	/// </summary>
	[JsonProperty("premium", NullValueHandling = NullValueHandling.Ignore)]
	public bool Premium { get; internal set; }

	/// <summary>
	/// Gets the sku slug.
	/// </summary>
	[JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
	public string Slug { get; internal set; }

	/// <summary>
	/// Gets the sku price.
	/// </summary>
	[JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
	public SkuPrice Price { get; internal set; }

	/// <summary>
	/// Gets the sku flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public SkuFlags Flags { get; internal set; }

	/// <summary>
	/// Gets the product line.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public ProductLine ProductLine { get; internal set; }

	/// <summary>
	/// Gets whether to show a age gate.
	/// </summary>
	[JsonProperty("show_age_gate", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ShowAgeGate { get; internal set; }

	/// <summary>
	/// Checks whether this <see cref="DiscordSku"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordSku"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordSku);

	/// <summary>
	/// Checks whether this <see cref="DiscordSku"/> is equal to another <see cref="DiscordSku"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordSku"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordSku"/> is equal to this <see cref="DiscordSku"/>.</returns>
	public bool Equals(DiscordSku e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordSku"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordSku"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordSku"/> objects are equal.
	/// </summary>
	/// <param name="e1">First sku to compare.</param>
	/// <param name="e2">Second sku to compare.</param>
	/// <returns>Whether the two skus are equal.</returns>
	public static bool operator ==(DiscordSku e1, DiscordSku e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null)
		       && (o1 == null || o2 != null)
		       && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordSku"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First sku to compare.</param>
	/// <param name="e2">Second sku to compare.</param>
	/// <returns>Whether the two skus are not equal.</returns>
	public static bool operator !=(DiscordSku e1, DiscordSku e2)
		=> !(e1 == e2);
}

public sealed class SkuPrice
{
	[JsonProperty("amount")]
	public double Amount { get; internal set; }

	[JsonProperty("currency")]
	public string Currency { get; internal set; }
}
