// This file is part of the DisCatSharp project.
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
using DisCatSharp;

namespace DisCatSharp.Net
{

    /// <summary>
    /// The endpoints.
    /// </summary>
    internal static class Endpoints
    {
        /// <summary>
        /// The base discord api uri
        /// </summary>
        public const string BASE_URI = "https://discord.com/api/v9";

        /// <summary>
        /// The oauth2 endpoint
        /// </summary>
        public const string OAUTH2 = "/oauth2";
        /// <summary>
        /// The oauth2 authorize endpoint
        /// </summary>
        public const string AUTHORIZE = "/authorize";
        /// <summary>
        /// The applications endpoint
        /// </summary>
        public const string APPLICATIONS = "/applications";
        /// <summary>
        /// The message reactions endpoint
        /// </summary>
        public const string REACTIONS = "/reactions";
        /// <summary>
        /// The self (@me) endpoint
        /// </summary>
        public const string ME = "/@me";
        /// <summary>
        /// The @original endpoint
        /// </summary>
        public const string ORIGINAL = "/@original";
        /// <summary>
        /// The permissions endpoint
        /// </summary>
        public const string PERMISSIONS = "/permissions";
        /// <summary>
        /// The recipients endpoint
        /// </summary>
        public const string RECIPIENTS = "/recipients";
        /// <summary>
        /// The bulk-delete endpoint
        /// </summary>
        public const string BULK_DELETE = "/bulk-delete";
        /// <summary>
        /// The integrations endpoint
        /// </summary>
        public const string INTEGRATIONS = "/integrations";
        /// <summary>
        /// The applications endpoint
        /// </summary>
        public const string SYNC = "/sync";
        /// <summary>
        /// The prune endpoint.
        /// Used for user removal
        /// </summary>
        public const string PRUNE = "/prune";
        /// <summary>
        /// The regions endpoint
        /// </summary>
        public const string REGIONS = "/regions";
        /// <summary>
        /// The connections endpoint
        /// </summary>
        public const string CONNECTIONS = "/connections";
        /// <summary>
        /// The icons endpoint
        /// </summary>
        public const string ICONS = "/icons";
        /// <summary>
        /// The gateway endpoint
        /// </summary>
        public const string GATEWAY = "/gateway";
        /// <summary>
        /// The oauth2 auth endpoint
        /// </summary>
        public const string AUTH = "/auth";
        /// <summary>
        /// The oauth2 login endpoint
        /// </summary>
        public const string LOGIN = "/login";
        /// <summary>
        /// The channels endpoint
        /// </summary>
        public const string CHANNELS = "/channels";
        /// <summary>
        /// The messages endpoint
        /// </summary>
        public const string MESSAGES = "/messages";
        /// <summary>
        /// The pinned messages endpoint
        /// </summary>
        public const string PINS = "/pins";
        /// <summary>
        /// The users endpoint
        /// </summary>
        public const string USERS = "/users";
        /// <summary>
        /// The guilds endpoint
        /// </summary>
        public const string GUILDS = "/guilds";
        /// <summary>
        /// The guild discovery splash endpoint
        /// </summary>
        public const string GUILD_DISCOVERY_SPLASHES = "/discovery-splashes";
        /// <summary>
        /// The guild splash endpoint
        /// </summary>
        public const string SPLASHES = "/splashes";
        /// <summary>
        /// The search endpoint
        /// </summary>
        public const string SEARCH = "/search";
        /// <summary>
        /// The invites endpoint
        /// </summary>
        public const string INVITES = "/invites";
        /// <summary>
        /// The roles endpoint
        /// </summary>
        public const string ROLES = "/roles";
        /// <summary>
        /// The members endpoint
        /// </summary>
        public const string MEMBERS = "/members";
        /// <summary>
        /// The typing endpoint.
        /// Triggers a typing indicator inside a channel
        /// </summary>
        public const string TYPING = "/typing";
        /// <summary>
        /// The avatars endpoint
        /// </summary>
        public const string AVATARS = "/avatars";
        /// <summary>
        /// The bans endpoint
        /// </summary>
        public const string BANS = "/bans";
        /// <summary>
        /// The webhook endpoint
        /// </summary>
        public const string WEBHOOKS = "/webhooks";
        /// <summary>
        /// The slack endpoint.
        /// Used for <see cref="Entities.DiscordWebhook"/>
        /// </summary>
        public const string SLACK = "/slack";
        /// <summary>
        /// The github endpoint.
        /// Used for <see cref="Entities.DiscordWebhook"/>
        /// </summary>
        public const string GITHUB = "/github";
        /// <summary>
        /// The bot endpoint
        /// </summary>
        public const string BOT = "/bot";
        /// <summary>
        /// The voice endpoint
        /// </summary>
        public const string VOICE = "/voice";
        /// <summary>
        /// The audit logs endpoint
        /// </summary>
        public const string AUDIT_LOGS = "/audit-logs";
        /// <summary>
        /// The acknowledge endpoint.
        /// Indicates that a message is read
        /// </summary>
        public const string ACK = "/ack";
        /// <summary>
        /// The nickname endpoint
        /// </summary>
        public const string NICK = "/nick";
        /// <summary>
        /// The assets endpoint
        /// </summary>
        public const string ASSETS = "/assets";
        /// <summary>
        /// The embed endpoint
        /// </summary>
        public const string EMBED = "/embed";
        /// <summary>
        /// The emojis endpoint
        /// </summary>
        public const string EMOJIS = "/emojis";
        /// <summary>
        /// The vanity url endpoint
        /// </summary>
        public const string VANITY_URL = "/vanity-url";
        /// <summary>
        /// The guild preview endpoint
        /// </summary>
        public const string PREVIEW = "/preview";
        /// <summary>
        /// The followers endpoint
        /// </summary>
        public const string FOLLOWERS = "/followers";
        /// <summary>
        /// The crosspost endpoint
        /// </summary>
        public const string CROSSPOST = "/crosspost";
        /// <summary>
        /// The guild widget endpoint
        /// </summary>
        public const string WIDGET = "/widget";
        /// <summary>
        /// The guild widget json endpoint
        /// </summary>
        public const string WIDGET_JSON = "/widget.json";
        /// <summary>
        /// The guild widget png endpoint
        /// </summary>
        public const string WIDGET_PNG = "/widget.png";
        /// <summary>
        /// The templates endpoint
        /// </summary>
        public const string TEMPLATES = "/templates";
        /// <summary>
        /// The member verification gate endpoint
        /// </summary>
        public const string MEMBER_VERIFICATION = "/member-verification";
        /// <summary>
        /// The slash commands endpoint
        /// </summary>
        public const string COMMANDS = "/commands";
        /// <summary>
        /// The interactions endpoint
        /// </summary>
        public const string INTERACTIONS = "/interactions";
        /// <summary>
        /// The interaction/command callback endpoint
        /// </summary>
        public const string CALLBACK = "/callback";
        /// <summary>
        /// The welcome screen endpoint
        /// </summary>
        public const string WELCOME_SCREEN = "/welcome-screen";
        /// <summary>
        /// The voice states endpoint
        /// </summary>
        public const string VOICE_STATES = "/voice-states";
        /// <summary>
        /// The stage instances endpoint
        /// </summary>
        public const string STAGE_INSTANCES = "/stage-instances";
        /// <summary>
        /// The threads endpoint
        /// </summary>
        public const string THREADS = "/threads";
        /// <summary>
        /// The public threads endpoint
        /// </summary>
        public const string THREAD_PUBLIC = "/public";
        /// <summary>
        /// The private threads endpoint
        /// </summary>
        public const string THREAD_PRIVATE = "/private";
        /// <summary>
        /// The active threads endpoint
        /// </summary>
        public const string THREAD_ACTIVE = "/active";
        /// <summary>
        /// The archived threads endpoint
        /// </summary>
        public const string THREAD_ARCHIVED = "/archived";
        /// <summary>
        /// The thread members endpoint
        /// </summary>
        public const string THREAD_MEMBERS = "/thread-members";
        /// <summary>
        /// The guild events endpoint.
        /// Used for stage events
        /// </summary>
        public const string EVENTS = "/events";
        /// <summary>
        /// The stickers endpoint
        /// </summary>
        public const string STICKERS = "/stickers";
        /// <summary>
        /// The sticker packs endpoint.
        /// Global nitro sticker packs.
        /// </summary>
        public const string STICKERPACKS = "/sticker-packs";
        /// <summary>
        /// The store endpoint
        /// </summary>
        public const string STORE = "/store";
        /// <summary>
        /// The app assets endpoint
        /// </summary>
        public const string APP_ASSETS = "/app-assets";
        /// <summary>
        /// The app icons endpoint
        /// </summary>
        public const string APP_ICONS = "/app-icons";
        /// <summary>
        /// The team icons endpoint
        /// </summary>
        public const string TEAM_ICONS = "/team-icons";
        /// <summary>
        /// The channel icons endpoint
        /// </summary>
        public const string CHANNEL_ICONS = "/channel-icons";
        /// <summary>
        /// The user banners endpoint
        /// </summary>
        public const string BANNERS = "/banners";
        /// <summary>
        /// The sticker endpoint.
        /// This endpoint is the static nitro sticker application
        /// </summary>
        public const string STICKER_APPLICATION = "/710982414301790216";
    }
}
