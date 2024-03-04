using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an attachment for a message.
/// </summary>
public class DiscordAttachment : NullableSnowflakeObject
{
	/// <summary>
	/// Gets the name of the file.
	/// </summary>
	[JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
	public string Filename { get; internal set; }

	/// <summary>
	/// Gets the description of the file.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	/// <summary>
	/// Gets the media, or MIME, type of the file.
	/// </summary>
	[JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
	public string MediaType { get; internal set; }

	/// <summary>
	/// Gets the file size in bytes.
	/// </summary>
	[JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
	public int? FileSize { get; internal set; }

	/// <summary>
	/// Gets the URL of the file.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string Url { get; internal set; }

	/// <summary>
	/// Gets the proxied URL of the file.
	/// </summary>
	[JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
	public string ProxyUrl { get; internal set; }

	/// <summary>
	/// Gets the height. Applicable only if the attachment is an image.
	/// </summary>
	[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
	public int? Height { get; internal set; }

	/// <summary>
	/// Gets the width. Applicable only if the attachment is an image.
	/// </summary>
	[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
	public int? Width { get; internal set; }

	/// <summary>
	/// Gets the uploaded filename if the attachment was uploaded via GCP.
	/// </summary>
	[JsonProperty("uploaded_filename", NullValueHandling = NullValueHandling.Ignore)]
	internal string? UploadedFilename { get; set; }

	/// <summary>
	/// Gets whether this attachment is ephemeral.
	/// Ephemeral attachments will automatically be removed after a set period of time.
	/// Ephemeral attachments on messages are guaranteed to be available as long as the message itself exists.
	/// </summary>
	[JsonProperty("ephemeral", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Ephemeral { get; internal set; }

	/// <summary>
	/// <para>The duration in seconds of the audio file (currently only for voice messages).</para>
	/// <para>Only presented when the message flags include <see cref="MessageFlags.IsVoiceMessage"/> and for the attached voice message.</para>
	/// </summary>
	[JsonProperty("duration_secs", NullValueHandling = NullValueHandling.Ignore)]
	public float? DurationSecs { get; internal set; }

	/// <summary>
	/// <para>The base64 encoded byte-array representing a sampled waveform (currently only for voice messages).</para>
	/// <para>Only presented when the message flags include <see cref="MessageFlags.IsVoiceMessage"/> and for the attached voice message.</para>
	/// </summary>
	[JsonProperty("waveform", NullValueHandling = NullValueHandling.Ignore)]
	public string WaveForm { get; internal set; }

	/// <summary>
	/// Gets the attachment flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public AttachmentFlags Flags { get; internal set; } = AttachmentFlags.None;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordAttachment"/> class.
	/// </summary>
	internal DiscordAttachment()
	{ }
}
