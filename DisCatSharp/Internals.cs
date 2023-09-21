using System.Collections.Generic;
using System.Text;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
/// Internal tools.
/// </summary>
public static class Internals
{
	/// <summary>
	/// Gets the version of the library
	/// </summary>
	private static string s_versionHeader
		=> Utilities.VersionHeader;

	/// <summary>
	/// Gets the permission strings.
	/// </summary>
	private static Dictionary<Permissions, string> s_permissionStrings
		=> Utilities.PermissionStrings;

	/// <summary>
	/// Gets the utf8 encoding
	/// </summary>
	internal static UTF8Encoding Utf8
		=> Utilities.UTF8;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is joinable via voice.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsVoiceJoinable(this DiscordChannel channel)
		=> channel.Type
			   is ChannelType.Voice
			   or ChannelType.Stage;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> can have threads.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsThreadHolder(this DiscordChannel channel)
		=> channel.Type
			   is ChannelType.Text
			   or ChannelType.News
			   or ChannelType.Forum;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is related to threads.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsThread(this DiscordChannel channel)
		=> channel.Type
			   is ChannelType.PublicThread
			   or ChannelType.PrivateThread
			   or ChannelType.NewsThread;

	/// <summary>
	/// Whether users can write the <see cref="DiscordChannel"/>.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsWritable(this DiscordChannel channel)
		=> channel.Type
			   is ChannelType.PublicThread
			   or ChannelType.PrivateThread
			   or ChannelType.NewsThread
			   or ChannelType.Text
			   or ChannelType.News
			   or ChannelType.Group
			   or ChannelType.Private
			   or ChannelType.Voice;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is moveable in a parent.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsMovableInParent(this DiscordChannel channel)
		=> channel.Type
			   is ChannelType.Voice
			   or ChannelType.Stage
			   or ChannelType.Text
			   or ChannelType.Forum
			   or ChannelType.News;

	/// <summary>
	/// Whether the <see cref="DiscordChannel"/> is moveable.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal static bool IsMovable(this DiscordChannel channel)
		=> channel.Type == ChannelType.Category || channel.IsMovableInParent();
}
