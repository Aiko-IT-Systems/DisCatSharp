using System;

using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity;

/// <summary>
///     The page.
/// </summary>
public class Page
{
	/// <summary>
	///     <para>Initializes a new instance of the <see cref="Page" /> class.</para>
	///     <para>You need to specify at least <paramref name="content" /> or <paramref name="embed" /> or both.</para>
	/// </summary>
	/// <param name="content">The content.</param>
	/// <param name="embed">The embed.</param>
	public Page(string? content = null, DiscordEmbedBuilder? embed = null)
	{
		if (string.IsNullOrEmpty(content) && embed is null)
			throw new ArgumentException("You need to specify at least content or embed or both.");

		this.Content = content;
		this.Embed = embed?.Build();
	}

	/// <summary>
	///     Gets or sets the content.
	/// </summary>
	public string? Content { get; set; }

	/// <summary>
	///     Gets or sets the embed.
	/// </summary>
	public DiscordEmbed? Embed { get; set; }
}
