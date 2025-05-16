using System;
using System.IO;
using System.Text;

using DisCatSharp.Entities;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Adds methods to <see cref="DiscordMessageBuilder" />.
/// </summary>
public static class DiscordMessageBuilderMethodHooks
{
	/// <summary>
	///     Adds a <see cref="GcpAttachment" /> to the <see cref="DiscordMessageBuilder" />.
	/// </summary>
	/// <param name="builder">The  <see cref="DiscordMessageBuilder" /> to add the attachment to.</param>
	/// <param name="gcpAttachment">The attachment to add.</param>
	/// <param name="isVoice">Whether this is a voice message attachment.</param>
	/// <param name="originalStream">
	///     The voice message's stream, required if <paramref name="isVoice" /> is
	///     <see langword="true" />.
	/// </param>
	/// <returns>The chained <see cref="DiscordMessageBuilder" />.</returns>
	public static DiscordMessageBuilder AddGcpAttachment(this DiscordMessageBuilder builder, GcpAttachmentUploadInformation gcpAttachment, bool isVoice = false, Stream? originalStream = null)
	{
		if (!isVoice)
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description
			});
		else
		{
			ArgumentNullException.ThrowIfNull(originalStream, nameof(originalStream));
			var (durationSeconds, waveform) = originalStream.GetDurationAndWaveformBytes();
			Console.WriteLine($"Waveform length: {waveform.Length} bytes");
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description,
				DurationSecs = durationSeconds,
				WaveForm = waveform
			});
			builder.AsVoiceMessage();
		}

		return builder;
	}

	
	/// <summary>
	///     Adds a manual GCP (Google Cloud Platform) voice message attachment to the <see cref="DiscordMessageBuilder" />.
	/// </summary>
	/// <param name="builder">The <see cref="DiscordMessageBuilder" /> to add the attachment to.</param>
	/// <param name="filename">The original filename of the attachment.</param>
	/// <param name="uploadedFilename">The filename assigned after upload to GCP.</param>
	/// <param name="durationSeconds">The duration of the voice message in seconds.</param>
	/// <param name="description">An optional description for the attachment.</param>
	/// <param name="waveform">A string representing the waveform data for the voice message, encoded as UTF-8 bytes.</param>
	/// <returns>The chained <see cref="DiscordMessageBuilder" />.</returns>
	public static DiscordMessageBuilder AddManualGcpAttachment(this DiscordMessageBuilder builder, string filename, string uploadedFilename, float durationSeconds, string? description = null, string waveform = "")
	{
		builder.AttachmentsInternal.Add(new()
			{
				Filename = filename,
				UploadedFilename = uploadedFilename,
				Description = description,
				DurationSecs = durationSeconds,
				WaveForm = Encoding.UTF8.GetBytes(waveform)
			});
			builder.AsVoiceMessage();

		return builder;
	}

	/// <summary>
	///     Adds a <see cref="GcpAttachment" /> to the <see cref="DiscordInteractionResponseBuilder" />.
	/// </summary>
	/// <param name="builder">The  <see cref="DiscordInteractionResponseBuilder" /> to add the attachment to.</param>
	/// <param name="gcpAttachment">The attachment to add.</param>
	/// <returns>The chained <see cref="DiscordInteractionResponseBuilder" />.</returns>
	public static DiscordInteractionResponseBuilder AddGcpAttachment(this DiscordInteractionResponseBuilder builder, GcpAttachmentUploadInformation gcpAttachment)
	{
		var isVoice = false;
		Stream? originalStream = null;
		if (!isVoice)
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description
			});
		else
		{
			ArgumentNullException.ThrowIfNull(originalStream, nameof(originalStream));
			var (durationSeconds, waveform) = originalStream.GetDurationAndWaveformBytes();
			Console.WriteLine($"Waveform length: {waveform.Length} bytes");
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description,
				DurationSecs = durationSeconds,
				WaveForm = waveform
			});
			builder.AsVoiceMessage();
		}

		return builder;
	}

	/// <summary>
	///     Adds a <see cref="GcpAttachment" /> to the <see cref="DiscordWebhookBuilder" />.
	/// </summary>
	/// <param name="builder">The  <see cref="DiscordWebhookBuilder" /> to add the attachment to.</param>
	/// <param name="gcpAttachment">The attachment to add.</param>
	/// <returns>The chained <see cref="DiscordWebhookBuilder" />.</returns>
	public static DiscordWebhookBuilder AddGcpAttachment(this DiscordWebhookBuilder builder, GcpAttachmentUploadInformation gcpAttachment)
	{
		var isVoice = false;
		Stream? originalStream = null;
		if (!isVoice)
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description
			});
		else
		{
			ArgumentNullException.ThrowIfNull(originalStream, nameof(originalStream));
			var (durationSeconds, waveform) = originalStream.GetDurationAndWaveformBytes();
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description,
				DurationSecs = durationSeconds,
				WaveForm = waveform
			});
			builder.AsVoiceMessage();
		}

		return builder;
	}

	/// <summary>
	///     Adds a <see cref="GcpAttachment" /> to the <see cref="DiscordFollowupMessageBuilder" />.
	/// </summary>
	/// <param name="builder">The  <see cref="DiscordFollowupMessageBuilder" /> to add the attachment to.</param>
	/// <param name="gcpAttachment">The attachment to add.</param>
	/// <returns>The chained <see cref="DiscordFollowupMessageBuilder" />.</returns>
	public static DiscordFollowupMessageBuilder AddGcpAttachment(this DiscordFollowupMessageBuilder builder, GcpAttachmentUploadInformation gcpAttachment)
	{
		var isVoice = false;
		Stream? originalStream = null;
		if (!isVoice)
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description
			});
		else
		{
			ArgumentNullException.ThrowIfNull(originalStream, nameof(originalStream));
			var (durationSeconds, waveform) = originalStream.GetDurationAndWaveformBytes();
			builder.AttachmentsInternal.Add(new()
			{
				Filename = gcpAttachment.Filename,
				UploadedFilename = gcpAttachment.UploadFilename,
				Description = gcpAttachment.Description,
				DurationSecs = durationSeconds,
				WaveForm = waveform
			});
			builder.AsVoiceMessage();
		}

		return builder;
	}
}
