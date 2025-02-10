using System.Collections.Generic;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Extension methods for <see cref="ChannelType" />.
/// </summary>
public static class ChannelTypeExtensions
{
	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_callable = [1, 3];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_textual = [0, 1, 2, 3, 5, 10, 11, 12, 13];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildThreadsOnly = [15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_stickers = [0, 1, 2, 3, 5, 10, 11, 12, 13, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_readable = [0, 1, 2, 3, 5, 10, 11, 12, 13, 14, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guild = [0, 2, 4, 5, 6, 10, 11, 12, 13, 14, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_threads = [10, 11, 12];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_publicThreads = [10, 11];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildThreaded = [0, 5, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildStored = [0, 2, 4, 5, 6, 13, 14, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildTextual = [0, 2, 5, 10, 11, 12, 13];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildVocal = [2, 13];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_vocalThread = [11, 12];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_vocal = [1, 2, 3, 11, 12, 13];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_voiceEffects = [1, 2, 3, 11, 12];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildTextOnly = [0, 5, 10, 11, 12];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_limitedChannelName = [0, 5, 10, 11, 12, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_searchable = [0, 1, 2, 3, 5, 10, 11, 12, 13, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildUserContent = [0, 2, 5, 10, 11, 12, 13, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildTopical = [0, 5, 13, 14, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildWebhooks = [0, 2, 5, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildSystemChannel = [0, 5];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildParentable = [0, 2, 5, 10, 11, 12, 13, 14, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildAutoModerated = [0, 2, 5, 10, 11, 12, 13, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildBasic = [0, 2, 4];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_createableGuildChannels = [0, 2, 4, 5, 6, 13, 14, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_multiUserDms = [3];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_allDms = [1, 3];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_invitable = [0, 2, 3, 5, 6, 13, 14, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_guildFeedFeaturableMessages = [0, 5, 11];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_roleSubscriptions = [0, 2, 5, 13, 15, 16];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_iconEmojis = [0, 2, 5, 13, 15];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_summarizeable = [0];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_contentEntryEmbeds = [0, 1, 5];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_polls = [0, 1, 2, 3, 5, 10, 11, 12, 13];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_activityLaunchable = [0, 1, 2, 3];

	/// <summary>
	///     Contains sets used by Discord to determine features of a channel.
	/// </summary>
	private static readonly HashSet<int> s_all = [0, 1, 2, 3, 4, 5, 6, 10, 11, 12, 13, 14, 15, 16];

	/// <summary>
	///     Determines if the channel type is callable.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is callable, otherwise false.</returns>
	public static bool IsCallable(this ChannelType type)
		=> s_callable.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is textual.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is textual, otherwise false.</returns>
	public static bool IsTextual(this ChannelType type)
		=> s_textual.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is guild threads only.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is guild threads only, otherwise false.</returns>
	public static bool IsGuildThreadsOnly(this ChannelType type)
		=> s_guildThreadsOnly.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports stickers.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports stickers, otherwise false.</returns>
	public static bool IsStickers(this ChannelType type)
		=> s_stickers.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is readable.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is readable, otherwise false.</returns>
	public static bool IsReadable(this ChannelType type)
		=> s_readable.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild channel, otherwise false.</returns>
	public static bool IsGuild(this ChannelType type)
		=> s_guild.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a thread.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a thread, otherwise false.</returns>
	public static bool IsThreads(this ChannelType type)
		=> s_threads.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a public thread.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a public thread, otherwise false.</returns>
	public static bool IsPublicThreads(this ChannelType type)
		=> s_publicThreads.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild threaded channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild threaded channel, otherwise false.</returns>
	public static bool IsGuildThreaded(this ChannelType type)
		=> s_guildThreaded.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild stored channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild stored channel, otherwise false.</returns>
	public static bool IsGuildStored(this ChannelType type)
		=> s_guildStored.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild textual channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild textual channel, otherwise false.</returns>
	public static bool IsGuildTextual(this ChannelType type)
		=> s_guildTextual.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild vocal channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild vocal channel, otherwise false.</returns>
	public static bool IsGuildVocal(this ChannelType type)
		=> s_guildVocal.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a vocal thread.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a vocal thread, otherwise false.</returns>
	public static bool IsVocalThread(this ChannelType type)
		=> s_vocalThread.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is vocal.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is vocal, otherwise false.</returns>
	public static bool IsVocal(this ChannelType type)
		=> s_vocal.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports voice effects.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports voice effects, otherwise false.</returns>
	public static bool IsVoiceEffects(this ChannelType type)
		=> s_voiceEffects.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild text only channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild text only channel, otherwise false.</returns>
	public static bool IsGuildTextOnly(this ChannelType type)
		=> s_guildTextOnly.Contains((int)type);

	/// <summary>
	///     Determines if the channel type has a limited channel name.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type has a limited channel name, otherwise false.</returns>
	public static bool IsLimitedChannelName(this ChannelType type)
		=> s_limitedChannelName.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is searchable.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is searchable, otherwise false.</returns>
	public static bool IsSearchable(this ChannelType type)
		=> s_searchable.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports guild user content.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports guild user content, otherwise false.</returns>
	public static bool IsGuildUserContent(this ChannelType type)
		=> s_guildUserContent.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild topical channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild topical channel, otherwise false.</returns>
	public static bool IsGuildTopical(this ChannelType type)
		=> s_guildTopical.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports guild webhooks.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports guild webhooks, otherwise false.</returns>
	public static bool IsGuildWebhooks(this ChannelType type)
		=> s_guildWebhooks.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild system channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild system channel, otherwise false.</returns>
	public static bool IsGuildSystemChannel(this ChannelType type)
		=> s_guildSystemChannel.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild parentable channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild parentable channel, otherwise false.</returns>
	public static bool IsGuildParentable(this ChannelType type)
		=> s_guildParentable.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is guild auto moderated.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is guild auto moderated, otherwise false.</returns>
	public static bool IsGuildAutoModerated(this ChannelType type)
		=> s_guildAutoModerated.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a guild basic channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a guild basic channel, otherwise false.</returns>
	public static bool IsGuildBasic(this ChannelType type)
		=> s_guildBasic.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a createable guild channel.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a createable guild channel, otherwise false.</returns>
	public static bool IsCreateableGuildChannels(this ChannelType type)
		=> s_createableGuildChannels.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a multi-user DM.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a multi-user DM, otherwise false.</returns>
	public static bool IsMultiUserDms(this ChannelType type)
		=> s_multiUserDms.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is a DM.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is a DM, otherwise false.</returns>
	public static bool IsAllDms(this ChannelType type)
		=> s_allDms.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is invitable.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is invitable, otherwise false.</returns>
	public static bool IsInvitable(this ChannelType type)
		=> s_invitable.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports guild feed featurable messages.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports guild feed featurable messages, otherwise false.</returns>
	public static bool IsGuildFeedFeaturableMessages(this ChannelType type)
		=> s_guildFeedFeaturableMessages.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports role subscriptions.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports role subscriptions, otherwise false.</returns>
	public static bool IsRoleSubscriptions(this ChannelType type)
		=> s_roleSubscriptions.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports icon emojis.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports icon emojis, otherwise false.</returns>
	public static bool IsIconEmojis(this ChannelType type)
		=> s_iconEmojis.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is summarizeable.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is summarizeable, otherwise false.</returns>
	public static bool IsSummarizeable(this ChannelType type)
		=> s_summarizeable.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports content entry embeds.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports content entry embeds, otherwise false.</returns>
	public static bool IsContentEntryEmbeds(this ChannelType type) => s_contentEntryEmbeds.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports polls.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports polls, otherwise false.</returns>
	public static bool IsPolls(this ChannelType type)
		=> s_polls.Contains((int)type);

	/// <summary>
	///     Determines if the channel type supports activity launch.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type supports activity launch, otherwise false.</returns>
	public static bool IsActivityLaunchable(this ChannelType type)
		=> s_activityLaunchable.Contains((int)type);

	/// <summary>
	///     Determines if the channel type is any type. Not really useful, but here for completeness.
	/// </summary>
	/// <param name="type">The channel type.</param>
	/// <returns>True if the channel type is any type, otherwise false.</returns>
	public static bool IsAll(this ChannelType type)
		=> s_all.Contains((int)type);
}
