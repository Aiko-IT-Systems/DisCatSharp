// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

namespace DisCatSharp.Net
{

    /// <summary>
    /// The discord endpoints.
    /// </summary>
    public static class Endpoints
    {
        /// <summary>
        /// The base discord api uri.
        /// </summary>
        public const string BaseUri = "https://discord.com/api/v";

        /// <summary>
        /// The base discord canary api uri.
        /// </summary>
        public const string CanaryUri = "https://canary.discord.com/api/v";

        /// <summary>
        /// The oauth2 endpoint.
        /// </summary>
        public const string Oauth2 = "/oauth2";
        /// <summary>
        /// The oauth2 authorize endpoint.
        /// </summary>
        public const string Authorize = "/authorize";
        /// <summary>
        /// The applications endpoint.
        /// </summary>
        public const string Applications = "/applications";
        /// <summary>
        /// The message reactions endpoint.
        /// </summary>
        public const string Reactions = "/reactions";
        /// <summary>
        /// The self (@me) endpoint.
        /// </summary>
        public const string Me = "/@me";
        /// <summary>
        /// The @original endpoint.
        /// </summary>
        public const string Original = "/@original";
        /// <summary>
        /// The permissions endpoint.
        /// </summary>
        public const string Permissions = "/permissions";
        /// <summary>
        /// The recipients endpoint.
        /// </summary>
        public const string Recipients = "/recipients";
        /// <summary>
        /// The bulk-delete endpoint.
        /// </summary>
        public const string BulkDelete = "/bulk-delete";
        /// <summary>
        /// The integrations endpoint.
        /// </summary>
        public const string Integrations = "/integrations";
        /// <summary>
        /// The applications endpoint.
        /// </summary>
        public const string Sync = "/sync";
        /// <summary>
        /// The prune endpoint.
        /// Used for user removal.
        /// </summary>
        public const string Prune = "/prune";
        /// <summary>
        /// The regions endpoint.
        /// </summary>
        public const string Regions = "/regions";
        /// <summary>
        /// The connections endpoint.
        /// </summary>
        public const string Connections = "/connections";
        /// <summary>
        /// The icons endpoint.
        /// </summary>
        public const string Icons = "/icons";
        /// <summary>
        /// The gateway endpoint.
        /// </summary>
        public const string Gateway = "/gateway";
        /// <summary>
        /// The oauth2 auth endpoint.
        /// </summary>
        public const string Auth = "/auth";
        /// <summary>
        /// The oauth2 login endpoint.
        /// </summary>
        public const string Login = "/login";
        /// <summary>
        /// The channels endpoint.
        /// </summary>
        public const string Channels = "/channels";
        /// <summary>
        /// The messages endpoint.
        /// </summary>
        public const string Messages = "/messages";
        /// <summary>
        /// The pinned messages endpoint.
        /// </summary>
        public const string Pins = "/pins";
        /// <summary>
        /// The users endpoint.
        /// </summary>
        public const string Users = "/users";
        /// <summary>
        /// The guilds endpoint.
        /// </summary>
        public const string Guilds = "/guilds";
        /// <summary>
        /// The guild discovery splash endpoint.
        /// </summary>
        public const string GuildDiscoverySplashes = "/discovery-splashes";
        /// <summary>
        /// The guild splash endpoint.
        /// </summary>
        public const string Splashes = "/splashes";
        /// <summary>
        /// The search endpoint.
        /// </summary>
        public const string Search = "/search";
        /// <summary>
        /// The invites endpoint.
        /// </summary>
        public const string Invites = "/invites";
        /// <summary>
        /// The roles endpoint.
        /// </summary>
        public const string Roles = "/roles";
        /// <summary>
        /// The members endpoint.
        /// </summary>
        public const string Members = "/members";
        /// <summary>
        /// The typing endpoint.
        /// Triggers a typing indicator inside a channel.
        /// </summary>
        public const string Typing = "/typing";
        /// <summary>
        /// The avatars endpoint.
        /// </summary>
        public const string Avatars = "/avatars";
        /// <summary>
        /// The bans endpoint.
        /// </summary>
        public const string Bans = "/bans";
        /// <summary>
        /// The webhook endpoint.
        /// </summary>
        public const string Webhooks = "/webhooks";
        /// <summary>
        /// The slack endpoint.
        /// Used for <see cref="Entities.DiscordWebhook"/>.
        /// </summary>
        public const string Slack = "/slack";
        /// <summary>
        /// The github endpoint.
        /// Used for <see cref="Entities.DiscordWebhook"/>.
        /// </summary>
        public const string Github = "/github";
        /// <summary>
        /// The bot endpoint.
        /// </summary>
        public const string Bot = "/bot";
        /// <summary>
        /// The voice endpoint.
        /// </summary>
        public const string Voice = "/voice";
        /// <summary>
        /// The audit logs endpoint.
        /// </summary>
        public const string AuditLogs = "/audit-logs";
        /// <summary>
        /// The acknowledge endpoint.
        /// Indicates that a message is read.
        /// </summary>
        public const string Ack = "/ack";
        /// <summary>
        /// The nickname endpoint.
        /// </summary>
        public const string Nick = "/nick";
        /// <summary>
        /// The assets endpoint.
        /// </summary>
        public const string Assets = "/assets";
        /// <summary>
        /// The embed endpoint.
        /// </summary>
        public const string Embed = "/embed";
        /// <summary>
        /// The emojis endpoint.
        /// </summary>
        public const string Emojis = "/emojis";
        /// <summary>
        /// The vanity url endpoint.
        /// </summary>
        public const string VanityUrl = "/vanity-url";
        /// <summary>
        /// The guild preview endpoint.
        /// </summary>
        public const string Preview = "/preview";
        /// <summary>
        /// The followers endpoint.
        /// </summary>
        public const string Followers = "/followers";
        /// <summary>
        /// The crosspost endpoint.
        /// </summary>
        public const string Crosspost = "/crosspost";
        /// <summary>
        /// The guild widget endpoint.
        /// </summary>
        public const string Widget = "/widget";
        /// <summary>
        /// The guild widget json endpoint.
        /// </summary>
        public const string WidgetJson = "/widget.json";
        /// <summary>
        /// The guild widget png endpoint.
        /// </summary>
        public const string WidgetPng = "/widget.png";
        /// <summary>
        /// The templates endpoint.
        /// </summary>
        public const string Templates = "/templates";
        /// <summary>
        /// The member verification gate endpoint.
        /// </summary>
        public const string MemberVerification = "/member-verification";
        /// <summary>
        /// The slash commands endpoint.
        /// </summary>
        public const string Commands = "/commands";
        /// <summary>
        /// The interactions endpoint.
        /// </summary>
        public const string Interactions = "/interactions";
        /// <summary>
        /// The interaction/command callback endpoint.
        /// </summary>
        public const string Callback = "/callback";
        /// <summary>
        /// The welcome screen endpoint.
        /// </summary>
        public const string WelcomeScreen = "/welcome-screen";
        /// <summary>
        /// The voice states endpoint.
        /// </summary>
        public const string VoiceStates = "/voice-states";
        /// <summary>
        /// The stage instances endpoint.
        /// </summary>
        public const string StageInstances = "/stage-instances";
        /// <summary>
        /// The threads endpoint.
        /// </summary>
        public const string Threads = "/threads";
        /// <summary>
        /// The public threads endpoint.
        /// </summary>
        public const string ThreadPublic = "/public";
        /// <summary>
        /// The private threads endpoint.
        /// </summary>
        public const string ThreadPrivate = "/private";
        /// <summary>
        /// The active threads endpoint.
        /// </summary>
        public const string ThreadActive = "/active";
        /// <summary>
        /// The archived threads endpoint.
        /// </summary>
        public const string ThreadArchived = "/archived";
        /// <summary>
        /// The thread members endpoint.
        /// </summary>
        public const string ThreadMembers = "/thread-members";
        /// <summary>
        /// The guild sheduled events endpoint.
        /// </summary>
        public const string ScheduledEvents = "/scheduled-events";
        /// <summary>
        /// The stickers endpoint.
        /// </summary>
        public const string Stickers = "/stickers";
        /// <summary>
        /// The sticker packs endpoint.
        /// Global nitro sticker packs.
        /// </summary>
        public const string Stickerpacks = "/sticker-packs";
        /// <summary>
        /// The store endpoint.
        /// </summary>
        public const string Store = "/store";
        /// <summary>
        /// The app assets endpoint.
        /// </summary>
        public const string AppAssets = "/app-assets";
        /// <summary>
        /// The app icons endpoint.
        /// </summary>
        public const string AppIcons = "/app-icons";
        /// <summary>
        /// The team icons endpoint.
        /// </summary>
        public const string TeamIcons = "/team-icons";
        /// <summary>
        /// The channel icons endpoint.
        /// </summary>
        public const string ChannelIcons = "/channel-icons";
        /// <summary>
        /// The user banners endpoint.
        /// </summary>
        public const string Banners = "/banners";
        /// <summary>
        /// The sticker endpoint.
        /// This endpoint is the static nitro sticker application.
        /// </summary>
        public const string StickerApplication = "/710982414301790216";
        /// <summary>
        /// The role subscription endpoint.
        /// </summary>
        public const string RoleSubscriptions = "/role-subscriptions";
        /// <summary>
        /// The group listings endpoint.
        /// </summary>
        public const string GroupListings = "/group-listings";
        /// <summary>
        /// The subscription listings endpoint.
        /// </summary>
        public const string SubscriptionListings = "/subscription-listings";
        /// <summary>
        /// The directory entries endpoint.
        /// </summary>
        public const string DirectoryEntries = "/directory-entries";
        /// <summary>
        /// The counts endpoint.
        /// </summary>
        public const string Counts = "/counts";
        /// <summary>
        /// The list endpoint.
        /// </summary>
        public const string List = "/list";
        /// <summary>
        /// The role icons endpoint.
        /// </summary>
        public const string RoleIcons = "/role-icons";
        /// <summary>
        /// The activities endpoint.
        /// </summary>
        public const string Activities = "/activities";
        /// <summary>
        /// The config endpoint.
        /// </summary>
        public const string Config = "/config";
        /// <summary>
        /// The ephemeral attachments endpoint.
        /// </summary>
        public const string EphemeralAttachments = "/ephemeral-attachments";
    }
}
