using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a partial emoji to be send to discord.
/// </summary>
public sealed class PartialEmoji
{
	/// <summary>
	/// Sets the guild emoji.
	/// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? Id { get; internal set; }

    /// <summary>
    /// Sets the unicode emoji.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
    public string? Name { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="PartialEmoji"/> from a guild emoji id.
	/// </summary>
	/// <param name="id">The emojis id.</param>
	public PartialEmoji(ulong id)
	{
		this.Id = id;
		this.Name = null;
	}

	/// <summary>
	/// Constructs a new <see cref="PartialEmoji"/> from an unicode emoji.
	/// </summary>
	/// <param name="name">The unicode. I.e. 😀</param>
	public PartialEmoji(string name)
	{
		this.Id = null;
		this.Name = name;
	}

	/// <summary>
	/// Constructs a new <see cref="PartialEmoji"/> from a <see cref="DiscordEmoji"/>
	/// </summary>
	/// <param name="discordEmoji"></param>
	public PartialEmoji(DiscordEmoji discordEmoji)
	{
		if (discordEmoji.Id is 0)
		{
			this.Id = null;
			this.Name = discordEmoji.UnicodeEmoji;
		}
		else
		{
			this.Id = discordEmoji.Id;
			this.Name = null;
		}
	}

	/// <summary>
	/// Creates a new empty <see cref="PartialEmoji"/>
	/// </summary>
	internal PartialEmoji() { }

	/// <summary>
	/// Implicitly converts <see cref="DiscordEmoji"/> to <see cref="PartialEmoji"/>.
	/// </summary>
	/// <param name="discordEmoji"></param>
	public static implicit operator PartialEmoji(DiscordEmoji discordEmoji)
		=> new(discordEmoji);
}
