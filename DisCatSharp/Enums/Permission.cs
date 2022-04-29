// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

namespace DisCatSharp
{
	/// <summary>
	/// Represents permission methods.
	/// </summary>
	public static class PermissionMethods
	{
		/// <summary>
		/// Gets the full permissions enum (long).
		/// </summary>
		internal static Permissions FullPerms { get; } = (Permissions)2199023255551L;

		/// <summary>
		/// Calculates whether this permission set contains the given permission.
		/// </summary>
		/// <param name="p">The permissions to calculate from</param>
		/// <param name="permission">permission you want to check</param>
		/// <returns></returns>
		public static bool HasPermission(this Permissions p, Permissions permission)
			=> p.HasFlag(Permissions.Administrator) || (p & permission) == permission;

		/// <summary>
		/// Grants permissions.
		/// </summary>
		/// <param name="p">The permissions to add to.</param>
		/// <param name="grant">Permission to add.</param>
		/// <returns></returns>
		public static Permissions Grant(this Permissions p, Permissions grant) => p | grant;

		/// <summary>
		/// Revokes permissions.
		/// </summary>
		/// <param name="p">The permissions to take from.</param>
		/// <param name="revoke">Permission to take.</param>
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
		/// </summary>
		[PermissionString("No Permissions")]
		None = 0x0000000000000000,

		/// <summary>
		/// Indicates all permissions are granted
		/// </summary>
		[PermissionString("All Permissions")]
		All = 2199023255551,

		/// <summary>
		/// Includes all general server permissions.
		/// </summary>
		[PermissionString("General Server Permissions")]
		GeneralServerPermissions = ViewChannels | ManageChannels | ManageRoles |
			ManageEmojisAndStickers | ViewAuditLog | ViewGuildInsights |
			ManageWebhooks | ManageGuild,

		/// <summary>
		/// Includes all membership permissions.
		/// </summary>
		[PermissionString("Membership Permissions")]
		MembershipPermissions = CreateInstantInvite | ChangeNickname | ManageNicknames |
			KickApproveAndRejectMembers | BanMembers | TimeoutMembers,

		/// <summary>
		/// Includes all text channel permissions.
		/// </summary>
		[PermissionString("Text Channel Permissions")]
		TextChannelPermissions = SendMessagesAndCreatePosts | SendMessagesInThreadsAndPosts |
			CreatePublicThreads | CreatePrivateThreads | EmbedLinks | AttachFiles |
			AddReactions | UseExternalEmojis | UseExternalStickers | MentionEveryone |
			ManageMessages | ManageThreadsAndPosts | ReadMessageHistory | UseApplicationCommands,

		/// <summary>
		/// Includes all voice channel permissions.
		/// </summary>
		[PermissionString("Voice Channel Permissions")]
		VoiceChannelPermissions = Connect | Speak | Video | UseActivities | PrioritySpeaker |
			MuteMembers | DeafenMembers | MoveMembers,

		/// <summary>
		/// Includes all stage channel permissions.
		/// </summary>
		[PermissionString("Stage Channel Permissions")]
		StageChannelPermissions = RequestToSpeak,
		/// <summary>
		/// Includes all event permissions.
		/// </summary>
		[PermissionString("Event Permissions")]
		EventPermissions = ManageEvents,

		/// <summary>
		/// Includes all advanced permissions.
		/// </summary>
		[PermissionString("Advanced Permissions")]
		AdvancedPermissions = Administrator,

		/// <summary>
		/// Allows members to invite new people to this server.
		/// </summary>
		[PermissionString("Create Instant Invite")]
		CreateInstantInvite = 0x0000000000000001,

		/// <summary>
		/// Allows members to invite new people to this server.
		/// </summary>
		[PermissionString("Create Invite")]
		CreateInvite = CreateInstantInvite,

		/// <summary>
		/// Allows kicking members.
		/// </summary>
		[PermissionString("Kick Members"), Obsolete("Use KickApproveAndRejectMembers instead")]
		KickMembers = 0x0000000000000002,

