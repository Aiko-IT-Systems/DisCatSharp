using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an attachment for a message.
/// </summary>
public class DiscordAttachment : NullableSnowflakeObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordAttachment" /> class.
	/// </summary>
	internal DiscordAttachment()
	{ }

	/// <summary>
	///     Gets the name of the file.
	/// </summary>
	[JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
	public string Filename { get; internal set; }

	/// <summary>
	///     Gets the title of the file.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string? Title { get; set; }

	/// <summary>
	///     Gets the description of the file.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	/// <summary>
	///     Gets the media, or MIME, type of the file.
	/// </summary>
	[JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
	public string MediaType { get; internal set; }

	/// <summary>
	///     Gets the file size in bytes.
	/// </summary>
	[JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
	public int? FileSize { get; internal set; }

	/// <summary>
	///     Gets the URL of the file.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string Url { get; internal set; }

	/// <summary>
	///     Gets the proxied URL of the file.
	/// </summary>
	[JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
	public string ProxyUrl { get; internal set; }

	/// <summary>
	///     Gets the height. Applicable only if the attachment is an image.
	/// </summary>
	[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
	public int? Height { get; internal set; }

	/// <summary>
	///     Gets the width. Applicable only if the attachment is an image.
	/// </summary>
	[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
	public int? Width { get; internal set; }

	/// <summary>
	///     Gets the uploaded filename if the attachment was uploaded via GCP.
	/// </summary>
	[JsonProperty("uploaded_filename", NullValueHandling = NullValueHandling.Ignore)]
	internal string? UploadedFilename { get; set; }

	/// <summary>
	///     Gets whether this attachment is ephemeral.
	///     Ephemeral attachments will automatically be removed after a set period of time.
	///     Ephemeral attachments on messages are guaranteed to be available as long as the message itself exists.
	/// </summary>
	[JsonProperty("ephemeral", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Ephemeral { get; internal set; }

	/// <summary>
	///     <para>The duration in seconds of the audio file (currently only for voice messages).</para>
	///     <para>
	///         Only presented when the message flags include <see cref="MessageFlags.IsVoiceMessage" /> and for the attached
	///         voice message.
	///     </para>
	/// </summary>
	[JsonProperty("duration_secs", NullValueHandling = NullValueHandling.Ignore)]
	public float? DurationSecs { get; internal set; }

	/// <summary>
	///     <para>The base64 encoded byte-array representing a sampled waveform (currently only for voice messages).</para>
	///     <para>
	///         Only presented when the message flags include <see cref="MessageFlags.IsVoiceMessage" /> and for the attached
	///         voice message.
	///     </para>
	/// </summary>
	[JsonProperty("waveform", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(WaveformConverter))]
	public byte[]? WaveForm { get; internal set; }

	/// <summary>
	///     Gets the attachment flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public AttachmentFlags Flags { get; internal set; } = AttachmentFlags.None;

	/// <summary>
	///     Gets the clip participant, if applicable.
	/// </summary>
	[JsonIgnore]
	public List<DiscordUser>? ClipParticipants
		=> this.ClipParticipantsInternal?.Select(part => new DiscordUser(part)).ToList();

	/// <summary>
	///     Gets the clip participant, if applicable.
	/// </summary>
	[JsonProperty("clip_participants", NullValueHandling = NullValueHandling.Ignore)]
	internal List<TransportUser>? ClipParticipantsInternal { get; set; }

	/// <summary>
	///     Gets the clip's creation timestamp.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? ClipCreatedAt
		=> !string.IsNullOrWhiteSpace(this.ClipCreatedAtRaw) && DateTimeOffset.TryParse(this.ClipCreatedAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets the clip's creation timestamp as raw string.
	/// </summary>
	[JsonProperty("clip_created_at", NullValueHandling = NullValueHandling.Ignore)]
	internal string? ClipCreatedAtRaw { get; set; }

	/// <summary>
	///     Gets the clip application, if applicable and recognized.
	/// </summary>
	[JsonIgnore]
	public DiscordApplication? Application
		=> this.ApplicationInternal is not null ? new DiscordApplication(this.ApplicationInternal) : null;

	/// <summary>
	///     Gets the clip application, if applicable and recognized.
	/// </summary>
	[JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
	internal TransportApplication? ApplicationInternal { get; set; }

	/// <summary>
	///     Visualizes the <see cref="WaveForm" /> as image.
	/// </summary>
	/// <param name="colorful">Whether to use a colorful image. Defaults to <see langword="true" />.</param>
	/// <returns>
	///     A waveform visualizer object, or <see langword="nulL" /> if <see cref="WaveForm" /> is <see langword="nulL" />
	///     .
	/// </returns>
	public WaveformVisualizer? VisualizeWaveForm(bool colorful = true)
		=> this.WaveForm is not null
			? colorful
				? new WaveformVisualizer().WithWaveformByteData(this.WaveForm).CreateColorfulWaveformImage()
				: new WaveformVisualizer().WithWaveformByteData(this.WaveForm).CreateWaveformImage()
			: null;
}
