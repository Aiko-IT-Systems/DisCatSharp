using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord emoji.
/// </summary>
public partial class DiscordEmoji : SnowflakeObject, IEquatable<DiscordEmoji>
{
	private readonly Lazy<IReadOnlyList<ulong>> _rolesLazy;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordEmoji" /> class.
	/// </summary>
	internal DiscordEmoji()
	{
		this._rolesLazy = new(() => new ReadOnlyCollection<ulong>(this.RolesInternal));
	}

	/// <summary>
	///     Gets the name of this emoji.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	///     Gets IDs the roles this emoji is enabled for.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<ulong> Roles
		=> this._rolesLazy.Value;

	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	internal List<ulong> RolesInternal { get; set; } = [];

	/// <summary>
	///     Gets whether this emoji requires colons to use.
	/// </summary>
	[JsonProperty("require_colons")]
	public bool RequiresColons { get; internal set; }

	/// <summary>
	///     Gets whether this emoji is managed by an integration.
	/// </summary>
	[JsonProperty("managed")]
	public bool IsManaged { get; internal set; }

	/// <summary>
	///     Gets whether this emoji is animated.
	/// </summary>
	[JsonProperty("animated")]
	public bool IsAnimated { get; internal set; }

	/// <summary>
	///     Gets whether the emoji is available for use.
	///     An emoji may not be available due to loss of server boost.
	/// </summary>
	[JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsAvailable { get; internal set; }

	/// <summary>
	///     Gets the image URL of this emoji.
	/// </summary>
	[JsonIgnore]
	public string Url
		=> this.Id == 0
			? throw new InvalidOperationException("Cannot get URL of unicode emojis.")
			: this.IsAnimated
				? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.EMOJIS}/{this.Id.ToString(CultureInfo.InvariantCulture)}.gif"
				: $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.EMOJIS}/{this.Id.ToString(CultureInfo.InvariantCulture)}.png";

	/// <summary>
	///     Gets the unicode version of this emoji.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown if emoji is not a unicode emoji.</exception>
	[JsonIgnore]
	public string UnicodeEmoji
		=> this.Id != 0
			? throw new InvalidOperationException("Emoji is not a unicode emoji")
			: s_aliasNameToCanonical.TryGetValue(this.Name, out var canonicalName)
				? s_unicodeEmojis[canonicalName]
				: s_unicodeEmojis.TryGetValue(this.Name, out var value)
					? value
					: s_discordNameLookup.ContainsKey(this.Name)
						? this.Name
						: throw new InvalidOperationException("Emoji is not a unicode emoji");

	/// <summary>
	///     Checks whether this <see cref="DiscordEmoji" /> is equal to another <see cref="DiscordEmoji" />.
	/// </summary>
	/// <param name="e"><see cref="DiscordEmoji" /> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordEmoji" /> is equal to this <see cref="DiscordEmoji" />.</returns>
	public bool Equals(DiscordEmoji e)
		=> e is not null && (ReferenceEquals(this, e) || (this.Id == e.Id && this.Name == e.Name));

	/// <summary>
	///     Gets emoji's name in non-Unicode format (eg. :thinking: instead of the Unicode representation of the emoji).
	/// </summary>
	public string GetDiscordName()
	{
		s_discordNameLookup.TryGetValue(this.Name, out var name);

		return name ?? $":{this.Name}:";
	}

	/// <summary>
	///     Returns a string representation of this emoji.
	/// </summary>
	/// <returns>String representation of this emoji.</returns>
	public override string ToString()
		=> this.Id != 0
			? this.IsAnimated
				? $"<a:{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}>"
				: $"<:{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}>"
			: this.Name;

	/// <summary>
	///     Checks whether this <see cref="DiscordEmoji" /> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordEmoji" />.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordEmoji);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordEmoji" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordEmoji" />.</returns>
	public override int GetHashCode()
	{
		var hash = 13;
		hash = (hash * 7) + this.Id.GetHashCode();
		hash = (hash * 7) + this.Name.GetHashCode();

		return hash;
	}

	/// <summary>
	///     Gets the reactions string.
	/// </summary>
	internal string ToReactionString()
		=> this.Id != 0 ? $"{this.Name}:{this.Id.ToString(CultureInfo.InvariantCulture)}" : this.Name;

	/// <summary>
	///     Gets whether the two <see cref="DiscordEmoji" /> objects are equal.
	/// </summary>
	/// <param name="e1">First emoji to compare.</param>
	/// <param name="e2">Second emoji to compare.</param>
	/// <returns>Whether the two emoji are equal.</returns>
	public static bool operator ==(DiscordEmoji e1, DiscordEmoji e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || (e1.Id == e2.Id && e1.Name == e2.Name));
	}

	/// <summary>
	///     Gets whether the two <see cref="DiscordEmoji" /> objects are not equal.
	/// </summary>
	/// <param name="e1">First emoji to compare.</param>
	/// <param name="e2">Second emoji to compare.</param>
	/// <returns>Whether the two emoji are not equal.</returns>
	public static bool operator !=(DiscordEmoji e1, DiscordEmoji e2)
		=> !(e1 == e2);

	/// <summary>
	///     Implicitly converts this emoji to its string representation.
	/// </summary>
	/// <param name="e1">Emoji to convert.</param>
	public static implicit operator string(DiscordEmoji e1)
		=> e1?.ToString();

	/// <summary>
	///     Checks whether specified unicode entity is a valid unicode emoji.
	/// </summary>
	/// <param name="unicodeEntity">Entity to check.</param>
	/// <returns>Whether it's a valid emoji.</returns>
	public static bool IsValidUnicode(string unicodeEntity)
		=> s_discordNameLookup.ContainsKey(unicodeEntity);

	/// <summary>
	///     Creates an emoji object from a unicode entity.
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="unicodeEntity">Unicode entity to create the object from.</param>
	/// <returns>Create <see cref="DiscordEmoji" /> object.</returns>
	public static DiscordEmoji FromUnicode(BaseDiscordClient client, string unicodeEntity)
		=> !IsValidUnicode(unicodeEntity)
			? throw new ArgumentException("Specified unicode entity is not a valid unicode emoji.", nameof(unicodeEntity))
			: new DiscordEmoji
			{
				Name = unicodeEntity,
				Discord = client
			};

	/// <summary>
	///     Creates an emoji object from a unicode entity.
	/// </summary>
	/// <param name="unicodeEntity">Unicode entity to create the object from.</param>
	/// <returns>Create <see cref="DiscordEmoji" /> object.</returns>
	public static DiscordEmoji FromUnicode(string unicodeEntity)
		=> FromUnicode(null, unicodeEntity);

	/// <summary>
	///     Attempts to create an emoji object from a unicode entity.
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="unicodeEntity">Unicode entity to create the object from.</param>
	/// <param name="emoji">Resulting <see cref="DiscordEmoji" /> object.</param>
	/// <returns>Whether the operation was successful.</returns>
	public static bool TryFromUnicode(BaseDiscordClient client, string unicodeEntity, out DiscordEmoji emoji)
	{
		emoji = null;
		if (!s_discordNameLookup.TryGetValue(unicodeEntity, out var discordName))
			return false;

		if (!s_unicodeEmojis.TryGetValue(discordName, out unicodeEntity))
			return false;

		emoji = new()
		{
			Name = unicodeEntity,
			Discord = client
		};
		return true;
	}

	/// <summary>
	///     Attempts to create an emoji object from a unicode entity.
	/// </summary>
	/// <param name="unicodeEntity">Unicode entity to create the object from.</param>
	/// <param name="emoji">Resulting <see cref="DiscordEmoji" /> object.</param>
	/// <returns>Whether the operation was successful.</returns>
	public static bool TryFromUnicode(string unicodeEntity, out DiscordEmoji emoji)
		=> TryFromUnicode(null, unicodeEntity, out emoji);

	/// <summary>
	///     Gets an emoji object from an guild emote.
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="id">Id of the emote.</param>
	/// <returns>Create <see cref="DiscordEmoji" /> object.</returns>
	public static DiscordEmoji FromGuildEmote(BaseDiscordClient client, ulong id)
	{
		if (client == null)
			throw new ArgumentNullException(nameof(client), "Client cannot be null.");

		foreach (var guild in client.Guilds.Values)
			if (guild.Emojis.TryGetValue(id, out var found))
				return found;

		throw new KeyNotFoundException("Given emote was not found.");
	}

	/// <summary>
	///     Gets an emoji object from an application emote.
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="id">Id of the emote.</param>
	/// <returns>Create <see cref="DiscordEmoji" /> object.</returns>
	public static DiscordEmoji FromApplicationEmote(BaseDiscordClient client, ulong id)
	{
		if (client == null)
			throw new ArgumentNullException(nameof(client), "Client cannot be null.");

		if (client.Emojis.TryGetValue(id, out var found))
			return found;

		throw new KeyNotFoundException("Given emote was not found.");
	}

	/// <summary>
	///     Attempts to get an emoji object from an guild emote.
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="id">Id of the emote.</param>
	/// <param name="emoji">Resulting <see cref="DiscordEmoji" /> object.</param>
	/// <returns>Whether the operation was successful.</returns>
	public static bool TryFromGuildEmote(BaseDiscordClient client, ulong id, out DiscordEmoji emoji)
	{
		if (client == null)
			throw new ArgumentNullException(nameof(client), "Client cannot be null.");

		foreach (var guild in client.Guilds.Values)
			if (guild.Emojis.TryGetValue(id, out emoji))
				return true;

		emoji = null;
		return false;
	}

	/// <summary>
	///     Gets an emoji object from emote name that includes colons (eg. :thinking:). This method also supports
	///     skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), guild emoji, as well as application
	///     emoji
	///     (still specified by :name:).
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
	/// <param name="includeGuilds">Should guild emojis be included in the search.</param>
	/// <param name="includeApplication">Should application emojis be included in the search.</param>
	/// <returns>Create <see cref="DiscordEmoji" /> object.</returns>
	public static DiscordEmoji FromName(BaseDiscordClient client, string name, bool includeGuilds = true, bool includeApplication = true)
	{
		if (client == null)
			throw new ArgumentNullException(nameof(client), "Client cannot be null.");

		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name), "Name cannot be empty or null.");


		if (s_aliasNameToCanonical.TryGetValue(name, out var canonicalName))
			name = canonicalName;

		if (s_unicodeEmojis.TryGetValue(name, out var unicodeEntity))
			return new()
			{
				Discord = client,
				Name = unicodeEntity
			};

		if (includeGuilds)
		{
			var allEmojis = client.Guilds.Values
				.SelectMany(xg => xg.Emojis.Values); // save cycles - don't order

			var ek = name.AsSpan().Slice(1, name.Length - 2);
			foreach (var emoji in allEmojis)
				if (emoji.Name.AsSpan().SequenceEqual(ek))
					return emoji;
		}

		if (includeApplication)
		{
			var ek = name.AsSpan().Slice(1, name.Length - 2);
			foreach (var emoji in client.Emojis.Values)
				if (emoji.Name.AsSpan().SequenceEqual(ek))
					return emoji;
		}

		throw new ArgumentException("Invalid emoji name specified.", nameof(name));
	}

	/// <summary>
	///     Attempts to create an emoji object from emote name that includes colons (eg. :thinking:). This method also
	///     supports skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), as well as guild
	///     emoji (still specified by :name:).
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
	/// <param name="emoji">Resulting <see cref="DiscordEmoji" /> object.</param>
	/// <returns>Whether the operation was successful.</returns>
	public static bool TryFromName(BaseDiscordClient client, string name, out DiscordEmoji emoji)
		=> TryFromName(client, name, true, true, out emoji);

	/// <summary>
	///     Attempts to get an emoji object from emote name that includes colons (eg. :thinking:). This method also
	///     supports skin tone variations (eg. :ok_hand::skin-tone-2:), standard emoticons (eg. :D), as well as guild
	///     emoji (still specified by :name:).
	/// </summary>
	/// <param name="client"><see cref="BaseDiscordClient" /> to attach to the object.</param>
	/// <param name="name">Name of the emote to find, including colons (eg. :thinking:).</param>
	/// <param name="includeGuilds">Should guild emojis be included in the search.</param>
	/// <param name="includeApplication">Should application emojis be included in the search.</param>
	/// <param name="emoji">Resulting <see cref="DiscordEmoji" /> object.</param>
	/// <returns>Whether the operation was successful.</returns>
	public static bool TryFromName(BaseDiscordClient client, string name, bool includeGuilds, bool includeApplication, out DiscordEmoji emoji)
	{
		if (client == null)
			throw new ArgumentNullException(nameof(client), "Client cannot be null.");

		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name), "Name cannot be empty or null.");

		if (s_aliasNameToCanonical.TryGetValue(name, out var canonicalName))
			name = canonicalName;

		if (s_unicodeEmojis.TryGetValue(name, out var unicodeEntity))
		{
			emoji = new()
			{
				Discord = client,
				Name = unicodeEntity
			};
			return true;
		}

		if (includeGuilds)
		{
			var allEmojis = client.Guilds.Values
				.SelectMany(xg => xg.Emojis.Values); // save cycles - don't order

			var ek = name.AsSpan().Slice(1, name.Length - 2);
			foreach (var xemoji in allEmojis)
				if (xemoji.Name.AsSpan().SequenceEqual(ek))
				{
					emoji = xemoji;
					return true;
				}
		}

		if (includeApplication)
		{
			var ek = name.AsSpan().Slice(1, name.Length - 2);
			foreach (var xemoji in client.Emojis.Values)
				if (xemoji.Name.AsSpan().SequenceEqual(ek))
				{
					emoji = xemoji;
					return true;
				}
		}

		emoji = null;
		return false;
	}
}
