// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents permission methods.
/// </summary>
public static class PermissionMethods
{
	// TODO: Update FullPerms value.
	/// <summary>
	/// Gets the full permissions enum (long).
	/// </summary>
	internal static Permissions FullPerms { get; } = (Permissions)4398046511103L;

	/// <summary>
	/// Calculates whether this permission set contains the given permission.
	/// </summary>
	/// <param name="p">The permissions to calculate from.</param>
	/// <param name="permission">The permission you want to check.</param>
	/// <returns></returns>
	public static bool HasPermission(this Permissions p, Permissions permission)
		=> p.HasFlag(Permissions.Administrator) || (p & permission) == permission;

	/// <summary>
	/// Grants permissions.
	/// </summary>
	/// <param name="p">The permissions to add to.</param>
	/// <param name="grant">The permission to add.</param>
	/// <returns></returns>
	public static Permissions Grant(this Permissions p, Permissions grant) => p | grant;

	/// <summary>
	/// Revokes permissions.
	/// </summary>
	/// <param name="p">The permissions to take from.</param>
	/// <param name="revoke">The permission to take.</param>
	/// <returns></returns>
	public static Permissions Revoke(this Permissions p, Permissions revoke) => p & ~revoke;
}

/// <summary>
/// Whether a permission is allowed, denied or unset
/// </summary>
public enum PermissionLevel
{
	/// <summary>
	/// Said permission is Allowed
	/// </summary>
	Allowed,

	/// <summary>
	/// Said permission is Denied
	/// </summary>
	Denied,

	/// <summary>
	/// Said permission is Unset
	/// </summary>
	Unset
}

/// <summary>
/// Bitwise permission flags.
/// </summary>
[Flags]
public enum Permissions : long
{
	/// <summary>
	/// Indicates no permissions given.
	/// This disallows users to run application command per default.
	/// </summary>
	[PermissionString("No permissions")]
	None = 0,

	// TODO: Update All permission value.
	/// <summary>
	/// Indicates all permissions are granted.
	/// </summary>
	[PermissionString("All permissions")]
	All = 0b0011_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111,

	/// <summary>
	/// Allows creation of instant channel invites.
	/// </summary>
	[PermissionString("Create instant invites")]
	CreateInstantInvite = 1L << 0,

	/// <summary>
	/// Allows kicking members.
	/// </summary>
	[PermissionString("Kick members")]
	KickMembers = 1L << 1,

	/// <summary>
	/// Allows banning and unbanning members.
	/// </summary>
	[PermissionString("Ban members")]
	BanMembers = 1L << 2,

	/// <summary>
	/// Enables full access on a given guild. This also overrides other permissions.
	/// </summary>
	[PermissionString("Administrator")]
	Administrator = 1L << 3,

	/// <summary>
	/// Allows managing channels.
	/// </summary>
	[PermissionString("Manage channels")]
	ManageChannels = 1L << 4,

	/// <summary>
	/// Allows managing the guild.
	/// </summary>
	[PermissionString("Manage guild")]
	ManageGuild = 1L << 5,

	/// <summary>
	/// Allows adding reactions to messages.
	/// </summary>
	[PermissionString("Add reactions")]
	AddReactions = 1L << 6,

	/// <summary>
	/// Allows viewing audit log entries.
	/// </summary>
	[PermissionString("View audit log")]
	ViewAuditLog = 1L << 7,

	/// <summary>
	/// Allows the use of priority speaker.
	/// </summary>
	[PermissionString("Use priority speaker")]
	PrioritySpeaker = 1L << 8,

	/// <summary>
	/// Allows the user to go live.
	/// </summary>
	[PermissionString("Allow stream")]
	Stream  = 1L << 9,

	/// <summary>
	/// Allows accessing text and voice channels. Disabling this permission hides channels.
	/// </summary>
	[PermissionString("Read messages")]
	AccessChannels = 1L << 10,

	/// <summary>
	/// Allows sending messages (does not allow sending messages in threads).
	/// </summary>
	[PermissionString("Send messages")]
	SendMessages = 1L << 11,

	/// <summary>
	/// Allows sending text-to-speech messages.
	/// </summary>
	[PermissionString("Send TTS messages")]
	SendTtsMessages = 1L << 12,

	/// <summary>
	/// Allows managing messages of other users.
	/// </summary>
	[PermissionString("Manage messages")]
	ManageMessages = 1L << 13,

	/// <summary>
	/// Allows embedding content in messages.
	/// </summary>
	[PermissionString("Use embeds")]
	EmbedLinks = 1L << 14,

	/// <summary>
	/// Allows uploading files.
	/// </summary>
	[PermissionString("Attach files")]
	AttachFiles = 1L << 15,

	/// <summary>
	/// Allows reading message history.
	/// </summary>
	[PermissionString("Read message history")]
	ReadMessageHistory = 1L << 16,

	/// <summary>
	/// Allows using @everyone and @here mentions.
	/// </summary>
	[PermissionString("Mention everyone")]
	MentionEveryone = 1L << 17,

	/// <summary>
	/// Allows using emojis from external servers, such as twitch or nitro emojis.
	/// </summary>
	[PermissionString("Use external emojis")]
	UseExternalEmojis = 1L << 18,

	/// <summary>
	/// Allows to view guild insights.
	/// </summary>
	[PermissionString("View guild insights")]
	ViewGuildInsights = 1L << 19,