		/// <summary>
		/// Kick will remove other members from this server.
		/// Kicked members will be able to rejoin if they have another invite.
		/// If the server enables Member Requirements, this permission enables the ability to approve or reject members who request to join.
		/// </summary>
		[PermissionString("Kick, Approve and Reject Members")]
		KickApproveAndRejectMembers = 0x0000000000000002,

		/// <summary>
		/// Allows members to permanently ban other members from this server.
		/// </summary>
		[PermissionString("Ban Members")]
		BanMembers = 0x0000000000000004,

		/// <summary>
		/// Members with this permission will have every permission and will also bypass all channel specific permissions or restrictions (for example, these members would get access to all private channels).
		/// <note type="warning">This is a dangerous permission to grant.</note>
		/// </summary>
		[PermissionString("Administrator")]
		Administrator = 0x0000000000000008,

		/// <summary>
		/// Allows members to create, edit, or delete channels.
		/// </summary>
		[PermissionString("Manage Channels")]
		ManageChannels = 0x0000000000000010,

		/// <summary>
		/// Allows members to change this server’s name, switch regions, and add bots to this server.
		/// </summary>
		[PermissionString("Manage Guild")]
		ManageGuild = 0x0000000000000020,
		
		/// <summary>
		/// Allows members to change this server’s name, switch regions, and add bots to this server.
		/// </summary>
		[PermissionString("Manage Server")]
		ManageServer = ManageGuild,

		/// <summary>
		/// Allows members to add new emoji reactions to a message.
		/// If this permission is disabled, members can still react using any existing reactions on a message.
		/// </summary>
		[PermissionString("Add Reactions")]
		AddReactions = 0x0000000000000040,

		/// <summary>
		/// Allows members to view a record of who made which changes in this server.
		/// </summary>
		[PermissionString("View Audit Log")]
		ViewAuditLog = 0x0000000000000080,

		/// <summary>
		/// Allows members to be more easily heard in voice channels.
		/// When activated, the volume of others without this permission will be automatically lowered.
		/// Priority Speaker is activated by using the Push to Talk (Priority) keybind.
		/// </summary>
		[PermissionString("Priority Speaker")]
		PrioritySpeaker = 0x0000000000000100,

		/// <summary>
		/// Allows accessing text and voice channels. Disabling this permission hides channels.
		/// </summary>
		[PermissionString("Read messages"), Obsolete("Use ViewChannels instead.")]
		AccessChannels = 0x0000000000000400,

		/// <summary>
		/// Allows members to view channels by default (excluding private channels).
		/// </summary>
		[PermissionString("View Channels")]
		ViewChannels = 0x0000000000000400,

		/// <summary>
		/// Allows sending messages (does not allow sending messages in threads).
		/// </summary>
		[PermissionString("Send Messages"), Obsolete("Use SendMessagesAndCreatePosts instead")]
		SendMessages = 0x0000000000000800,

		/// <summary>
		/// Allow members to send messages in text channels and create posts in forum channels.
		/// </summary>
		[PermissionString("Send Messages and Create Posts")]
		SendMessagesAndCreatePosts = 0x0000000000000800,

		/// <summary>
		/// Allows members to send text-to-speech messages by starting a message with /tts.
		/// These messages can be heard by anyone focused on the channel.
		/// </summary>
		[PermissionString("Send Text-to-Speech Messages")]
		SendTtsMessages = 0x0000000000001000,

		/// <summary>
		/// Allows members to delete messages by other members or pin any message.
		/// </summary>
		[PermissionString("Manage Messages")]
		ManageMessages = 0x0000000000002000,

		/// <summary>
		/// Allows links that members share to show embedded content in text channels.
		/// </summary>
		[PermissionString("Embed Links")]
		EmbedLinks = 0x0000000000004000,

