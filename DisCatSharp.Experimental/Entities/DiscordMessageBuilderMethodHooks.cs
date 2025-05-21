using System;
using System.IO;

using DisCatSharp.Entities;
using DisCatSharp.Entities.Core;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Adds methods to <see cref="DisCatSharpBuilder" />.
/// </summary>
public static class DisCatSharpBuilderMethodHooks
{
	/// <summary>
	///     Adds a <see cref="GcpAttachment" /> to the <typeparamref name="T"/>.
	/// </summary>
	/// <param name="builder">The <typeparamref name="T"/> to add the attachment to.</param>
	/// <param name="gcpAttachment">The attachment to add.</param>
	/// <param name="isVoice">Whether this is a voice message attachment.</param>
	/// <param name="originalStream">
	///     The voice message's stream, required if <paramref name="isVoice" /> is
	///     <see langword="true" />.
	/// </param>
	/// <typeparam name="T">The builder type.</typeparam>
	/// <returns>The chained <typeparamref name="T"/>.</returns>
	public static T AddGcpAttachment<T>(this T builder, GcpAttachmentUploadInformation gcpAttachment, bool isVoice = false, Stream? originalStream = null) where T : DisCatSharpBuilder
	{
		builder.AttachmentsInternal ??= [];
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
			builder.IsVoiceMessage = true;
		}

		return builder;
	}

	
	/// <summary>
	///     Adds a manual GCP (Google Cloud Platform) voice message attachment to the <typeparamref name="T"/>.
	/// </summary>
	/// <param name="builder">The <typeparamref name="T"/> to add the attachment to.</param>
	/// <param name="filename">The original filename of the attachment.</param>
	/// <param name="uploadedFilename">The filename assigned after upload to GCP.</param>
	/// <param name="durationSeconds">The duration of the voice message in seconds.</param>
	/// <param name="waveform">A byte-array representing the waveform data for the voice message.</param>
	/// <param name="description">An optional description for the attachment.</param>
	/// <typeparam name="T">The builder type.</typeparam>
	/// <returns>The chained <typeparamref name="T"/>.</returns>
	public static T AddManualGcpAttachment<T>(this T builder, string filename, string uploadedFilename, float durationSeconds, byte[] waveform, string? description = null) where T : DisCatSharpBuilder
	{
		builder.AttachmentsInternal ??= [];
		builder.AttachmentsInternal.Add(new()
			{
				Filename = filename,
				UploadedFilename = uploadedFilename,
				Description = description,
				DurationSecs = durationSeconds,
				WaveForm = waveform
			});
			builder.IsVoiceMessage = true;

		return builder;
	}
	/// <summary>
	///     Adds a manual voice message attachment to the <typeparamref name="T"/>.
	/// </summary>
	/// <param name="builder">The <typeparamref name="T"/> to add the attachment to.</param>
	/// <param name="filename">The original filename of the attachment.</param>
	/// <param name="durationSeconds">The duration of the voice message in seconds.</param>
	/// <param name="waveform">A byte-array representing the waveform data for the voice message.</param>
	/// <param name="description">An optional description for the attachment.</param>
	/// <typeparam name="T">The builder type.</typeparam>
	/// <returns>The chained <typeparamref name="T"/>.</returns>
	public static T AddVoiceMessage<T>(this T builder, string filename, float durationSeconds, byte[] waveform, string? description = null) where T : DisCatSharpBuilder
	{
		builder.AttachmentsInternal ??= [];
		builder.AttachmentsInternal.Add(new()
			{
				Filename = filename,
				Description = description,
				DurationSecs = durationSeconds,
				WaveForm = waveform
			});
			builder.IsVoiceMessage = true;

		return builder;
	}
}
