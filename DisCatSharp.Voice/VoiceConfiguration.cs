using Microsoft.Extensions.DependencyInjection;

using DisCatSharp.Voice.Enums;
using DisCatSharp.Voice.Entities;

namespace DisCatSharp.Voice;

/// <summary>
///     Voice client configuration.
/// </summary>
public sealed class VoiceConfiguration
{
	/// <summary>
	///     Creates a new instance of <see cref="VoiceConfiguration" />.
	/// </summary>
	[ActivatorUtilitiesConstructor]
	public VoiceConfiguration()
	{ }

	/// <summary>
	///     Creates a new instance of <see cref="VoiceConfiguration" />, copying the properties of another configuration.
	/// </summary>
	/// <param name="other">Configuration the properties of which are to be copied.</param>
	public VoiceConfiguration(VoiceConfiguration other)
	{
		this.AudioFormat = new(other.AudioFormat.SampleRate, other.AudioFormat.ChannelCount, other.AudioFormat.VoiceApplication);
		this.EnableIncoming = other.EnableIncoming;
		this.PacketQueueSize = other.PacketQueueSize;
		this.MaxDaveProtocolVersion = other.MaxDaveProtocolVersion;
		this.EnableDebugLogging = other.EnableDebugLogging;
		this.DavePendingAudioBehavior = other.DavePendingAudioBehavior;
	}

	/// <summary>
	///     <para>Sets the audio format for Opus. This will determine the quality of the audio output.</para>
	///     <para>Defaults to <see cref="AudioFormat.Default" />.</para>
	/// </summary>
	public AudioFormat AudioFormat { internal get; set; } = AudioFormat.Default;

	/// <summary>
	///     <para>Sets whether incoming voice receiver should be enabled.</para>
	///     <para>Defaults to false.</para>
	/// </summary>
	public bool EnableIncoming { internal get; set; } = false;

	/// <summary>
	///     <para>Sets the size of the packet queue.</para>
	///     <para>Defaults to 25 or ~500ms.</para>
	/// </summary>
	public int PacketQueueSize { internal get; set; } = 25;

	/// <summary>
	///     <para>Sets the maximum DAVE protocol version this client advertises to the gateway.</para>
	///     <para>A value of 0 disables DAVE negotiation entirely (server will not initiate DAVE).</para>
	///     <para>Defaults to 1.</para>
	/// </summary>
	public int MaxDaveProtocolVersion { internal get; set; } = 1;

	/// <summary>
	///     <para>Controls whether debug and trace logs from the <c>DisCatSharp.Voice</c> project are emitted.</para>
	///     <para>Warnings and errors are always logged regardless of this value.</para>
	///     <para>Defaults to false.</para>
	/// </summary>
	public bool EnableDebugLogging { internal get; set; } = false;

	/// <summary>
	///     <para>Controls outbound audio behavior while DAVE is negotiated but not active.</para>
	///     <para>Use <see cref="DavePendingAudioBehavior.Throw"/> to fail fast when a producer attempts to send before DAVE is active.</para>
	///     <para>Defaults to <see cref="DavePendingAudioBehavior.PassThrough"/>.</para>
	/// </summary>
	public DavePendingAudioBehavior DavePendingAudioBehavior { internal get; set; } = DavePendingAudioBehavior.PassThrough;
}