	/// <summary>
	/// Allows connecting to voice chat.
	/// </summary>
	[PermissionString("Use voice chat")]
	UseVoice = 1L << 20,

	/// <summary>
	/// Allows speaking in voice chat.
	/// </summary>
	[PermissionString("Speak")]
	Speak = 1L << 21,

	/// <summary>
	/// Allows muting other members in voice chat.
	/// </summary>
	[PermissionString("Mute voice chat members")]
	MuteMembers = 1L << 22,

	/// <summary>
	/// Allows deafening other members in voice chat.
	/// </summary>
	[PermissionString("Deafen voice chat members")]
	DeafenMembers = 1L << 23,

	/// <summary>
	/// Allows moving voice chat members.
	/// </summary>
	[PermissionString("Move voice chat members")]
	MoveMembers = 1L << 24,

	/// <summary>
	/// Allows using voice activation in voice chat. Revoking this will usage of push-to-talk.
	/// </summary>
	[PermissionString("Use voice activity detection")]
	UseVoiceDetection = 1L << 25,

	/// <summary>
	/// Allows changing of own nickname.
	/// </summary>
	[PermissionString("Change own nickname")]
	ChangeNickname = 1L << 26,

	/// <summary>
	/// Allows managing nicknames of other members.
	/// </summary>
	[PermissionString("Manage nicknames")]
	ManageNicknames = 1L << 27,

	/// <summary>
	/// Allows managing roles in a guild.
	/// </summary>
	[PermissionString("Manage roles")]
	ManageRoles = 1L << 28,

	/// <summary>
	/// Allows managing webhooks in a guild.
	/// </summary>
	[PermissionString("Manage webhooks")]
	ManageWebhooks = 1L << 29,

	/// <summary>
	/// Allows managing guild emojis, stickers and soundboard sounds.
	/// </summary>
	[PermissionString("Manage emojis, stickers & soundboard sounds")]
	ManageGuildExpressions = 1L << 30,

	/// <summary>
	/// Allows the user to use slash commands.
	/// </summary>
	[PermissionString("Use application commands")]
	UseApplicationCommands = 1L << 31,

	/// <summary>
	/// Allows for requesting to speak in stage channels.
	/// </summary>
	[PermissionString("Request to speak")]
	RequestToSpeak = 1L << 32,

	/// <summary>
	/// Allows managing guild events.
	/// </summary>
	[PermissionString("Manage Events")]
	ManageEvents = 1L << 33,

	/// <summary>
	/// Allows for deleting and archiving threads, and viewing all private threads.
	/// </summary>
	[PermissionString("Manage Threads")]
	ManageThreads = 1L << 34,

	/// <summary>
	/// Allows for creating threads.
	/// </summary>
	[PermissionString("Create Public Threads")]
	CreatePublicThreads = 1L << 35,

	/// <summary>
	/// Allows for creating private threads.
	/// </summary>
	[PermissionString("Create Private Threads")]
	CreatePrivateThreads = 1L << 36,

	/// <summary>
	/// Allows the usage of custom stickers from other servers.
	/// </summary>
	[PermissionString("Use external Stickers")]
	UseExternalStickers = 1L << 37,

	/// <summary>
	/// Allows for sending messages in threads.
	/// </summary>
	[PermissionString("Send messages in Threads")]
	SendMessagesInThreads = 1L << 38,

	/// <summary>
	/// Allows for launching activities (applications with the `EMBEDDED` flag) in a voice channel.
	/// </summary>
	[PermissionString("Start Embedded Activities")]
	StartEmbeddedActivities = 1L << 39,

	/// <summary>
	/// Allows to perform limited moderation actions (timeout).
	/// </summary>
	[PermissionString("Moderate Members")]
	ModerateMembers = 1L << 40,

	/// <summary>
	/// Allows to view creator monetization insights.
	/// </summary>
	[PermissionString("View Creator Monetization Insights")]
	ViewCreatorMonetizationInsights = 1L << 41,

	/// <summary>
	/// Allows to use soundboard sounds in voice channels.
	/// </summary>
	[PermissionString("Use Soundboard"), DiscordInExperiment]
	UseSoundboard = 1L << 42,

	/// <summary>
	/// Allows to create guild expressions.
	/// </summary>
	[PermissionString("Create Guild Expressions"), DiscordUnreleased]
	CreateGuildExpressions = 1L << 43,

	/// <summary>
	/// Allows to create guild events.
	/// </summary>
	[PermissionString("Create Events"), DiscordUnreleased]
	CreateEvents = 1L << 44,

	/// <summary>
	/// Allows the usage of custom soundboard sounds from other servers.
	/// </summary>
	[PermissionString("Use External Sounds"), DiscordInExperiment]
	UseExternalSounds = 1L << 45,

	/// <summary>
	/// Allows members to send voice messages.
	/// </summary>
	[PermissionString("Send Voice Messages"), DiscordInExperiment]
	SendVoiceMessages = 1L << 46
}

/// <summary>
/// Defines a readable name for this permission.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class PermissionStringAttribute : Attribute
{
	/// <summary>
	/// Gets the readable name for this permission.
	/// </summary>
	public string String { get; }

	/// <summary>
	/// Defines a readable name for this permission.
	/// </summary>
	/// <param name="str">Readable name for this permission.</param>
	public PermissionStringAttribute(string str)
	{
		this.String = str;
	}
}
