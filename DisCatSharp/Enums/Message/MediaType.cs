namespace DisCatSharp.Enums;

/// <summary>
///     Represents a categorized media type for attachments.
/// </summary>
public enum MediaType
{
	/// <summary>
	///     Unknown or unrecognized media type.
	/// </summary>
	Unknown,

	/// <summary>
	///     Media type for Discord voice messages (special case, e.g., voice_message.ogg).
	/// </summary>
	VoiceMessage,

	/// <summary>
	///     Media type for application data (e.g., application/json, application/pdf).
	/// </summary>
	Application,

	/// <summary>
	///     Media type for audio data (e.g., audio/mpeg, audio/ogg).
	/// </summary>
	Audio,

	/// <summary>
	///     Media type for font data (e.g., font/woff, font/ttf).
	/// </summary>
	Font,

	/// <summary>
	///     Media type for haptics data (e.g., haptics/ivs).
	/// </summary>
	Haptics,

	/// <summary>
	///     Media type for image data (e.g., image/png, image/jpeg).
	/// </summary>
	Image,

	/// <summary>
	///     Media type for model data (e.g., model/gltf+json, model/3mf).
	/// </summary>
	Model,

	/// <summary>
	///     Media type for message data (e.g., message/rfc822).
	/// </summary>
	Message,

	/// <summary>
	///     Media type for multipart data (e.g., multipart/form-data).
	/// </summary>
	Multipart,

	/// <summary>
	///     Media type for text data (e.g., text/plain, text/html).
	/// </summary>
	Text,

	/// <summary>
	///     Media type for video data (e.g., video/mp4, video/webm).
	/// </summary>
	Video
}
