namespace DisCatSharp.Voice.Enums.Dave;

/// <summary>
///     Codec type for a DAVE-framed media stream.
/// </summary>
internal enum DaveCodec
{
	/// <summary>
	///     Codec is not recognized or not specified.
	/// </summary>
	Unknown = 0,

	/// <summary>
	///     Opus audio codec — the primary DAVE voice format. The first byte (TOC byte) is left unencrypted.
	/// </summary>
	Opus = 1,

	/// <summary>
	///     VP8 video codec.
	/// </summary>
	VP8 = 2,

	/// <summary>
	///     VP9 video codec.
	/// </summary>
	VP9 = 3,

	/// <summary>
	///     H.264 (AVC) video codec.
	/// </summary>
	H264 = 4,

	/// <summary>
	///     H.265 (HEVC) video codec.
	/// </summary>
	H265 = 5,

	/// <summary>
	///     AV1 video codec.
	/// </summary>
	AV1 = 6
}
