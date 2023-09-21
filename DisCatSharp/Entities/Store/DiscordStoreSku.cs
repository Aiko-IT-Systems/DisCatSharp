using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a <see cref="DiscordStoreSku"/>.
/// </summary>
public class DiscordStoreSku : SnowflakeObject, IEquatable<DiscordStoreSku>
{
	/// <summary>
	/// Gets the store skus summary.
	/// </summary>
	[JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
	public string Summary { get; set; }

	/// <summary>
	/// Gets the store skus sku.
	/// </summary>
	[JsonProperty("sku", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordSku Sku { get; set; }

	/// <summary>
	/// Gets the store skus description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; set; }

	/// <summary>
	/// Checks whether this <see cref="DiscordStoreSku"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordStoreSku"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordStoreSku);

	/// <summary>
	/// Checks whether this <see cref="DiscordStoreSku"/> is equal to another <see cref="DiscordStoreSku"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordStoreSku"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordStoreSku"/> is equal to this <see cref="DiscordStoreSku"/>.</returns>
	public bool Equals(DiscordStoreSku e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordStoreSku"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordStoreSku"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordStoreSku"/> objects are equal.
	/// </summary>
	/// <param name="e1">First store sku to compare.</param>
	/// <param name="e2">Second store sku to compare.</param>
	/// <returns>Whether the two store skus are equal.</returns>
	public static bool operator ==(DiscordStoreSku e1, DiscordStoreSku e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null)
		       && (o1 == null || o2 != null)
		       && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordStoreSku"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First store sku to compare.</param>
	/// <param name="e2">Second store sku to compare.</param>
	/// <returns>Whether the two store skus are not equal.</returns>
	public static bool operator !=(DiscordStoreSku e1, DiscordStoreSku e2)
		=> !(e1 == e2);
}
