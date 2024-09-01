using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Adds methods to <see cref="DiscordChannel" />
/// </summary>
public static class DiscordChannelMethodHooks
{
	/// <summary>
	///     Uploads a file to the channel for later use in a <see cref="DiscordMessageBuilder" />.
	/// </summary>
	/// <param name="channel">The channel to upload the file to.</param>
	/// <param name="name">The name of the file to upload.</param>
	/// <param name="stream">The stream of the file to upload.</param>
	/// <param name="description">The description of the file to upload.</param>
	/// <returns>The upload information.</returns>
	public static async Task<GcpAttachmentUploadInformation> UploadFileAsync(this DiscordChannel channel, string name, Stream stream, string? description = null)
	{
		DiscordApiClientHook hook = new(channel.Discord.ApiClient);
		GcpAttachment attachment = new(name, stream);
		var response = await hook.RequestFileUploadAsync(channel.Id, attachment).ConfigureAwait(false);
		var target = response.Attachments.First();
		hook.UploadGcpFile(target, stream);
		target.Filename = name;
		target.Description = description;
		return target;
	}
}