		/// <summary>
		/// Allows members to upload files or media in text channels.
		/// </summary>
		[PermissionString("Attach Files")]
		AttachFiles = 0x0000000000008000,

		/// <summary>
		/// Allows members to read previous messages sent in channels.
		/// If this permission is disabled, members only see messages sent when they are online and focused on that channel.
		/// </summary>
		[PermissionString("Read Message History")]
		ReadMessageHistory = 0x0000000000010000,

		/// <summary>
		/// Allows members to use @everyone (everyone in the server) or @here (only online members in that channel).
		/// They can also @mention all roles, even if the role’s “Allow anyone to mention this role” permission is disabled.
		/// </summary>
		[PermissionString("Mention @everyone, @here and All Roles")]
		MentionEveryone = 0x0000000000020000,

		/// <summary>
		/// Allows members to use emoji from other servers, if they’re a Discord Nitro member.
		/// </summary>
		[PermissionString("Use External Emoji")]
		UseExternalEmojis = 0x0000000000040000,

		/// <summary>
		/// Allows members to join voice channels and hear others.
		/// </summary>
		[PermissionString("Use voice chat"), Obsolete("Use Connect instead")]
		UseVoice = 0x0000000000100000,
		
		/// <summary>
		/// Allows members to join voice channels and hear others.
		/// </summary>
		[PermissionString("Connect")]
		Connect = 0x0000000000100000,

		/// <summary>
		/// Allows members to talk in voice channels.
		/// If this permission is disabled, members are default muted until somebody with the “Mute Members” permission un-mutes them.
		/// </summary>
		[PermissionString("Speak")]
		Speak = 0x0000000000200000,

		/// <summary>
		/// Allows members to mute other members in voice channels for everyone.
		/// </summary>
		[PermissionString("Mute Members")]
		MuteMembers = 0x0000000000400000,

		/// <summary>
		/// Allows members to deafen other members in voice channels, which means they won’t be able to speak or hear others.
		/// </summary>
		[PermissionString("Deafen Members")]
		DeafenMembers = 0x0000000000800000,

		/// <summary>
		/// Allows members to move other members between voice channels that the member with this permission has access to.
		/// </summary>
		[PermissionString("Move Members")]
		MoveMembers = 0x0000000001000000,

		/// <summary>
		/// Allows members to speak in voice channels by simply talking.
		/// If this permission is disabled, members are required to use Push-to-talk.
		/// Good for controlling background noise or noisy members.
		/// </summary>
		[PermissionString("Use Voice Activity Detection"), Obsolete("Use UseVoiceActivity instead.")]
		UseVoiceDetection = 0x0000000002000000,
		
		/// <summary>
		/// Allows members to speak in voice channels by simply talking.
		/// If this permission is disabled, members are required to use Push-to-talk.
		/// Good for controlling background noise or noisy members.
		/// </summary>
		[PermissionString("Use Voice Activity")]
		UseVoiceActivity = 0x0000000002000000,

		/// <summary>
		/// Allows members to change their own nickname, a custom name for just this server.
		/// </summary>
		[PermissionString("Change Nickname")]
		ChangeNickname = 0x0000000004000000,

		/// <summary>
		/// Allows members to change the nicknames of other members.
		/// </summary>
		[PermissionString("Manage Nicknames")]
		ManageNicknames = 0x0000000008000000,

		/// <summary>
		/// Allows members to create new roles and edit or delete roles lower than their highest role.
		/// Also allows members to change permissions of individual channels that they have access to.
		/// </summary>
		[PermissionString("Manage Roles")]
		ManageRoles = 0x0000000010000000,

		/// <summary>
		/// Allows members to create, edit, or delete webhooks, which can post messages from other apps or sites into this server.
		/// </summary>
		[PermissionString("Manage Webhooks")]
		ManageWebhooks = 0x0000000020000000,

