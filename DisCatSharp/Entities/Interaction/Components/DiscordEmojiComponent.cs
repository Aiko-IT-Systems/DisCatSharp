using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an emoji to add to a component.
/// </summary>
public sealed class DiscordComponentEmoji : ObservableApiObject
{
	/// <summary>
	///     Constructs a new component emoji to add to a <see cref="DiscordComponent" />.
	/// </summary>
	public DiscordComponentEmoji()
	{ }

	/// <summary>
	///     Constructs a new component emoji from an emoji Id.
	/// </summary>
	/// <param name="id">The Id of the emoji to use. Any valid emoji Id can be passed.</param>
	public DiscordComponentEmoji(ulong id)
	{
		this.Id = id;
	}

	/// <summary>
	///     Constructs a new component emoji from unicode.
	/// </summary>
	/// <param name="name">The unicode emoji to set.</param>
	public DiscordComponentEmoji(string name)
	{
		if (!DiscordEmoji.IsValidUnicode(name))
			throw new ArgumentException("Only unicode emojis can be passed.");

		this.Name = name;
	}

	/// <summary>
	///     Constructs a new component emoji from an existing <see cref="DiscordEmoji" />.
	/// </summary>
	/// <param name="emoji">The emoji to use.</param>
	public DiscordComponentEmoji(DiscordEmoji emoji)
	{
		this.Id = emoji.Id;
		this.Name = emoji.Name;
	}

	/// <summary>
	///     The Id of the emoji to use.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; set; }

	/// <summary>
	///     The name of the emoji to use. Ignored if <see cref="Id" /> is set.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }
}
