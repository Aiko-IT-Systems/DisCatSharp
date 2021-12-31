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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DisCatSharp
{
    /// <summary>
    /// Represents a discord client.
    /// </summary>
    public sealed partial class DiscordClient
    {
        #region Private Fields

        private string _sessionId;
        private bool _guildDownloadCompleted = false;

        #endregion

        #region Dispatch Handler

        /// <summary>
        /// Handles the dispatch.
        /// </summary>
        /// <param name="Payload">The payload.</param>

        internal async Task HandleDispatchAsync(GatewayPayload Payload)
        {
            if (Payload.Data is not JObject dat)
            {
                this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Invalid payload body (this message is probably safe to ignore); opcode: {0} event: {1}; payload: {2}", Payload.OpCode, Payload.EventName, Payload.Data);
                return;
            }

            await this._payloadReceived.InvokeAsync(this, new(this.ServiceProvider)
            {
                EventName = Payload.EventName,
                PayloadObject = dat
            }).ConfigureAwait(false);

            DiscordChannel chn;
            ulong gid;
            ulong cid;
            ulong uid;
            DiscordStageInstance stg = default;
            DiscordIntegration itg = default;
            DiscordThreadChannel trd = default;
            DiscordThreadChannelMember trdm = default;
            DiscordScheduledEvent gse = default;
            TransportUser usr = default;
            TransportMember mbr = default;
            TransportUser refUsr = default;
            TransportMember refMbr = default;
            JToken rawMbr = default;
            var rawRefMsg = dat["referenced_message"];

            switch (Payload.EventName.ToLowerInvariant())
            {
                #region Gateway Status

                case "ready":
                    var glds = (JArray)dat["guilds"];
                    await this.OnReadyEventAsync(dat.ToObject<ReadyPayload>(), glds).ConfigureAwait(false);
                    break;

                case "resumed":
                    await this.OnResumedAsync().ConfigureAwait(false);
                    break;

                #endregion

                #region Channel

                case "channel_create":
                    chn = dat.ToObject<DiscordChannel>();
                    await this.OnChannelCreateEventAsync(chn).ConfigureAwait(false);
                    break;

                case "channel_update":
                    await this.OnChannelUpdateEventAsync(dat.ToObject<DiscordChannel>()).ConfigureAwait(false);
                    break;

                case "channel_delete":
                    chn = dat.ToObject<DiscordChannel>();
                    await this.OnChannelDeleteEventAsync(chn.IsPrivate ? dat.ToObject<DiscordDmChannel>() : chn).ConfigureAwait(false);
                    break;

                case "channel_pins_update":
                    cid = (ulong)dat["channel_id"];
                    var ts = (string)dat["last_pin_timestamp"];
                    await this.OnChannelPinsUpdateAsync((ulong?)dat["guild_id"], cid, ts != null ? DateTimeOffset.Parse(ts, CultureInfo.InvariantCulture) : default(DateTimeOffset?)).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild

                case "guild_create":
                    await this.OnGuildCreateEventAsync(dat.ToDiscordObject<DiscordGuild>(), (JArray)dat["members"], dat["presences"].ToDiscordObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
                    break;

                case "guild_update":
                    await this.OnGuildUpdateEventAsync(dat.ToDiscordObject<DiscordGuild>(), (JArray)dat["members"]).ConfigureAwait(false);
                    break;

                case "guild_delete":
                    await this.OnGuildDeleteEventAsync(dat.ToDiscordObject<DiscordGuild>()).ConfigureAwait(false);
                    break;

                case "guild_sync":
                    gid = (ulong)dat["id"];
                    await this.OnGuildSyncEventAsync(this._guilds[gid], (bool)dat["large"], (JArray)dat["members"], dat["presences"].ToDiscordObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
                    break;

                case "guild_emojis_update":
                    gid = (ulong)dat["guild_id"];
                    var ems = dat["emojis"].ToObject<IEnumerable<DiscordEmoji>>();
                    await this.OnGuildEmojisUpdateEventAsync(this._guilds[gid], ems).ConfigureAwait(false);
                    break;

                case "guild_stickers_update":
                    var strs = dat["stickers"].ToDiscordObject<IEnumerable<DiscordSticker>>();
                    await this.OnStickersUpdatedAsync(strs, dat).ConfigureAwait(false);
                    break;

                case "guild_integrations_update":
                    gid = (ulong)dat["guild_id"];

                    // discord fires this event inconsistently if the current user leaves a guild.
                    if (!this._guilds.ContainsKey(gid))
                        return;

                    await this.OnGuildIntegrationsUpdateEventAsync(this._guilds[gid]).ConfigureAwait(false);
                    break;

                /*
                 Ok soooo.. this isn't documented yet
                 It seems to be part of the next version of membership screening (https://discord.com/channels/641574644578648068/689591708962652289/845836910991507486)

                 advaith said the following (https://discord.com/channels/641574644578648068/689591708962652289/845838160047112202):
                 > iirc it happens when a user leaves a server where they havent completed screening yet

                 We have to wait till it's documented, but the fields are:
                 { "user_id": "snowflake_user", "guild_id": "snowflake_guild" }

                 We could handle it rn, but due to the fact that it isn't documented, it's not an good idea.
                 */
                case "guild_join_request_delete":
                    break;

                #endregion

                #region Guild Ban

                case "guild_ban_add":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildBanAddEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_ban_remove":
                    usr = dat["user"].ToObject<TransportUser>();
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildBanRemoveEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild Event

                case "guild_scheduled_event_create":
                    gse = dat.ToObject<DiscordScheduledEvent>();
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildScheduledEventCreateEventAsync(gse, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_scheduled_event_update":
                    gse = dat.ToObject<DiscordScheduledEvent>();
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildScheduledEventUpdateEventAsync(gse, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_scheduled_event_delete":
                    gse = dat.ToObject<DiscordScheduledEvent>();
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildScheduledEventDeleteEventAsync(gse, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_scheduled_event_user_add":
                    gid = (ulong)dat["guild_id"];
                    uid = (ulong)dat["user_id"];
                    await this.OnGuildScheduledEventUserAddedEventAsync((ulong)dat["guild_scheduled_event_id"], uid, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_scheduled_event_user_remove":
                    gid = (ulong)dat["guild_id"];
                    uid = (ulong)dat["user_id"];
                    await this.OnGuildScheduledEventUserRemovedEventAsync((ulong)dat["guild_scheduled_event_id"], uid, this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild Integration

                case "integration_create":
                    gid = (ulong)dat["guild_id"];
                    itg = dat.ToObject<DiscordIntegration>();

                    // discord fires this event inconsistently if the current user leaves a guild.
                    if (!this._guilds.ContainsKey(gid))
                        return;

                    await this.OnGuildIntegrationCreateEventAsync(this._guilds[gid], itg).ConfigureAwait(false);
                    break;

                case "integration_update":
                    gid = (ulong)dat["guild_id"];
                    itg = dat.ToObject<DiscordIntegration>();

                    // discord fires this event inconsistently if the current user leaves a guild.
                    if (!this._guilds.ContainsKey(gid))
                        return;

                    await this.OnGuildIntegrationUpdateEventAsync(this._guilds[gid], itg).ConfigureAwait(false);
                    break;

                case "integration_delete":
                    gid = (ulong)dat["guild_id"];

                    // discord fires this event inconsistently if the current user leaves a guild.
                    if (!this._guilds.ContainsKey(gid))
                        return;

                    await this.OnGuildIntegrationDeleteEventAsync(this._guilds[gid], (ulong)dat["id"], (ulong?)dat["application_id"]).ConfigureAwait(false);
                    break;
                #endregion

                #region Guild Member

                case "guild_member_add":
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildMemberAddEventAsync(dat.ToObject<TransportMember>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_member_remove":
                    gid = (ulong)dat["guild_id"];
                    usr = dat["user"].ToObject<TransportUser>();

                    if (!this._guilds.ContainsKey(gid))
                    {
                        // discord fires this event inconsistently if the current user leaves a guild.
                        if (usr.Id != this.CurrentUser.Id)
                            this.Logger.LogError(LoggerEvents.WebSocketReceive, "Could not find {0} in guild cache", gid);
                        return;
                    }

                    await this.OnGuildMemberRemoveEventAsync(usr, this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_member_update":
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildMemberUpdateEventAsync(dat.ToDiscordObject<TransportMember>(), this._guilds[gid], dat["roles"].ToObject<IEnumerable<ulong>>(), (string)dat["nick"], (bool?)dat["pending"]).ConfigureAwait(false);
                    break;

                case "guild_members_chunk":
                    await this.OnGuildMembersChunkEventAsync(dat).ConfigureAwait(false);
                    break;

                #endregion

                #region Guild Role

                case "guild_role_create":
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildRoleCreateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_role_update":
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildRoleUpdateEventAsync(dat["role"].ToObject<DiscordRole>(), this._guilds[gid]).ConfigureAwait(false);
                    break;

                case "guild_role_delete":
                    gid = (ulong)dat["guild_id"];
                    await this.OnGuildRoleDeleteEventAsync((ulong)dat["role_id"], this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Invite

                case "invite_create":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await this.OnInviteCreateEventAsync(cid, gid, dat.ToObject<DiscordInvite>()).ConfigureAwait(false);
                    break;

                case "invite_delete":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await this.OnInviteDeleteEventAsync(cid, gid, dat).ConfigureAwait(false);
                    break;

                #endregion

                #region Message

                case "message_ack":
                    cid = (ulong)dat["channel_id"];
                    var mid = (ulong)dat["message_id"];
                    await this.OnMessageAckEventAsync(this.InternalGetCachedChannel(cid), mid).ConfigureAwait(false);
                    break;

                case "message_create":
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    if (rawRefMsg != null && rawRefMsg.HasValues)
                    {
                        if (rawRefMsg.SelectToken("author") != null)
                        {
                            refUsr = rawRefMsg.SelectToken("author").ToObject<TransportUser>();
                        }

                        if (rawRefMsg.SelectToken("member") != null)
                        {
                            refMbr = rawRefMsg.SelectToken("member").ToObject<TransportMember>();
                        }
                    }

                    await this.OnMessageCreateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"].ToObject<TransportUser>(), mbr, refUsr, refMbr).ConfigureAwait(false);
                    break;

                case "message_update":
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    if (rawRefMsg != null && rawRefMsg.HasValues)
                    {
                        if (rawRefMsg.SelectToken("author") != null)
                        {
                            refUsr = rawRefMsg.SelectToken("author").ToObject<TransportUser>();
                        }

                        if (rawRefMsg.SelectToken("member") != null)
                        {
                            refMbr = rawRefMsg.SelectToken("member").ToObject<TransportMember>();
                        }
                    }

                    await this.OnMessageUpdateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"]?.ToObject<TransportUser>(), mbr, refUsr, refMbr).ConfigureAwait(false);
                    break;

                // delete event does *not* include message object
                case "message_delete":
                    await this.OnMessageDeleteEventAsync((ulong)dat["id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "message_delete_bulk":
                    await this.OnMessageBulkDeleteEventAsync(dat["ids"].ToObject<ulong[]>(), (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                #endregion

                #region Message Reaction

                case "message_reaction_add":
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    await this.OnMessageReactionAddAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], mbr, dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                    break;

                case "message_reaction_remove":
                    await this.OnMessageReactionRemoveAsync((ulong)dat["user_id"], (ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"], dat["emoji"].ToObject<DiscordEmoji>()).ConfigureAwait(false);
                    break;

                case "message_reaction_remove_all":
                    await this.OnMessageReactionRemoveAllAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "message_reaction_remove_emoji":
                    await this.OnMessageReactionRemoveEmojiAsync((ulong)dat["message_id"], (ulong)dat["channel_id"], (ulong)dat["guild_id"], dat["emoji"]).ConfigureAwait(false);
                    break;

                #endregion

                #region Stage Instance

                case "stage_instance_create":
                    stg = dat.ToObject<DiscordStageInstance>();
                    await this.OnStageInstanceCreateEventAsync(stg).ConfigureAwait(false);
                    break;

                case "stage_instance_update":
                    stg = dat.ToObject<DiscordStageInstance>();
                    await this.OnStageInstanceUpdateEventAsync(stg).ConfigureAwait(false);
                    break;

                case "stage_instance_delete":
                    stg = dat.ToObject<DiscordStageInstance>();
                    await this.OnStageInstanceDeleteEventAsync(stg).ConfigureAwait(false);
                    break;

                #endregion

                #region Thread

                case "thread_create":
                    trd = dat.ToObject<DiscordThreadChannel>();
                    await this.OnThreadCreateEventAsync(trd).ConfigureAwait(false);
                    break;

                case "thread_update":
                    trd = dat.ToObject<DiscordThreadChannel>();
                    await this.OnThreadUpdateEventAsync(trd).ConfigureAwait(false);
                    break;

                case "thread_delete":
                    trd = dat.ToObject<DiscordThreadChannel>();
                    await this.OnThreadDeleteEventAsync(trd).ConfigureAwait(false);
                    break;

                case "thread_list_sync":
                    gid = (ulong)dat["guild_id"]; //get guild
                    await this.OnThreadListSyncEventAsync(this._guilds[gid], dat["channel_ids"].ToObject<IReadOnlyList<ulong?>>(), dat["threads"].ToObject<IReadOnlyList<DiscordThreadChannel>>(), dat["members"].ToObject<IReadOnlyList<DiscordThreadChannelMember>>()).ConfigureAwait(false);
                    break;

                case "thread_member_update":
                    trdm = dat.ToObject<DiscordThreadChannelMember>();
                    await this.OnThreadMemberUpdateEventAsync(trdm).ConfigureAwait(false);
                    break;

                case "thread_members_update":
                    gid = (ulong)dat["guild_id"];

                    await this.OnThreadMembersUpdateEventAsync(this._guilds[gid], (ulong)dat["id"], (JArray)dat["added_members"], (JArray)dat["removed_member_ids"], (int)dat["member_count"]).ConfigureAwait(false);
                    break;

                #endregion

                #region Activities
                case "embedded_activity_update":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await this.OnEmbeddedActivityUpdateAsync((JObject)dat["embedded_activity"], this._guilds[gid], cid, (JArray)dat["users"], (ulong)dat["embedded_activity"]["application_id"]).ConfigureAwait(false);
                    break;
                #endregion

                #region User/Presence Update

                case "presence_update":
                    await this.OnPresenceUpdateEventAsync(dat, (JObject)dat["user"]).ConfigureAwait(false);
                    break;

                case "user_settings_update":
                    await this.OnUserSettingsUpdateEventAsync(dat.ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                case "user_update":
                    await this.OnUserUpdateEventAsync(dat.ToObject<TransportUser>()).ConfigureAwait(false);
                    break;

                #endregion

                #region Voice

                case "voice_state_update":
                    await this.OnVoiceStateUpdateEventAsync(dat).ConfigureAwait(false);
                    break;

                case "voice_server_update":
                    gid = (ulong)dat["guild_id"];
                    await this.OnVoiceServerUpdateEventAsync((string)dat["endpoint"], (string)dat["token"], this._guilds[gid]).ConfigureAwait(false);
                    break;

                #endregion

                #region Interaction/Integration/Application

                case "interaction_create":
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                    {
                        mbr = dat["member"].ToObject<TransportMember>();
                        usr = mbr.User;
                    }
                    else
                    {
                        usr = dat["user"].ToObject<TransportUser>();
                    }

                    cid = (ulong)dat["channel_id"];
                    await this.OnInteractionCreateAsync((ulong?)dat["guild_id"], cid, usr, mbr, dat.ToDiscordObject<DiscordInteraction>()).ConfigureAwait(false);
                    break;

                case "application_command_create":
                    await this.OnApplicationCommandCreateAsync(dat.ToObject<DiscordApplicationCommand>(), (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "application_command_update":
                    await this.OnApplicationCommandUpdateAsync(dat.ToObject<DiscordApplicationCommand>(), (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "application_command_delete":
                    await this.OnApplicationCommandDeleteAsync(dat.ToObject<DiscordApplicationCommand>(), (ulong?)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "guild_application_command_counts_update":
                    var counts = dat["application_command_counts"];
                    await this.OnGuildApplicationCommandCountsUpdateAsync((int)counts["1"], (int)counts["2"], (int)counts["3"], (ulong)dat["guild_id"]).ConfigureAwait(false);
                    break;

                case "application_command_permissions_update":
                    var aid = (ulong)dat["application_id"];
                    if (aid != this.CurrentApplication.Id)
                        return;

                    var pms = dat["permissions"].ToObject<IEnumerable<DiscordApplicationCommandPermission>>();
                    gid = (ulong)dat["guild_id"];
                    await this.OnApplicationCommandPermissionsUpdateAsync(pms, (ulong)dat["id"], gid, aid).ConfigureAwait(false);
                    break;

                #endregion

                #region Misc

                case "gift_code_update": //Not supposed to be dispatched to bots
                    break;

                case "typing_start":
                    cid = (ulong)dat["channel_id"];
                    rawMbr = dat["member"];

                    if (rawMbr != null)
                        mbr = rawMbr.ToObject<TransportMember>();

                    await this.OnTypingStartEventAsync((ulong)dat["user_id"], cid, this.InternalGetCachedChannel(cid), (ulong?)dat["guild_id"], Utilities.GetDateTimeOffset((long)dat["timestamp"]), mbr).ConfigureAwait(false);
                    break;

                case "webhooks_update":
                    gid = (ulong)dat["guild_id"];
                    cid = (ulong)dat["channel_id"];
                    await this.OnWebhooksUpdateAsync(this._guilds[gid].GetChannel(cid), this._guilds[gid]).ConfigureAwait(false);
                    break;

                default:
                    await this.OnUnknownEventAsync(Payload).ConfigureAwait(false);
                    this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown event: {0}\npayload: {1}", Payload.EventName, Payload.Data);
                    break;

                    #endregion
            }
        }

        #endregion

        #region Events

        #region Gateway

        /// <summary>
        /// Handles the ready event.
        /// </summary>
        /// <param name="Ready">The ready.</param>
        /// <param name="RawGuilds">The raw guilds.</param>

        internal async Task OnReadyEventAsync(ReadyPayload Ready, JArray RawGuilds)
        {
            //ready.CurrentUser.Discord = this;

            var rusr = Ready.CurrentUser;
            this.CurrentUser.Username = rusr.Username;
            this.CurrentUser.Discriminator = rusr.Discriminator;
            this.CurrentUser.AvatarHash = rusr.AvatarHash;
            this.CurrentUser.MfaEnabled = rusr.MfaEnabled;
            this.CurrentUser.Verified = rusr.Verified;
            this.CurrentUser.IsBot = rusr.IsBot;

            this.GatewayVersion = Ready.GatewayVersion;
            this._sessionId = Ready.SessionId;
            var rawGuildIndex = RawGuilds.ToDictionary(Xt => (ulong)Xt["id"], Xt => (JObject)Xt);

            this._guilds.Clear();
            foreach (var guild in Ready.Guilds)
            {
                guild.Discord = this;

                if (guild._channels == null)
                    guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();

                foreach (var xc in guild.Channels.Values)
                {
                    xc.GuildId = guild.Id;
                    xc.Discord = this;
                    foreach (var xo in xc._permissionOverwrites)
                    {
                        xo.Discord = this;
                        xo._channelId = xc.Id;
                    }
                }

                if (guild._roles == null)
                    guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();

                foreach (var xr in guild.Roles.Values)
                {
                    xr.Discord = this;
                    xr._guildId = guild.Id;
                }

                var rawGuild = rawGuildIndex[guild.Id];
                var rawMembers = (JArray)rawGuild["members"];

                if (guild._members != null)
                    guild._members.Clear();
                else
                    guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

                if (rawMembers != null)
                {
                    foreach (var xj in rawMembers)
                    {
                        var xtm = xj.ToObject<TransportMember>();

                        var xu = new DiscordUser(xtm.User) { Discord = this };
                        xu = this.UserCache.AddOrUpdate(xtm.User.Id, xu, (Id, Old) =>
                        {
                            Old.Username = xu.Username;
                            Old.Discriminator = xu.Discriminator;
                            Old.AvatarHash = xu.AvatarHash;
                            return Old;
                        });

                        guild._members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, _guildId = guild.Id };
                    }
                }

                if (guild._emojis == null)
                    guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();

                foreach (var xe in guild.Emojis.Values)
                    xe.Discord = this;

                if (guild._voiceStates == null)
                    guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();

                foreach (var xvs in guild.VoiceStates.Values)
                    xvs.Discord = this;

                this._guilds[guild.Id] = guild;
            }

            await this._ready.InvokeAsync(this, new ReadyEventArgs(this.ServiceProvider)).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the resumed.
        /// </summary>

        internal Task OnResumedAsync()
        {
            this.Logger.LogInformation(LoggerEvents.SessionUpdate, "Session resumed");
            return this._resumed.InvokeAsync(this, new ReadyEventArgs(this.ServiceProvider));
        }

        #endregion

        #region Channel

        /// <summary>
        /// Handles the channel create event.
        /// </summary>
        /// <param name="Channel">The channel.</param>

        internal async Task OnChannelCreateEventAsync(DiscordChannel Channel)
        {
            Channel.Discord = this;
            foreach (var xo in Channel._permissionOverwrites)
            {
                xo.Discord = this;
                xo._channelId = Channel.Id;
            }

            this._guilds[Channel.GuildId.Value]._channels[Channel.Id] = Channel;

            /*if (this.Configuration.AutoRefreshChannelCache)
            {
                await this.RefreshChannelsAsync(channel.Guild.Id);
            }*/

            await this._channelCreated.InvokeAsync(this, new ChannelCreateEventArgs(this.ServiceProvider) { Channel = Channel, Guild = Channel.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the channel update event.
        /// </summary>
        /// <param name="Channel">The channel.</param>

        internal async Task OnChannelUpdateEventAsync(DiscordChannel Channel)
        {
            if (Channel == null)
                return;

            Channel.Discord = this;

            var gld = Channel.Guild;

            var channelNew = this.InternalGetCachedChannel(Channel.Id);
            DiscordChannel channelOld = null;

            if (channelNew != null)
            {
                channelOld = new DiscordChannel
                {
                    Bitrate = channelNew.Bitrate,
                    Discord = this,
                    GuildId = channelNew.GuildId,
                    Id = channelNew.Id,
                    //IsPrivate = channel_new.IsPrivate,
                    LastMessageId = channelNew.LastMessageId,
                    Name = channelNew.Name,
                    _permissionOverwrites = new List<DiscordOverwrite>(channelNew._permissionOverwrites),
                    Position = channelNew.Position,
                    Topic = channelNew.Topic,
                    Type = channelNew.Type,
                    UserLimit = channelNew.UserLimit,
                    ParentId = channelNew.ParentId,
                    IsNsfw = channelNew.IsNsfw,
                    PerUserRateLimit = channelNew.PerUserRateLimit,
                    RtcRegionId = channelNew.RtcRegionId,
                    QualityMode = channelNew.QualityMode,
                    DefaultAutoArchiveDuration = channelNew.DefaultAutoArchiveDuration
                };

                channelNew.Bitrate = Channel.Bitrate;
                channelNew.Name = Channel.Name;
                channelNew.Position = Channel.Position;
                channelNew.Topic = Channel.Topic;
                channelNew.UserLimit = Channel.UserLimit;
                channelNew.ParentId = Channel.ParentId;
                channelNew.IsNsfw = Channel.IsNsfw;
                channelNew.PerUserRateLimit = Channel.PerUserRateLimit;
                channelNew.Type = Channel.Type;
                channelNew.RtcRegionId = Channel.RtcRegionId;
                channelNew.QualityMode = Channel.QualityMode;
                channelNew.DefaultAutoArchiveDuration = Channel.DefaultAutoArchiveDuration;

                channelNew._permissionOverwrites.Clear();

                foreach (var po in Channel._permissionOverwrites)
                {
                    po.Discord = this;
                    po._channelId = Channel.Id;
                }

                channelNew._permissionOverwrites.AddRange(Channel._permissionOverwrites);

                if (this.Configuration.AutoRefreshChannelCache && gld != null)
                {
                    await this.RefreshChannelsAsync(Channel.Guild.Id);
                }
            }
            else if (gld != null)
            {
                gld._channels[Channel.Id] = Channel;

                if (this.Configuration.AutoRefreshChannelCache)
                {
                    await this.RefreshChannelsAsync(Channel.Guild.Id);
                }
            }

            await this._channelUpdated.InvokeAsync(this, new ChannelUpdateEventArgs(this.ServiceProvider) { ChannelAfter = channelNew, Guild = gld, ChannelBefore = channelOld }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the channel delete event.
        /// </summary>
        /// <param name="Channel">The channel.</param>

        internal async Task OnChannelDeleteEventAsync(DiscordChannel Channel)
        {
            if (Channel == null)
                return;

            Channel.Discord = this;

            //if (channel.IsPrivate)
            if (Channel.Type == ChannelType.Group || Channel.Type == ChannelType.Private)
            {
                var dmChannel = Channel as DiscordDmChannel;

                await this._dmChannelDeleted.InvokeAsync(this, new DmChannelDeleteEventArgs(this.ServiceProvider) { Channel = dmChannel }).ConfigureAwait(false);
            }
            else
            {
                var gld = Channel.Guild;

                if (gld._channels.TryRemove(Channel.Id, out var cachedChannel)) Channel = cachedChannel;

                if (this.Configuration.AutoRefreshChannelCache)
                {
                    await this.RefreshChannelsAsync(Channel.Guild.Id);
                }

                await this._channelDeleted.InvokeAsync(this, new ChannelDeleteEventArgs(this.ServiceProvider) { Channel = Channel, Guild = gld }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refreshes the channels.
        /// </summary>
        /// <param name="GuildId">The guild id.</param>
        internal async Task RefreshChannelsAsync(ulong GuildId)
        {
            var guild = this.InternalGetCachedGuild(GuildId);
            var channels = await this.ApiClient.GetGuildChannelsAsync(GuildId);
            guild._channels.Clear();
            foreach (var channel in channels.ToList())
            {
                channel.Discord = this;
                foreach (var xo in channel._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channelId = channel.Id;
                }
                guild._channels[channel.Id] = channel;
            }
        }

        /// <summary>
        /// Handles the channel pins update.
        /// </summary>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="LastPinTimestamp">The last pin timestamp.</param>

        internal async Task OnChannelPinsUpdateAsync(ulong? GuildId, ulong ChannelId, DateTimeOffset? LastPinTimestamp)
        {
            var guild = this.InternalGetCachedGuild(GuildId);
            var channel = this.InternalGetCachedChannel(ChannelId) ?? this.InternalGetCachedThread(ChannelId);

            var ea = new ChannelPinsUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Channel = channel,
                LastPinTimestamp = LastPinTimestamp
            };
            await this._channelPinsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild

        /// <summary>
        /// Handles the guild create event.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="RawMembers">The raw members.</param>
        /// <param name="Presences">The presences.</param>

        internal async Task OnGuildCreateEventAsync(DiscordGuild Guild, JArray RawMembers, IEnumerable<DiscordPresence> Presences)
        {
            if (Presences != null)
            {
                foreach (var xp in Presences)
                {
                    xp.Discord = this;
                    xp.GuildId = Guild.Id;
                    xp.Activity = new DiscordActivity(xp.RawActivity);
                    if (xp.RawActivities != null)
                    {
                        xp._internalActivities = new DiscordActivity[xp.RawActivities.Length];
                        for (var i = 0; i < xp.RawActivities.Length; i++)
                            xp._internalActivities[i] = new DiscordActivity(xp.RawActivities[i]);
                    }
                    this._presences[xp.InternalUser.Id] = xp;
                }
            }

            var exists = this._guilds.TryGetValue(Guild.Id, out var foundGuild);

            Guild.Discord = this;
            Guild.IsUnavailable = false;
            var eventGuild = Guild;
            if (exists)
                Guild = foundGuild;

            if (Guild._channels == null)
                Guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if (Guild._threads == null)
                Guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
            if (Guild._roles == null)
                Guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (Guild._threads == null)
                Guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
            if (Guild._stickers == null)
                Guild._stickers = new ConcurrentDictionary<ulong, DiscordSticker>();
            if (Guild._emojis == null)
                Guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (Guild._voiceStates == null)
                Guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (Guild._members == null)
                Guild._members = new ConcurrentDictionary<ulong, DiscordMember>();
            if (Guild._scheduledEvents == null)
                Guild._scheduledEvents = new ConcurrentDictionary<ulong, DiscordScheduledEvent>();

            this.UpdateCachedGuild(eventGuild, RawMembers);

            Guild.JoinedAt = eventGuild.JoinedAt;
            Guild.IsLarge = eventGuild.IsLarge;
            Guild.MemberCount = Math.Max(eventGuild.MemberCount, Guild._members.Count);
            Guild.IsUnavailable = eventGuild.IsUnavailable;
            Guild.PremiumSubscriptionCount = eventGuild.PremiumSubscriptionCount;
            Guild.PremiumTier = eventGuild.PremiumTier;
            Guild.BannerHash = eventGuild.BannerHash;
            Guild.VanityUrlCode = eventGuild.VanityUrlCode;
            Guild.Description = eventGuild.Description;
            Guild.IsNsfw = eventGuild.IsNsfw;

            foreach (var kvp in eventGuild._voiceStates) Guild._voiceStates[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._channels) Guild._channels[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._roles) Guild._roles[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._emojis) Guild._emojis[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._threads) Guild._threads[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._stickers) Guild._stickers[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._stageInstances) Guild._stageInstances[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._scheduledEvents) Guild._scheduledEvents[kvp.Key] = kvp.Value;

            foreach (var xc in Guild._channels.Values)
            {
                xc.GuildId = Guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channelId = xc.Id;
                }
            }
            foreach (var xt in Guild._threads.Values)
            {
                xt.GuildId = Guild.Id;
                xt.Discord = this;
            }
            foreach (var xe in Guild._emojis.Values)
                xe.Discord = this;
            foreach (var xs in Guild._stickers.Values)
                xs.Discord = this;
            foreach (var xvs in Guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xsi in Guild._stageInstances.Values)
            {
                xsi.Discord = this;
                xsi.GuildId = Guild.Id;
            }
            foreach (var xr in Guild._roles.Values)
            {
                xr.Discord = this;
                xr._guildId = Guild.Id;
            }
            foreach (var xse in Guild._scheduledEvents.Values)
            {
                xse.Discord = this;
                xse.GuildId = Guild.Id;
                if (xse.Creator != null)
                    xse.Creator.Discord = this;
            }

            var old = Volatile.Read(ref this._guildDownloadCompleted);
            var dcompl = this._guilds.Values.All(Xg => !Xg.IsUnavailable);
            Volatile.Write(ref this._guildDownloadCompleted, dcompl);

            if (exists)
                await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs(this.ServiceProvider) { Guild = Guild }).ConfigureAwait(false);
            else
                await this._guildCreated.InvokeAsync(this, new GuildCreateEventArgs(this.ServiceProvider) { Guild = Guild }).ConfigureAwait(false);

            if (dcompl && !old)
                await this._guildDownloadCompletedEv.InvokeAsync(this, new GuildDownloadCompletedEventArgs(this.Guilds, this.ServiceProvider)).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild update event.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="RawMembers">The raw members.</param>

        internal async Task OnGuildUpdateEventAsync(DiscordGuild Guild, JArray RawMembers)
        {
            DiscordGuild oldGuild;

            if (!this._guilds.ContainsKey(Guild.Id))
            {
                this._guilds[Guild.Id] = Guild;
                oldGuild = null;
            }
            else
            {
                var gld = this._guilds[Guild.Id];

                oldGuild = new DiscordGuild
                {
                    Discord = gld.Discord,
                    Name = gld.Name,
                    AfkChannelId = gld.AfkChannelId,
                    AfkTimeout = gld.AfkTimeout,
                    ApplicationId = gld.ApplicationId,
                    DefaultMessageNotifications = gld.DefaultMessageNotifications,
                    ExplicitContentFilter = gld.ExplicitContentFilter,
                    RawFeatures = gld.RawFeatures,
                    IconHash = gld.IconHash,
                    Id = gld.Id,
                    IsLarge = gld.IsLarge,
                    IsSynced = gld.IsSynced,
                    IsUnavailable = gld.IsUnavailable,
                    JoinedAt = gld.JoinedAt,
                    MemberCount = gld.MemberCount,
                    MaxMembers = gld.MaxMembers,
                    MaxPresences = gld.MaxPresences,
                    ApproximateMemberCount = gld.ApproximateMemberCount,
                    ApproximatePresenceCount = gld.ApproximatePresenceCount,
                    MaxVideoChannelUsers = gld.MaxVideoChannelUsers,
                    DiscoverySplashHash = gld.DiscoverySplashHash,
                    PreferredLocale = gld.PreferredLocale,
                    MfaLevel = gld.MfaLevel,
                    OwnerId = gld.OwnerId,
                    SplashHash = gld.SplashHash,
                    SystemChannelId = gld.SystemChannelId,
                    SystemChannelFlags = gld.SystemChannelFlags,
                    Description = gld.Description,
                    WidgetEnabled = gld.WidgetEnabled,
                    WidgetChannelId = gld.WidgetChannelId,
                    VerificationLevel = gld.VerificationLevel,
                    RulesChannelId = gld.RulesChannelId,
                    PublicUpdatesChannelId = gld.PublicUpdatesChannelId,
                    VoiceRegionId = gld.VoiceRegionId,
                    IsNsfw = gld.IsNsfw,
                    PremiumProgressBarEnabled = gld.PremiumProgressBarEnabled,
                    PremiumSubscriptionCount = gld.PremiumSubscriptionCount,
                    PremiumTier = gld.PremiumTier,
                    _channels = new ConcurrentDictionary<ulong, DiscordChannel>(),
                    _threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>(),
                    _emojis = new ConcurrentDictionary<ulong, DiscordEmoji>(),
                    _stickers = new ConcurrentDictionary<ulong, DiscordSticker>(),
                    _members = new ConcurrentDictionary<ulong, DiscordMember>(),
                    _roles = new ConcurrentDictionary<ulong, DiscordRole>(),
                    _stageInstances = new ConcurrentDictionary<ulong, DiscordStageInstance>(),
                    _voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>(),
                    _scheduledEvents = new ConcurrentDictionary<ulong, DiscordScheduledEvent>()
                };

                foreach (var kvp in gld._channels) oldGuild._channels[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._threads) oldGuild._threads[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._emojis) oldGuild._emojis[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._stickers) oldGuild._stickers[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._roles) oldGuild._roles[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._voiceStates) oldGuild._voiceStates[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._members) oldGuild._members[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._stageInstances) oldGuild._stageInstances[kvp.Key] = kvp.Value;
                foreach (var kvp in gld._scheduledEvents) oldGuild._scheduledEvents[kvp.Key] = kvp.Value;
            }

            Guild.Discord = this;
            Guild.IsUnavailable = false;
            var eventGuild = Guild;
            Guild = this._guilds[eventGuild.Id];

            if (Guild._channels == null)
                Guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if (Guild._threads == null)
                Guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
            if (Guild._roles == null)
                Guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (Guild._emojis == null)
                Guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (Guild._stickers == null)
                Guild._stickers = new ConcurrentDictionary<ulong, DiscordSticker>();
            if (Guild._voiceStates == null)
                Guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (Guild._stageInstances == null)
                Guild._stageInstances = new ConcurrentDictionary<ulong, DiscordStageInstance>();
            if (Guild._members == null)
                Guild._members = new ConcurrentDictionary<ulong, DiscordMember>();
            if (Guild._scheduledEvents == null)
                Guild._scheduledEvents = new ConcurrentDictionary<ulong, DiscordScheduledEvent>();

            this.UpdateCachedGuild(eventGuild, RawMembers);

            foreach (var xc in Guild._channels.Values)
            {
                xc.GuildId = Guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channelId = xc.Id;
                }
            }
            foreach (var xc in Guild._threads.Values)
            {
                xc.GuildId = Guild.Id;
                xc.Discord = this;
            }
            foreach (var xe in Guild._emojis.Values)
                xe.Discord = this;
            foreach (var xs in Guild._stickers.Values)
                xs.Discord = this;
            foreach (var xvs in Guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xr in Guild._roles.Values)
            {
                xr.Discord = this;
                xr._guildId = Guild.Id;
            }
            foreach (var xsi in Guild._stageInstances.Values)
            {
                xsi.Discord = this;
                xsi.GuildId = Guild.Id;
            }
            foreach (var xse in Guild._scheduledEvents.Values)
            {
                xse.Discord = this;
                xse.GuildId = Guild.Id;
                if (xse.Creator != null)
                    xse.Creator.Discord = this;
            }

            await this._guildUpdated.InvokeAsync(this, new GuildUpdateEventArgs(this.ServiceProvider) { GuildBefore = oldGuild, GuildAfter = Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild delete event.
        /// </summary>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildDeleteEventAsync(DiscordGuild Guild)
        {
            if (Guild.IsUnavailable)
            {
                if (!this._guilds.TryGetValue(Guild.Id, out var gld))
                    return;

                gld.IsUnavailable = true;

                await this._guildUnavailable.InvokeAsync(this, new GuildDeleteEventArgs(this.ServiceProvider) { Guild = Guild, Unavailable = true }).ConfigureAwait(false);
            }
            else
            {
                if (!this._guilds.TryRemove(Guild.Id, out var gld))
                    return;

                await this._guildDeleted.InvokeAsync(this, new GuildDeleteEventArgs(this.ServiceProvider) { Guild = gld }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handles the guild sync event.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="IsLarge">If true, is large.</param>
        /// <param name="RawMembers">The raw members.</param>
        /// <param name="Presences">The presences.</param>

        internal async Task OnGuildSyncEventAsync(DiscordGuild Guild, bool IsLarge, JArray RawMembers, IEnumerable<DiscordPresence> Presences)
        {
            Presences = Presences.Select(Xp => { Xp.Discord = this; Xp.Activity = new DiscordActivity(Xp.RawActivity); return Xp; });
            foreach (var xp in Presences)
                this._presences[xp.InternalUser.Id] = xp;

            Guild.IsSynced = true;
            Guild.IsLarge = IsLarge;

            this.UpdateCachedGuild(Guild, RawMembers);

            await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs(this.ServiceProvider) { Guild = Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild emojis update event.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="NewEmojis">The new emojis.</param>

        internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild Guild, IEnumerable<DiscordEmoji> NewEmojis)
        {
            var oldEmojis = new ConcurrentDictionary<ulong, DiscordEmoji>(Guild._emojis);
            Guild._emojis.Clear();

            foreach (var emoji in NewEmojis)
            {
                emoji.Discord = this;
                Guild._emojis[emoji.Id] = emoji;
            }

            var ea = new GuildEmojisUpdateEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                EmojisAfter = Guild.Emojis,
                EmojisBefore = new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(oldEmojis)
            };
            await this._guildEmojisUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the stickers updated.
        /// </summary>
        /// <param name="NewStickers">The new stickers.</param>
        /// <param name="Raw">The raw.</param>

        internal async Task OnStickersUpdatedAsync(IEnumerable<DiscordSticker> NewStickers, JObject Raw)
        {
            var guild = this.InternalGetCachedGuild((ulong)Raw["guild_id"]);
            var oldStickers = new ConcurrentDictionary<ulong, DiscordSticker>(guild._stickers);
            guild._stickers.Clear();

            foreach (var nst in NewStickers)
            {
                if (nst.User is not null)
                {
                    nst.User.Discord = this;
                    this.UserCache.AddOrUpdate(nst.User.Id, nst.User, (Old, New) => New);
                }
                nst.Discord = this;

                guild._stickers[nst.Id] = nst;
            }

            var sea = new GuildStickersUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                StickersBefore = oldStickers,
                StickersAfter = guild.Stickers
            };

            await this._guildStickersUpdated.InvokeAsync(this, sea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild integrations update event.
        /// </summary>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild Guild)
        {
            var ea = new GuildIntegrationsUpdateEventArgs(this.ServiceProvider)
            {
                Guild = Guild
            };
            await this._guildIntegrationsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Ban

        /// <summary>
        /// Handles the guild ban add event.
        /// </summary>
        /// <param name="User">The user.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildBanAddEventAsync(TransportUser User, DiscordGuild Guild)
        {
            var usr = new DiscordUser(User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(User.Id, usr, (Id, Old) =>
            {
                Old.Username = usr.Username;
                Old.Discriminator = usr.Discriminator;
                Old.AvatarHash = usr.AvatarHash;
                return Old;
            });

            if (!Guild.Members.TryGetValue(User.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guildId = Guild.Id };
            var ea = new GuildBanAddEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Member = mbr
            };
            await this._guildBanAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild ban remove event.
        /// </summary>
        /// <param name="User">The user.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildBanRemoveEventAsync(TransportUser User, DiscordGuild Guild)
        {
            var usr = new DiscordUser(User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(User.Id, usr, (Id, Old) =>
            {
                Old.Username = usr.Username;
                Old.Discriminator = usr.Discriminator;
                Old.AvatarHash = usr.AvatarHash;
                return Old;
            });

            if (!Guild.Members.TryGetValue(User.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guildId = Guild.Id };
            var ea = new GuildBanRemoveEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Member = mbr
            };
            await this._guildBanRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Scheduled Event

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventCreated"/> event.
        /// </summary>
        /// <param name="scheduled_event">The created event.</param>
        /// <param name="Guild">The target guild.</param>
        internal async Task OnGuildScheduledEventCreateEventAsync(DiscordScheduledEvent ScheduledEvent, DiscordGuild Guild)
        {
            ScheduledEvent.Discord = this;

            Guild._scheduledEvents.AddOrUpdate(ScheduledEvent.Id, ScheduledEvent, (Old, NewScheduledEvent) => NewScheduledEvent);

            if (ScheduledEvent.Creator != null)
            {
                ScheduledEvent.Creator.Discord = this;
                this.UserCache.AddOrUpdate(ScheduledEvent.Creator.Id, ScheduledEvent.Creator, (Id, Old) =>
                {
                    Old.Username = ScheduledEvent.Creator.Username;
                    Old.Discriminator = ScheduledEvent.Creator.Discriminator;
                    Old.AvatarHash = ScheduledEvent.Creator.AvatarHash;
                    Old.Flags = ScheduledEvent.Creator.Flags;
                    return Old;
                });
            }

            await this._guildScheduledEventCreated.InvokeAsync(this, new GuildScheduledEventCreateEventArgs(this.ServiceProvider) { ScheduledEvent = ScheduledEvent, Guild = ScheduledEvent.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventUpdated"/> event.
        /// </summary>
        /// <param name="scheduled_event">The updated event.</param>
        /// <param name="Guild">The target guild.</param>
        internal async Task OnGuildScheduledEventUpdateEventAsync(DiscordScheduledEvent ScheduledEvent, DiscordGuild Guild)
        {
            if (Guild == null)
                return;

            DiscordScheduledEvent oldEvent;
            if (!Guild._scheduledEvents.ContainsKey(ScheduledEvent.Id))
            {
                oldEvent = null;
            }
            else
            {
                var ev = Guild._scheduledEvents[ScheduledEvent.Id];
                oldEvent = new DiscordScheduledEvent
                {
                    Id = ev.Id,
                    ChannelId = ev.ChannelId,
                    EntityId = ev.EntityId,
                    EntityMetadata = ev.EntityMetadata,
                    CreatorId = ev.CreatorId,
                    Creator = ev.Creator,
                    Discord = this,
                    Description = ev.Description,
                    EntityType = ev.EntityType,
                    ScheduledStartTimeRaw = ev.ScheduledStartTimeRaw,
                    ScheduledEndTimeRaw = ev.ScheduledEndTimeRaw,
                    GuildId = ev.GuildId,
                    Status = ev.Status,
                    Name = ev.Name,
                    UserCount = ev.UserCount
                };

            }
            if (ScheduledEvent.Creator != null)
            {
                ScheduledEvent.Creator.Discord = this;
                this.UserCache.AddOrUpdate(ScheduledEvent.Creator.Id, ScheduledEvent.Creator, (Id, Old) =>
                {
                    Old.Username = ScheduledEvent.Creator.Username;
                    Old.Discriminator = ScheduledEvent.Creator.Discriminator;
                    Old.AvatarHash = ScheduledEvent.Creator.AvatarHash;
                    Old.Flags = ScheduledEvent.Creator.Flags;
                    return Old;
                });
            }

            if (ScheduledEvent.Status == ScheduledEventStatus.Completed)
            {
                Guild._scheduledEvents.TryRemove(ScheduledEvent.Id, out var deletedEvent);
                await this._guildScheduledEventDeleted.InvokeAsync(this, new GuildScheduledEventDeleteEventArgs(this.ServiceProvider) { ScheduledEvent = ScheduledEvent, Guild = Guild, Reason = ScheduledEventStatus.Completed }).ConfigureAwait(false);
            }
            else if (ScheduledEvent.Status == ScheduledEventStatus.Canceled)
            {
                Guild._scheduledEvents.TryRemove(ScheduledEvent.Id, out var deletedEvent);
                ScheduledEvent.Status = ScheduledEventStatus.Canceled;
                await this._guildScheduledEventDeleted.InvokeAsync(this, new GuildScheduledEventDeleteEventArgs(this.ServiceProvider) { ScheduledEvent = ScheduledEvent, Guild = Guild, Reason = ScheduledEventStatus.Canceled }).ConfigureAwait(false);
            }
            else
            {
                this.UpdateScheduledEvent(ScheduledEvent, Guild);
                await this._guildScheduledEventUpdated.InvokeAsync(this, new GuildScheduledEventUpdateEventArgs(this.ServiceProvider) { ScheduledEventBefore = oldEvent, ScheduledEventAfter = ScheduledEvent, Guild = Guild }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventDeleted"/> event.
        /// </summary>
        /// <param name="scheduled_event">The deleted event.</param>
        /// <param name="Guild">The target guild.</param>
        internal async Task OnGuildScheduledEventDeleteEventAsync(DiscordScheduledEvent ScheduledEvent, DiscordGuild Guild)
        {
            ScheduledEvent.Discord = this;

            if (ScheduledEvent.Status == ScheduledEventStatus.Scheduled)
                ScheduledEvent.Status = ScheduledEventStatus.Canceled;

            if (ScheduledEvent.Creator != null)
            {
                ScheduledEvent.Creator.Discord = this;
                this.UserCache.AddOrUpdate(ScheduledEvent.Creator.Id, ScheduledEvent.Creator, (Id, Old) =>
                {
                    Old.Username = ScheduledEvent.Creator.Username;
                    Old.Discriminator = ScheduledEvent.Creator.Discriminator;
                    Old.AvatarHash = ScheduledEvent.Creator.AvatarHash;
                    Old.Flags = ScheduledEvent.Creator.Flags;
                    return Old;
                });
            }

            await this._guildScheduledEventDeleted.InvokeAsync(this, new GuildScheduledEventDeleteEventArgs(this.ServiceProvider) { ScheduledEvent = ScheduledEvent, Guild = ScheduledEvent.Guild, Reason = ScheduledEvent.Status }).ConfigureAwait(false);
            Guild._scheduledEvents.TryRemove(ScheduledEvent.Id, out var deletedEvent);
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventUserAdded"/> event.
        /// <param name="guild_scheduled_event_id">The target event.</param>
        /// <param name="user_id">The added user id.</param>
        /// <param name="Guild">The target guild.</param>
        /// </summary>
        internal async Task OnGuildScheduledEventUserAddedEventAsync(ulong GuildScheduledEventId, ulong UserId, DiscordGuild Guild)
        {
            var scheduledEvent = this.InternalGetCachedScheduledEvent(GuildScheduledEventId) ?? this.UpdateScheduledEvent(new DiscordScheduledEvent
            {
                Id = GuildScheduledEventId,
                GuildId = Guild.Id,
                Discord = this,
                UserCount = 0
            }, Guild);

            scheduledEvent.UserCount++;
            scheduledEvent.Discord = this;
            Guild.Discord = this;

            var user = this.GetUserAsync(UserId, true).Result;
            user.Discord = this;
            var member = Guild.Members.TryGetValue(UserId, out var mem) ? mem : Guild.GetMemberAsync(UserId).Result;
            member.Discord = this;

            await this._guildScheduledEventUserAdded.InvokeAsync(this, new GuildScheduledEventUserAddEventArgs(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = Guild, User = user, Member = member }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventUserRemoved"/> event.
        /// <param name="guild_scheduled_event_id">The target event.</param>
        /// <param name="user_id">The removed user id.</param>
        /// <param name="Guild">The target guild.</param>
        /// </summary>
        internal async Task OnGuildScheduledEventUserRemovedEventAsync(ulong GuildScheduledEventId, ulong UserId, DiscordGuild Guild)
        {
            var scheduledEvent = this.InternalGetCachedScheduledEvent(GuildScheduledEventId) ?? this.UpdateScheduledEvent(new DiscordScheduledEvent
            {
                Id = GuildScheduledEventId,
                GuildId = Guild.Id,
                Discord = this,
                UserCount = 0
            }, Guild);

            scheduledEvent.UserCount = scheduledEvent.UserCount == 0 ? 0 : scheduledEvent.UserCount - 1;
            scheduledEvent.Discord = this;
            Guild.Discord = this;

            var user = this.GetUserAsync(UserId, true).Result;
            user.Discord = this;
            var member = Guild.Members.TryGetValue(UserId, out var mem) ? mem : Guild.GetMemberAsync(UserId).Result;
            member.Discord = this;

            await this._guildScheduledEventUserRemoved.InvokeAsync(this, new GuildScheduledEventUserRemoveEventArgs(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = Guild, User = user, Member = member }).ConfigureAwait(false);
        }

        #endregion

        #region Guild Integration

        /// <summary>
        /// Handles the guild integration create event.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="Integration">The integration.</param>

        internal async Task OnGuildIntegrationCreateEventAsync(DiscordGuild Guild, DiscordIntegration Integration)
        {
            Integration.Discord = this;

            await this._guildIntegrationCreated.InvokeAsync(this, new GuildIntegrationCreateEventArgs(this.ServiceProvider) { Integration = Integration, Guild = Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild integration update event.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="Integration">The integration.</param>

        internal async Task OnGuildIntegrationUpdateEventAsync(DiscordGuild Guild, DiscordIntegration Integration)
        {
            Integration.Discord = this;

            await this._guildIntegrationUpdated.InvokeAsync(this, new GuildIntegrationUpdateEventArgs(this.ServiceProvider) { Integration = Integration, Guild = Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild integration delete event.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="integration_id">The integration_id.</param>
        /// <param name="application_id">The application_id.</param>

        internal async Task OnGuildIntegrationDeleteEventAsync(DiscordGuild Guild, ulong IntegrationId, ulong? ApplicationId)
            => await this._guildIntegrationDeleted.InvokeAsync(this, new GuildIntegrationDeleteEventArgs(this.ServiceProvider) { Guild = Guild, IntegrationId = IntegrationId, ApplicationId = ApplicationId }).ConfigureAwait(false);

        #endregion

        #region Guild Member

        /// <summary>
        /// Handles the guild member add event.
        /// </summary>
        /// <param name="Member">The member.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildMemberAddEventAsync(TransportMember Member, DiscordGuild Guild)
        {
            var usr = new DiscordUser(Member.User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(Member.User.Id, usr, (Id, Old) =>
            {
                Old.Username = usr.Username;
                Old.Discriminator = usr.Discriminator;
                Old.AvatarHash = usr.AvatarHash;
                return Old;
            });

            var mbr = new DiscordMember(Member)
            {
                Discord = this,
                _guildId = Guild.Id
            };

            Guild._members[mbr.Id] = mbr;
            Guild.MemberCount++;

            var ea = new GuildMemberAddEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Member = mbr
            };
            await this._guildMemberAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild member remove event.
        /// </summary>
        /// <param name="User">The user.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildMemberRemoveEventAsync(TransportUser User, DiscordGuild Guild)
        {
            var usr = new DiscordUser(User);

            if (!Guild._members.TryRemove(User.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guildId = Guild.Id };
            Guild.MemberCount--;

            _ = this.UserCache.AddOrUpdate(User.Id, usr, (Old, New) => New);

            var ea = new GuildMemberRemoveEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Member = mbr
            };
            await this._guildMemberRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild member update event.
        /// </summary>
        /// <param name="Member">The member.</param>
        /// <param name="Guild">The guild.</param>
        /// <param name="Roles">The roles.</param>
        /// <param name="Nick">The nick.</param>
        /// <param name="Pending">If true, pending.</param>

        internal async Task OnGuildMemberUpdateEventAsync(TransportMember Member, DiscordGuild Guild, IEnumerable<ulong> Roles, string Nick, bool? Pending)
        {
            var usr = new DiscordUser(Member.User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(usr.Id, usr, (Id, Old) =>
            {
                Old.Username = usr.Username;
                Old.Discriminator = usr.Discriminator;
                Old.AvatarHash = usr.AvatarHash;
                return Old;
            });

            if (!Guild.Members.TryGetValue(Member.User.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guildId = Guild.Id };

            var nickOld = mbr.Nickname;
            var pendingOld = mbr.IsPending;
            var rolesOld = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));
            var cduOld = mbr.CommunicationDisabledUntil;
            mbr._avatarHash = Member.AvatarHash;
            mbr.GuildAvatarHash = Member.GuildAvatarHash;
            mbr.Nickname = Nick;
            mbr.IsPending = Pending;
            mbr.CommunicationDisabledUntil = Member.CommunicationDisabledUntil;
            mbr._roleIds.Clear();
            mbr._roleIds.AddRange(Roles);

            var ea = new GuildMemberUpdateEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Member = mbr,

                NicknameAfter = mbr.Nickname,
                RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),
                PendingAfter = mbr.IsPending,
                TimeoutAfter = mbr.CommunicationDisabledUntil,

                NicknameBefore = nickOld,
                RolesBefore = rolesOld,
                PendingBefore = pendingOld,
                TimeoutBefore = cduOld
            };
            await this._guildMemberUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild members chunk event.
        /// </summary>
        /// <param name="Dat">The dat.</param>

        internal async Task OnGuildMembersChunkEventAsync(JObject Dat)
        {
            var guild = this.Guilds[(ulong)Dat["guild_id"]];
            var chunkIndex = (int)Dat["chunk_index"];
            var chunkCount = (int)Dat["chunk_count"];
            var nonce = (string)Dat["nonce"];

            var mbrs = new HashSet<DiscordMember>();
            var pres = new HashSet<DiscordPresence>();

            var members = Dat["members"].ToObject<TransportMember[]>();

            var memCount = members.Count();
            for (var i = 0; i < memCount; i++)
            {
                var mbr = new DiscordMember(members[i]) { Discord = this, _guildId = guild.Id };

                if (!this.UserCache.ContainsKey(mbr.Id))
                    this.UserCache[mbr.Id] = new DiscordUser(members[i].User) { Discord = this };

                guild._members[mbr.Id] = mbr;

                mbrs.Add(mbr);
            }

            guild.MemberCount = guild._members.Count;

            var ea = new GuildMembersChunkEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Members = new ReadOnlySet<DiscordMember>(mbrs),
                ChunkIndex = chunkIndex,
                ChunkCount = chunkCount,
                Nonce = nonce,
            };

            if (Dat["presences"] != null)
            {
                var presences = Dat["presences"].ToObject<DiscordPresence[]>();

                var presCount = presences.Count();
                for (var i = 0; i < presCount; i++)
                {
                    var xp = presences[i];
                    xp.Discord = this;
                    xp.Activity = new DiscordActivity(xp.RawActivity);

                    if (xp.RawActivities != null)
                    {
                        xp._internalActivities = new DiscordActivity[xp.RawActivities.Length];
                        for (var j = 0; j < xp.RawActivities.Length; j++)
                            xp._internalActivities[j] = new DiscordActivity(xp.RawActivities[j]);
                    }

                    pres.Add(xp);
                }

                ea.Presences = new ReadOnlySet<DiscordPresence>(pres);
            }

            if (Dat["not_found"] != null)
            {
                var nf = Dat["not_found"].ToObject<ISet<ulong>>();
                ea.NotFound = new ReadOnlySet<ulong>(nf);
            }

            await this._guildMembersChunked.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Role

        /// <summary>
        /// Handles the guild role create event.
        /// </summary>
        /// <param name="Role">The role.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildRoleCreateEventAsync(DiscordRole Role, DiscordGuild Guild)
        {
            Role.Discord = this;
            Role._guildId = Guild.Id;

            Guild._roles[Role.Id] = Role;

            var ea = new GuildRoleCreateEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Role = Role
            };
            await this._guildRoleCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild role update event.
        /// </summary>
        /// <param name="Role">The role.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildRoleUpdateEventAsync(DiscordRole Role, DiscordGuild Guild)
        {
            var newRole = Guild.GetRole(Role.Id);
            var oldRole = new DiscordRole
            {
                _guildId = Guild.Id,
                _color = newRole._color,
                Discord = this,
                IsHoisted = newRole.IsHoisted,
                Id = newRole.Id,
                IsManaged = newRole.IsManaged,
                IsMentionable = newRole.IsMentionable,
                Name = newRole.Name,
                Permissions = newRole.Permissions,
                Position = newRole.Position
            };

            newRole._guildId = Guild.Id;
            newRole._color = Role._color;
            newRole.IsHoisted = Role.IsHoisted;
            newRole.IsManaged = Role.IsManaged;
            newRole.IsMentionable = Role.IsMentionable;
            newRole.Name = Role.Name;
            newRole.Permissions = Role.Permissions;
            newRole.Position = Role.Position;

            var ea = new GuildRoleUpdateEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                RoleAfter = newRole,
                RoleBefore = oldRole
            };
            await this._guildRoleUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild role delete event.
        /// </summary>
        /// <param name="RoleId">The role id.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnGuildRoleDeleteEventAsync(ulong RoleId, DiscordGuild Guild)
        {
            if (!Guild._roles.TryRemove(RoleId, out var role))
                this.Logger.LogWarning($"Attempted to delete a nonexistent role ({RoleId}) from guild ({Guild}).");

            var ea = new GuildRoleDeleteEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Role = role
            };
            await this._guildRoleDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Invite

        /// <summary>
        /// Handles the invite create event.
        /// </summary>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Invite">The invite.</param>

        internal async Task OnInviteCreateEventAsync(ulong ChannelId, ulong GuildId, DiscordInvite Invite)
        {
            var guild = this.InternalGetCachedGuild(GuildId);
            var channel = this.InternalGetCachedChannel(ChannelId);

            Invite.Discord = this;

            if (Invite.Inviter is not null)
            {
                Invite.Inviter.Discord = this;
                this.UserCache.AddOrUpdate(Invite.Inviter.Id, Invite.Inviter, (Old, New) => New);
            }

            guild._invites[Invite.Code] = Invite;

            var ea = new InviteCreateEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                Guild = guild,
                Invite = Invite
            };
            await this._inviteCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the invite delete event.
        /// </summary>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Dat">The dat.</param>

        internal async Task OnInviteDeleteEventAsync(ulong ChannelId, ulong GuildId, JToken Dat)
        {
            var guild = this.InternalGetCachedGuild(GuildId);
            var channel = this.InternalGetCachedChannel(ChannelId);

            if (!guild._invites.TryRemove(Dat["code"].ToString(), out var invite))
            {
                invite = Dat.ToObject<DiscordInvite>();
                invite.Discord = this;
            }

            invite.IsRevoked = true;

            var ea = new InviteDeleteEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                Guild = guild,
                Invite = invite
            };
            await this._inviteDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Message

        /// <summary>
        /// Handles the message ack event.
        /// </summary>
        /// <param name="Chn">The chn.</param>
        /// <param name="MessageId">The message id.</param>

        internal async Task OnMessageAckEventAsync(DiscordChannel Chn, ulong MessageId)
        {
            if (this.MessageCache == null || !this.MessageCache.TryGet(Xm => Xm.Id == MessageId && Xm.ChannelId == Chn.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = MessageId,
                    ChannelId = Chn.Id,
                    Discord = this,
                };
            }

            await this._messageAcknowledged.InvokeAsync(this, new MessageAcknowledgeEventArgs(this.ServiceProvider) { Message = msg }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message create event.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="Author">The author.</param>
        /// <param name="Member">The member.</param>
        /// <param name="ReferenceAuthor">The reference author.</param>
        /// <param name="ReferenceMember">The reference member.</param>

        internal async Task OnMessageCreateEventAsync(DiscordMessage Message, TransportUser Author, TransportMember Member, TransportUser ReferenceAuthor, TransportMember ReferenceMember)
        {
            Message.Discord = this;
            this.PopulateMessageReactionsAndCache(Message, Author, Member);
            Message.PopulateMentions();

            if (Message.Channel == null && Message.ChannelId == default)
                this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Channel which the last message belongs to is not in cache - cache state might be invalid!");

            if (Message.ReferencedMessage != null)
            {
                Message.ReferencedMessage.Discord = this;
                this.PopulateMessageReactionsAndCache(Message.ReferencedMessage, ReferenceAuthor, ReferenceMember);
                Message.ReferencedMessage.PopulateMentions();
            }

            foreach (var sticker in Message.Stickers)
                sticker.Discord = this;

            var ea = new MessageCreateEventArgs(this.ServiceProvider)
            {
                Message = Message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(Message._mentionedUsers),
                MentionedRoles = Message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(Message._mentionedRoles) : null,
                MentionedChannels = Message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(Message._mentionedChannels) : null
            };
            await this._messageCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message update event.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="Author">The author.</param>
        /// <param name="Member">The member.</param>
        /// <param name="ReferenceAuthor">The reference author.</param>
        /// <param name="ReferenceMember">The reference member.</param>

        internal async Task OnMessageUpdateEventAsync(DiscordMessage Message, TransportUser Author, TransportMember Member, TransportUser ReferenceAuthor, TransportMember ReferenceMember)
        {
            DiscordGuild guild;

            Message.Discord = this;
            var eventMessage = Message;

            DiscordMessage oldmsg = null;
            if (this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(Xm => Xm.Id == eventMessage.Id && Xm.ChannelId == eventMessage.ChannelId, out Message))
            {
                Message = eventMessage;
                this.PopulateMessageReactionsAndCache(Message, Author, Member);
                guild = Message.Channel?.Guild;

                if (Message.ReferencedMessage != null)
                {
                    Message.ReferencedMessage.Discord = this;
                    this.PopulateMessageReactionsAndCache(Message.ReferencedMessage, ReferenceAuthor, ReferenceMember);
                    Message.ReferencedMessage.PopulateMentions();
                }
            }
            else
            {
                oldmsg = new DiscordMessage(Message);

                guild = Message.Channel?.Guild;
                Message.EditedTimestampRaw = eventMessage.EditedTimestampRaw;
                if (eventMessage.Content != null)
                    Message.Content = eventMessage.Content;
                Message._embeds.Clear();
                Message._embeds.AddRange(eventMessage._embeds);
                Message.Pinned = eventMessage.Pinned;
                Message.IsTts = eventMessage.IsTts;
            }

            Message.PopulateMentions();

            var ea = new MessageUpdateEventArgs(this.ServiceProvider)
            {
                Message = Message,
                MessageBefore = oldmsg,
                MentionedUsers = new ReadOnlyCollection<DiscordUser>(Message._mentionedUsers),
                MentionedRoles = Message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(Message._mentionedRoles) : null,
                MentionedChannels = Message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(Message._mentionedChannels) : null
            };
            await this._messageUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message delete event.
        /// </summary>
        /// <param name="MessageId">The message id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>

        internal async Task OnMessageDeleteEventAsync(ulong MessageId, ulong ChannelId, ulong? GuildId)
        {
            var channel = this.InternalGetCachedChannel(ChannelId) ?? this.InternalGetCachedThread(ChannelId);
            var guild = this.InternalGetCachedGuild(GuildId);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(Xm => Xm.Id == MessageId && Xm.ChannelId == ChannelId, out var msg))
            {
                msg = new DiscordMessage
                {

                    Id = MessageId,
                    ChannelId = ChannelId,
                    Discord = this,
                };
            }

            if (this.Configuration.MessageCacheSize > 0)
                this.MessageCache?.Remove(Xm => Xm.Id == msg.Id && Xm.ChannelId == ChannelId);

            var ea = new MessageDeleteEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                Message = msg,
                Guild = guild
            };
            await this._messageDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message bulk delete event.
        /// </summary>
        /// <param name="MessageIds">The message ids.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>

        internal async Task OnMessageBulkDeleteEventAsync(ulong[] MessageIds, ulong ChannelId, ulong? GuildId)
        {
            var channel = this.InternalGetCachedChannel(ChannelId) ?? this.InternalGetCachedThread(ChannelId);

            var msgs = new List<DiscordMessage>(MessageIds.Length);
            foreach (var messageId in MessageIds)
            {
                if (channel == null
                    || this.Configuration.MessageCacheSize == 0
                    || this.MessageCache == null
                    || !this.MessageCache.TryGet(Xm => Xm.Id == messageId && Xm.ChannelId == ChannelId, out var msg))
                {
                    msg = new DiscordMessage
                    {
                        Id = messageId,
                        ChannelId = ChannelId,
                        Discord = this,
                    };
                }
                if (this.Configuration.MessageCacheSize > 0)
                    this.MessageCache?.Remove(Xm => Xm.Id == msg.Id && Xm.ChannelId == ChannelId);
                msgs.Add(msg);
            }

            var guild = this.InternalGetCachedGuild(GuildId);

            var ea = new MessageBulkDeleteEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                Messages = new ReadOnlyCollection<DiscordMessage>(msgs),
                Guild = guild
            };
            await this._messagesBulkDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Message Reaction

        /// <summary>
        /// Handles the message reaction add.
        /// </summary>
        /// <param name="UserId">The user id.</param>
        /// <param name="MessageId">The message id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Mbr">The mbr.</param>
        /// <param name="Emoji">The emoji.</param>

        internal async Task OnMessageReactionAddAsync(ulong UserId, ulong MessageId, ulong ChannelId, ulong? GuildId, TransportMember Mbr, DiscordEmoji Emoji)
        {
            var channel = this.InternalGetCachedChannel(ChannelId) ?? this.InternalGetCachedThread(ChannelId);
            var guild = this.InternalGetCachedGuild(GuildId);
            Emoji.Discord = this;

            var usr = this.UpdateUser(new DiscordUser { Id = UserId, Discord = this }, GuildId, guild, Mbr);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(Xm => Xm.Id == MessageId && Xm.ChannelId == ChannelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = MessageId,
                    ChannelId = ChannelId,
                    Discord = this,
                    _reactions = new List<DiscordReaction>()
                };
            }

            var react = msg._reactions.FirstOrDefault(Xr => Xr.Emoji == Emoji);
            if (react == null)
            {
                msg._reactions.Add(react = new DiscordReaction
                {
                    Count = 1,
                    Emoji = Emoji,
                    IsMe = this.CurrentUser.Id == UserId
                });
            }
            else
            {
                react.Count++;
                react.IsMe |= this.CurrentUser.Id == UserId;
            }

            var ea = new MessageReactionAddEventArgs(this.ServiceProvider)
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = Emoji
            };
            await this._messageReactionAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message reaction remove.
        /// </summary>
        /// <param name="UserId">The user id.</param>
        /// <param name="MessageId">The message id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Emoji">The emoji.</param>

        internal async Task OnMessageReactionRemoveAsync(ulong UserId, ulong MessageId, ulong ChannelId, ulong? GuildId, DiscordEmoji Emoji)
        {
            var channel = this.InternalGetCachedChannel(ChannelId) ?? this.InternalGetCachedThread(ChannelId);

            Emoji.Discord = this;

            if (!this.UserCache.TryGetValue(UserId, out var usr))
                usr = new DiscordUser { Id = UserId, Discord = this };

            if (channel?.Guild != null)
                usr = channel.Guild.Members.TryGetValue(UserId, out var member)
                    ? member
                    : new DiscordMember(usr) { Discord = this, _guildId = channel.GuildId.Value };

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(Xm => Xm.Id == MessageId && Xm.ChannelId == ChannelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = MessageId,
                    ChannelId = ChannelId,
                    Discord = this
                };
            }

            var react = msg._reactions?.FirstOrDefault(Xr => Xr.Emoji == Emoji);
            if (react != null)
            {
                react.Count--;
                react.IsMe &= this.CurrentUser.Id != UserId;

                if (msg._reactions != null && react.Count <= 0) // shit happens
                    for (var i = 0; i < msg._reactions.Count; i++)
                        if (msg._reactions[i].Emoji == Emoji)
                        {
                            msg._reactions.RemoveAt(i);
                            break;
                        }
            }

            var guild = this.InternalGetCachedGuild(GuildId);

            var ea = new MessageReactionRemoveEventArgs(this.ServiceProvider)
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = Emoji
            };
            await this._messageReactionRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message reaction remove all.
        /// </summary>
        /// <param name="MessageId">The message id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>

        internal async Task OnMessageReactionRemoveAllAsync(ulong MessageId, ulong ChannelId, ulong? GuildId)
        {
            var channel = this.InternalGetCachedChannel(ChannelId) ?? this.InternalGetCachedThread(ChannelId);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(Xm => Xm.Id == MessageId && Xm.ChannelId == ChannelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = MessageId,
                    ChannelId = ChannelId,
                    Discord = this
                };
            }

            msg._reactions?.Clear();

            var guild = this.InternalGetCachedGuild(GuildId);

            var ea = new MessageReactionsClearEventArgs(this.ServiceProvider)
            {
                Message = msg
            };

            await this._messageReactionsCleared.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message reaction remove emoji.
        /// </summary>
        /// <param name="MessageId">The message id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Dat">The dat.</param>

        internal async Task OnMessageReactionRemoveEmojiAsync(ulong MessageId, ulong ChannelId, ulong GuildId, JToken Dat)
        {
            var guild = this.InternalGetCachedGuild(GuildId);
            var channel = this.InternalGetCachedChannel(ChannelId) ?? this.InternalGetCachedThread(ChannelId);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(Xm => Xm.Id == MessageId && Xm.ChannelId == ChannelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = MessageId,
                    ChannelId = ChannelId,
                    Discord = this
                };
            }

            var partialEmoji = Dat.ToObject<DiscordEmoji>();

            if (!guild._emojis.TryGetValue(partialEmoji.Id, out var emoji))
            {
                emoji = partialEmoji;
                emoji.Discord = this;
            }

            msg._reactions?.RemoveAll(R => R.Emoji.Equals(emoji));

            var ea = new MessageReactionRemoveEmojiEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                Guild = guild,
                Message = msg,
                Emoji = emoji
            };

            await this._messageReactionRemovedEmoji.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Stage Instance

        /// <summary>
        /// Dispatches the <see cref="StageInstanceCreated"/> event.
        /// </summary>
        /// <param name="Stage">The created stage instance.</param>
        internal async Task OnStageInstanceCreateEventAsync(DiscordStageInstance Stage)
        {
            Stage.Discord = this;

            var guild = this.InternalGetCachedGuild(Stage.GuildId);
            guild._stageInstances[Stage.Id] = Stage;

            await this._stageInstanceCreated.InvokeAsync(this, new StageInstanceCreateEventArgs(this.ServiceProvider) { StageInstance = Stage, Guild = Stage.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="StageInstanceUpdated"/> event.
        /// </summary>
        /// <param name="Stage">The updated stage instance.</param>
        internal async Task OnStageInstanceUpdateEventAsync(DiscordStageInstance Stage)
        {
            Stage.Discord = this;
            var guild = this.InternalGetCachedGuild(Stage.GuildId);
            guild._stageInstances[Stage.Id] = Stage;

            await this._stageInstanceUpdated.InvokeAsync(this, new StageInstanceUpdateEventArgs(this.ServiceProvider) { StageInstance = Stage, Guild = Stage.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="StageInstanceDeleted"/> event.
        /// </summary>
        /// <param name="Stage">The deleted stage instance.</param>
        internal async Task OnStageInstanceDeleteEventAsync(DiscordStageInstance Stage)
        {
            Stage.Discord = this;
            var guild = this.InternalGetCachedGuild(Stage.GuildId);
            guild._stageInstances[Stage.Id] = Stage;

            await this._stageInstanceDeleted.InvokeAsync(this, new StageInstanceDeleteEventArgs(this.ServiceProvider) { StageInstance = Stage, Guild = Stage.Guild }).ConfigureAwait(false);
        }

        #endregion

        #region Thread

        /// <summary>
        /// Dispatches the <see cref="ThreadCreated"/> event.
        /// </summary>
        /// <param name="Thread">The created thread.</param>
        internal async Task OnThreadCreateEventAsync(DiscordThreadChannel Thread)
        {
            Thread.Discord = this;
            this.InternalGetCachedGuild(Thread.GuildId)._threads.AddOrUpdate(Thread.Id, Thread, (OldThread, NewThread) => NewThread);

            await this._threadCreated.InvokeAsync(this, new ThreadCreateEventArgs(this.ServiceProvider) { Thread = Thread, Guild = Thread.Guild, Parent = Thread.Parent }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="DiscordClient.ThreadUpdated"/> event.
        /// </summary>
        /// <param name="Thread">The updated thread.</param>
        internal async Task OnThreadUpdateEventAsync(DiscordThreadChannel Thread)
        {
            if (Thread == null)
                return;

            Thread.Discord = this;
            var guild = Thread.Guild;

            var threadNew = this.InternalGetCachedThread(Thread.Id);
            DiscordThreadChannel threadOld = null;
            ThreadUpdateEventArgs updateEvent;

            if (threadNew != null)
            {
                threadOld = new DiscordThreadChannel
                {
                    Discord = this,
                    Type = threadNew.Type,
                    ThreadMetadata = Thread.ThreadMetadata,
                    _threadMembers = threadNew._threadMembers,
                    ParentId = Thread.ParentId,
                    OwnerId = Thread.OwnerId,
                    Name = Thread.Name,
                    LastMessageId = threadNew.LastMessageId,
                    MessageCount = Thread.MessageCount,
                    MemberCount = Thread.MemberCount,
                    GuildId = Thread.GuildId,
                    LastPinTimestampRaw = threadNew.LastPinTimestampRaw,
                    PerUserRateLimit = threadNew.PerUserRateLimit,
                    CurrentMember = threadNew.CurrentMember
                };

                threadNew.ThreadMetadata = Thread.ThreadMetadata;
                threadNew.ParentId = Thread.ParentId;
                threadNew.OwnerId = Thread.OwnerId;
                threadNew.Name = Thread.Name;
                threadNew.LastMessageId = Thread.LastMessageId.HasValue ? Thread.LastMessageId : threadOld.LastMessageId;
                threadNew.MessageCount = Thread.MessageCount;
                threadNew.MemberCount = Thread.MemberCount;
                threadNew.GuildId = Thread.GuildId;

                updateEvent = new ThreadUpdateEventArgs(this.ServiceProvider)
                {
                    ThreadAfter = Thread,
                    ThreadBefore = threadOld,
                    Guild = Thread.Guild,
                    Parent = Thread.Parent
                };
            }
            else
            {
                updateEvent = new ThreadUpdateEventArgs(this.ServiceProvider)
                {
                    ThreadAfter = Thread,
                    Guild = Thread.Guild,
                    Parent = Thread.Parent
                };
                guild._threads[Thread.Id] = Thread;
            }

            await this._threadUpdated.InvokeAsync(this, updateEvent).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadDeleted"/> event.
        /// </summary>
        /// <param name="Thread">The deleted thread.</param>
        internal async Task OnThreadDeleteEventAsync(DiscordThreadChannel Thread)
        {
            if (Thread == null)
                return;

            Thread.Discord = this;

            var gld = Thread.Guild;
            if (gld._threads.TryRemove(Thread.Id, out var cachedThread))
                Thread = cachedThread;

            await this._threadDeleted.InvokeAsync(this, new ThreadDeleteEventArgs(this.ServiceProvider) { Thread = Thread, Guild = Thread.Guild, Parent = Thread.Parent, Type = Thread.Type }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadListSynced"/> event.
        /// </summary>
        /// <param name="Guild">The synced guild.</param>
        /// <param name="channel_ids">The synced channel ids.</param>
        /// <param name="Threads">The synced threads.</param>
        /// <param name="Members">The synced members.</param>
        internal async Task OnThreadListSyncEventAsync(DiscordGuild Guild, IReadOnlyList<ulong?> ChannelIds, IReadOnlyList<DiscordThreadChannel> Threads, IReadOnlyList<DiscordThreadChannelMember> Members)
        {
            Guild.Discord = this;

            var channels = ChannelIds.Select(X => Guild.GetChannel(X.Value)); //getting channel objects
            foreach (var chan in channels)
            {
                chan.Discord = this;
            }
            Threads.Select(X => X.Discord = this);

            await this._threadListSynced.InvokeAsync(this, new ThreadListSyncEventArgs(this.ServiceProvider) { Guild = Guild, Channels = channels.ToList().AsReadOnly(), Threads = Threads, Members = Members.ToList().AsReadOnly() }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadMemberUpdated"/> event.
        /// </summary>
        /// <param name="Member">The updated member.</param>
        internal async Task OnThreadMemberUpdateEventAsync(DiscordThreadChannelMember Member)
        {
            Member.Discord = this;
            var thread = this.InternalGetCachedThread(Member.Id);
            if (thread == null)
            {
                var tempThread = await this.ApiClient.GetThreadAsync(Member.Id);
                thread = this._guilds[Member._guildId]._threads.AddOrUpdate(Member.Id, tempThread, (Old, NewThread) => NewThread);
            }

            thread.CurrentMember = Member;
            thread.Guild._threads.AddOrUpdate(Member.Id, thread, (OldThread, NewThread) => NewThread);


            await this._threadMemberUpdated.InvokeAsync(this, new ThreadMemberUpdateEventArgs(this.ServiceProvider) { ThreadMember = Member, Thread = thread }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadMembersUpdated"/> event.
        /// </summary>
        /// <param name="Guild">The target guild.</param>
        /// <param name="thread_id">The thread id of the target thread this update belongs to.</param>
        /// <param name="added_members">The added members.</param>
        /// <param name="removed_members">The ids of the removed members.</param>
        /// <param name="member_count">The new member count.</param>
        internal async Task OnThreadMembersUpdateEventAsync(DiscordGuild Guild, ulong ThreadId, JArray AddedMembers, JArray RemovedMembers, int MemberCount)
        {
            var thread = this.InternalGetCachedThread(ThreadId);
            if (thread == null)
            {
                var tempThread = await this.ApiClient.GetThreadAsync(ThreadId);
                thread = Guild._threads.AddOrUpdate(ThreadId, tempThread, (Old, NewThread) => NewThread);
            }

            thread.Discord = this;
            Guild.Discord = this;
            List<DiscordThreadChannelMember> addedMembers = new();
            List<ulong> removedMemberIds = new();

            if (AddedMembers != null)
            {
                foreach (var xj in AddedMembers)
                {
                    var xtm = xj.ToDiscordObject<DiscordThreadChannelMember>();
                    xtm.Discord = this;
                    xtm._guildId = Guild.Id;
                    if (xtm != null)
                        addedMembers.Add(xtm);

                    if (xtm.Id == this.CurrentUser.Id)
                        thread.CurrentMember = xtm;
                }
            }

            var removedMembers = new List<DiscordMember>();
            if (RemovedMembers != null)
            {
                foreach (var removedId in RemovedMembers)
                {
                    removedMembers.Add(Guild._members.TryGetValue((ulong)removedId, out var member) ? member : new DiscordMember { Id = (ulong)removedId, _guildId = Guild.Id, Discord = this });
                }
            }

            if (removedMemberIds.Contains(this.CurrentUser.Id)) //indicates the bot was removed from the thread
                thread.CurrentMember = null;

            thread.MemberCount = MemberCount;

            var threadMembersUpdateArg = new ThreadMembersUpdateEventArgs(this.ServiceProvider)
            {
                Guild = Guild,
                Thread = thread,
                AddedMembers = addedMembers,
                RemovedMembers = removedMembers,
                MemberCount = MemberCount
            };

            await this._threadMembersUpdated.InvokeAsync(this, threadMembersUpdateArg).ConfigureAwait(false);
        }

        #endregion

        #region Activities
        /// <summary>
        /// Dispatches the <see cref="EmbeddedActivityUpdated"/> event.
        /// </summary>
        /// <param name="tr_activity">The transport activity.</param>
        /// <param name="Guild">The guild.</param>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="j_users">The users in the activity.</param>
        /// <param name="app_id">The application id.</param>
        /// <returns>A Task.</returns>
        internal async Task OnEmbeddedActivityUpdateAsync(JObject TrActivity, DiscordGuild Guild, ulong ChannelId, JArray JUsers, ulong AppId)
            => await Task.Delay(20);

        /*{
            try
            {
                var users = j_users?.ToObject<List<ulong>>();

                DiscordActivity old = null;
                var uid = $"{guild.Id}_{channel_id}_{app_id}";

                if (this._embeddedActivities.TryGetValue(uid, out var activity))
                {
                    old = new DiscordActivity(activity);
                    DiscordJson.PopulateObject(tr_activity, activity);
                }
                else
                {
                    activity = tr_activity.ToObject<DiscordActivity>();
                    this._embeddedActivities[uid] = activity;
                }

                var activity_users = new List<DiscordMember>();

                var channel = this.InternalGetCachedChannel(channel_id) ?? await this.ApiClient.GetChannelAsync(channel_id);

                if (users != null)
                {
                    foreach (var user in users)
                    {
                        var activity_user = guild._members.TryGetValue(user, out var member) ? member : new DiscordMember { Id = user, _guild_id = guild.Id, Discord = this };
                        activity_users.Add(activity_user);
                    }
                }
                else
                    activity_users = null;

                var ea = new EmbeddedActivityUpdateEventArgs(this.ServiceProvider)
                {
                    Guild = guild,
                    Users = activity_users,
                    Channel = channel

                };
                await this._embeddedActivityUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
            } catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
            }
        }*/

        #endregion

        #region User/Presence Update

        /// <summary>
        /// Handles the presence update event.
        /// </summary>
        /// <param name="RawPresence">The raw presence.</param>
        /// <param name="RawUser">The raw user.</param>

        internal async Task OnPresenceUpdateEventAsync(JObject RawPresence, JObject RawUser)
        {
            var uid = (ulong)RawUser["id"];
            DiscordPresence old = null;

            if (this._presences.TryGetValue(uid, out var presence))
            {
                old = new DiscordPresence(presence);
                DiscordJson.PopulateObject(RawPresence, presence);
            }
            else
            {
                presence = RawPresence.ToObject<DiscordPresence>();
                presence.Discord = this;
                presence.Activity = new DiscordActivity(presence.RawActivity);
                this._presences[presence.InternalUser.Id] = presence;
            }

            // reuse arrays / avoid linq (this is a hot zone)
            if (presence.Activities == null || RawPresence["activities"] == null)
            {
                presence._internalActivities = Array.Empty<DiscordActivity>();
            }
            else
            {
                if (presence._internalActivities.Length != presence.RawActivities.Length)
                    presence._internalActivities = new DiscordActivity[presence.RawActivities.Length];

                for (var i = 0; i < presence._internalActivities.Length; i++)
                    presence._internalActivities[i] = new DiscordActivity(presence.RawActivities[i]);

                if (presence._internalActivities.Length > 0)
                {
                    presence.RawActivity = presence.RawActivities[0];

                    if (presence.Activity != null)
                        presence.Activity.UpdateWith(presence.RawActivity);
                    else
                        presence.Activity = new DiscordActivity(presence.RawActivity);
                }
            }

            if (this.UserCache.TryGetValue(uid, out var usr))
            {
                if (old != null)
                {
                    old.InternalUser.Username = usr.Username;
                    old.InternalUser.Discriminator = usr.Discriminator;
                    old.InternalUser.AvatarHash = usr.AvatarHash;
                }

                if (RawUser["username"] is object)
                    usr.Username = (string)RawUser["username"];
                if (RawUser["discriminator"] is object)
                    usr.Discriminator = (string)RawUser["discriminator"];
                if (RawUser["avatar"] is object)
                    usr.AvatarHash = (string)RawUser["avatar"];

                presence.InternalUser.Username = usr.Username;
                presence.InternalUser.Discriminator = usr.Discriminator;
                presence.InternalUser.AvatarHash = usr.AvatarHash;
            }

            var usrafter = usr ?? new DiscordUser(presence.InternalUser);
            var ea = new PresenceUpdateEventArgs(this.ServiceProvider)
            {
                Status = presence.Status,
                Activity = presence.Activity,
                User = usr,
                PresenceBefore = old,
                PresenceAfter = presence,
                UserBefore = old != null ? new DiscordUser(old.InternalUser) : usrafter,
                UserAfter = usrafter
            };
            await this._presenceUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the user settings update event.
        /// </summary>
        /// <param name="User">The user.</param>

        internal async Task OnUserSettingsUpdateEventAsync(TransportUser User)
        {
            var usr = new DiscordUser(User) { Discord = this };

            var ea = new UserSettingsUpdateEventArgs(this.ServiceProvider)
            {
                User = usr
            };
            await this._userSettingsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the user update event.
        /// </summary>
        /// <param name="User">The user.</param>

        internal async Task OnUserUpdateEventAsync(TransportUser User)
        {
            var usrOld = new DiscordUser
            {
                AvatarHash = this.CurrentUser.AvatarHash,
                Discord = this,
                Discriminator = this.CurrentUser.Discriminator,
                Email = this.CurrentUser.Email,
                Id = this.CurrentUser.Id,
                IsBot = this.CurrentUser.IsBot,
                MfaEnabled = this.CurrentUser.MfaEnabled,
                Username = this.CurrentUser.Username,
                Verified = this.CurrentUser.Verified
            };

            this.CurrentUser.AvatarHash = User.AvatarHash;
            this.CurrentUser.Discriminator = User.Discriminator;
            this.CurrentUser.Email = User.Email;
            this.CurrentUser.Id = User.Id;
            this.CurrentUser.IsBot = User.IsBot;
            this.CurrentUser.MfaEnabled = User.MfaEnabled;
            this.CurrentUser.Username = User.Username;
            this.CurrentUser.Verified = User.Verified;

            var ea = new UserUpdateEventArgs(this.ServiceProvider)
            {
                UserAfter = this.CurrentUser,
                UserBefore = usrOld
            };
            await this._userUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Voice

        /// <summary>
        /// Handles the voice state update event.
        /// </summary>
        /// <param name="Raw">The raw.</param>

        internal async Task OnVoiceStateUpdateEventAsync(JObject Raw)
        {
            var gid = (ulong)Raw["guild_id"];
            var uid = (ulong)Raw["user_id"];
            var gld = this._guilds[gid];

            var vstateNew = Raw.ToObject<DiscordVoiceState>();
            vstateNew.Discord = this;

            gld._voiceStates.TryRemove(uid, out var vstateOld);

            if (vstateNew.Channel != null)
            {
                gld._voiceStates[vstateNew.UserId] = vstateNew;
            }

            if (gld._members.TryGetValue(uid, out var mbr))
            {
                mbr.IsMuted = vstateNew.IsServerMuted;
                mbr.IsDeafened = vstateNew.IsServerDeafened;
            }
            else
            {
                var transportMbr = vstateNew.TransportMember;
                this.UpdateUser(new DiscordUser(transportMbr.User) { Discord = this }, gid, gld, transportMbr);
            }

            var ea = new VoiceStateUpdateEventArgs(this.ServiceProvider)
            {
                Guild = vstateNew.Guild,
                Channel = vstateNew.Channel,
                User = vstateNew.User,
                SessionId = vstateNew.SessionId,

                Before = vstateOld,
                After = vstateNew
            };
            await this._voiceStateUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the voice server update event.
        /// </summary>
        /// <param name="Endpoint">The endpoint.</param>
        /// <param name="Token">The token.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnVoiceServerUpdateEventAsync(string Endpoint, string Token, DiscordGuild Guild)
        {
            var ea = new VoiceServerUpdateEventArgs(this.ServiceProvider)
            {
                Endpoint = Endpoint,
                VoiceToken = Token,
                Guild = Guild
            };
            await this._voiceServerUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Handles the application command create.
        /// </summary>
        /// <param name="Cmd">The cmd.</param>
        /// <param name="guild_id">The guild_id.</param>

        internal async Task OnApplicationCommandCreateAsync(DiscordApplicationCommand Cmd, ulong? GuildId)
        {
            Cmd.Discord = this;

            var guild = this.InternalGetCachedGuild(GuildId);

            if (guild == null && GuildId.HasValue)
            {
                guild = new DiscordGuild
                {
                    Id = GuildId.Value,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Command = Cmd
            };

            await this._applicationCommandCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the application command update.
        /// </summary>
        /// <param name="Cmd">The cmd.</param>
        /// <param name="guild_id">The guild_id.</param>

        internal async Task OnApplicationCommandUpdateAsync(DiscordApplicationCommand Cmd, ulong? GuildId)
        {
            Cmd.Discord = this;

            var guild = this.InternalGetCachedGuild(GuildId);

            if (guild == null && GuildId.HasValue)
            {
                guild = new DiscordGuild
                {
                    Id = GuildId.Value,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Command = Cmd
            };

            await this._applicationCommandUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the application command delete.
        /// </summary>
        /// <param name="Cmd">The cmd.</param>
        /// <param name="guild_id">The guild_id.</param>

        internal async Task OnApplicationCommandDeleteAsync(DiscordApplicationCommand Cmd, ulong? GuildId)
        {
            Cmd.Discord = this;

            var guild = this.InternalGetCachedGuild(GuildId);

            if (guild == null && GuildId.HasValue)
            {
                guild = new DiscordGuild
                {
                    Id = GuildId.Value,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Command = Cmd
            };

            await this._applicationCommandDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild application command counts update.
        /// </summary>
        /// <param name="Sc">The <see cref="ApplicationCommandType.ChatInput"/> count.</param>
        /// <param name="Ucmc">The <see cref="ApplicationCommandType.User"/> count.</param>
        /// <param name="Mcmc">The <see cref="ApplicationCommandType.Message"/> count.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>Count of application commands.</returns>
        internal async Task OnGuildApplicationCommandCountsUpdateAsync(int Sc, int Ucmc, int Mcmc, ulong GuildId)
        {
            var guild = this.InternalGetCachedGuild(GuildId);

            if (guild == null)
            {
                guild = new DiscordGuild
                {
                    Id = GuildId,
                    Discord = this
                };
            }

            var ea = new GuildApplicationCommandCountEventArgs(this.ServiceProvider)
            {
                SlashCommands = Sc,
                UserContextMenuCommands = Ucmc,
                MessageContextMenuCommands = Mcmc,
                Guild = guild
            };

            await this._guildApplicationCommandCountUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the application command permissions update.
        /// </summary>
        /// <param name="Perms">The new permissions.</param>
        /// <param name="c_id">The command id.</param>
        /// <param name="guild_id">The guild id.</param>
        /// <param name="a_id">The application id.</param>
        internal async Task OnApplicationCommandPermissionsUpdateAsync(IEnumerable<DiscordApplicationCommandPermission> Perms, ulong CId, ulong GuildId, ulong AId)
        {
            if (AId != this.CurrentApplication.Id)
                return;

            var guild = this.InternalGetCachedGuild(GuildId);

            DiscordApplicationCommand cmd;
            try
            {
                cmd = await this.GetGuildApplicationCommand(GuildId, CId);
            }
            catch (NotFoundException)
            {
                cmd = await this.GetGlobalApplicationCommand(CId);
            }

            if (guild == null)
            {
                guild = new DiscordGuild
                {
                    Id = GuildId,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandPermissionsUpdateEventArgs(this.ServiceProvider)
            {
                Permissions = Perms.ToList(),
                Command = cmd,
                ApplicationId = AId,
                Guild = guild
            };

            await this._applicationCommandPermissionsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Interaction

        /// <summary>
        /// Handles the interaction create.
        /// </summary>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="User">The user.</param>
        /// <param name="Member">The member.</param>
        /// <param name="Interaction">The interaction.</param>
        internal async Task OnInteractionCreateAsync(ulong? GuildId, ulong ChannelId, TransportUser User, TransportMember Member, DiscordInteraction Interaction)
        {
            var usr = new DiscordUser(User) { Discord = this };

            Interaction.ChannelId = ChannelId;
            Interaction.GuildId = GuildId;
            Interaction.Discord = this;
            Interaction.Data.Discord = this;

            if (Member != null)
            {
                usr = new DiscordMember(Member) { _guildId = GuildId.Value, Discord = this };
                this.UpdateUser(usr, GuildId, Interaction.Guild, Member);
            }
            else
            {
                this.UserCache.AddOrUpdate(usr.Id, usr, (Old, New) => New);
            }

            Interaction.User = usr;

            var resolved = Interaction.Data.Resolved;
            if (resolved != null)
            {
                if (resolved.Users != null)
                {
                    foreach (var c in resolved.Users)
                    {
                        c.Value.Discord = this;
                        this.UserCache.AddOrUpdate(c.Value.Id, c.Value, (Old, New) => New);
                    }
                }

                if (resolved.Members != null)
                {
                    foreach (var c in resolved.Members)
                    {
                        c.Value.Discord = this;
                        c.Value.Id = c.Key;
                        c.Value._guildId = GuildId.Value;
                        c.Value.User.Discord = this;
                        this.UserCache.AddOrUpdate(c.Value.User.Id, c.Value.User, (Old, New) => New);
                    }
                }

                if (resolved.Channels != null)
                {
                    foreach (var c in resolved.Channels)
                    {
                        c.Value.Discord = this;

                        if (GuildId.HasValue)
                            c.Value.GuildId = GuildId.Value;
                    }
                }

                if (resolved.Roles != null)
                {
                    foreach (var c in resolved.Roles)
                    {
                        c.Value.Discord = this;

                        if (GuildId.HasValue)
                            c.Value._guildId = GuildId.Value;
                    }
                }


                if (resolved.Messages != null)
                {
                    foreach (var m in resolved.Messages)
                    {
                        m.Value.Discord = this;

                        if (GuildId.HasValue)
                            m.Value.GuildId = GuildId.Value;
                    }
                }
            }

            if (Interaction.Type is InteractionType.Component || Interaction.Type is InteractionType.ModalSubmit)
            {
                if (Interaction.Message != null)
                {
                    Interaction.Message.Discord = this;
                    Interaction.Message.ChannelId = Interaction.ChannelId;
                }
                var cea = new ComponentInteractionCreateEventArgs(this.ServiceProvider)
                {
                    Message = Interaction.Message,
                    Interaction = Interaction
                };

                await this._componentInteractionCreated.InvokeAsync(this, cea).ConfigureAwait(false);
            }
            else
            {
                if (Interaction.Data.Target.HasValue) // Context-Menu. //
                {
                    var targetId = Interaction.Data.Target.Value;
                    DiscordUser targetUser = null;
                    DiscordMember targetMember = null;
                    DiscordMessage targetMessage = null;

                    Interaction.Data.Resolved.Messages?.TryGetValue(targetId, out targetMessage);
                    Interaction.Data.Resolved.Members?.TryGetValue(targetId, out targetMember);
                    Interaction.Data.Resolved.Users?.TryGetValue(targetId, out targetUser);

                    var ctea = new ContextMenuInteractionCreateEventArgs(this.ServiceProvider)
                    {
                        Interaction = Interaction,
                        TargetUser = targetMember ?? targetUser,
                        TargetMessage = targetMessage,
                        Type = Interaction.Data.Type,
                    };
                    await this._contextMenuInteractionCreated.InvokeAsync(this, ctea).ConfigureAwait(false);
                }
                else
                {
                    var ea = new InteractionCreateEventArgs(this.ServiceProvider)
                    {
                        Interaction = Interaction
                    };

                    await this._interactionCreated.InvokeAsync(this, ea).ConfigureAwait(false);
                }
            }
        }

        #endregion

        #region Misc

        /// <summary>
        /// Handles the typing start event.
        /// </summary>
        /// <param name="UserId">The user id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="Channel">The channel.</param>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Started">The started.</param>
        /// <param name="Mbr">The mbr.</param>

        internal async Task OnTypingStartEventAsync(ulong UserId, ulong ChannelId, DiscordChannel Channel, ulong? GuildId, DateTimeOffset Started, TransportMember Mbr)
        {
            if (Channel == null)
            {
                Channel = new DiscordChannel
                {
                    Discord = this,
                    Id = ChannelId,
                    GuildId = GuildId ?? default,
                };
            }

            var guild = this.InternalGetCachedGuild(GuildId);
            var usr = this.UpdateUser(new DiscordUser { Id = UserId, Discord = this }, GuildId, guild, Mbr);

            var ea = new TypingStartEventArgs(this.ServiceProvider)
            {
                Channel = Channel,
                User = usr,
                Guild = guild,
                StartedAt = Started
            };
            await this._typingStarted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the webhooks update.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        /// <param name="Guild">The guild.</param>

        internal async Task OnWebhooksUpdateAsync(DiscordChannel Channel, DiscordGuild Guild)
        {
            var ea = new WebhooksUpdateEventArgs(this.ServiceProvider)
            {
                Channel = Channel,
                Guild = Guild
            };
            await this._webhooksUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the unknown event.
        /// </summary>
        /// <param name="Payload">The payload.</param>

        internal async Task OnUnknownEventAsync(GatewayPayload Payload)
        {
            var ea = new UnknownEventArgs(this.ServiceProvider) { EventName = Payload.EventName, Json = (Payload.Data as JObject)?.ToString() };
            await this._unknownEvent.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}
