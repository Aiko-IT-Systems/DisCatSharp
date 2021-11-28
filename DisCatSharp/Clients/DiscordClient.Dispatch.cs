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
        /// <param name="payload">The payload.</param>

        internal async Task HandleDispatchAsync(GatewayPayload payload)
        {
            if (payload.Data is not JObject dat)
            {
                this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Invalid payload body (this message is probably safe to ignore); opcode: {0} event: {1}; payload: {2}", payload.OpCode, payload.EventName, payload.Data);
                return;
            }

            await this._payloadReceived.InvokeAsync(this, new(this.ServiceProvider)
            {
                EventName = payload.EventName,
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

            switch (payload.EventName.ToLowerInvariant())
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
                    await this.OnUnknownEventAsync(payload).ConfigureAwait(false);
                    this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown event: {0}\npayload: {1}", payload.EventName, payload.Data);
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
        /// <param name="ready">The ready.</param>
        /// <param name="rawGuilds">The raw guilds.</param>

        internal async Task OnReadyEventAsync(ReadyPayload ready, JArray rawGuilds)
        {
            //ready.CurrentUser.Discord = this;

            var rusr = ready.CurrentUser;
            this.CurrentUser.Username = rusr.Username;
            this.CurrentUser.Discriminator = rusr.Discriminator;
            this.CurrentUser.AvatarHash = rusr.AvatarHash;
            this.CurrentUser.MfaEnabled = rusr.MfaEnabled;
            this.CurrentUser.Verified = rusr.Verified;
            this.CurrentUser.IsBot = rusr.IsBot;

            this.GatewayVersion = ready.GatewayVersion;
            this._sessionId = ready.SessionId;
            var raw_guild_index = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

            this._guilds.Clear();
            foreach (var guild in ready.Guilds)
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
                        xo._channel_id = xc.Id;
                    }
                }

                if (guild._roles == null)
                    guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();

                foreach (var xr in guild.Roles.Values)
                {
                    xr.Discord = this;
                    xr._guild_id = guild.Id;
                }

                var raw_guild = raw_guild_index[guild.Id];
                var raw_members = (JArray)raw_guild["members"];

                if (guild._members != null)
                    guild._members.Clear();
                else
                    guild._members = new ConcurrentDictionary<ulong, DiscordMember>();

                if (raw_members != null)
                {
                    foreach (var xj in raw_members)
                    {
                        var xtm = xj.ToObject<TransportMember>();

                        var xu = new DiscordUser(xtm.User) { Discord = this };
                        xu = this.UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
                        {
                            old.Username = xu.Username;
                            old.Discriminator = xu.Discriminator;
                            old.AvatarHash = xu.AvatarHash;
                            return old;
                        });

                        guild._members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, _guild_id = guild.Id };
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
        /// <param name="channel">The channel.</param>

        internal async Task OnChannelCreateEventAsync(DiscordChannel channel)
        {
            channel.Discord = this;
            foreach (var xo in channel._permissionOverwrites)
            {
                xo.Discord = this;
                xo._channel_id = channel.Id;
            }

            this._guilds[channel.GuildId.Value]._channels[channel.Id] = channel;

            /*if (this.Configuration.AutoRefreshChannelCache)
            {
                await this.RefreshChannelsAsync(channel.Guild.Id);
            }*/

            await this._channelCreated.InvokeAsync(this, new ChannelCreateEventArgs(this.ServiceProvider) { Channel = channel, Guild = channel.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the channel update event.
        /// </summary>
        /// <param name="channel">The channel.</param>

        internal async Task OnChannelUpdateEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            var gld = channel.Guild;

            var channel_new = this.InternalGetCachedChannel(channel.Id);
            DiscordChannel channel_old = null;

            if (channel_new != null)
            {
                channel_old = new DiscordChannel
                {
                    Bitrate = channel_new.Bitrate,
                    Discord = this,
                    GuildId = channel_new.GuildId,
                    Id = channel_new.Id,
                    //IsPrivate = channel_new.IsPrivate,
                    LastMessageId = channel_new.LastMessageId,
                    Name = channel_new.Name,
                    _permissionOverwrites = new List<DiscordOverwrite>(channel_new._permissionOverwrites),
                    Position = channel_new.Position,
                    Topic = channel_new.Topic,
                    Type = channel_new.Type,
                    UserLimit = channel_new.UserLimit,
                    ParentId = channel_new.ParentId,
                    IsNSFW = channel_new.IsNSFW,
                    PerUserRateLimit = channel_new.PerUserRateLimit,
                    RtcRegionId = channel_new.RtcRegionId,
                    QualityMode = channel_new.QualityMode,
                    DefaultAutoArchiveDuration = channel_new.DefaultAutoArchiveDuration
                };

                channel_new.Bitrate = channel.Bitrate;
                channel_new.Name = channel.Name;
                channel_new.Position = channel.Position;
                channel_new.Topic = channel.Topic;
                channel_new.UserLimit = channel.UserLimit;
                channel_new.ParentId = channel.ParentId;
                channel_new.IsNSFW = channel.IsNSFW;
                channel_new.PerUserRateLimit = channel.PerUserRateLimit;
                channel_new.Type = channel.Type;
                channel_new.RtcRegionId = channel.RtcRegionId;
                channel_new.QualityMode = channel.QualityMode;
                channel_new.DefaultAutoArchiveDuration = channel.DefaultAutoArchiveDuration;

                channel_new._permissionOverwrites.Clear();

                foreach (var po in channel._permissionOverwrites)
                {
                    po.Discord = this;
                    po._channel_id = channel.Id;
                }

                channel_new._permissionOverwrites.AddRange(channel._permissionOverwrites);

                if (this.Configuration.AutoRefreshChannelCache && gld != null)
                {
                    await this.RefreshChannelsAsync(channel.Guild.Id);
                }
            }
            else if (gld != null)
            {
                gld._channels[channel.Id] = channel;

                if (this.Configuration.AutoRefreshChannelCache)
                {
                    await this.RefreshChannelsAsync(channel.Guild.Id);
                }
            }

            await this._channelUpdated.InvokeAsync(this, new ChannelUpdateEventArgs(this.ServiceProvider) { ChannelAfter = channel_new, Guild = gld, ChannelBefore = channel_old }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the channel delete event.
        /// </summary>
        /// <param name="channel">The channel.</param>

        internal async Task OnChannelDeleteEventAsync(DiscordChannel channel)
        {
            if (channel == null)
                return;

            channel.Discord = this;

            //if (channel.IsPrivate)
            if (channel.Type == ChannelType.Group || channel.Type == ChannelType.Private)
            {
                var dmChannel = channel as DiscordDmChannel;

                await this._dmChannelDeleted.InvokeAsync(this, new DmChannelDeleteEventArgs(this.ServiceProvider) { Channel = dmChannel }).ConfigureAwait(false);
            }
            else
            {
                var gld = channel.Guild;

                if (gld._channels.TryRemove(channel.Id, out var cachedChannel)) channel = cachedChannel;

                if(this.Configuration.AutoRefreshChannelCache)
                {
                    await this.RefreshChannelsAsync(channel.Guild.Id);
                }

                await this._channelDeleted.InvokeAsync(this, new ChannelDeleteEventArgs(this.ServiceProvider) { Channel = channel, Guild = gld }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refreshes the channels.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        internal async Task RefreshChannelsAsync(ulong guildId)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channels = await this.ApiClient.GetGuildChannelsAsync(guildId);
            guild._channels.Clear();
            foreach (var channel in channels.ToList())
            {
                channel.Discord = this;
                foreach (var xo in channel._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = channel.Id;
                }
                guild._channels[channel.Id] = channel;
            }
        }

        /// <summary>
        /// Handles the channel pins update.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="lastPinTimestamp">The last pin timestamp.</param>

        internal async Task OnChannelPinsUpdateAsync(ulong? guildId, ulong channelId, DateTimeOffset? lastPinTimestamp)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

            var ea = new ChannelPinsUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Channel = channel,
                LastPinTimestamp = lastPinTimestamp
            };
            await this._channelPinsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild

        /// <summary>
        /// Handles the guild create event.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="rawMembers">The raw members.</param>
        /// <param name="presences">The presences.</param>

        internal async Task OnGuildCreateEventAsync(DiscordGuild guild, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            if (presences != null)
            {
                foreach (var xp in presences)
                {
                    xp.Discord = this;
                    xp.GuildId = guild.Id;
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

            var exists = this._guilds.TryGetValue(guild.Id, out var foundGuild);

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            if (exists)
                guild = foundGuild;

            if (guild._channels == null)
                guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if(guild._threads == null)
                guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
            if (guild._roles == null)
                guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (guild._threads == null)
                guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
            if (guild._stickers == null)
                guild._stickers = new ConcurrentDictionary<ulong, DiscordSticker>();
            if (guild._emojis == null)
                guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (guild._voiceStates == null)
                guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (guild._members == null)
                guild._members = new ConcurrentDictionary<ulong, DiscordMember>();
            if (guild._scheduledEvents == null)
                guild._scheduledEvents = new ConcurrentDictionary<ulong, DiscordScheduledEvent>();

            this.UpdateCachedGuild(eventGuild, rawMembers);

            guild.JoinedAt = eventGuild.JoinedAt;
            guild.IsLarge = eventGuild.IsLarge;
            guild.MemberCount = Math.Max(eventGuild.MemberCount, guild._members.Count);
            guild.IsUnavailable = eventGuild.IsUnavailable;
            guild.PremiumSubscriptionCount = eventGuild.PremiumSubscriptionCount;
            guild.PremiumTier = eventGuild.PremiumTier;
            guild.BannerHash = eventGuild.BannerHash;
            guild.VanityUrlCode = eventGuild.VanityUrlCode;
            guild.Description = eventGuild.Description;
            guild.IsNSFW = eventGuild.IsNSFW;

            foreach (var kvp in eventGuild._voiceStates) guild._voiceStates[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._channels) guild._channels[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._roles) guild._roles[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._emojis) guild._emojis[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._threads) guild._threads[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._stickers) guild._stickers[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._stageInstances) guild._stageInstances[kvp.Key] = kvp.Value;
            foreach (var kvp in eventGuild._scheduledEvents) guild._scheduledEvents[kvp.Key] = kvp.Value;

            foreach (var xc in guild._channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach(var xt in guild._threads.Values)
            {
                xt.GuildId = guild.Id;
                xt.Discord = this;
            }
            foreach (var xe in guild._emojis.Values)
                xe.Discord = this;
            foreach (var xs in guild._stickers.Values)
                xs.Discord = this;
            foreach (var xvs in guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xsi in guild._stageInstances.Values)
            {
                xsi.Discord = this;
                xsi.GuildId = guild.Id;
            }
            foreach (var xr in guild._roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }
            foreach (var xse in guild._scheduledEvents.Values)
            {
                xse.Discord = this;
                xse.GuildId = guild.Id;
                if (xse.Creator != null)
                    xse.Creator.Discord = this;
            }

            var old = Volatile.Read(ref this._guildDownloadCompleted);
            var dcompl = this._guilds.Values.All(xg => !xg.IsUnavailable);
            Volatile.Write(ref this._guildDownloadCompleted, dcompl);

            if (exists)
                await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs(this.ServiceProvider) { Guild = guild }).ConfigureAwait(false);
            else
                await this._guildCreated.InvokeAsync(this, new GuildCreateEventArgs(this.ServiceProvider) { Guild = guild }).ConfigureAwait(false);

            if (dcompl && !old)
                await this._guildDownloadCompletedEv.InvokeAsync(this, new GuildDownloadCompletedEventArgs(this.Guilds, this.ServiceProvider)).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild update event.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="rawMembers">The raw members.</param>

        internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
        {
            DiscordGuild oldGuild;

            if (!this._guilds.ContainsKey(guild.Id))
            {
                this._guilds[guild.Id] = guild;
                oldGuild = null;
            }
            else
            {
                var gld = this._guilds[guild.Id];

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
                    IsNSFW = gld.IsNSFW,
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

            guild.Discord = this;
            guild.IsUnavailable = false;
            var eventGuild = guild;
            guild = this._guilds[eventGuild.Id];

            if (guild._channels == null)
                guild._channels = new ConcurrentDictionary<ulong, DiscordChannel>();
            if (guild._threads == null)
                guild._threads = new ConcurrentDictionary<ulong, DiscordThreadChannel>();
            if (guild._roles == null)
                guild._roles = new ConcurrentDictionary<ulong, DiscordRole>();
            if (guild._emojis == null)
                guild._emojis = new ConcurrentDictionary<ulong, DiscordEmoji>();
            if (guild._stickers == null)
                guild._stickers = new ConcurrentDictionary<ulong, DiscordSticker>();
            if (guild._voiceStates == null)
                guild._voiceStates = new ConcurrentDictionary<ulong, DiscordVoiceState>();
            if (guild._stageInstances == null)
                guild._stageInstances = new ConcurrentDictionary<ulong, DiscordStageInstance>();
            if (guild._members == null)
                guild._members = new ConcurrentDictionary<ulong, DiscordMember>();
            if (guild._scheduledEvents == null)
                guild._scheduledEvents = new ConcurrentDictionary<ulong, DiscordScheduledEvent>();

            this.UpdateCachedGuild(eventGuild, rawMembers);

            foreach (var xc in guild._channels.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
                foreach (var xo in xc._permissionOverwrites)
                {
                    xo.Discord = this;
                    xo._channel_id = xc.Id;
                }
            }
            foreach (var xc in guild._threads.Values)
            {
                xc.GuildId = guild.Id;
                xc.Discord = this;
            }
            foreach (var xe in guild._emojis.Values)
                xe.Discord = this;
            foreach (var xs in guild._stickers.Values)
                xs.Discord = this;
            foreach (var xvs in guild._voiceStates.Values)
                xvs.Discord = this;
            foreach (var xr in guild._roles.Values)
            {
                xr.Discord = this;
                xr._guild_id = guild.Id;
            }
            foreach (var xsi in guild._stageInstances.Values)
            {
                xsi.Discord = this;
                xsi.GuildId = guild.Id;
            }
            foreach (var xse in guild._scheduledEvents.Values)
            {
                xse.Discord = this;
                xse.GuildId = guild.Id;
                if (xse.Creator != null)
                    xse.Creator.Discord = this;
            }

            await this._guildUpdated.InvokeAsync(this, new GuildUpdateEventArgs(this.ServiceProvider) { GuildBefore = oldGuild, GuildAfter = guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild delete event.
        /// </summary>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildDeleteEventAsync(DiscordGuild guild)
        {
            if (guild.IsUnavailable)
            {
                if (!this._guilds.TryGetValue(guild.Id, out var gld))
                    return;

                gld.IsUnavailable = true;

                await this._guildUnavailable.InvokeAsync(this, new GuildDeleteEventArgs(this.ServiceProvider) { Guild = guild, Unavailable = true }).ConfigureAwait(false);
            }
            else
            {
                if (!this._guilds.TryRemove(guild.Id, out var gld))
                    return;

                await this._guildDeleted.InvokeAsync(this, new GuildDeleteEventArgs(this.ServiceProvider) { Guild = gld }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handles the guild sync event.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="isLarge">If true, is large.</param>
        /// <param name="rawMembers">The raw members.</param>
        /// <param name="presences">The presences.</param>

        internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool isLarge, JArray rawMembers, IEnumerable<DiscordPresence> presences)
        {
            presences = presences.Select(xp => { xp.Discord = this; xp.Activity = new DiscordActivity(xp.RawActivity); return xp; });
            foreach (var xp in presences)
                this._presences[xp.InternalUser.Id] = xp;

            guild.IsSynced = true;
            guild.IsLarge = isLarge;

            this.UpdateCachedGuild(guild, rawMembers);

            await this._guildAvailable.InvokeAsync(this, new GuildCreateEventArgs(this.ServiceProvider) { Guild = guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild emojis update event.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="newEmojis">The new emojis.</param>

        internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
        {
            var oldEmojis = new ConcurrentDictionary<ulong, DiscordEmoji>(guild._emojis);
            guild._emojis.Clear();

            foreach (var emoji in newEmojis)
            {
                emoji.Discord = this;
                guild._emojis[emoji.Id] = emoji;
            }

            var ea = new GuildEmojisUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                EmojisAfter = guild.Emojis,
                EmojisBefore = new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(oldEmojis)
            };
            await this._guildEmojisUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the stickers updated.
        /// </summary>
        /// <param name="newStickers">The new stickers.</param>
        /// <param name="raw">The raw.</param>

        internal async Task OnStickersUpdatedAsync(IEnumerable<DiscordSticker> newStickers, JObject raw)
        {
            var guild = this.InternalGetCachedGuild((ulong)raw["guild_id"]);
            var oldStickers = new ConcurrentDictionary<ulong, DiscordSticker>(guild._stickers);
            guild._stickers.Clear();

            foreach (var nst in newStickers)
            {
                if (nst.User is not null)
                {
                    nst.User.Discord = this;
                    this.UserCache.AddOrUpdate(nst.User.Id, nst.User, (old, @new) => @new);
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
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildIntegrationsUpdateEventAsync(DiscordGuild guild)
        {
            var ea = new GuildIntegrationsUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild
            };
            await this._guildIntegrationsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Ban

        /// <summary>
        /// Handles the guild ban add event.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanAddEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildBanAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild ban remove event.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user) { Discord = this };
            usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            var ea = new GuildBanRemoveEventArgs(this.ServiceProvider)
            {
                Guild = guild,
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
        /// <param name="guild">The target guild.</param>
        internal async Task OnGuildScheduledEventCreateEventAsync(DiscordScheduledEvent scheduled_event, DiscordGuild guild)
        {
            scheduled_event.Discord = this;

            guild._scheduledEvents.AddOrUpdate(scheduled_event.Id, scheduled_event, (old, newScheduledEvent) => newScheduledEvent);

            if (scheduled_event.Creator != null)
            {
                scheduled_event.Creator.Discord = this;
                this.UserCache.AddOrUpdate(scheduled_event.Creator.Id, scheduled_event.Creator, (id, old) =>
                {
                    old.Username = scheduled_event.Creator.Username;
                    old.Discriminator = scheduled_event.Creator.Discriminator;
                    old.AvatarHash = scheduled_event.Creator.AvatarHash;
                    old.Flags = scheduled_event.Creator.Flags;
                    return old;
                });
            }

            await this._guildScheduledEventCreated.InvokeAsync(this, new GuildScheduledEventCreateEventArgs(this.ServiceProvider) { ScheduledEvent = scheduled_event, Guild = scheduled_event.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventUpdated"/> event.
        /// </summary>
        /// <param name="scheduled_event">The updated event.</param>
        /// <param name="guild">The target guild.</param>
        internal async Task OnGuildScheduledEventUpdateEventAsync(DiscordScheduledEvent scheduled_event, DiscordGuild guild)
        {
            if (guild == null)
                return;

            DiscordScheduledEvent old_event;
            if (!guild._scheduledEvents.ContainsKey(scheduled_event.Id))
            {
                old_event = null;
            } else {
                var ev = guild._scheduledEvents[scheduled_event.Id];
                old_event = new DiscordScheduledEvent
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
            if (scheduled_event.Creator != null)
            {
                scheduled_event.Creator.Discord = this;
                this.UserCache.AddOrUpdate(scheduled_event.Creator.Id, scheduled_event.Creator, (id, old) =>
                {
                    old.Username = scheduled_event.Creator.Username;
                    old.Discriminator = scheduled_event.Creator.Discriminator;
                    old.AvatarHash = scheduled_event.Creator.AvatarHash;
                    old.Flags = scheduled_event.Creator.Flags;
                    return old;
                });
            }

            if (scheduled_event.Status == ScheduledEventStatus.Completed)
            {
                guild._scheduledEvents.TryRemove(scheduled_event.Id, out var deleted_event);
                await this._guildScheduledEventDeleted.InvokeAsync(this, new GuildScheduledEventDeleteEventArgs(this.ServiceProvider) { ScheduledEvent = scheduled_event, Guild = guild, Reason = ScheduledEventStatus.Completed }).ConfigureAwait(false);
            }
            else if (scheduled_event.Status == ScheduledEventStatus.Canceled)
            {
                guild._scheduledEvents.TryRemove(scheduled_event.Id, out var deleted_event);
                scheduled_event.Status = ScheduledEventStatus.Canceled;
                await this._guildScheduledEventDeleted.InvokeAsync(this, new GuildScheduledEventDeleteEventArgs(this.ServiceProvider) { ScheduledEvent = scheduled_event, Guild = guild, Reason = ScheduledEventStatus.Canceled }).ConfigureAwait(false);
            }
            else
            {
                this.UpdateScheduledEvent(scheduled_event, guild);
                await this._guildScheduledEventUpdated.InvokeAsync(this, new GuildScheduledEventUpdateEventArgs(this.ServiceProvider) { ScheduledEventBefore = old_event, ScheduledEventAfter = scheduled_event, Guild = guild }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventDeleted"/> event.
        /// </summary>
        /// <param name="scheduled_event">The deleted event.</param>
        /// <param name="guild">The target guild.</param>
        internal async Task OnGuildScheduledEventDeleteEventAsync(DiscordScheduledEvent scheduled_event, DiscordGuild guild)
        {
            scheduled_event.Discord = this;

            if (scheduled_event.Status == ScheduledEventStatus.Scheduled)
                scheduled_event.Status = ScheduledEventStatus.Canceled;

            if (scheduled_event.Creator != null)
            {
                scheduled_event.Creator.Discord = this;
                this.UserCache.AddOrUpdate(scheduled_event.Creator.Id, scheduled_event.Creator, (id, old) =>
                {
                    old.Username = scheduled_event.Creator.Username;
                    old.Discriminator = scheduled_event.Creator.Discriminator;
                    old.AvatarHash = scheduled_event.Creator.AvatarHash;
                    old.Flags = scheduled_event.Creator.Flags;
                    return old;
                });
            }

            await this._guildScheduledEventDeleted.InvokeAsync(this, new GuildScheduledEventDeleteEventArgs(this.ServiceProvider) { ScheduledEvent = scheduled_event, Guild = scheduled_event.Guild, Reason = scheduled_event.Status }).ConfigureAwait(false);
            guild._scheduledEvents.TryRemove(scheduled_event.Id, out var deleted_event);
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventUserAdded"/> event.
        /// <param name="guild_scheduled_event_id">The target event.</param>
        /// <param name="user_id">The added user id.</param>
        /// <param name="guild">The target guild.</param>
        /// </summary>
        internal async Task OnGuildScheduledEventUserAddedEventAsync(ulong guild_scheduled_event_id, ulong user_id, DiscordGuild guild)
        {
            var scheduled_event = this.InternalGetCachedScheduledEvent(guild_scheduled_event_id) ?? this.UpdateScheduledEvent(new DiscordScheduledEvent {
                Id = guild_scheduled_event_id,
                GuildId = guild.Id,
                Discord = this,
                UserCount = 0
            }, guild);

            scheduled_event.UserCount++;
            scheduled_event.Discord = this;
            guild.Discord = this;

            var user = this.GetUserAsync(user_id, true).Result;
            user.Discord = this;
            var member = guild.Members.TryGetValue(user_id, out var mem) ? mem : guild.GetMemberAsync(user_id).Result;
            member.Discord = this;

            await this._guildScheduledEventUserAdded.InvokeAsync(this, new GuildScheduledEventUserAddEventArgs(this.ServiceProvider) { ScheduledEvent = scheduled_event, Guild = guild, User = user, Member = member }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="GuildScheduledEventUserRemoved"/> event.
        /// <param name="guild_scheduled_event_id">The target event.</param>
        /// <param name="user_id">The removed user id.</param>
        /// <param name="guild">The target guild.</param>
        /// </summary>
        internal async Task OnGuildScheduledEventUserRemovedEventAsync(ulong guild_scheduled_event_id, ulong user_id, DiscordGuild guild)
        {
            var scheduled_event = this.InternalGetCachedScheduledEvent(guild_scheduled_event_id) ?? this.UpdateScheduledEvent(new DiscordScheduledEvent
            {
                Id = guild_scheduled_event_id,
                GuildId = guild.Id,
                Discord = this,
                UserCount = 0
            }, guild);

            scheduled_event.UserCount = scheduled_event.UserCount == 0 ? 0 : scheduled_event.UserCount - 1;
            scheduled_event.Discord = this;
            guild.Discord = this;

            var user = this.GetUserAsync(user_id, true).Result;
            user.Discord = this;
            var member = guild.Members.TryGetValue(user_id, out var mem) ? mem : guild.GetMemberAsync(user_id).Result;
            member.Discord = this;

            await this._guildScheduledEventUserRemoved.InvokeAsync(this, new GuildScheduledEventUserRemoveEventArgs(this.ServiceProvider) { ScheduledEvent = scheduled_event, Guild = guild, User = user, Member = member }).ConfigureAwait(false);
        }

        #endregion

        #region Guild Integration

        /// <summary>
        /// Handles the guild integration create event.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="integration">The integration.</param>

        internal async Task OnGuildIntegrationCreateEventAsync(DiscordGuild guild, DiscordIntegration integration)
        {
            integration.Discord = this;

            await this._guildIntegrationCreated.InvokeAsync(this, new GuildIntegrationCreateEventArgs(this.ServiceProvider) { Integration = integration, Guild = guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild integration update event.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="integration">The integration.</param>

        internal async Task OnGuildIntegrationUpdateEventAsync(DiscordGuild guild, DiscordIntegration integration)
        {
            integration.Discord = this;

            await this._guildIntegrationUpdated.InvokeAsync(this, new GuildIntegrationUpdateEventArgs(this.ServiceProvider) { Integration = integration, Guild = guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild integration delete event.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="integration_id">The integration_id.</param>
        /// <param name="application_id">The application_id.</param>

        internal async Task OnGuildIntegrationDeleteEventAsync(DiscordGuild guild, ulong integration_id, ulong? application_id)
            => await this._guildIntegrationDeleted.InvokeAsync(this, new GuildIntegrationDeleteEventArgs(this.ServiceProvider) { Guild = guild, IntegrationId = integration_id, ApplicationId = application_id }).ConfigureAwait(false);

        #endregion

        #region Guild Member

        /// <summary>
        /// Handles the guild member add event.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
        {
            var usr = new DiscordUser(member.User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(member.User.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            var mbr = new DiscordMember(member)
            {
                Discord = this,
                _guild_id = guild.Id
            };

            guild._members[mbr.Id] = mbr;
            guild.MemberCount++;

            var ea = new GuildMemberAddEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildMemberAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild member remove event.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
        {
            var usr = new DiscordUser(user);

            if (!guild._members.TryRemove(user.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };
            guild.MemberCount--;

            _ = this.UserCache.AddOrUpdate(user.Id, usr, (old, @new) => @new);

            var ea = new GuildMemberRemoveEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Member = mbr
            };
            await this._guildMemberRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild member update event.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="guild">The guild.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="nick">The nick.</param>
        /// <param name="pending">If true, pending.</param>

        internal async Task OnGuildMemberUpdateEventAsync(TransportMember member, DiscordGuild guild, IEnumerable<ulong> roles, string nick, bool? pending)
        {
            var usr = new DiscordUser(member.User) { Discord = this };
            usr = this.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            if (!guild.Members.TryGetValue(member.User.Id, out var mbr))
                mbr = new DiscordMember(usr) { Discord = this, _guild_id = guild.Id };

            var nick_old = mbr.Nickname;
            var pending_old = mbr.IsPending;
            var roles_old = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));
            var cdu_old = mbr.CommunicationDisabledUntil;
            mbr._avatarHash = member.AvatarHash;
            mbr.GuildAvatarHash = member.GuildAvatarHash;
            mbr.Nickname = nick;
            mbr.IsPending = pending;
            mbr.CommunicationDisabledUntil = member.CommunicationDisabledUntil;
            mbr._role_ids.Clear();
            mbr._role_ids.AddRange(roles);

            var ea = new GuildMemberUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Member = mbr,

                NicknameAfter = mbr.Nickname,
                RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),
                PendingAfter = mbr.IsPending,
                TimeoutAfter = mbr.CommunicationDisabledUntil,

                NicknameBefore = nick_old,
                RolesBefore = roles_old,
                PendingBefore = pending_old,
                TimeoutBefore = cdu_old
            };
            await this._guildMemberUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild members chunk event.
        /// </summary>
        /// <param name="dat">The dat.</param>

        internal async Task OnGuildMembersChunkEventAsync(JObject dat)
        {
            var guild = this.Guilds[(ulong)dat["guild_id"]];
            var chunkIndex = (int)dat["chunk_index"];
            var chunkCount = (int)dat["chunk_count"];
            var nonce = (string)dat["nonce"];

            var mbrs = new HashSet<DiscordMember>();
            var pres = new HashSet<DiscordPresence>();

            var members = dat["members"].ToObject<TransportMember[]>();

            var memCount = members.Count();
            for (var i = 0; i < memCount; i++)
            {
                var mbr = new DiscordMember(members[i]) { Discord = this, _guild_id = guild.Id };

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

            if (dat["presences"] != null)
            {
                var presences = dat["presences"].ToObject<DiscordPresence[]>();

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

            if (dat["not_found"] != null)
            {
                var nf = dat["not_found"].ToObject<ISet<ulong>>();
                ea.NotFound = new ReadOnlySet<ulong>(nf);
            }

            await this._guildMembersChunked.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Guild Role

        /// <summary>
        /// Handles the guild role create event.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildRoleCreateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            role.Discord = this;
            role._guild_id = guild.Id;

            guild._roles[role.Id] = role;

            var ea = new GuildRoleCreateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Role = role
            };
            await this._guildRoleCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild role update event.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildRoleUpdateEventAsync(DiscordRole role, DiscordGuild guild)
        {
            var newRole = guild.GetRole(role.Id);
            var oldRole = new DiscordRole
            {
                _guild_id = guild.Id,
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

            newRole._guild_id = guild.Id;
            newRole._color = role._color;
            newRole.IsHoisted = role.IsHoisted;
            newRole.IsManaged = role.IsManaged;
            newRole.IsMentionable = role.IsMentionable;
            newRole.Name = role.Name;
            newRole.Permissions = role.Permissions;
            newRole.Position = role.Position;

            var ea = new GuildRoleUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                RoleAfter = newRole,
                RoleBefore = oldRole
            };
            await this._guildRoleUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild role delete event.
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnGuildRoleDeleteEventAsync(ulong roleId, DiscordGuild guild)
        {
            if (!guild._roles.TryRemove(roleId, out var role))
                this.Logger.LogWarning($"Attempted to delete a nonexistent role ({roleId}) from guild ({guild}).");

            var ea = new GuildRoleDeleteEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Role = role
            };
            await this._guildRoleDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Invite

        /// <summary>
        /// Handles the invite create event.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>
        /// <param name="invite">The invite.</param>

        internal async Task OnInviteCreateEventAsync(ulong channelId, ulong guildId, DiscordInvite invite)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            invite.Discord = this;

            if(invite.Inviter is not null)
            {
                invite.Inviter.Discord = this;
                this.UserCache.AddOrUpdate(invite.Inviter.Id, invite.Inviter, (old, @new) => @new);
            }

            guild._invites[invite.Code] = invite;

            var ea = new InviteCreateEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                Guild = guild,
                Invite = invite
            };
            await this._inviteCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the invite delete event.
        /// </summary>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>
        /// <param name="dat">The dat.</param>

        internal async Task OnInviteDeleteEventAsync(ulong channelId, ulong guildId, JToken dat)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId);

            if (!guild._invites.TryRemove(dat["code"].ToString(), out var invite))
            {
                invite = dat.ToObject<DiscordInvite>();
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
        /// <param name="chn">The chn.</param>
        /// <param name="messageId">The message id.</param>

        internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong messageId)
        {
            if (this.MessageCache == null || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == chn.Id, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = chn.Id,
                    Discord = this,
                };
            }

            await this._messageAcknowledged.InvokeAsync(this, new MessageAcknowledgeEventArgs(this.ServiceProvider) { Message = msg }).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message create event.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="author">The author.</param>
        /// <param name="member">The member.</param>
        /// <param name="referenceAuthor">The reference author.</param>
        /// <param name="referenceMember">The reference member.</param>

        internal async Task OnMessageCreateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
        {
            message.Discord = this;
            this.PopulateMessageReactionsAndCache(message, author, member);
            message.PopulateMentions();

            if (message.Channel == null && message.ChannelId == default)
                this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Channel which the last message belongs to is not in cache - cache state might be invalid!");

            if (message.ReferencedMessage != null)
            {
                message.ReferencedMessage.Discord = this;
                this.PopulateMessageReactionsAndCache(message.ReferencedMessage, referenceAuthor, referenceMember);
                message.ReferencedMessage.PopulateMentions();
            }

            foreach (var sticker in message.Stickers)
                sticker.Discord = this;

            var ea = new MessageCreateEventArgs(this.ServiceProvider)
            {
                Message = message,

                MentionedUsers = new ReadOnlyCollection<DiscordUser>(message._mentionedUsers),
                MentionedRoles = message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message._mentionedRoles) : null,
                MentionedChannels = message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message._mentionedChannels) : null
            };
            await this._messageCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message update event.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="author">The author.</param>
        /// <param name="member">The member.</param>
        /// <param name="referenceAuthor">The reference author.</param>
        /// <param name="referenceMember">The reference member.</param>

        internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
        {
            DiscordGuild guild;

            message.Discord = this;
            var event_message = message;

            DiscordMessage oldmsg = null;
            if (this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == event_message.Id && xm.ChannelId == event_message.ChannelId, out message))
            {
                message = event_message;
                this.PopulateMessageReactionsAndCache(message, author, member);
                guild = message.Channel?.Guild;

                if (message.ReferencedMessage != null)
                {
                    message.ReferencedMessage.Discord = this;
                    this.PopulateMessageReactionsAndCache(message.ReferencedMessage, referenceAuthor, referenceMember);
                    message.ReferencedMessage.PopulateMentions();
                }
            }
            else
            {
                oldmsg = new DiscordMessage(message);

                guild = message.Channel?.Guild;
                message.EditedTimestampRaw = event_message.EditedTimestampRaw;
                if (event_message.Content != null)
                    message.Content = event_message.Content;
                message._embeds.Clear();
                message._embeds.AddRange(event_message._embeds);
                message.Pinned = event_message.Pinned;
                message.IsTTS = event_message.IsTTS;
            }

            message.PopulateMentions();

            var ea = new MessageUpdateEventArgs(this.ServiceProvider)
            {
                Message = message,
                MessageBefore = oldmsg,
                MentionedUsers = new ReadOnlyCollection<DiscordUser>(message._mentionedUsers),
                MentionedRoles = message._mentionedRoles != null ? new ReadOnlyCollection<DiscordRole>(message._mentionedRoles) : null,
                MentionedChannels = message._mentionedChannels != null ? new ReadOnlyCollection<DiscordChannel>(message._mentionedChannels) : null
            };
            await this._messageUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message delete event.
        /// </summary>
        /// <param name="messageId">The message id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>

        internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);
            var guild = this.InternalGetCachedGuild(guildId);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {

                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this,
                };
            }

            if (this.Configuration.MessageCacheSize > 0)
                this.MessageCache?.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channelId);

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
        /// <param name="messageIds">The message ids.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>

        internal async Task OnMessageBulkDeleteEventAsync(ulong[] messageIds, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

            var msgs = new List<DiscordMessage>(messageIds.Length);
            foreach (var messageId in messageIds)
            {
                if (channel == null
                    || this.Configuration.MessageCacheSize == 0
                    || this.MessageCache == null
                    || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
                {
                    msg = new DiscordMessage
                    {
                        Id = messageId,
                        ChannelId = channelId,
                        Discord = this,
                    };
                }
                if (this.Configuration.MessageCacheSize > 0)
                    this.MessageCache?.Remove(xm => xm.Id == msg.Id && xm.ChannelId == channelId);
                msgs.Add(msg);
            }

            var guild = this.InternalGetCachedGuild(guildId);

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
        /// <param name="userId">The user id.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>
        /// <param name="mbr">The mbr.</param>
        /// <param name="emoji">The emoji.</param>

        internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, TransportMember mbr, DiscordEmoji emoji)
        {
            var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);
            var guild = this.InternalGetCachedGuild(guildId);
            emoji.Discord = this;

            var usr = this.UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this,
                    _reactions = new List<DiscordReaction>()
                };
            }

            var react = msg._reactions.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react == null)
            {
                msg._reactions.Add(react = new DiscordReaction
                {
                    Count = 1,
                    Emoji = emoji,
                    IsMe = this.CurrentUser.Id == userId
                });
            }
            else
            {
                react.Count++;
                react.IsMe |= this.CurrentUser.Id == userId;
            }

            var ea = new MessageReactionAddEventArgs(this.ServiceProvider)
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = emoji
            };
            await this._messageReactionAdded.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message reaction remove.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>
        /// <param name="emoji">The emoji.</param>

        internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji)
        {
            var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

            emoji.Discord = this;

            if (!this.UserCache.TryGetValue(userId, out var usr))
                usr = new DiscordUser { Id = userId, Discord = this };

            if (channel?.Guild != null)
                usr = channel.Guild.Members.TryGetValue(userId, out var member)
                    ? member
                    : new DiscordMember(usr) { Discord = this, _guild_id = channel.GuildId.Value };

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            var react = msg._reactions?.FirstOrDefault(xr => xr.Emoji == emoji);
            if (react != null)
            {
                react.Count--;
                react.IsMe &= this.CurrentUser.Id != userId;

                if (msg._reactions != null && react.Count <= 0) // shit happens
                    for (var i = 0; i < msg._reactions.Count; i++)
                        if (msg._reactions[i].Emoji == emoji)
                        {
                            msg._reactions.RemoveAt(i);
                            break;
                        }
            }

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageReactionRemoveEventArgs(this.ServiceProvider)
            {
                Message = msg,
                User = usr,
                Guild = guild,
                Emoji = emoji
            };
            await this._messageReactionRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message reaction remove all.
        /// </summary>
        /// <param name="messageId">The message id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>

        internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, ulong channelId, ulong? guildId)
        {
            var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            msg._reactions?.Clear();

            var guild = this.InternalGetCachedGuild(guildId);

            var ea = new MessageReactionsClearEventArgs(this.ServiceProvider)
            {
                Message = msg
            };

            await this._messageReactionsCleared.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the message reaction remove emoji.
        /// </summary>
        /// <param name="messageId">The message id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="guildId">The guild id.</param>
        /// <param name="dat">The dat.</param>

        internal async Task OnMessageReactionRemoveEmojiAsync(ulong messageId, ulong channelId, ulong guildId, JToken dat)
        {
            var guild = this.InternalGetCachedGuild(guildId);
            var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

            if (channel == null
                || this.Configuration.MessageCacheSize == 0
                || this.MessageCache == null
                || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
            {
                msg = new DiscordMessage
                {
                    Id = messageId,
                    ChannelId = channelId,
                    Discord = this
                };
            }

            var partialEmoji = dat.ToObject<DiscordEmoji>();

            if (!guild._emojis.TryGetValue(partialEmoji.Id, out var emoji))
            {
                emoji = partialEmoji;
                emoji.Discord = this;
            }

            msg._reactions?.RemoveAll(r => r.Emoji.Equals(emoji));

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
        /// <param name="stage">The created stage instance.</param>
        internal async Task OnStageInstanceCreateEventAsync(DiscordStageInstance stage)
        {
            stage.Discord = this;

            var guild = this.InternalGetCachedGuild(stage.GuildId);
            guild._stageInstances[stage.Id] = stage;

            await this._stageInstanceCreated.InvokeAsync(this, new StageInstanceCreateEventArgs(this.ServiceProvider) { StageInstance = stage, Guild = stage.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="StageInstanceUpdated"/> event.
        /// </summary>
        /// <param name="stage">The updated stage instance.</param>
        internal async Task OnStageInstanceUpdateEventAsync(DiscordStageInstance stage)
        {
            stage.Discord = this;
            var guild = this.InternalGetCachedGuild(stage.GuildId);
            guild._stageInstances[stage.Id] = stage;

            await this._stageInstanceUpdated.InvokeAsync(this, new StageInstanceUpdateEventArgs(this.ServiceProvider) { StageInstance = stage, Guild = stage.Guild }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="StageInstanceDeleted"/> event.
        /// </summary>
        /// <param name="stage">The deleted stage instance.</param>
        internal async Task OnStageInstanceDeleteEventAsync(DiscordStageInstance stage)
        {
            stage.Discord = this;
            var guild = this.InternalGetCachedGuild(stage.GuildId);
            guild._stageInstances[stage.Id] = stage;

            await this._stageInstanceDeleted.InvokeAsync(this, new StageInstanceDeleteEventArgs(this.ServiceProvider) { StageInstance = stage, Guild = stage.Guild }).ConfigureAwait(false);
        }

        #endregion

        #region Thread

        /// <summary>
        /// Dispatches the <see cref="ThreadCreated"/> event.
        /// </summary>
        /// <param name="thread">The created thread.</param>
        internal async Task OnThreadCreateEventAsync(DiscordThreadChannel thread)
        {
            thread.Discord = this;
            this.InternalGetCachedGuild(thread.GuildId)._threads.AddOrUpdate(thread.Id, thread, (oldThread, newThread) => newThread);

            await this._threadCreated.InvokeAsync(this, new ThreadCreateEventArgs(this.ServiceProvider) { Thread = thread, Guild = thread.Guild, Parent = thread.Parent }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="DiscordClient.ThreadUpdated"/> event.
        /// </summary>
        /// <param name="thread">The updated thread.</param>
        internal async Task OnThreadUpdateEventAsync(DiscordThreadChannel thread)
        {
            if (thread == null)
                return;

            thread.Discord = this;
            var guild = thread.Guild;

            var threadNew = this.InternalGetCachedThread(thread.Id);
            DiscordThreadChannel threadOld = null;
            ThreadUpdateEventArgs updateEvent;

            if (threadNew != null)
            {
                threadOld = new DiscordThreadChannel
                {
                    Discord = this,
                    Type = threadNew.Type,
                    ThreadMetadata = thread.ThreadMetadata,
                    _threadMembers = threadNew._threadMembers,
                    ParentId = thread.ParentId,
                    OwnerId = thread.OwnerId,
                    Name = thread.Name,
                    LastMessageId = threadNew.LastMessageId,
                    MessageCount = thread.MessageCount,
                    MemberCount = thread.MemberCount,
                    GuildId = thread.GuildId,
                    LastPinTimestampRaw = threadNew.LastPinTimestampRaw,
                    PerUserRateLimit = threadNew.PerUserRateLimit,
                    CurrentMember = threadNew.CurrentMember
                };

                threadNew.ThreadMetadata = thread.ThreadMetadata;
                threadNew.ParentId = thread.ParentId;
                threadNew.OwnerId = thread.OwnerId;
                threadNew.Name = thread.Name;
                threadNew.LastMessageId = thread.LastMessageId.HasValue ? thread.LastMessageId : threadOld.LastMessageId;
                threadNew.MessageCount = thread.MessageCount;
                threadNew.MemberCount = thread.MemberCount;
                threadNew.GuildId = thread.GuildId;

                updateEvent = new ThreadUpdateEventArgs(this.ServiceProvider)
                {
                    ThreadAfter = thread,
                    ThreadBefore = threadOld,
                    Guild = thread.Guild,
                    Parent = thread.Parent
                };
            }
            else
            {
                updateEvent = new ThreadUpdateEventArgs(this.ServiceProvider)
                {
                    ThreadAfter = thread,
                    Guild = thread.Guild,
                    Parent = thread.Parent
                };
                guild._threads[thread.Id] = thread;
            }

            await this._threadUpdated.InvokeAsync(this, updateEvent).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadDeleted"/> event.
        /// </summary>
        /// <param name="thread">The deleted thread.</param>
        internal async Task OnThreadDeleteEventAsync(DiscordThreadChannel thread)
        {
            if (thread == null)
                return;

            thread.Discord = this;

            var gld = thread.Guild;
            if (gld._threads.TryRemove(thread.Id, out var cachedThread))
                thread = cachedThread;

            await this._threadDeleted.InvokeAsync(this, new ThreadDeleteEventArgs(this.ServiceProvider) { Thread = thread, Guild = thread.Guild, Parent = thread.Parent, Type = thread.Type }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadListSynced"/> event.
        /// </summary>
        /// <param name="guild">The synced guild.</param>
        /// <param name="channel_ids">The synced channel ids.</param>
        /// <param name="threads">The synced threads.</param>
        /// <param name="members">The synced members.</param>
        internal async Task OnThreadListSyncEventAsync(DiscordGuild guild, IReadOnlyList<ulong?> channel_ids, IReadOnlyList<DiscordThreadChannel> threads, IReadOnlyList<DiscordThreadChannelMember> members)
        {
            guild.Discord = this;

            var channels = channel_ids.Select(x => guild.GetChannel(x.Value)); //getting channel objects
            foreach (var chan in channels)
            {
                chan.Discord = this;
            }
            threads.Select(x => x.Discord = this);

            await this._threadListSynced.InvokeAsync(this, new ThreadListSyncEventArgs(this.ServiceProvider) { Guild = guild, Channels = channels.ToList().AsReadOnly(), Threads = threads, Members = members.ToList().AsReadOnly() }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadMemberUpdated"/> event.
        /// </summary>
        /// <param name="member">The updated member.</param>
        internal async Task OnThreadMemberUpdateEventAsync(DiscordThreadChannelMember member)
        {
            member.Discord = this;
            var thread = this.InternalGetCachedThread(member.Id);
            if (thread == null)
            {
                var tempThread = await this.ApiClient.GetThreadAsync(member.Id);
                thread = this._guilds[member._guild_id]._threads.AddOrUpdate(member.Id, tempThread, (old, newThread) => newThread);
            }

            thread.CurrentMember = member;
            thread.Guild._threads.AddOrUpdate(member.Id, thread, (oldThread, newThread) => newThread);


            await this._threadMemberUpdated.InvokeAsync(this, new ThreadMemberUpdateEventArgs(this.ServiceProvider) { ThreadMember = member, Thread = thread }).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispatches the <see cref="ThreadMembersUpdated"/> event.
        /// </summary>
        /// <param name="guild">The target guild.</param>
        /// <param name="thread_id">The thread id of the target thread this update belongs to.</param>
        /// <param name="added_members">The added members.</param>
        /// <param name="removed_members">The ids of the removed members.</param>
        /// <param name="member_count">The new member count.</param>
        internal async Task OnThreadMembersUpdateEventAsync(DiscordGuild guild, ulong thread_id, JArray added_members, JArray removed_members, int member_count)
        {
            var thread = this.InternalGetCachedThread(thread_id);
            if (thread == null)
            {
                var tempThread = await this.ApiClient.GetThreadAsync(thread_id);
                thread = guild._threads.AddOrUpdate(thread_id, tempThread, (old, newThread) => newThread);
            }

            thread.Discord = this;
            guild.Discord = this;
            List<DiscordThreadChannelMember> addedMembers = new();
            List<ulong> removed_member_ids = new();

            if (added_members != null)
            {
                foreach (var xj in added_members)
                {
                    var xtm = xj.ToDiscordObject<DiscordThreadChannelMember>();
                    xtm.Discord = this;
                    xtm._guild_id = guild.Id;
                    if(xtm != null)
                        addedMembers.Add(xtm);

                    if (xtm.Id == this.CurrentUser.Id)
                        thread.CurrentMember = xtm;
                }
            }

            var removedMembers = new List<DiscordMember>();
            if (removed_members != null)
            {
                foreach (var removedId in removed_members)
                {
                    removedMembers.Add(guild._members.TryGetValue((ulong)removedId, out var member) ? member : new DiscordMember { Id = (ulong)removedId, _guild_id = guild.Id, Discord = this });
                }
            }

            if (removed_member_ids.Contains(this.CurrentUser.Id)) //indicates the bot was removed from the thread
                thread.CurrentMember = null;

            thread.MemberCount = member_count;

            var threadMembersUpdateArg = new ThreadMembersUpdateEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Thread = thread,
                AddedMembers = addedMembers,
                RemovedMembers = removedMembers,
                MemberCount = member_count
            };

            await this._threadMembersUpdated.InvokeAsync(this, threadMembersUpdateArg).ConfigureAwait(false);
        }

        #endregion

        #region Activities
        /// <summary>
        /// Dispatches the <see cref="EmbeddedActivityUpdated"/> event.
        /// </summary>
        /// <param name="tr_activity">The transport activity.</param>
        /// <param name="guild">The guild.</param>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="j_users">The users in the activity.</param>
        /// <param name="app_id">The application id.</param>
        /// <returns>A Task.</returns>
        internal async Task OnEmbeddedActivityUpdateAsync(JObject tr_activity, DiscordGuild guild, ulong channel_id, JArray j_users, ulong app_id)
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
        /// <param name="rawPresence">The raw presence.</param>
        /// <param name="rawUser">The raw user.</param>

        internal async Task OnPresenceUpdateEventAsync(JObject rawPresence, JObject rawUser)
        {
            var uid = (ulong)rawUser["id"];
            DiscordPresence old = null;

            if (this._presences.TryGetValue(uid, out var presence))
            {
                old = new DiscordPresence(presence);
                DiscordJson.PopulateObject(rawPresence, presence);
            }
            else
            {
                presence = rawPresence.ToObject<DiscordPresence>();
                presence.Discord = this;
                presence.Activity = new DiscordActivity(presence.RawActivity);
                this._presences[presence.InternalUser.Id] = presence;
            }

            // reuse arrays / avoid linq (this is a hot zone)
            if (presence.Activities == null || rawPresence["activities"] == null)
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

                if (rawUser["username"] is object)
                    usr.Username = (string)rawUser["username"];
                if (rawUser["discriminator"] is object)
                    usr.Discriminator = (string)rawUser["discriminator"];
                if (rawUser["avatar"] is object)
                    usr.AvatarHash = (string)rawUser["avatar"];

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
        /// <param name="user">The user.</param>

        internal async Task OnUserSettingsUpdateEventAsync(TransportUser user)
        {
            var usr = new DiscordUser(user) { Discord = this };

            var ea = new UserSettingsUpdateEventArgs(this.ServiceProvider)
            {
                User = usr
            };
            await this._userSettingsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the user update event.
        /// </summary>
        /// <param name="user">The user.</param>

        internal async Task OnUserUpdateEventAsync(TransportUser user)
        {
            var usr_old = new DiscordUser
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

            this.CurrentUser.AvatarHash = user.AvatarHash;
            this.CurrentUser.Discriminator = user.Discriminator;
            this.CurrentUser.Email = user.Email;
            this.CurrentUser.Id = user.Id;
            this.CurrentUser.IsBot = user.IsBot;
            this.CurrentUser.MfaEnabled = user.MfaEnabled;
            this.CurrentUser.Username = user.Username;
            this.CurrentUser.Verified = user.Verified;

            var ea = new UserUpdateEventArgs(this.ServiceProvider)
            {
                UserAfter = this.CurrentUser,
                UserBefore = usr_old
            };
            await this._userUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Voice

        /// <summary>
        /// Handles the voice state update event.
        /// </summary>
        /// <param name="raw">The raw.</param>

        internal async Task OnVoiceStateUpdateEventAsync(JObject raw)
        {
            var gid = (ulong)raw["guild_id"];
            var uid = (ulong)raw["user_id"];
            var gld = this._guilds[gid];

            var vstateNew = raw.ToObject<DiscordVoiceState>();
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
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="token">The token.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnVoiceServerUpdateEventAsync(string endpoint, string token, DiscordGuild guild)
        {
            var ea = new VoiceServerUpdateEventArgs(this.ServiceProvider)
            {
                Endpoint = endpoint,
                VoiceToken = token,
                Guild = guild
            };
            await this._voiceServerUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Handles the application command create.
        /// </summary>
        /// <param name="cmd">The cmd.</param>
        /// <param name="guild_id">The guild_id.</param>

        internal async Task OnApplicationCommandCreateAsync(DiscordApplicationCommand cmd, ulong? guild_id)
        {
            cmd.Discord = this;

            var guild = this.InternalGetCachedGuild(guild_id);

            if (guild == null && guild_id.HasValue)
            {
                guild = new DiscordGuild
                {
                    Id = guild_id.Value,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Command = cmd
            };

            await this._applicationCommandCreated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the application command update.
        /// </summary>
        /// <param name="cmd">The cmd.</param>
        /// <param name="guild_id">The guild_id.</param>

        internal async Task OnApplicationCommandUpdateAsync(DiscordApplicationCommand cmd, ulong? guild_id)
        {
            cmd.Discord = this;

            var guild = this.InternalGetCachedGuild(guild_id);

            if (guild == null && guild_id.HasValue)
            {
                guild = new DiscordGuild
                {
                    Id = guild_id.Value,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Command = cmd
            };

            await this._applicationCommandUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the application command delete.
        /// </summary>
        /// <param name="cmd">The cmd.</param>
        /// <param name="guild_id">The guild_id.</param>

        internal async Task OnApplicationCommandDeleteAsync(DiscordApplicationCommand cmd, ulong? guild_id)
        {
            cmd.Discord = this;

            var guild = this.InternalGetCachedGuild(guild_id);

            if (guild == null && guild_id.HasValue)
            {
                guild = new DiscordGuild
                {
                    Id = guild_id.Value,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandEventArgs(this.ServiceProvider)
            {
                Guild = guild,
                Command = cmd
            };

            await this._applicationCommandDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the guild application command counts update.
        /// </summary>
        /// <param name="sc">The <see cref="ApplicationCommandType.ChatInput"/> count.</param>
        /// <param name="ucmc">The <see cref="ApplicationCommandType.User"/> count.</param>
        /// <param name="mcmc">The <see cref="ApplicationCommandType.Message"/> count.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>Count of application commands.</returns>
        internal async Task OnGuildApplicationCommandCountsUpdateAsync(int sc, int ucmc, int mcmc, ulong guild_id)
        {
            var guild = this.InternalGetCachedGuild(guild_id);

            if (guild == null)
            {
                guild = new DiscordGuild
                {
                    Id = guild_id,
                    Discord = this
                };
            }

            var ea = new GuildApplicationCommandCountEventArgs(this.ServiceProvider)
            {
                SlashCommands = sc,
                UserContextMenuCommands = ucmc,
                MessageContextMenuCommands = mcmc,
                Guild = guild
            };

            await this._guildApplicationCommandCountUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the application command permissions update.
        /// </summary>
        /// <param name="perms">The new permissions.</param>
        /// <param name="c_id">The command id.</param>
        /// <param name="guild_id">The guild id.</param>
        /// <param name="a_id">The application id.</param>
        internal async Task OnApplicationCommandPermissionsUpdateAsync(IEnumerable<DiscordApplicationCommandPermission> perms, ulong c_id, ulong guild_id, ulong a_id)
        {
            if (a_id != this.CurrentApplication.Id)
                return;

            var guild = this.InternalGetCachedGuild(guild_id);

            DiscordApplicationCommand cmd;
            try
            {
                cmd = await this.GetGuildApplicationCommandAsync(guild_id, c_id);
            }
            catch(NotFoundException)
            {
                cmd = await this.GetGlobalApplicationCommandAsync(c_id);
            }

            if (guild == null)
            {
                guild = new DiscordGuild
                {
                    Id = guild_id,
                    Discord = this
                };
            }

            var ea = new ApplicationCommandPermissionsUpdateEventArgs(this.ServiceProvider)
            {
                Permissions = perms.ToList(),
                Command = cmd,
                ApplicationId = a_id,
                Guild = guild
            };

            await this._applicationCommandPermissionsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #region Interaction

        /// <summary>
        /// Handles the interaction create.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="user">The user.</param>
        /// <param name="member">The member.</param>
        /// <param name="interaction">The interaction.</param>

        internal async Task OnInteractionCreateAsync(ulong? guildId, ulong channelId, TransportUser user, TransportMember member, DiscordInteraction interaction)
        {
            var usr = new DiscordUser(user) { Discord = this };

            interaction.ChannelId = channelId;
            interaction.GuildId = guildId;
            interaction.Discord = this;
            interaction.Data.Discord = this;

            if (member != null)
            {
                usr = new DiscordMember(member) { _guild_id = guildId.Value, Discord = this };
                this.UpdateUser(usr, guildId, interaction.Guild, member);
            }
            else
            {
                this.UserCache.AddOrUpdate(usr.Id, usr, (old, @new) => @new);
            }

            interaction.User = usr;

            var resolved = interaction.Data.Resolved;
            if (resolved != null)
            {
                if (resolved.Users != null)
                {
                    foreach (var c in resolved.Users)
                    {
                        c.Value.Discord = this;
                        this.UserCache.AddOrUpdate(c.Value.Id, c.Value, (old, @new) => @new);
                    }
                }

                if (resolved.Members != null)
                {
                    foreach (var c in resolved.Members)
                    {
                        c.Value.Discord = this;
                        c.Value.Id = c.Key;
                        c.Value._guild_id = guildId.Value;
                        c.Value.User.Discord = this;
                        this.UserCache.AddOrUpdate(c.Value.User.Id, c.Value.User, (old, @new) => @new);
                    }
                }

                if (resolved.Channels != null)
                {
                    foreach (var c in resolved.Channels)
                    {
                        c.Value.Discord = this;

                        if (guildId.HasValue)
                            c.Value.GuildId = guildId.Value;
                    }
                }

                if (resolved.Roles != null)
                {
                    foreach (var c in resolved.Roles)
                    {
                        c.Value.Discord = this;

                        if (guildId.HasValue)
                            c.Value._guild_id = guildId.Value;
                    }
                }


                if (resolved.Messages != null)
                {
                    foreach (var m in resolved.Messages)
                    {
                        m.Value.Discord = this;

                        if (guildId.HasValue)
                            m.Value.GuildId = guildId.Value;
                    }
                }
            }

            if (interaction.Type is InteractionType.Component)
            {

                interaction.Message.Discord = this;
                interaction.Message.ChannelId = interaction.ChannelId;
                var cea = new ComponentInteractionCreateEventArgs(this.ServiceProvider)
                {
                    Message = interaction.Message,
                    Interaction = interaction
                };

                await this._componentInteractionCreated.InvokeAsync(this, cea).ConfigureAwait(false);
            }
            else
            {
                if (interaction.Data.Target.HasValue) // Context-Menu. //
                {
                    var targetId = interaction.Data.Target.Value;
                    DiscordUser targetUser = null;
                    DiscordMember targetMember = null;
                    DiscordMessage targetMessage = null;

                    interaction.Data.Resolved.Messages?.TryGetValue(targetId, out targetMessage);
                    interaction.Data.Resolved.Members?.TryGetValue(targetId, out targetMember);
                    interaction.Data.Resolved.Users?.TryGetValue(targetId, out targetUser);

                    var ctea = new ContextMenuInteractionCreateEventArgs(this.ServiceProvider)
                    {
                        Interaction = interaction,
                        TargetUser = targetMember ?? targetUser,
                        TargetMessage = targetMessage,
                        Type = interaction.Data.Type,
                    };
                    await this._contextMenuInteractionCreated.InvokeAsync(this, ctea).ConfigureAwait(false);
                }
                else
                {
                    var ea = new InteractionCreateEventArgs(this.ServiceProvider)
                    {
                        Interaction = interaction
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
        /// <param name="userId">The user id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="guildId">The guild id.</param>
        /// <param name="started">The started.</param>
        /// <param name="mbr">The mbr.</param>

        internal async Task OnTypingStartEventAsync(ulong userId, ulong channelId, DiscordChannel channel, ulong? guildId, DateTimeOffset started, TransportMember mbr)
        {
            if (channel == null)
            {
                channel = new DiscordChannel
                {
                    Discord = this,
                    Id = channelId,
                    GuildId = guildId ?? default,
                };
            }

            var guild = this.InternalGetCachedGuild(guildId);
            var usr = this.UpdateUser(new DiscordUser { Id = userId, Discord = this }, guildId, guild, mbr);

            var ea = new TypingStartEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                User = usr,
                Guild = guild,
                StartedAt = started
            };
            await this._typingStarted.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the webhooks update.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="guild">The guild.</param>

        internal async Task OnWebhooksUpdateAsync(DiscordChannel channel, DiscordGuild guild)
        {
            var ea = new WebhooksUpdateEventArgs(this.ServiceProvider)
            {
                Channel = channel,
                Guild = guild
            };
            await this._webhooksUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the unknown event.
        /// </summary>
        /// <param name="payload">The payload.</param>

        internal async Task OnUnknownEventAsync(GatewayPayload payload)
        {
            var ea = new UnknownEventArgs(this.ServiceProvider) { EventName = payload.EventName, Json = (payload.Data as JObject)?.ToString() };
            await this._unknownEvent.InvokeAsync(this, ea).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}
