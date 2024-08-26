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
	/// <returns>The chained <see cref="DiscordMessageBuilder" />.</returns>
	public static DiscordMessageBuilder AddGcpAttachment(this DiscordMessageBuilder builder, GcpAttachmentUploadInformation gcpAttachment)
	{
		builder.AttachmentsInternal.Add(new()
		{
			Filename = gcpAttachment.Filename,
			UploadedFilename = gcpAttachment.UploadFilename,
			Description = gcpAttachment.Description
		});

		return builder;
	}
}
