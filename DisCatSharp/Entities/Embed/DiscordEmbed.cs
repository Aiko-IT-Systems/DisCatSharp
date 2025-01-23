using System;
using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a discord embed.
/// </summary>
public sealed class DiscordEmbed : ObservableApiObject
{
	[JsonIgnore]
	private readonly Lazy<Optional<DiscordColor>> _colorLazy;

	[JsonProperty("color", NullValueHandling = NullValueHandling.Include)]
	internal Optional<int> ColorInternal;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordEmbed" /> class.
	/// </summary>
	internal DiscordEmbed()
	{
		this._colorLazy = new(() => this.ColorInternal.Map<DiscordColor>(c => c));
	}

	/// <summary>
	///     Gets the embed's title.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	///     <para>Gets the embed's type.</para>
	///     <list type="table">
	///         <listheader>
	///             <term>Type</term>
	///             <description>Description</description>
	///         </listheader>
	///         <item>
	///             <term>rich</term>
	///             <description>generic embed rendered from embed attributes</description>
	///         </item>
	///         <item>
	///             <term>image</term>
	///             <description>image embed</description>
	///         </item>
	///         <item>
	///             <term>video</term>
	///             <description>video embed</description>
	///         </item>
	///         <item>
	///             <term>gifv</term>
	///             <description>animated gif image embed rendered as a video embed</description>
	///         </item>
	///         <item>
	///             <term>article</term>
	///             <description>article embed</description>
	///         </item>
	///         <item>
	///             <term>link</term>
	///             <description>link embed</description>
	///         </item>
	///         <item>
	///             <term>poll_result</term>
	///             <description>poll result embed</description>
	///         </item>
	///         <item>
	///             <term>auto_moderation_message</term>
	///             <description>auto moderation embed</description>
	///         </item>
	///     </list>
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; internal set; }

	/// <summary>
	///     Gets the embed's description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	///     Gets the embed's url.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public Uri Url { get; internal set; }

	/// <summary>
	///     Gets the embed's timestamp.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? Timestamp { get; internal set; }

	/// <summary>
	///     Gets the embed's color.
	/// </summary>
	[JsonIgnore]
	public Optional<DiscordColor> Color
		=> this._colorLazy.Value;

	/// <summary>
	///     Gets the embed's footer.
	/// </summary>
	[JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmbedFooter Footer { get; internal set; }

	/// <summary>
	///     Gets the embed's image.
	/// </summary>
	[JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmbedImage Image { get; internal set; }

	/// <summary>
	///     Gets the embed's thumbnail.
	/// </summary>
	[JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmbedThumbnail Thumbnail { get; internal set; }

	/// <summary>
	///     Gets the embed's video.
	/// </summary>
	[JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmbedVideo Video { get; internal set; }

	/// <summary>
	///     Gets the embed's provider.
	/// </summary>
	[JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmbedProvider Provider { get; internal set; }

	/// <summary>
	///     Gets the embed's author.
	/// </summary>
	[JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmbedAuthor Author { get; internal set; }

	/// <summary>
	///     Gets the embed's fields.
	/// </summary>
	[JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordEmbedField> Fields { get; internal set; }

	/// <summary>
	///     Gets the embed's flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public EmbedFlags Flags { get; internal set; } = EmbedFlags.None;
}