		/// <summary>
		/// Allows members to add or remove custom emojis and stickers in this server.
		/// </summary>
		[PermissionString("Manage Emojis and Stickers")]
		ManageEmojisAndStickers = 0x0000000040000000,

		/// <summary>
		/// Allows members to share their video, screen share, or stream a game in this server.
		/// </summary>
		[PermissionString("Allow Stream")]
		Stream  = 0x0000000000000200,

		/// <summary>
		/// Allows members to share their video, screen share, or stream a game in this server.
		/// </summary>
		[PermissionString("Video")]
		Video = Stream,

		/// <summary>
		/// Allows members to use commands from applications, including slash commands and context menu commands.
		/// </summary>
		[PermissionString("Use Application Commands")]
		UseApplicationCommands = 0x0000000080000000,

		/// <summary>
		/// Allow requests to speak in Stage channels.
		/// Stage moderators manually approve or deny each request.
		/// </summary>
		[PermissionString("Request to Speak")]
		RequestToSpeak = 0x0000000100000000,

		/// <summary>
		/// Allows members to create, edit, and cancel events.
		/// </summary>
		[PermissionString("Manage Events")]
		ManageEvents = 0x0000000200000000,

		/// <summary>
		/// Allows for deleting and archiving threads, and viewing all private threads.
		/// </summary>
		[PermissionString("Manage Threads")]
		ManageThreads = 0x0000000400000000,

		/// <summary>
		/// Allows members to rename, delete, archive/unarchive, and turn on slow mode for threads and posts.
		/// They can also view private threads.
		/// </summary>
		[PermissionString("Manage Threads and Posts")]
		ManageThreadsAndPosts = ManageThreads,

		/// <summary>
		/// Allow members to create threads that everyone in a channel can view.
		/// </summary>
		[PermissionString("Create Public Threads")]
		CreatePublicThreads = 0x0000000800000000,

		/// <summary>
		/// Allow members to create invite-only threads.
		/// </summary>
		[PermissionString("Create Private Threads")]
		CreatePrivateThreads = 0x0000001000000000,

		/// <summary>
		/// Allows members to use stickers from other servers, if they’re a Discord Nitro member.
		/// </summary>
		[PermissionString("Use External Stickers")]
		UseExternalStickers = 0x0000002000000000,

		/// <summary>
		/// Allows for sending messages in threads.
		/// </summary>
		[PermissionString("Send messages in Threads"), Obsolete("Use SendMessagesInThreadsAndPosts instead.")]
		SendMessagesInThreads = 0x0000004000000000,

		/// <summary>
		/// Allow members to send messages in threads and in posts on forum channels.
		/// </summary>
		[PermissionString("Send Messages in Threads and Posts")]
		SendMessagesInThreadsAndPosts = 0x0000004000000000,

		/// <summary>
		/// Allows members to use Activities in this server.
		/// </summary>
		[PermissionString("Use Activities")]
		UseActivities = 0x0000008000000000,

		/// <summary>
		/// Allows to perform limited moderation actions (timeout).
		/// When you put a user in timeout they will not be able to send messages in chat, reply within threads, react to messages, or speak in voice or Stage channels.
		/// </summary>
		[PermissionString("Moderate Members")]
		ModerateMembers = 0x0000010000000000,

		/// <summary>
		/// Allows to perform limited moderation actions (timeout).
		/// When you put a user in timeout they will not be able to send messages in chat, reply within threads, react to messages, or speak in voice or Stage channels.
		/// </summary>
		[PermissionString("Timeout Members")]
		TimeoutMembers = ModerateMembers,

		/// <summary>
		/// Allows members to view Server Insights, which shows data on community growth, engagement, and more.
		/// </summary>
		[PermissionString("View Server Insights")]
		ViewGuildInsights = 0x0000000000080000,

		/// <summary>
		/// Allows members to view Server Insights, which shows data on community growth, engagement, and more.
		/// </summary>
		[PermissionString("View Server Insights")]
		ViewServerInsights = ViewGuildInsights
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
}
