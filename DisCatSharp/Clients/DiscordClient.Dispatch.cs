using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp;

/// <summary>
/// Represents a discord Logger.ent.L
/// </summary>
public sealed partial class DiscordClient
{
	#region Private Fields

	private string _resumeGatewayUrl;
	private string _sessionId;
	private bool _guildDownloadCompleted;

	private readonly Dictionary<string, KeyValuePair<TimeoutHandler, Timer>> _tempTimers = new();

	/// <summary>
	/// Represents a timeout handler.
	/// </summary>
	internal class TimeoutHandler
	{
		/// <summary>
		/// Gets the member.
		/// </summary>
		internal readonly DiscordMember Member;

		/// <summary>
		/// Gets the guild.
		/// </summary>
		internal readonly DiscordGuild Guild;

		/// <summary>
		/// Gets the old timeout value.
		/// </summary>
		internal DateTime? TimeoutUntilOld;

		/// <summary>
		/// Gets the new timeout value.
		/// </summary>
		internal DateTime? TimeoutUntilNew;

		/// <summary>
		/// Constructs a new <see cref="TimeoutHandler"/>.
		/// </summary>
		/// <param name="mbr">The affected member.</param>
		/// <param name="guild">The affected guild.</param>
		/// <param name="too">The old timeout value.</param>
		/// <param name="ton">The new timeout value.</param>
		internal TimeoutHandler(DiscordMember mbr, DiscordGuild guild, DateTime? too, DateTime? ton)
		{
			this.Guild = guild;
			this.Member = mbr;
			this.TimeoutUntilOld = too;
			this.TimeoutUntilNew = ton;
		}
	}

	#endregion

	#region Dispatch Handler

	/// <summary>
	/// Handles the dispatch payloads.
	/// </summary>
	/// <param name="payload">The payload.</param>
	internal async Task HandleDispatchAsync(GatewayPayload payload)
	{
		if (payload.Data is not JObject dat)
		{
			this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Invalid payload body (this message is probably safe to ignore); opcode: {op} event: {event}; payload: {payload}", payload.OpCode, payload.EventName, payload.Data);
			return;
		}

		if ((this.Configuration?.EnableLibraryDeveloperMode ?? false) || (this.Configuration?.EnablePayloadReceivedEvent ?? false))
			await this._payloadReceived.InvokeAsync(this, new(this.ServiceProvider)
			{
				EventName = payload.EventName,
				PayloadObject = dat
			}).ConfigureAwait(false);


#region Default objects

		var payloadString = dat.ToString();
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
		DiscordApplicationCommand ac = default;
		DiscordEntitlement ent = default;
		JToken rawMbr = default;
		var rawRefMsg = dat["referenced_message"]; // TODO: Can we remove this?
		#endregion

		switch (payload.EventName.ToLowerInvariant())
		{
			#region Gateway Status

			case "ready":
				var glds = (JArray)dat["guilds"]!;
				await this.OnReadyEventAsync(dat!.ToObject<ReadyPayload>()!, glds).ConfigureAwait(false);
				break;

			case "resumed":
				await this.OnResumedAsync().ConfigureAwait(false);
				break;

			#endregion

			#region Channel

			case "channel_create":
				chn = DiscordJson.DeserializeObject<DiscordChannel>(payloadString, this);
				await this.OnChannelCreateEventAsync(chn).ConfigureAwait(false);
				break;

			case "channel_update":
				chn = DiscordJson.DeserializeObject<DiscordChannel>(payloadString, this);
				await this.OnChannelUpdateEventAsync(chn).ConfigureAwait(false);
				break;

			case "channel_delete":
				chn = DiscordJson.DeserializeObject<DiscordChannel>(payloadString, this);
				await this.OnChannelDeleteEventAsync(chn.IsPrivate ? chn as DiscordDmChannel : chn).ConfigureAwait(false);
				break;

			case "channel_pins_update":
				cid = (ulong)dat["channel_id"]!;
				var ts = (string)dat["last_pin_timestamp"]!;
				await this.OnChannelPinsUpdateAsync((ulong?)dat["guild_id"], cid, ts != null ? DateTimeOffset.Parse(ts, CultureInfo.InvariantCulture) : default(DateTimeOffset?)).ConfigureAwait(false);
				break;

			case "voice_channel_status_update":
				cid = (ulong)dat["id"]!;
				var status = (string?)dat["status"]!;
				await this.OnVoiceChannelStatusUpdateAsync((ulong)dat["guild_id"], cid, status).ConfigureAwait(false);
				break;

			case "channel_topic_update":
				// It's fired incorrectly.
				break;

			#endregion

			#region Guild

			case "guild_create":
				await this.OnGuildCreateEventAsync(dat.ToDiscordObject<DiscordGuild>(), (JArray)dat["members"]!, dat["presences"]!.ToDiscordObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
				break;

			case "guild_update":
				await this.OnGuildUpdateEventAsync(dat.ToDiscordObject<DiscordGuild>(), (JArray)dat["members"]!).ConfigureAwait(false);
				break;

			case "guild_delete":
				await this.OnGuildDeleteEventAsync(DiscordJson.DeserializeObject<DiscordGuild>(payloadString, this)).ConfigureAwait(false);
				break;

			case "guild_audit_log_entry_create":
				gid = (ulong)dat["guild_id"]!;
				dat.Remove("guild_id");
				await this.OnGuildAuditLogEntryCreateEventAsync(this.GuildsInternal[gid], dat).ConfigureAwait(false);
				break;

			case "guild_sync":
				gid = (ulong)dat["id"]!;
				await this.OnGuildSyncEventAsync(this.GuildsInternal[gid], (bool)dat["large"]!, (JArray)dat["members"]!, dat["presences"]!.ToDiscordObject<IEnumerable<DiscordPresence>>()).ConfigureAwait(false);
				break;

			case "guild_emojis_update":
				gid = (ulong)dat["guild_id"]!;
				var ems = dat["emojis"]!.ToObject<IEnumerable<DiscordEmoji>>()!;
				await this.OnGuildEmojisUpdateEventAsync(this.GuildsInternal[gid], ems).ConfigureAwait(false);
				break;

			case "guild_stickers_update":
				gid = (ulong)dat["guild_id"]!;
				var strs = dat["stickers"]!.ToDiscordObject<IEnumerable<DiscordSticker>>();
				await this.OnStickersUpdatedAsync(strs, gid).ConfigureAwait(false);
				break;

			case "guild_integrations_update":
				gid = (ulong)dat["guild_id"]!;

				// discord fires this event inconsistently if the current user leaves a guild.
				if (!this.GuildsInternal.ContainsKey(gid))
					return;

				await this.OnGuildIntegrationsUpdateEventAsync(this.GuildsInternal[gid]).ConfigureAwait(false);
				break;
			#endregion

			#region Guild Automod
			case "auto_moderation_rule_create":
				await this.OnAutomodRuleCreated(dat.ToDiscordObject<AutomodRule>()).ConfigureAwait(false);
				break;
			case "auto_moderation_rule_update":
				await this.OnAutomodRuleUpdated(dat.ToDiscordObject<AutomodRule>()).ConfigureAwait(false);
				break;
			case "auto_moderation_rule_delete":
				await this.OnAutomodRuleDeleted(dat.ToDiscordObject<AutomodRule>()).ConfigureAwait(false);
				break;
			case "auto_moderation_action_execution":
				gid = (ulong)dat["guild_id"]!;
				await this.OnAutomodActionExecuted(this.GuildsInternal[gid], dat).ConfigureAwait(false);
				break;
			#endregion

			#region Guild Ban

			case "guild_ban_add":
				usr = DiscordJson.DeserializeObject<TransportUser>(dat["user"]!.ToString(), this);
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildBanAddEventAsync(usr, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_ban_remove":
				usr = DiscordJson.DeserializeObject<TransportUser>(dat["user"]!.ToString(), this);
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildBanRemoveEventAsync(usr, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			#endregion

			#region Guild Event

			case "guild_scheduled_event_create":
				gse = DiscordJson.DeserializeObject<DiscordScheduledEvent>(payloadString, this);
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildScheduledEventCreateEventAsync(gse, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_scheduled_event_update":
				gse = DiscordJson.DeserializeObject<DiscordScheduledEvent>(payloadString, this);
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildScheduledEventUpdateEventAsync(gse, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_scheduled_event_delete":
				gse = DiscordJson.DeserializeObject<DiscordScheduledEvent>(payloadString, this);
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildScheduledEventDeleteEventAsync(gse, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_scheduled_event_user_add":
				gid = (ulong)dat["guild_id"]!;
				uid = (ulong)dat["user_id"]!;
				await this.OnGuildScheduledEventUserAddedEventAsync((ulong)dat["guild_scheduled_event_id"]!, uid, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_scheduled_event_user_remove":
				gid = (ulong)dat["guild_id"]!;
				uid = (ulong)dat["user_id"]!;
				await this.OnGuildScheduledEventUserRemovedEventAsync((ulong)dat["guild_scheduled_event_id"]!, uid, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			#endregion

			#region Guild Integration

			case "integration_create":
				gid = (ulong)dat["guild_id"]!;
				itg = DiscordJson.DeserializeObject<DiscordIntegration>(payloadString, this);

				// discord fires this event inconsistently if the current user leaves a guild.
				if (!this.GuildsInternal.ContainsKey(gid))
					return;

				await this.OnGuildIntegrationCreateEventAsync(this.GuildsInternal[gid], itg).ConfigureAwait(false);
				break;

			case "integration_update":
				gid = (ulong)dat["guild_id"]!;
				itg = DiscordJson.DeserializeObject<DiscordIntegration>(payloadString, this);

				// discord fires this event inconsistently if the current user leaves a guild.
				if (!this.GuildsInternal.ContainsKey(gid))
					return;

				await this.OnGuildIntegrationUpdateEventAsync(this.GuildsInternal[gid], itg).ConfigureAwait(false);
				break;

			case "integration_delete":
				gid = (ulong)dat["guild_id"]!;

				// discord fires this event inconsistently if the current user leaves a guild.
				if (!this.GuildsInternal.ContainsKey(gid))
					return;

				await this.OnGuildIntegrationDeleteEventAsync(this.GuildsInternal[gid], (ulong)dat["id"], (ulong?)dat["application_id"]).ConfigureAwait(false);
				break;
			#endregion

			#region Guild Member

			case "guild_member_add":
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildMemberAddEventAsync(DiscordJson.DeserializeObject<TransportMember>(payloadString, this), this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_member_remove":
				gid = (ulong)dat["guild_id"]!;
				usr = DiscordJson.DeserializeObject<TransportUser>(dat["user"]!.ToString(), this);

				if (!this.GuildsInternal.ContainsKey(gid))
				{
					// discord fires this event inconsistently if the current user leaves a guild.
					if (usr.Id != this.CurrentUser.Id)
						this.Logger.LogError(LoggerEvents.WebSocketReceive, "Could not find {0} in guild cache", gid);
					return;
				}

				await this.OnGuildMemberRemoveEventAsync(usr, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_member_update":
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildMemberUpdateEventAsync(DiscordJson.DeserializeObject<TransportMember>(payloadString, this), this.GuildsInternal[gid], dat["roles"]!.ToObject<IEnumerable<ulong>>()!, (string)dat["nick"]!, (bool?)dat["pending"]).ConfigureAwait(false);
				break;

			case "guild_members_chunk":
				await this.OnGuildMembersChunkEventAsync(dat).ConfigureAwait(false);
				break;

			#endregion

			#region Guild Role

			case "guild_role_create":
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildRoleCreateEventAsync(DiscordJson.DeserializeObject<DiscordRole>(dat["role"]!.ToString(), this), this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_role_update":
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildRoleUpdateEventAsync(DiscordJson.DeserializeObject<DiscordRole>(dat["role"]!.ToString(), this), this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_role_delete":
				gid = (ulong)dat["guild_id"]!;
				await this.OnGuildRoleDeleteEventAsync((ulong)dat["role_id"]!, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			#endregion

			#region Invite

			case "invite_create":
				gid = (ulong)dat["guild_id"]!;
				cid = (ulong)dat["channel_id"]!;
				await this.OnInviteCreateEventAsync(cid, gid, DiscordJson.DeserializeObject<DiscordInvite>(payloadString, this)).ConfigureAwait(false);
				break;

			case "invite_delete":
				gid = (ulong)dat["guild_id"]!;
				cid = (ulong)dat["channel_id"]!;
				await this.OnInviteDeleteEventAsync(cid, gid, dat).ConfigureAwait(false);
				break;

			#endregion

			#region Message

			case "message_ack":
				cid = (ulong)dat["channel_id"]!;
				var mid = (ulong)dat["message_id"]!;
				await this.OnMessageAckEventAsync(this.InternalGetCachedChannel(cid), mid).ConfigureAwait(false);
				break;

			case "message_create":
				rawMbr = dat["member"]!;

				if (rawMbr != null)
					mbr = DiscordJson.DeserializeObject<TransportMember>(rawMbr.ToString(), this);

				if (rawRefMsg != null && rawRefMsg.HasValues)
				{
					if (rawRefMsg.SelectToken("author") != null)
						refUsr = DiscordJson.DeserializeObject<TransportUser>(rawRefMsg.SelectToken("author")!.ToString(), this);

					if (rawRefMsg.SelectToken("member") != null)
						refMbr = DiscordJson.DeserializeObject<TransportMember>(json: rawRefMsg.SelectToken("member")!.ToString(), this);
				}
				await this.OnMessageCreateEventAsync(dat.ToDiscordObject<DiscordMessage>(), dat["author"].ToObject<TransportUser>(), mbr, refUsr, refMbr).ConfigureAwait(false);
				break;

			case "message_update":
				rawMbr = dat["member"]!;

				if (rawMbr != null)
					mbr = DiscordJson.DeserializeObject<TransportMember>(rawMbr.ToString(), this);

				if (rawRefMsg != null && rawRefMsg.HasValues)
				{
					if (rawRefMsg.SelectToken("author") != null)
						refUsr = DiscordJson.DeserializeObject<TransportUser>(rawRefMsg.SelectToken("author")!.ToString(), this);

					if (rawRefMsg.SelectToken("member") != null)
						refMbr = DiscordJson.DeserializeObject<TransportMember>(json: rawRefMsg.SelectToken("member")!.ToString(), this);
				}

				await this.OnMessageUpdateEventAsync(DiscordJson.DeserializeObject<DiscordMessage>(payloadString, this), dat["author"] != null ? DiscordJson.DeserializeObject<TransportUser>(dat["author"]!.ToString(), this) : null, mbr!, refUsr!, refMbr!).ConfigureAwait(false);
				break;

			// delete event does *not* include message object
			case "message_delete":
				await this.OnMessageDeleteEventAsync((ulong)dat["id"]!, (ulong)dat["channel_id"]!, (ulong?)dat["guild_id"]).ConfigureAwait(false);
				break;

			case "message_delete_bulk":
				await this.OnMessageBulkDeleteEventAsync(dat["ids"]!.ToObject<ulong[]>()!, (ulong)dat["channel_id"]!, (ulong?)dat["guild_id"]).ConfigureAwait(false);
				break;

			#endregion

			#region Message Reaction

			case "message_reaction_add":
				rawMbr = dat["member"]!;

				if (rawMbr != null)
					mbr = DiscordJson.DeserializeObject<TransportMember>(rawMbr.ToString(), this);
				// TODO: Add burst stuff
				await this.OnMessageReactionAddAsync((ulong)dat["user_id"]!, (ulong)dat["message_id"]!, (ulong)dat["channel_id"]!, (ulong?)dat["guild_id"], mbr, DiscordJson.DeserializeObject<DiscordEmoji>(dat["emoji"]!.ToString(), this), (bool)dat["burst"]!).ConfigureAwait(false);
				break;

			case "message_reaction_remove":
				await this.OnMessageReactionRemoveAsync((ulong)dat["user_id"]!, (ulong)dat["message_id"]!, (ulong)dat["channel_id"]!, (ulong?)dat["guild_id"], DiscordJson.DeserializeObject<DiscordEmoji>(dat["emoji"]!.ToString(), this), (bool)dat["burst"]!).ConfigureAwait(false);
				break;

			case "message_reaction_remove_all":
				await this.OnMessageReactionRemoveAllAsync((ulong)dat["message_id"]!, (ulong)dat["channel_id"]!, (ulong?)dat["guild_id"]).ConfigureAwait(false);
				break;

			case "message_reaction_remove_emoji":
				await this.OnMessageReactionRemoveEmojiAsync((ulong)dat["message_id"]!, (ulong)dat["channel_id"]!, (ulong)dat["guild_id"]!, DiscordJson.DeserializeObject<DiscordEmoji>(dat["emoji"]!.ToString(), this)).ConfigureAwait(false);
				break;

			#endregion

			#region Stage Instance

			case "stage_instance_create":
				stg = DiscordJson.DeserializeObject<DiscordStageInstance>(payloadString, this);
				await this.OnStageInstanceCreateEventAsync(stg).ConfigureAwait(false);
				break;

			case "stage_instance_update":
				stg = DiscordJson.DeserializeObject<DiscordStageInstance>(payloadString, this);
				await this.OnStageInstanceUpdateEventAsync(stg).ConfigureAwait(false);
				break;

			case "stage_instance_delete":
				stg = DiscordJson.DeserializeObject<DiscordStageInstance>(payloadString, this);
				await this.OnStageInstanceDeleteEventAsync(stg).ConfigureAwait(false);
				break;

			#endregion

			#region Thread

			case "thread_create":
				trd = DiscordJson.DeserializeObject<DiscordThreadChannel>(payloadString, this);
				await this.OnThreadCreateEventAsync(trd).ConfigureAwait(false);
				break;

			case "thread_update":
				trd = DiscordJson.DeserializeObject<DiscordThreadChannel>(payloadString, this);
				await this.OnThreadUpdateEventAsync(trd).ConfigureAwait(false);
				break;

			case "thread_delete":
				trd = DiscordJson.DeserializeObject<DiscordThreadChannel>(payloadString, this);
				await this.OnThreadDeleteEventAsync(trd).ConfigureAwait(false);
				break;

			case "thread_list_sync":
				gid = (ulong)dat["guild_id"]!;
				var trds = DiscordJson.DeserializeIEnumerableObject<IReadOnlyList<DiscordThreadChannel>>(dat["threads"]!.ToString(), this);
				var trms = DiscordJson.DeserializeIEnumerableObject<IReadOnlyList<DiscordThreadChannelMember>>(dat["members"]!.ToString(), this);
				await this.OnThreadListSyncEventAsync(this.GuildsInternal[gid], dat["channel_ids"]!.ToObject<IReadOnlyList<ulong?>>()!, trds, trms).ConfigureAwait(false);
				break;

			case "thread_member_update":
				trdm = DiscordJson.DeserializeObject<DiscordThreadChannelMember>(payloadString, this);
				await this.OnThreadMemberUpdateEventAsync(trdm).ConfigureAwait(false);
				break;

			case "thread_members_update":
				gid = (ulong)dat["guild_id"]!;

				await this.OnThreadMembersUpdateEventAsync(this.GuildsInternal[gid], (ulong)dat["id"]!, (JArray)dat["added_members"]!, (JArray)dat["removed_member_ids"]!, (int)dat["member_count"]!).ConfigureAwait(false);
				break;

			#endregion

			#region Activities
			case "embedded_activity_update":
				gid = (ulong)dat["guild_id"]!;
				cid = (ulong)dat["channel_id"]!;
				await this.OnEmbeddedActivityUpdateAsync((JObject)dat["embedded_activity"]!, this.GuildsInternal[gid], cid, (JArray)dat["users"]!, (ulong)dat["embedded_activity"]["application_id"]!).ConfigureAwait(false);
				break;
			#endregion

			#region User/Presence Update

			case "presence_update":
				await this.OnPresenceUpdateEventAsync(dat, (JObject)dat["user"]!).ConfigureAwait(false);
				break;

			case "user_settings_update":
				usr = DiscordJson.DeserializeObject<TransportUser>(payloadString, this);
				await this.OnUserSettingsUpdateEventAsync(usr).ConfigureAwait(false);
				break;

			case "user_update":
				usr = DiscordJson.DeserializeObject<TransportUser>(payloadString, this);
				await this.OnUserUpdateEventAsync(usr).ConfigureAwait(false);
				break;

			#endregion

			#region Voice

			case "voice_state_update":
				await this.OnVoiceStateUpdateEventAsync(dat).ConfigureAwait(false);
				break;

			case "voice_server_update":
				gid = (ulong)dat["guild_id"]!;
				await this.OnVoiceServerUpdateEventAsync((string)dat["endpoint"]!, (string)dat["token"]!, this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			#endregion

			#region Interaction/Integration/Application

			case "interaction_create":
				rawMbr = dat["member"]!;

				if (rawMbr != null)
				{
					mbr = DiscordJson.DeserializeObject<TransportMember>(rawMbr.ToString(), this);
					usr = mbr.User;
				}
				else
					usr = DiscordJson.DeserializeObject<TransportUser>(dat["user"]!.ToString(), this);

				cid = (ulong)dat["channel_id"]!;
				// Console.WriteLine(dat.ToString()); // Get raw interaction payload.
				await this.OnInteractionCreateAsync((ulong?)dat["guild_id"], cid, usr, mbr, DiscordJson.DeserializeObject<DiscordInteraction>(payloadString, this), payloadString).ConfigureAwait(false);
				break;

			case "application_command_create":
				ac = DiscordJson.DeserializeObject<DiscordApplicationCommand>(payloadString, this);
				await this.OnApplicationCommandCreateAsync(ac, (ulong?)dat["guild_id"]).ConfigureAwait(false);
				break;

			case "application_command_update":
				ac = DiscordJson.DeserializeObject<DiscordApplicationCommand>(payloadString, this);
				await this.OnApplicationCommandUpdateAsync(ac, (ulong?)dat["guild_id"]).ConfigureAwait(false);
				break;

			case "application_command_delete":
				ac = DiscordJson.DeserializeObject<DiscordApplicationCommand>(payloadString, this);
				await this.OnApplicationCommandDeleteAsync(ac, (ulong?)dat["guild_id"]).ConfigureAwait(false);
				break;

			case "guild_application_command_counts_update":
				var counts = dat["application_command_counts"];
				await this.OnGuildApplicationCommandCountsUpdateAsync((int)counts["1"], (int)counts["2"], (int)counts["3"], (ulong)dat["guild_id"]).ConfigureAwait(false);
				break;

			case "guild_application_command_index_update":
				// TODO: Implement.
				break;

			case "application_command_permissions_update":
				var aid = (ulong)dat["application_id"]!;
				if (aid != this.CurrentApplication.Id)
					return;

				var pms = DiscordJson.DeserializeIEnumerableObject<IEnumerable<DiscordApplicationCommandPermission>>(dat["permissions"]!.ToString(), this);
				gid = (ulong)dat["guild_id"]!;
				await this.OnApplicationCommandPermissionsUpdateAsync(pms, (ulong)dat["id"]!, gid, aid).ConfigureAwait(false);
				break;

			#endregion

			#region Misc

			case "gift_code_update": //Not supposed to be dispatched to bots
				break;

			case "typing_start":
				cid = (ulong)dat["channel_id"];
				rawMbr = dat["member"];

				if (rawMbr != null)
					mbr = DiscordJson.DeserializeObject<TransportMember>(rawMbr.ToString(), this);

				await this.OnTypingStartEventAsync((ulong)dat["user_id"], cid, this.InternalGetCachedChannel(cid), (ulong?)dat["guild_id"], Utilities.GetDateTimeOffset((long)dat["timestamp"]), mbr).ConfigureAwait(false);
				break;

			case "webhooks_update":
				gid = (ulong)dat["guild_id"];
				cid = (ulong)dat["channel_id"];
				await this.OnWebhooksUpdateAsync(this.GuildsInternal[gid].GetChannel(cid), this.GuildsInternal[gid]).ConfigureAwait(false);
				break;

			case "guild_join_request_create":
			case "guild_join_request_update":
			case "guild_join_request_delete": // Deprecated
				break;

			case "entitlement_create":
				ent = DiscordJson.DeserializeObject<DiscordEntitlement>(payloadString, this);
				await this.OnEntitlementCreateAsync(ent).ConfigureAwait(false);
				break;
			case "entitlement_update":
				ent = DiscordJson.DeserializeObject<DiscordEntitlement>(payloadString, this);
				await this.OnEntitlementUpdateAsync(ent).ConfigureAwait(false);
				break;
			case "entitlement_delete":
				ent = DiscordJson.DeserializeObject<DiscordEntitlement>(payloadString, this);
				await this.OnEntitlementDeleteAsync(ent).ConfigureAwait(false);
				break;

			default:
				await this.OnUnknownEventAsync(payload).ConfigureAwait(false);
				this.Logger.LogWarning(LoggerEvents.WebSocketReceive, "Unknown event: {name}\npayload: {payload}", payload.EventName, dat.ToString(Formatting.Indented));
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
	/// <param name="ready">The ready payload.</param>
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
		this.CurrentUser.Flags = rusr.Flags;
		this.CurrentUser.GlobalName = rusr.GlobalName;

		this.GatewayVersion = ready.GatewayVersion;
		this._sessionId = ready.SessionId;
		this._resumeGatewayUrl = ready.ResumeGatewayUrl;
		var rawGuildIndex = rawGuilds.ToDictionary(xt => (ulong)xt["id"], xt => (JObject)xt);

		this.GuildsInternal.Clear();
		foreach (var guild in ready.Guilds)
		{
			guild.Discord = this;

			guild.ChannelsInternal ??= new();

			foreach (var xc in guild.Channels.Values)
			{
				xc.GuildId = guild.Id;
				xc.Initialize(this);
			}

			guild.RolesInternal ??= new();

			foreach (var xr in guild.Roles.Values)
			{
				xr.Discord = this;
				xr.GuildId = guild.Id;
			}

			var rawGuild = rawGuildIndex[guild.Id];
			var rawMembers = (JArray)rawGuild["members"];

			if (guild.MembersInternal != null)
				guild.MembersInternal.Clear();
			else
				guild.MembersInternal = new();

			if (rawMembers != null)
			{
				foreach (var xj in rawMembers)
				{
					var xtm = xj.ToObject<TransportMember>();

					var usr = new DiscordUser(xtm.User)
					{
						Discord = this
					};
					usr = this.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
					{
						old.Username = usr.Username;
						old.Discriminator = usr.Discriminator;
						old.AvatarHash = usr.AvatarHash;
						old.BannerHash = usr.BannerHash;
						old.BannerColorInternal = usr.BannerColorInternal;
						old.AvatarDecorationData = usr.AvatarDecorationData;
						old.ThemeColorsInternal = usr.ThemeColorsInternal;
						old.Pronouns = usr.Pronouns;
						old.Locale = usr.Locale;
						old.GlobalName = usr.GlobalName;
						return old;
					});

					guild.MembersInternal[xtm.User.Id] = new(xtm) { Discord = this, GuildId = guild.Id };
				}
			}

			guild.EmojisInternal ??= new();

			foreach (var xe in guild.Emojis.Values)
				xe.Discord = this;

			guild.StickersInternal ??= new();

			foreach (var xs in guild.Stickers.Values)
				xs.Discord = this;

			guild.VoiceStatesInternal ??= new();

			foreach (var xvs in guild.VoiceStates.Values)
				xvs.Discord = this;

			guild.ThreadsInternal ??= new();

			foreach (var xt in guild.ThreadsInternal.Values)
				xt.Discord = this;

			guild.StageInstancesInternal ??= new();

			foreach (var xsi in guild.StageInstancesInternal.Values)
				xsi.Discord = this;

			guild.ScheduledEventsInternal ??= new();

			foreach (var xse in guild.ScheduledEventsInternal.Values)
				xse.Discord = this;

			this.GuildsInternal[guild.Id] = guild;
		}

		await this._ready.InvokeAsync(this, new(this.ServiceProvider)).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the resumed event.
	/// </summary>
	internal Task OnResumedAsync()
	{
		this.Logger.LogInformation(LoggerEvents.SessionUpdate, "Session resumed");
		return this._resumed.InvokeAsync(this, new(this.ServiceProvider));
	}

	#endregion

	#region Channel

	/// <summary>
	/// Handles the channel create event.
	/// </summary>
	/// <param name="channel">The channel.</param>
	internal async Task OnChannelCreateEventAsync(DiscordChannel channel)
	{
		channel.Initialize(this);

		this.GuildsInternal[channel.GuildId.Value].ChannelsInternal[channel.Id] = channel;

		/*if (this.Configuration.AutoRefreshChannelCache)
            {
                await this.RefreshChannelsAsync(channel.Guild.Id);
            }*/

		await this._channelCreated.InvokeAsync(this, new(this.ServiceProvider) { Channel = channel, Guild = channel.Guild }).ConfigureAwait(false);
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

		var channelNew = this.InternalGetCachedChannel(channel.Id);
		DiscordChannel channelOld = null;

		if (channelNew != null)
		{
			channelOld = new()
			{
				Bitrate = channelNew.Bitrate,
				Discord = this,
				GuildId = channelNew.GuildId,
				Id = channelNew.Id,
				LastMessageId = channelNew.LastMessageId,
				Name = channelNew.Name,
				PermissionOverwritesInternal = new(channelNew.PermissionOverwritesInternal),
				Position = channelNew.Position,
				Topic = channelNew.Topic,
				Type = channelNew.Type,
				UserLimit = channelNew.UserLimit,
				ParentId = channelNew.ParentId,
				IsNsfw = channelNew.IsNsfw,
				PerUserRateLimit = channelNew.PerUserRateLimit,
				RtcRegionId = channelNew.RtcRegionId,
				QualityMode = channelNew.QualityMode,
				DefaultAutoArchiveDuration = channelNew.DefaultAutoArchiveDuration,
			};

			channelNew.Bitrate = channel.Bitrate;
			channelNew.Name = channel.Name;
			channelNew.Position = channel.Position;
			channelNew.Topic = channel.Topic;
			channelNew.UserLimit = channel.UserLimit;
			channelNew.ParentId = channel.ParentId;
			channelNew.IsNsfw = channel.IsNsfw;
			channelNew.PerUserRateLimit = channel.PerUserRateLimit;
			channelNew.Type = channel.Type;
			channelNew.RtcRegionId = channel.RtcRegionId;
			channelNew.QualityMode = channel.QualityMode;
			channelNew.DefaultAutoArchiveDuration = channel.DefaultAutoArchiveDuration;

			channelNew.PermissionOverwritesInternal.Clear();

			channel.Initialize(this);

			channelNew.PermissionOverwritesInternal.AddRange(channel.PermissionOverwritesInternal);


			if (channel.Type == ChannelType.Forum)
			{
				channelOld.PostCreateUserRateLimit = channelNew.PostCreateUserRateLimit;
				channelOld.InternalAvailableTags = channelNew.InternalAvailableTags;
				channelOld.Template = channelNew.Template;
				channelOld.DefaultReactionEmoji = channelNew.DefaultReactionEmoji;
				channelOld.DefaultSortOrder = channelNew.DefaultSortOrder;

				channelNew.PostCreateUserRateLimit = channel.PostCreateUserRateLimit;
				channelNew.Template = channel.Template;
				channelNew.DefaultReactionEmoji = channel.DefaultReactionEmoji;
				channelNew.DefaultSortOrder = channel.DefaultSortOrder;

				if (channelNew.InternalAvailableTags != null && channelNew.InternalAvailableTags.Any())
					channelNew.InternalAvailableTags.Clear();

				if (channel.InternalAvailableTags != null && channel.InternalAvailableTags.Any())
					channelNew.InternalAvailableTags.AddRange(channel.InternalAvailableTags);
			}
			else
			{
				channelOld.PostCreateUserRateLimit = null;
				channelOld.InternalAvailableTags = null;
				channelOld.Template = null;
				channelOld.DefaultReactionEmoji = null;
				channelOld.DefaultSortOrder = null;

				channelNew.PostCreateUserRateLimit = null;
				channelNew.InternalAvailableTags = null;
				channelNew.Template = null;
				channelNew.DefaultReactionEmoji = null;
				channelNew.DefaultSortOrder = null;
			}
			channelOld.Initialize(this);
			channelNew.Initialize(this);

			if (this.Configuration.AutoRefreshChannelCache && gld != null)
			{
				await this.RefreshChannelsAsync(channel.Guild.Id).ConfigureAwait(false);
			}
		}
		else if (gld != null)
		{
			gld.ChannelsInternal[channel.Id] = channel;

			if (this.Configuration.AutoRefreshChannelCache)
			{
				await this.RefreshChannelsAsync(channel.Guild.Id).ConfigureAwait(false);
			}
		}

		await this._channelUpdated.InvokeAsync(this, new(this.ServiceProvider) { ChannelAfter = channelNew, Guild = gld, ChannelBefore = channelOld }).ConfigureAwait(false);
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

			await this._dmChannelDeleted.InvokeAsync(this, new(this.ServiceProvider) { Channel = dmChannel }).ConfigureAwait(false);
		}
		else
		{
			var gld = channel.Guild;

			if (gld.ChannelsInternal.TryRemove(channel.Id, out var cachedChannel)) channel = cachedChannel;

			if (this.Configuration.AutoRefreshChannelCache)
			{
				await this.RefreshChannelsAsync(channel.Guild.Id).ConfigureAwait(false);
			}

			await this._channelDeleted.InvokeAsync(this, new(this.ServiceProvider) { Channel = channel, Guild = gld }).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Refreshes the channels.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task RefreshChannelsAsync(ulong guildId)
	{
		var guild = this.InternalGetCachedGuild(guildId);
		var channels = await this.ApiClient.GetGuildChannelsAsync(guildId).ConfigureAwait(false);
		guild.ChannelsInternal.Clear();
		foreach (var channel in channels.ToList())
		{
			channel.Initialize(this);
			guild.ChannelsInternal[channel.Id] = channel;
		}
	}

	/// <summary>
	/// Handles the channel pins update event.
	/// </summary>
	/// <param name="guildId">The optional guild id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="lastPinTimestamp">The optional last pin timestamp.</param>
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

	/// <summary>
	/// Handles the voice channel status update event.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="status">The optional status.</param>
	internal async Task OnVoiceChannelStatusUpdateAsync(ulong guildId, ulong channelId, string? status)
	{
		var guild = this.InternalGetCachedGuild(guildId);
		var channel = this.InternalGetCachedChannel(channelId);

		var ea = new VoiceChannelStatusUpdateEventArgs(this.ServiceProvider)
		{
			Guild = guild,
			Channel = channel,
			Status = status
		};
		await this._voiceChannelStatusUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
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
				xp.Activity = new(xp.RawActivity);
				if (xp.RawActivities != null)
				{
					xp.InternalActivities = xp.RawActivities
						.Select(x => new DiscordActivity(x)).ToArray();
				}
				this.PresencesInternal[xp.InternalUser.Id] = xp;
			}
		}

		var exists = this.GuildsInternal.TryGetValue(guild.Id, out var foundGuild);

		guild.Discord = this;
		guild.IsUnavailable = false;
		var eventGuild = guild;
		if (exists)
			guild = foundGuild;

		guild.ChannelsInternal ??= new();
		guild.ThreadsInternal ??= new();
		guild.RolesInternal ??= new();
		guild.ThreadsInternal ??= new();
		guild.StickersInternal ??= new();
		guild.EmojisInternal ??= new();
		guild.VoiceStatesInternal ??= new();
		guild.MembersInternal ??= new();
		guild.ScheduledEventsInternal ??= new();

		this.UpdateCachedGuild(eventGuild, rawMembers);

		guild.JoinedAt = eventGuild.JoinedAt;
		guild.IsLarge = eventGuild.IsLarge;
		guild.MemberCount = Math.Max(eventGuild.MemberCount, guild.MembersInternal.Count);
		guild.IsUnavailable = eventGuild.IsUnavailable;
		guild.PremiumSubscriptionCount = eventGuild.PremiumSubscriptionCount;
		guild.PremiumTier = eventGuild.PremiumTier;
		guild.BannerHash = eventGuild.BannerHash;
		guild.VanityUrlCode = eventGuild.VanityUrlCode;
		guild.Description = eventGuild.Description;
		guild.IsNsfw = eventGuild.IsNsfw;

		foreach (var kvp in eventGuild.VoiceStatesInternal) guild.VoiceStatesInternal[kvp.Key] = kvp.Value;
		foreach (var kvp in eventGuild.ChannelsInternal) guild.ChannelsInternal[kvp.Key] = kvp.Value;
		foreach (var kvp in eventGuild.RolesInternal) guild.RolesInternal[kvp.Key] = kvp.Value;
		foreach (var kvp in eventGuild.EmojisInternal) guild.EmojisInternal[kvp.Key] = kvp.Value;
		foreach (var kvp in eventGuild.ThreadsInternal) guild.ThreadsInternal[kvp.Key] = kvp.Value;
		foreach (var kvp in eventGuild.StickersInternal) guild.StickersInternal[kvp.Key] = kvp.Value;
		foreach (var kvp in eventGuild.StageInstancesInternal) guild.StageInstancesInternal[kvp.Key] = kvp.Value;
		foreach (var kvp in eventGuild.ScheduledEventsInternal) guild.ScheduledEventsInternal[kvp.Key] = kvp.Value;

		foreach (var xc in guild.ChannelsInternal.Values)
		{
			xc.GuildId = guild.Id;
			xc.Initialize(this);
		}
		foreach (var xt in guild.ThreadsInternal.Values)
		{
			xt.GuildId = guild.Id;
			xt.Discord = this;
		}
		foreach (var xe in guild.EmojisInternal.Values)
			xe.Discord = this;
		foreach (var xs in guild.StickersInternal.Values)
			xs.Discord = this;
		foreach (var xvs in guild.VoiceStatesInternal.Values)
			xvs.Discord = this;
		foreach (var xsi in guild.StageInstancesInternal.Values)
		{
			xsi.Discord = this;
			xsi.GuildId = guild.Id;
		}
		foreach (var xr in guild.RolesInternal.Values)
		{
			xr.Discord = this;
			xr.GuildId = guild.Id;
		}
		foreach (var xse in guild.ScheduledEventsInternal.Values)
		{
			xse.Discord = this;
			xse.GuildId = guild.Id;
			if (xse.Creator != null)
				xse.Creator.Discord = this;
		}

		var old = Volatile.Read(ref this._guildDownloadCompleted);
		var dcompl = this.GuildsInternal.Values.All(xg => !xg.IsUnavailable);
		Volatile.Write(ref this._guildDownloadCompleted, dcompl);

		if (exists)
			await this._guildAvailable.InvokeAsync(this, new(this.ServiceProvider) { Guild = guild }).ConfigureAwait(false);
		else
			await this._guildCreated.InvokeAsync(this, new(this.ServiceProvider) { Guild = guild }).ConfigureAwait(false);

		if (dcompl && !old)
			await this._guildDownloadCompletedEv.InvokeAsync(this, new(this.Guilds, this.ServiceProvider)).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the guild update event.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="rawMembers">The raw members.</param>
	internal async Task OnGuildUpdateEventAsync(DiscordGuild guild, JArray rawMembers)
	{
		DiscordGuild oldGuild;

		if (!this.GuildsInternal.ContainsKey(guild.Id))
		{
			this.GuildsInternal[guild.Id] = guild;
			oldGuild = null;
		}
		else
		{
			var gld = this.GuildsInternal[guild.Id];

			oldGuild = new()
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
				ChannelsInternal = new(),
				ThreadsInternal = new(),
				EmojisInternal = new(),
				StickersInternal = new(),
				MembersInternal = new(),
				RolesInternal = new(),
				StageInstancesInternal = new(),
				VoiceStatesInternal = new(),
				ScheduledEventsInternal = new()
			};

			foreach (var kvp in gld.ChannelsInternal) oldGuild.ChannelsInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.ThreadsInternal) oldGuild.ThreadsInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.EmojisInternal) oldGuild.EmojisInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.StickersInternal) oldGuild.StickersInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.RolesInternal) oldGuild.RolesInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.VoiceStatesInternal) oldGuild.VoiceStatesInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.MembersInternal) oldGuild.MembersInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.StageInstancesInternal) oldGuild.StageInstancesInternal[kvp.Key] = kvp.Value;
			foreach (var kvp in gld.ScheduledEventsInternal) oldGuild.ScheduledEventsInternal[kvp.Key] = kvp.Value;
		}

		guild.Discord = this;
		guild.IsUnavailable = false;
		var eventGuild = guild;
		guild = this.GuildsInternal[eventGuild.Id];

		guild.ChannelsInternal ??= new();
		guild.ThreadsInternal ??= new();
		guild.RolesInternal ??= new();
		guild.EmojisInternal ??= new();
		guild.StickersInternal ??= new();
		guild.VoiceStatesInternal ??= new();
		guild.StageInstancesInternal ??= new();
		guild.MembersInternal ??= new();
		guild.ScheduledEventsInternal ??= new();

		this.UpdateCachedGuild(eventGuild, rawMembers);

		foreach (var xc in guild.ChannelsInternal.Values)
		{
			xc.GuildId = guild.Id;
			xc.Initialize(this);
		}
		foreach (var xc in guild.ThreadsInternal.Values)
		{
			xc.GuildId = guild.Id;
			xc.Discord = this;
		}
		foreach (var xe in guild.EmojisInternal.Values)
			xe.Discord = this;
		foreach (var xs in guild.StickersInternal.Values)
			xs.Discord = this;
		foreach (var xvs in guild.VoiceStatesInternal.Values)
			xvs.Discord = this;
		foreach (var xr in guild.RolesInternal.Values)
		{
			xr.Discord = this;
			xr.GuildId = guild.Id;
		}
		foreach (var xsi in guild.StageInstancesInternal.Values)
		{
			xsi.Discord = this;
			xsi.GuildId = guild.Id;
		}
		foreach (var xse in guild.ScheduledEventsInternal.Values)
		{
			xse.Discord = this;
			xse.GuildId = guild.Id;
			if (xse.Creator != null)
				xse.Creator.Discord = this;
		}

		await this._guildUpdated.InvokeAsync(this, new(this.ServiceProvider) { GuildBefore = oldGuild, GuildAfter = guild }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the guild delete event.
	/// </summary>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildDeleteEventAsync(DiscordGuild guild)
	{
		if (guild.IsUnavailable)
		{
			if (!this.GuildsInternal.TryGetValue(guild.Id, out var gld))
				return;

			gld.IsUnavailable = true;

			await this._guildUnavailable.InvokeAsync(this, new(this.ServiceProvider) { Guild = guild, Unavailable = true }).ConfigureAwait(false);
		}
		else
		{
			if (!this.GuildsInternal.TryRemove(guild.Id, out var gld))
				return;

			await this._guildDeleted.InvokeAsync(this, new(this.ServiceProvider) { Guild = gld }).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Handles the guild audit log entry create event.
	/// </summary>
	/// <param name="guild">The guild where the audit log entry was created.</param>
	/// <param name="auditLogCreateEntry">The auditlog event.</param>
	internal async Task OnGuildAuditLogEntryCreateEventAsync(DiscordGuild guild, JObject auditLogCreateEntry)
	{
		try
		{
			var auditLogAction = DiscordJson.ToDiscordObject<AuditLogAction>(auditLogCreateEntry);
			List<AuditLog> workaroundAuditLogEntryList = new()
			{
				new()
				{
					Entries = new List<AuditLogAction>()
					{
						auditLogAction
					}
				}
			};

			var dataList = await guild.ProcessAuditLog(workaroundAuditLogEntryList).ConfigureAwait(false);

			await this._guildAuditLogEntryCreated.InvokeAsync(this, new(this.ServiceProvider) { Guild = guild, AuditLogEntry = dataList[0] }).ConfigureAwait(false);
		}
		catch (Exception)
		{ }
	}

	/// <summary>
	/// Handles the guild sync event.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="isLarge">Whether the guild is a large guild..</param>
	/// <param name="rawMembers">The raw members.</param>
	/// <param name="presences">The presences.</param>
	internal async Task OnGuildSyncEventAsync(DiscordGuild guild, bool isLarge, JArray rawMembers, IEnumerable<DiscordPresence> presences)
	{
		presences = presences.Select(xp => { xp.Discord = this; xp.Activity = new(xp.RawActivity); return xp; });
		foreach (var xp in presences)
			this.PresencesInternal[xp.InternalUser.Id] = xp;

		guild.IsSynced = true;
		guild.IsLarge = isLarge;

		this.UpdateCachedGuild(guild, rawMembers);

		await this._guildAvailable.InvokeAsync(this, new(this.ServiceProvider) { Guild = guild }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the guild emojis update event.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="newEmojis">The new emojis.</param>
	internal async Task OnGuildEmojisUpdateEventAsync(DiscordGuild guild, IEnumerable<DiscordEmoji> newEmojis)
	{
		var oldEmojis = new ConcurrentDictionary<ulong, DiscordEmoji>(guild.EmojisInternal);
		guild.EmojisInternal.Clear();

		foreach (var emoji in newEmojis)
		{
			emoji.Discord = this;
			guild.EmojisInternal[emoji.Id] = emoji;
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
	/// <param name="guildId">The guild id.</param>
	internal async Task OnStickersUpdatedAsync(IEnumerable<DiscordSticker> newStickers, ulong guildId)
	{
		var guild = this.InternalGetCachedGuild(guildId);
		var oldStickers = new ConcurrentDictionary<ulong, DiscordSticker>(guild.StickersInternal);
		guild.StickersInternal.Clear();

		foreach (var nst in newStickers)
		{
			if (nst.User is not null)
			{
				nst.User.Discord = this;
				this.UserCache.AddOrUpdate(nst.User.Id, nst.User, (old, @new) => @new);
			}
			nst.Discord = this;

			guild.StickersInternal[nst.Id] = nst;
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
	/// Handles the created rule.
	/// </summary>
	/// <param name="newRule">The new added rule.</param>
	internal async Task OnAutomodRuleCreated(AutomodRule newRule)
	{
		var sea = new AutomodRuleCreateEventArgs(this.ServiceProvider)
		{
			Rule = newRule
		};

		await this._automodRuleCreated.InvokeAsync(this, sea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the updated rule.
	/// </summary>
	/// <param name="updatedRule">The updated rule.</param>
	internal async Task OnAutomodRuleUpdated(AutomodRule updatedRule)
	{
		var sea = new AutomodRuleUpdateEventArgs(this.ServiceProvider)
		{
			Rule = updatedRule
		};

		await this._automodRuleUpdated.InvokeAsync(this, sea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the deleted rule.
	/// </summary>
	/// <param name="deletedRule">The deleted rule.</param>
	internal async Task OnAutomodRuleDeleted(AutomodRule deletedRule)
	{
		var sea = new AutomodRuleDeleteEventArgs(this.ServiceProvider)
		{
			Rule = deletedRule
		};

		await this._automodRuleDeleted.InvokeAsync(this, sea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the rule action execution.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="rawPayload">The raw payload.</param>
	internal async Task OnAutomodActionExecuted(DiscordGuild guild, JObject rawPayload)
	{
		var executedAction = rawPayload["action"].ToObject<AutomodAction>();
		var ruleId = (ulong)rawPayload["rule_id"];
		var triggerType = rawPayload["rule_trigger_type"].ToObject<AutomodTriggerType>();
		var userId = (ulong)rawPayload["user_id"];
		var channelId = rawPayload.TryGetValue("channel_id", out var value) ? (ulong?)value : null;
		var messageId = rawPayload.TryGetValue("message_id", out var value1) ? (ulong?)value1 : null;
		var alertMessageId = rawPayload.TryGetValue("alert_system_message_id", out var value2) ? (ulong?)value2 : null;
		var content = rawPayload.TryGetValue("content", out var value3) ?(string?)value3 : null;
		var matchedKeyword = rawPayload.TryGetValue("matched_keyword", out var value4) ? (string?)value4 : null;
		var matchedContent = rawPayload.TryGetValue("matched_content", out var value5) ? (string?)value5 : null;

		var ea = new AutomodActionExecutedEventArgs(this.ServiceProvider)
		{
			Guild = guild,
			Action = executedAction,
			RuleId = ruleId,
			TriggerType = triggerType,
			UserId = userId,
			ChannelId = channelId,
			MessageId = messageId,
			AlertMessageId = alertMessageId,
			MessageContent = content,
			MatchedKeyword = matchedKeyword,
			MatchedContent = matchedContent
		};

		await this._automodActionExecuted.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	#endregion

	#region Guild Ban

	/// <summary>
	/// Handles the guild ban add event.
	/// </summary>
	/// <param name="user">The transport user.</param>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildBanAddEventAsync(TransportUser user, DiscordGuild guild)
	{
		var usr = new DiscordUser(user) { Discord = this };
		usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
		{
			old.Username = usr.Username;
			old.Discriminator = usr.Discriminator;
			old.AvatarHash = usr.AvatarHash;
			old.GlobalName = usr.GlobalName;
			return old;
		});

		if (!guild.Members.TryGetValue(user.Id, out var mbr))
			mbr = new(usr) { Discord = this, GuildId = guild.Id };
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
	/// <param name="user">The transport user.</param>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildBanRemoveEventAsync(TransportUser user, DiscordGuild guild)
	{
		var usr = new DiscordUser(user) { Discord = this };
		usr = this.UserCache.AddOrUpdate(user.Id, usr, (id, old) =>
		{
			old.Username = usr.Username;
			old.Discriminator = usr.Discriminator;
			old.AvatarHash = usr.AvatarHash;
			old.GlobalName = usr.GlobalName;
			return old;
		});

		if (!guild.Members.TryGetValue(user.Id, out var mbr))
			mbr = new(usr) { Discord = this, GuildId = guild.Id };
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
	/// Handles the scheduled event create event.
	/// </summary>
	/// <param name="scheduledEvent">The created event.</param>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildScheduledEventCreateEventAsync(DiscordScheduledEvent scheduledEvent, DiscordGuild guild)
	{
		scheduledEvent.Discord = this;

		guild.ScheduledEventsInternal.AddOrUpdate(scheduledEvent.Id, scheduledEvent, (old, newScheduledEvent) => newScheduledEvent);

		if (scheduledEvent.Creator != null)
		{
			scheduledEvent.Creator.Discord = this;
			this.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (id, old) =>
			{
				old.Username = scheduledEvent.Creator.Username;
				old.Discriminator = scheduledEvent.Creator.Discriminator;
				old.AvatarHash = scheduledEvent.Creator.AvatarHash;
				old.Flags = scheduledEvent.Creator.Flags;
				old.GlobalName = scheduledEvent.Creator.GlobalName;
				return old;
			});
		}

		await this._guildScheduledEventCreated.InvokeAsync(this, new(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = scheduledEvent.Guild }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the scheduled event update event.
	/// </summary>
	/// <param name="scheduledEvent">The updated event.</param>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildScheduledEventUpdateEventAsync(DiscordScheduledEvent scheduledEvent, DiscordGuild guild)
	{
		if (guild == null)
			return;

		DiscordScheduledEvent oldEvent;
		if (!guild.ScheduledEventsInternal.ContainsKey(scheduledEvent.Id))
		{
			oldEvent = null;
		}
		else
		{
			var ev = guild.ScheduledEventsInternal[scheduledEvent.Id];
			oldEvent = new()
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
				UserCount = ev.UserCount,
				CoverImageHash = ev.CoverImageHash
			};

		}
		if (scheduledEvent.Creator != null)
		{
			scheduledEvent.Creator.Discord = this;
			this.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (id, old) =>
			{
				old.Username = scheduledEvent.Creator.Username;
				old.Discriminator = scheduledEvent.Creator.Discriminator;
				old.AvatarHash = scheduledEvent.Creator.AvatarHash;
				old.Flags = scheduledEvent.Creator.Flags;
				old.GlobalName = scheduledEvent.Creator.GlobalName;
				return old;
			});
		}

		if (scheduledEvent.Status == ScheduledEventStatus.Completed)
		{
			guild.ScheduledEventsInternal.TryRemove(scheduledEvent.Id, out var deletedEvent);
			await this._guildScheduledEventDeleted.InvokeAsync(this, new(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = guild, Reason = ScheduledEventStatus.Completed }).ConfigureAwait(false);
		}
		else if (scheduledEvent.Status == ScheduledEventStatus.Canceled)
		{
			guild.ScheduledEventsInternal.TryRemove(scheduledEvent.Id, out var deletedEvent);
			scheduledEvent.Status = ScheduledEventStatus.Canceled;
			await this._guildScheduledEventDeleted.InvokeAsync(this, new(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = guild, Reason = ScheduledEventStatus.Canceled }).ConfigureAwait(false);
		}
		else
		{
			this.UpdateScheduledEvent(scheduledEvent, guild);
			await this._guildScheduledEventUpdated.InvokeAsync(this, new(this.ServiceProvider) { ScheduledEventBefore = oldEvent, ScheduledEventAfter = scheduledEvent, Guild = guild }).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Handles the scheduled event delete event.
	/// </summary>
	/// <param name="scheduledEvent">The deleted event.</param>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildScheduledEventDeleteEventAsync(DiscordScheduledEvent scheduledEvent, DiscordGuild guild)
	{
		scheduledEvent.Discord = this;

		if (scheduledEvent.Status == ScheduledEventStatus.Scheduled)
			scheduledEvent.Status = ScheduledEventStatus.Canceled;

		if (scheduledEvent.Creator != null)
		{
			scheduledEvent.Creator.Discord = this;
			this.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (id, old) =>
			{
				old.Username = scheduledEvent.Creator.Username;
				old.Discriminator = scheduledEvent.Creator.Discriminator;
				old.AvatarHash = scheduledEvent.Creator.AvatarHash;
				old.Flags = scheduledEvent.Creator.Flags;
				old.GlobalName = scheduledEvent.Creator.GlobalName;
				return old;
			});
		}

		await this._guildScheduledEventDeleted.InvokeAsync(this, new(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = scheduledEvent.Guild, Reason = scheduledEvent.Status }).ConfigureAwait(false);
		guild.ScheduledEventsInternal.TryRemove(scheduledEvent.Id, out var deletedEvent);
	}

	/// <summary>
	/// Handles the scheduled event user add event.
	/// <param name="guildScheduledEventId">The event.</param>
	/// <param name="userId">The added user id.</param>
	/// <param name="guild">The guild.</param>
	/// </summary>
	internal async Task OnGuildScheduledEventUserAddedEventAsync(ulong guildScheduledEventId, ulong userId, DiscordGuild guild)
	{
		var scheduledEvent = this.InternalGetCachedScheduledEvent(guildScheduledEventId) ?? this.UpdateScheduledEvent(new()
		{
			Id = guildScheduledEventId,
			GuildId = guild.Id,
			Discord = this,
			UserCount = 0
		}, guild);

		scheduledEvent.UserCount++;
		scheduledEvent.Discord = this;
		guild.Discord = this;

		var user = this.GetUserAsync(userId, true).Result;
		user.Discord = this;
		var member = guild.Members.TryGetValue(userId, out var mem) ? mem : guild.GetMemberAsync(userId).Result;
		member.Discord = this;

		await this._guildScheduledEventUserAdded.InvokeAsync(this, new(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = guild, User = user, Member = member }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the scheduled event user remove event.
	/// <param name="guildScheduledEventId">The event.</param>
	/// <param name="userId">The removed user id.</param>
	/// <param name="guild">The guild.</param>
	/// </summary>
	internal async Task OnGuildScheduledEventUserRemovedEventAsync(ulong guildScheduledEventId, ulong userId, DiscordGuild guild)
	{
		var scheduledEvent = this.InternalGetCachedScheduledEvent(guildScheduledEventId) ?? this.UpdateScheduledEvent(new()
		{
			Id = guildScheduledEventId,
			GuildId = guild.Id,
			Discord = this,
			UserCount = 0
		}, guild);

		scheduledEvent.UserCount = scheduledEvent.UserCount == 0 ? 0 : scheduledEvent.UserCount - 1;
		scheduledEvent.Discord = this;
		guild.Discord = this;

		var user = this.GetUserAsync(userId, true).Result;
		user.Discord = this;
		var member = guild.Members.TryGetValue(userId, out var mem) ? mem : guild.GetMemberAsync(userId).Result;
		member.Discord = this;

		await this._guildScheduledEventUserRemoved.InvokeAsync(this, new(this.ServiceProvider) { ScheduledEvent = scheduledEvent, Guild = guild, User = user, Member = member }).ConfigureAwait(false);
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

		await this._guildIntegrationCreated.InvokeAsync(this, new(this.ServiceProvider) { Integration = integration, Guild = guild }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the guild integration update event.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="integration">The integration.</param>
	internal async Task OnGuildIntegrationUpdateEventAsync(DiscordGuild guild, DiscordIntegration integration)
	{
		integration.Discord = this;

		await this._guildIntegrationUpdated.InvokeAsync(this, new(this.ServiceProvider) { Integration = integration, Guild = guild }).ConfigureAwait(false);
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

	/// <summary>
	/// Handles the guild integration delete event.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="integrationId">The integration id.</param>
	/// <param name="applicationId">The optional application id.</param>
	internal async Task OnGuildIntegrationDeleteEventAsync(DiscordGuild guild, ulong integrationId, ulong? applicationId)
		=> await this._guildIntegrationDeleted.InvokeAsync(this, new(this.ServiceProvider) { Guild = guild, IntegrationId = integrationId, ApplicationId = applicationId }).ConfigureAwait(false);

	#endregion

	#region Guild Member

	/// <summary>
	/// Handles the guild member add event.
	/// </summary>
	/// <param name="member">The transport member.</param>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildMemberAddEventAsync(TransportMember member, DiscordGuild guild)
	{
		var usr = new DiscordUser(member.User) { Discord = this };
		usr = this.UserCache.AddOrUpdate(member.User.Id, usr, (id, old) =>
		{
			old.Username = usr.Username;
			old.Discriminator = usr.Discriminator;
			old.AvatarHash = usr.AvatarHash;
			old.BannerHash = usr.BannerHash;
			old.BannerColorInternal = usr.BannerColorInternal;
			old.AvatarDecorationData = usr.AvatarDecorationData;
			old.ThemeColorsInternal = usr.ThemeColorsInternal;
			old.Pronouns = usr.Pronouns;
			old.Locale = usr.Locale;
			old.GlobalName = usr.GlobalName;
			return old;
		});

		var mbr = new DiscordMember(member)
		{
			Discord = this,
			GuildId = guild.Id
		};

		guild.MembersInternal[mbr.Id] = mbr;
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
	/// <param name="user">The transport user.</param>
	/// <param name="guild">The guild.</param>
	internal async Task OnGuildMemberRemoveEventAsync(TransportUser user, DiscordGuild guild)
	{
		var usr = new DiscordUser(user);

		if (!guild.MembersInternal.TryRemove(user.Id, out var mbr))
			mbr = new(usr) { Discord = this, GuildId = guild.Id };
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
	/// <param name="member">The transport member.</param>
	/// <param name="guild">The guild.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="nick">The nick.</param>
	/// <param name="pending">Whether the member is pending.</param>
	internal async Task OnGuildMemberUpdateEventAsync(TransportMember member, DiscordGuild guild, IEnumerable<ulong> roles, string nick, bool? pending)
	{
		var usr = new DiscordUser(member.User) { Discord = this };
		usr = this.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
		{
			old.Username = usr.Username;
			old.Discriminator = usr.Discriminator;
			old.AvatarHash = usr.AvatarHash;
			old.BannerHash = usr.BannerHash;
			old.BannerColorInternal = usr.BannerColorInternal;
			old.AvatarDecorationData = usr.AvatarDecorationData;
			old.ThemeColorsInternal = usr.ThemeColorsInternal;
			old.Pronouns = usr.Pronouns;
			old.Locale = usr.Locale;
			old.GlobalName = usr.GlobalName;
			return old;
		});

		if (!guild.Members.TryGetValue(member.User.Id, out var mbr))
			mbr = new(usr) { Discord = this, GuildId = guild.Id };
		var old = mbr;

		var gAvOld = old.GuildAvatarHash;
		var avOld = old.AvatarHash;
		var nickOld = mbr.Nickname;
		var pendingOld = mbr.IsPending;
		var rolesOld = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles));
		var cduOld = mbr.CommunicationDisabledUntil;
		var udauOld = mbr.UnusualDmActivityUntil;
		mbr.MemberFlags = member.MemberFlags;
		mbr.AvatarHashInternal = member.AvatarHash;
		mbr.GuildAvatarHash = member.GuildAvatarHash;
		mbr.Nickname = nick;
		mbr.GuildPronouns = member.GuildPronouns;
		mbr.IsPending = pending;
		mbr.CommunicationDisabledUntil = member.CommunicationDisabledUntil;
		mbr.UnusualDmActivityUntil = member.UnusualDmActivityUntil;
		mbr.RoleIdsInternal.Clear();
		mbr.RoleIdsInternal.AddRange(roles);
		guild.MembersInternal.AddOrUpdate(member.User.Id, mbr, (id, oldMbr) => oldMbr);

		var timeoutUntil = member.CommunicationDisabledUntil;
		/*this.Logger.LogTrace($"Timeout:\nBefore - {cduOld}\nAfter - {timeoutUntil}");
		if ((timeoutUntil.HasValue && cduOld.HasValue) || (timeoutUntil == null && cduOld.HasValue) || (timeoutUntil.HasValue && cduOld == null))
		{
			// We are going to add a scheduled timer to assure that we get a auditlog entry.

			var id = $"tt-{mbr.Id}-{guild.Id}-{DateTime.Now.ToLongTimeString()}";

			this._tempTimers.Add(
				id,
				new(
					new TimeoutHandler(
						mbr,
						guild,
						cduOld,
						timeoutUntil
					),
					new Timer(
						this.TimeoutTimer,
						id,
						2000,
						Timeout.Infinite
					)
				)
			);

			this.Logger.LogTrace("Scheduling timeout event.");

			return;
		}*/

		//this.Logger.LogTrace("No timeout detected. Continuing on normal operation.");

		var eargs = new GuildMemberUpdateEventArgs(this.ServiceProvider)
		{
			Guild = guild,
			Member = mbr,

			NicknameAfter = mbr.Nickname,
			RolesAfter = new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(mbr.Roles)),
			PendingAfter = mbr.IsPending,
			TimeoutAfter = mbr.CommunicationDisabledUntil,
			AvatarHashAfter = mbr.AvatarHash,
			GuildAvatarHashAfter = mbr.GuildAvatarHash,
			UnusualDmActivityAfter = mbr.UnusualDmActivityUntil,

			NicknameBefore = nickOld,
			RolesBefore = rolesOld,
			PendingBefore = pendingOld,
			TimeoutBefore = cduOld,
			AvatarHashBefore = avOld,
			GuildAvatarHashBefore = gAvOld,
			UnusualDmActivityBefore = udauOld
		};
		await this._guildMemberUpdated.InvokeAsync(this, eargs).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles timeout events.
	/// </summary>
	/// <param name="state">Internally used as uid for the timer data.</param>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
	private async void TimeoutTimer(object state)
	{
		var tid = (string)state;
		var data = this._tempTimers.First(x=> x.Key == tid).Value.Key;
		var timer = this._tempTimers.First(x=> x.Key == tid).Value.Value;

		IReadOnlyList<DiscordAuditLogEntry> auditlog = null;
		DiscordAuditLogMemberUpdateEntry filtered = null;
		try
		{
			auditlog = await data.Guild.GetAuditLogsAsync(10, null, AuditLogActionType.MemberUpdate).ConfigureAwait(false);
			var preFiltered = auditlog.Select(x => x as DiscordAuditLogMemberUpdateEntry).Where(x => x.Target.Id == data.Member.Id);
			filtered = preFiltered.First();
		}
		catch (UnauthorizedException) { }
		catch (Exception)
		{
			this.Logger.LogTrace("Failing timeout event.");
			await timer.DisposeAsync().ConfigureAwait(false);
			this._tempTimers.Remove(tid);
			return;
		}

		var actor = filtered?.UserResponsible as DiscordMember;

		this.Logger.LogTrace("Trying to execute timeout event.");

		if (data.TimeoutUntilOld.HasValue && data.TimeoutUntilNew.HasValue)
		{
			// A timeout was updated.

			if (filtered != null && auditlog == null)
			{
				this.Logger.LogTrace("Re-scheduling timeout event.");
				timer.Change(2000, Timeout.Infinite);
				return;
			}

			var ea = new GuildMemberTimeoutUpdateEventArgs(this.ServiceProvider)
			{
				Guild = data.Guild,
				Target = data.Member,
				TimeoutBefore = data.TimeoutUntilOld.Value,
				TimeoutAfter = data.TimeoutUntilNew.Value,
				Actor = actor,
				AuditLogId = filtered?.Id,
				AuditLogReason = filtered?.Reason
			};
			await this._guildMemberTimeoutChanged.InvokeAsync(this, ea).ConfigureAwait(false);
		}
		else if (!data.TimeoutUntilOld.HasValue && data.TimeoutUntilNew.HasValue)
		{
			// A timeout was added.

			if (filtered != null && auditlog == null)
			{
				this.Logger.LogTrace("Re-scheduling timeout event.");
				timer.Change(2000, Timeout.Infinite);
				return;
			}

			var ea = new GuildMemberTimeoutAddEventArgs(this.ServiceProvider)
			{
				Guild = data.Guild,
				Target = data.Member,
				Timeout = data.TimeoutUntilNew.Value,
				Actor = actor,
				AuditLogId = filtered?.Id,
				AuditLogReason = filtered?.Reason
			};
			await this._guildMemberTimeoutAdded.InvokeAsync(this, ea).ConfigureAwait(false);
		}
		else if (data.TimeoutUntilOld.HasValue && !data.TimeoutUntilNew.HasValue)
		{
			// A timeout was removed.

			if (filtered != null && auditlog == null)
			{
				this.Logger.LogTrace("Re-scheduling timeout event.");
				timer.Change(2000, Timeout.Infinite);
				return;
			}

			var ea = new GuildMemberTimeoutRemoveEventArgs(this.ServiceProvider)
			{
				Guild = data.Guild,
				Target = data.Member,
				TimeoutBefore = data.TimeoutUntilOld.Value,
				Actor = actor,
				AuditLogId = filtered?.Id,
				AuditLogReason = filtered?.Reason
			};
			await this._guildMemberTimeoutRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
		}

		// Ending timer because it worked.
		this.Logger.LogTrace("Removing timeout event.");
		await timer.DisposeAsync().ConfigureAwait(false);
		this._tempTimers.Remove(tid);
	}

	/// <summary>
	/// Handles the guild members chunk event.
	/// </summary>
	/// <param name="dat">The raw chunk data.</param>
	internal async Task OnGuildMembersChunkEventAsync(JObject dat)
	{
		var guild = this.Guilds[(ulong)dat["guild_id"]!];
		var chunkIndex = (int)dat["chunk_index"]!;
		var chunkCount = (int)dat["chunk_count"]!;
		var nonce = (string)dat["nonce"]!;

		var mbrs = new HashSet<DiscordMember>();
		var pres = new HashSet<DiscordPresence>();

		var members = dat["members"]!.ToObject<TransportMember[]>()!;

		foreach (var member in members)
		{
			var mbr = new DiscordMember(member) { Discord = this, GuildId = guild.Id };

			if (!this.UserCache.ContainsKey(mbr.Id))
				this.UserCache[mbr.Id] = new(member.User) { Discord = this };

			guild.MembersInternal[mbr.Id] = mbr;

			mbrs.Add(mbr);
		}

		guild.MemberCount = guild.MembersInternal.Count;

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
			var presences = dat["presences"]!.ToObject<DiscordPresence[]>()!;

			var presCount = presences.Length;
			foreach (var presence in presences)
			{
				presence.Discord = this;
				presence.Activity = new(presence.RawActivity);

				if (presence.RawActivities != null)
				{
					presence.InternalActivities = presence.RawActivities
						.Select(x => new DiscordActivity(x)).ToArray();
				}

				pres.Add(presence);
			}

			ea.Presences = new ReadOnlySet<DiscordPresence>(pres);
		}

		if (dat["not_found"] != null)
		{
			var nf = dat["not_found"]!.ToObject<ISet<ulong>>()!;
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
		role.GuildId = guild.Id;

		guild.RolesInternal[role.Id] = role;

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
			GuildId = guild.Id,
			ColorInternal = newRole.ColorInternal,
			Discord = this,
			IsHoisted = newRole.IsHoisted,
			Id = newRole.Id,
			IsManaged = newRole.IsManaged,
			IsMentionable = newRole.IsMentionable,
			Name = newRole.Name,
			Permissions = newRole.Permissions,
			Position = newRole.Position,
			IconHash = newRole.IconHash,
			Tags = newRole.Tags ?? null,
			UnicodeEmojiString = newRole.UnicodeEmojiString
		};

		newRole.GuildId = guild.Id;
		newRole.ColorInternal = role.ColorInternal;
		newRole.IsHoisted = role.IsHoisted;
		newRole.IsManaged = role.IsManaged;
		newRole.IsMentionable = role.IsMentionable;
		newRole.Name = role.Name;
		newRole.Permissions = role.Permissions;
		newRole.Position = role.Position;
		newRole.IconHash = role.IconHash;
		newRole.Tags = role.Tags ?? null;
		newRole.UnicodeEmojiString = role.UnicodeEmojiString;

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
		if (!guild.RolesInternal.TryRemove(roleId, out var role))
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

		if (invite.Inviter is not null)
		{
			invite.Inviter.Discord = this;
			this.UserCache.AddOrUpdate(invite.Inviter.Id, invite.Inviter, (old, @new) => @new);
		}

		guild.Invites[invite.Code] = invite;

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
	/// <param name="dat">The raw invite.</param>
	internal async Task OnInviteDeleteEventAsync(ulong channelId, ulong guildId, JToken dat)
	{
		var guild = this.InternalGetCachedGuild(guildId);
		var channel = this.InternalGetCachedChannel(channelId);

		if (!guild.Invites.TryRemove(dat["code"].ToString(), out var invite))
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
	/// Handles the message acknowledge event.
	/// </summary>
	/// <param name="chn">The channel.</param>
	/// <param name="messageId">The message id.</param>
	internal async Task OnMessageAckEventAsync(DiscordChannel chn, ulong messageId)
	{
		if (this.MessageCache == null || !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == chn.Id, out var msg))
		{
			msg = new()
			{
				Id = messageId,
				ChannelId = chn.Id,
				Discord = this,
			};
		}

		await this._messageAcknowledged.InvokeAsync(this, new(this.ServiceProvider) { Message = msg }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the message create event.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <param name="author">The transport user (author).</param>
	/// <param name="member">The transport member.</param>
	/// <param name="referenceAuthor">The reference transport user (author).</param>
	/// <param name="referenceMember">The reference transport member.</param>
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

			MentionedUsers = new ReadOnlyCollection<DiscordUser>(message.MentionedUsersInternal),
			MentionedRoles = message.MentionedRolesInternal != null ? new ReadOnlyCollection<DiscordRole>(message.MentionedRolesInternal) : null,
			MentionedChannels = message.MentionedChannelsInternal != null ? new ReadOnlyCollection<DiscordChannel>(message.MentionedChannelsInternal) : null
		};
		await this._messageCreated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the message update event.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <param name="author">The transport user (author).</param>
	/// <param name="member">The transport member.</param>
	/// <param name="referenceAuthor">The reference transport user (author).</param>
	/// <param name="referenceMember">The reference transport member.</param>
	internal async Task OnMessageUpdateEventAsync(DiscordMessage message, TransportUser author, TransportMember member, TransportUser referenceAuthor, TransportMember referenceMember)
	{
		DiscordGuild guild;

		message.Discord = this;
		var eventMessage = message;

		DiscordMessage oldmsg = null;
		if (this.Configuration.MessageCacheSize == 0
			|| this.MessageCache == null
			|| !this.MessageCache.TryGet(xm => xm.Id == eventMessage.Id && xm.ChannelId == eventMessage.ChannelId, out message))
		{
			message = eventMessage;
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
			oldmsg = new(message);

			guild = message.Channel?.Guild;
			message.EditedTimestampRaw = eventMessage.EditedTimestampRaw;
			if (eventMessage.Content != null)
				message.Content = eventMessage.Content;
			message.EmbedsInternal.Clear();
			message.EmbedsInternal.AddRange(eventMessage.EmbedsInternal);
			message.Pinned = eventMessage.Pinned;
			message.IsTts = eventMessage.IsTts;
		}

		message.PopulateMentions();

		var ea = new MessageUpdateEventArgs(this.ServiceProvider)
		{
			Message = message,
			MessageBefore = oldmsg,
			MentionedUsers = new ReadOnlyCollection<DiscordUser>(message.MentionedUsersInternal),
			MentionedRoles = message.MentionedRolesInternal != null ? new ReadOnlyCollection<DiscordRole>(message.MentionedRolesInternal) : null,
			MentionedChannels = message.MentionedChannelsInternal != null ? new ReadOnlyCollection<DiscordChannel>(message.MentionedChannelsInternal) : null
		};
		await this._messageUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the message delete event.
	/// </summary>
	/// <param name="messageId">The message id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="guildId">The optional guild id.</param>
	internal async Task OnMessageDeleteEventAsync(ulong messageId, ulong channelId, ulong? guildId)
	{
		var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);
		var guild = this.InternalGetCachedGuild(guildId);

		if (channel == null
			|| this.Configuration.MessageCacheSize == 0
			|| this.MessageCache == null
			|| !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
		{
			msg = new()
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
	/// <param name="guildId">The optional guild id.</param>
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
				msg = new()
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
	/// Handles the message reaction add event.
	/// </summary>
	/// <param name="userId">The user id.</param>
	/// <param name="messageId">The message id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="guildId">The optional guild id.</param>
	/// <param name="mbr">The transport member.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="isBurst">Whether a burst reaction was added.</param>
	internal async Task OnMessageReactionAddAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, TransportMember mbr, DiscordEmoji emoji, bool isBurst)
	{
		var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);
		var guild = this.InternalGetCachedGuild(guildId);
		emoji.Discord = this;

		var usr = this.UpdateUser(new() { Id = userId, Discord = this }, guildId, guild, mbr);

		DiscordMessage? msg = null;

		if (channel is not null)
			msg = await channel.GetMessageAsync(messageId).ConfigureAwait(false);

		msg ??= new()
		{
			Id = messageId,
			ChannelId = channelId,
			Discord = this,
			ReactionsInternal = new()
		};

		var react = msg.ReactionsInternal.FirstOrDefault(xr => xr.Emoji == emoji);
		if (react == null)
		{
			msg.ReactionsInternal.Add(react = new()
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
			Emoji = emoji,
			IsBurst = isBurst,
			Channel = channel,
			ChannelId = channelId
		};
		await this._messageReactionAdded.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the message reaction remove event.
	/// </summary>
	/// <param name="userId">The user id.</param>
	/// <param name="messageId">The message id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="emoji">The emoji.</param>
	/// <param name="isBurst">Whether a burst reaction was added.</param>
	internal async Task OnMessageReactionRemoveAsync(ulong userId, ulong messageId, ulong channelId, ulong? guildId, DiscordEmoji emoji, bool isBurst)
	{
		var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId) ?? new DiscordChannel()
		{
			Type = ChannelType.Unknown,
			Id = channelId,
			GuildId = guildId,
			Discord = this
		};

		emoji.Discord = this;

		if (!this.UserCache.TryGetValue(userId, out var usr))
			usr = new() { Id = userId, Discord = this };

		if (channel?.Guild != null)
			usr = channel.Guild.Members.TryGetValue(userId, out var member)
				? member
				: new(usr) { Discord = this, GuildId = channel.GuildId.Value };

		DiscordMessage? msg = null;

		if (channel is not null)
			msg = await channel.GetMessageAsync(messageId).ConfigureAwait(false);

		msg ??= new()
		{
			Id = messageId,
			ChannelId = channelId,
			Discord = this
		};

		var react = msg.ReactionsInternal?.FirstOrDefault(xr => xr.Emoji == emoji);
		if (react != null)
		{
			react.Count--;
			react.IsMe &= this.CurrentUser.Id != userId;

			if (msg.ReactionsInternal != null && react.Count <= 0) // shit happens
				msg.ReactionsInternal.RemoveFirst(x => x.Emoji == emoji);
		}

		var guild = this.InternalGetCachedGuild(guildId);

		var ea = new MessageReactionRemoveEventArgs(this.ServiceProvider)
		{
			Message = msg,
			User = usr,
			Guild = guild,
			Emoji = emoji,
			IsBurst = isBurst,
			Channel = channel,
			ChannelId = channelId
		};
		await this._messageReactionRemoved.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the message reaction remove event.
	/// Fired when all message reactions were removed.
	/// </summary>
	/// <param name="messageId">The message id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="guildId">The optional guild id.</param>
	internal async Task OnMessageReactionRemoveAllAsync(ulong messageId, ulong channelId, ulong? guildId)
	{
		var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId) ?? new DiscordChannel()
		{
			Type = ChannelType.Unknown,
			Id = channelId,
			GuildId = guildId,
			Discord = this
		};

		if (channel == null
			|| this.Configuration.MessageCacheSize == 0
			|| this.MessageCache == null
			|| !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
		{
			msg = new()
			{
				Id = messageId,
				ChannelId = channelId,
				Discord = this
			};
		}

		msg.ReactionsInternal?.Clear();

		var guild = this.InternalGetCachedGuild(guildId);

		var ea = new MessageReactionsClearEventArgs(this.ServiceProvider)
		{
			Message = msg
		};

		await this._messageReactionsCleared.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the message reaction remove event.
	/// Fired when a emoji got removed.
	/// </summary>
	/// <param name="messageId">The message id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="partialEmoji">The raw discord emoji.</param>
	internal async Task OnMessageReactionRemoveEmojiAsync(ulong messageId, ulong channelId, ulong guildId, DiscordEmoji partialEmoji)
	{
		var guild = this.InternalGetCachedGuild(guildId);
		var channel = this.InternalGetCachedChannel(channelId) ?? this.InternalGetCachedThread(channelId);

		if (channel == null
			|| this.Configuration.MessageCacheSize == 0
			|| this.MessageCache == null
			|| !this.MessageCache.TryGet(xm => xm.Id == messageId && xm.ChannelId == channelId, out var msg))
		{
			msg = new()
			{
				Id = messageId,
				ChannelId = channelId,
				Discord = this
			};
		}

		if (!guild.EmojisInternal.TryGetValue(partialEmoji.Id, out var emoji))
		{
			emoji = partialEmoji;
			emoji.Discord = this;
		}

		msg.ReactionsInternal?.RemoveAll(r => r.Emoji.Equals(emoji));

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
	/// Handles the stage instance create event.
	/// </summary>
	/// <param name="stage">The created stage instance.</param>
	internal async Task OnStageInstanceCreateEventAsync(DiscordStageInstance stage)
	{
		stage.Discord = this;

		var guild = this.InternalGetCachedGuild(stage.GuildId);
		guild.StageInstancesInternal[stage.Id] = stage;

		await this._stageInstanceCreated.InvokeAsync(this, new(this.ServiceProvider) { StageInstance = stage, Guild = stage.Guild }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the stage instance update event.
	/// </summary>
	/// <param name="stage">The updated stage instance.</param>
	internal async Task OnStageInstanceUpdateEventAsync(DiscordStageInstance stage)
	{
		stage.Discord = this;
		var guild = this.InternalGetCachedGuild(stage.GuildId);
		guild.StageInstancesInternal[stage.Id] = stage;

		await this._stageInstanceUpdated.InvokeAsync(this, new(this.ServiceProvider) { StageInstance = stage, Guild = stage.Guild }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the stage instance delete event.
	/// </summary>
	/// <param name="stage">The deleted stage instance.</param>
	internal async Task OnStageInstanceDeleteEventAsync(DiscordStageInstance stage)
	{
		stage.Discord = this;
		var guild = this.InternalGetCachedGuild(stage.GuildId);
		guild.StageInstancesInternal[stage.Id] = stage;

		await this._stageInstanceDeleted.InvokeAsync(this, new(this.ServiceProvider) { StageInstance = stage, Guild = stage.Guild }).ConfigureAwait(false);
	}

	#endregion

	#region Thread

	/// <summary>
	/// Handles the thread create event.
	/// </summary>
	/// <param name="thread">The created thread.</param>
	internal async Task OnThreadCreateEventAsync(DiscordThreadChannel thread)
	{
		thread.Discord = this;
		this.InternalGetCachedGuild(thread.GuildId).ThreadsInternal.AddOrUpdate(thread.Id, thread, (oldThread, newThread) => newThread);

		await this._threadCreated.InvokeAsync(this, new(this.ServiceProvider) { Thread = thread, Guild = thread.Guild, Parent = thread.Parent }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the thread update event.
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
			threadOld = new()
			{
				Discord = this,
				Type = threadNew.Type,
				ThreadMetadata = thread.ThreadMetadata,
				ThreadMembersInternal = threadNew.ThreadMembersInternal,
				ParentId = thread.ParentId,
				OwnerId = thread.OwnerId,
				Name = thread.Name,
				LastMessageId = threadNew.LastMessageId,
				MessageCount = thread.MessageCount,
				MemberCount = thread.MemberCount,
				GuildId = thread.GuildId,
				LastPinTimestampRaw = threadNew.LastPinTimestampRaw,
				PerUserRateLimit = threadNew.PerUserRateLimit,
				CurrentMember = threadNew.CurrentMember,
				TotalMessagesSent = threadNew.TotalMessagesSent
			};

			if (this.Guilds != null)
			{
				if (thread.ParentId.HasValue && this.InternalGetCachedChannel(thread.ParentId.Value).Type == ChannelType.Forum)
				{
					threadOld.AppliedTagIdsInternal = threadNew.AppliedTagIdsInternal;
					threadNew.AppliedTagIdsInternal = thread.AppliedTagIdsInternal;
				}
				else
				{
					threadOld.AppliedTagIdsInternal = null;
					threadNew.AppliedTagIdsInternal = null;
				}
			}
			else
			{
				threadOld.AppliedTagIdsInternal = threadNew.AppliedTagIdsInternal;
				threadNew.AppliedTagIdsInternal = thread.AppliedTagIdsInternal;
			}

			threadNew.ThreadMetadata = thread.ThreadMetadata;
			threadNew.ParentId = thread.ParentId;
			threadNew.OwnerId = thread.OwnerId;
			threadNew.Name = thread.Name;
			threadNew.LastMessageId = thread.LastMessageId.HasValue ? thread.LastMessageId : threadOld.LastMessageId;
			threadNew.MessageCount = thread.MessageCount;
			threadNew.MemberCount = thread.MemberCount;
			threadNew.GuildId = thread.GuildId;
			threadNew.Discord = this;
			threadNew.TotalMessagesSent = thread.TotalMessagesSent;

			updateEvent = new(this.ServiceProvider)
			{
				ThreadAfter = thread,
				ThreadBefore = threadOld,
				Guild = thread.Guild,
				Parent = thread.Parent
			};
		}
		else
		{
			updateEvent = new(this.ServiceProvider)
			{
				ThreadAfter = thread,
				Guild = thread.Guild,
				Parent = thread.Parent
			};
			guild.ThreadsInternal[thread.Id] = thread;
		}

		await this._threadUpdated.InvokeAsync(this, updateEvent).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the thread delete event.
	/// </summary>
	/// <param name="thread">The deleted thread.</param>
	internal async Task OnThreadDeleteEventAsync(DiscordThreadChannel thread)
	{
		if (thread == null)
			return;

		thread.Discord = this;

		var gld = thread.Guild;
		if (gld.ThreadsInternal.TryRemove(thread.Id, out var cachedThread))
			thread = cachedThread;

		await this._threadDeleted.InvokeAsync(this, new(this.ServiceProvider) { Thread = thread, Guild = thread.Guild, Parent = thread.Parent, Type = thread.Type }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the thread list sync event.
	/// </summary>
	/// <param name="guild">The synced guild.</param>
	/// <param name="channelIds">The synced channel ids.</param>
	/// <param name="threads">The synced threads.</param>
	/// <param name="members">The synced thread members.</param>
	internal async Task OnThreadListSyncEventAsync(DiscordGuild guild, IReadOnlyList<ulong?> channelIds, IReadOnlyList<DiscordThreadChannel> threads, IReadOnlyList<DiscordThreadChannelMember> members)
	{
		guild.Discord = this;

		var channels = channelIds.Select(x => guild.GetChannel(x.Value)); //getting channel objects
		foreach (var chan in channels)
		{
			chan.Discord = this;
		}
		_ = threads.Select(x => x.Discord = this);

		await this._threadListSynced.InvokeAsync(this, new(this.ServiceProvider) { Guild = guild, Channels = channels.ToList().AsReadOnly(), Threads = threads, Members = members.ToList().AsReadOnly() }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the thread member update event.
	/// </summary>
	/// <param name="member">The updated member.</param>
	internal async Task OnThreadMemberUpdateEventAsync(DiscordThreadChannelMember member)
	{
		member.Discord = this;
		var thread = this.InternalGetCachedThread(member.Id);
		if (thread == null)
		{
			var tempThread = await this.ApiClient.GetThreadAsync(member.Id).ConfigureAwait(false);
			thread = this.GuildsInternal[member.GuildId].ThreadsInternal.AddOrUpdate(member.Id, tempThread, (old, newThread) => newThread);
		}

		thread.CurrentMember = member;
		thread.Guild.ThreadsInternal.AddOrUpdate(member.Id, thread, (oldThread, newThread) => newThread);


		await this._threadMemberUpdated.InvokeAsync(this, new(this.ServiceProvider) { ThreadMember = member, Thread = thread }).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the thread members update event.
	/// </summary>
	/// <param name="guild">The target guild.</param>
	/// <param name="threadId">The thread id of the target thread this update belongs to.</param>
	/// <param name="membersAdded">The added members.</param>
	/// <param name="membersRemoved">The ids of the removed members.</param>
	/// <param name="memberCount">The new member count.</param>
	internal async Task OnThreadMembersUpdateEventAsync(DiscordGuild guild, ulong threadId, JArray membersAdded, JArray membersRemoved, int memberCount)
	{
		var thread = this.InternalGetCachedThread(threadId);
		if (thread == null)
		{
			var tempThread = await this.ApiClient.GetThreadAsync(threadId).ConfigureAwait(false);
			thread = guild.ThreadsInternal.AddOrUpdate(threadId, tempThread, (old, newThread) => newThread);
		}

		thread.Discord = this;
		guild.Discord = this;
		List<DiscordThreadChannelMember> addedMembers = new();
		List<ulong> removedMemberIds = new();

		if (membersAdded != null)
		{
			foreach (var xj in membersAdded)
			{
				var xtm = xj.ToDiscordObject<DiscordThreadChannelMember>();
				xtm.Discord = this;
				xtm.GuildId = guild.Id;
				if (xtm != null)
					addedMembers.Add(xtm);

				if (xtm.Id == this.CurrentUser.Id)
					thread.CurrentMember = xtm;
			}
		}

		var removedMembers = new List<DiscordMember>();
		if (membersRemoved != null)
		{
			foreach (var removedId in membersRemoved)
			{
				removedMembers.Add(guild.MembersInternal.TryGetValue((ulong)removedId, out var member) ? member : new() { Id = (ulong)removedId, GuildId = guild.Id, Discord = this });
			}
		}

		if (removedMemberIds.Contains(this.CurrentUser.Id)) //indicates the bot was removed from the thread
			thread.CurrentMember = null;

		thread.MemberCount = memberCount;

		var threadMembersUpdateArg = new ThreadMembersUpdateEventArgs(this.ServiceProvider)
		{
			Guild = guild,
			Thread = thread,
			AddedMembers = addedMembers,
			RemovedMembers = removedMembers,
			MemberCount = memberCount
		};

		await this._threadMembersUpdated.InvokeAsync(this, threadMembersUpdateArg).ConfigureAwait(false);
	}

	#endregion

	#region Activities
	/// <summary>
	/// Dispatches the <see cref="EmbeddedActivityUpdated"/> event.
	/// </summary>
	/// <param name="trActivity">The transport activity.</param>
	/// <param name="guild">The guild.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="jUsers">The users in the activity.</param>
	/// <param name="appId">The application id.</param>
	internal async Task OnEmbeddedActivityUpdateAsync(JObject trActivity, DiscordGuild guild, ulong channelId, JArray jUsers, ulong appId)
		=> await Task.Delay(20).ConfigureAwait(false);

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
		var uid = (ulong)rawUser["id"]!;
		DiscordPresence old = null;

		if (this.PresencesInternal.TryGetValue(uid, out var presence))
		{
			old = new(presence);
			DiscordJson.PopulateObject(rawPresence, presence);
		}
		else
		{
			presence = DiscordJson.DeserializeObject<DiscordPresence>(rawPresence.ToString(), this); ;
			presence.Discord = this;
			presence.Activity = new(presence.RawActivity);
			this.PresencesInternal[presence.InternalUser.Id] = presence;
		}

		// reuse arrays / avoid linq (this is a hot zone)
		if (presence.Activities == null || rawPresence["activities"] == null)
		{
			presence.InternalActivities = Array.Empty<DiscordActivity>();
		}
		else
		{
			if (presence.InternalActivities.Length != presence.RawActivities.Length)
				presence.InternalActivities = new DiscordActivity[presence.RawActivities.Length];

			for (var i = 0; i < presence.InternalActivities.Length; i++)
				presence.InternalActivities[i] = new(presence.RawActivities[i]);

			if (presence.InternalActivities.Length > 0)
			{
				presence.RawActivity = presence.RawActivities[0];

				if (presence.Activity != null)
					presence.Activity.UpdateWith(presence.RawActivity);
				else
					presence.Activity = new(presence.RawActivity);
			}
		}

		var ea = new PresenceUpdateEventArgs(this.ServiceProvider)
		{
			Status = presence.Status,
			Activity = presence.Activity,
			User = presence.User,
			PresenceBefore = old,
			PresenceAfter = presence
		};
		await this._presenceUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the user settings update event.
	/// </summary>
	/// <param name="user">The transport user.</param>

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
	/// <param name="user">The transport user.</param>

	internal async Task OnUserUpdateEventAsync(TransportUser user)
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
			UserBefore = usrOld
		};
		await this._userUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	#endregion

	#region Voice

	/// <summary>
	/// Handles the voice state update event.
	/// </summary>
	/// <param name="raw">The raw voice state update object.</param>

	internal async Task OnVoiceStateUpdateEventAsync(JObject raw)
	{
		var gid = (ulong)raw["guild_id"];
		var uid = (ulong)raw["user_id"];
		var gld = this.GuildsInternal[gid];

		var vstateNew = DiscordJson.DeserializeObject<DiscordVoiceState>(raw.ToString(), this);
		vstateNew.Discord = this;

		gld.VoiceStatesInternal.TryRemove(uid, out var vstateOld);

		if (vstateNew.Channel != null)
		{
			gld.VoiceStatesInternal[vstateNew.UserId] = vstateNew;
		}

		if (gld.MembersInternal.TryGetValue(uid, out var mbr))
		{
			mbr.IsMuted = vstateNew.IsServerMuted;
			mbr.IsDeafened = vstateNew.IsServerDeafened;
		}
		else
		{
			var transportMbr = vstateNew.TransportMember;
			this.UpdateUser(new(transportMbr.User) { Discord = this }, gid, gld, transportMbr);
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
	/// <param name="endpoint">The new endpoint.</param>
	/// <param name="token">The new token.</param>
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
	/// Handles the application command create event.
	/// </summary>
	/// <param name="cmd">The application command.</param>
	/// <param name="guildId">The optional guild id.</param>

	internal async Task OnApplicationCommandCreateAsync(DiscordApplicationCommand cmd, ulong? guildId)
	{
		cmd.Discord = this;

		var guild = this.InternalGetCachedGuild(guildId);

		if (guild == null && guildId.HasValue)
		{
			guild = new()
			{
				Id = guildId.Value,
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
	/// Handles the application command update event.
	/// </summary>
	/// <param name="cmd">The application command.</param>
	/// <param name="guildId">The optional guild id.</param>

	internal async Task OnApplicationCommandUpdateAsync(DiscordApplicationCommand cmd, ulong? guildId)
	{
		cmd.Discord = this;

		var guild = this.InternalGetCachedGuild(guildId);

		if (guild == null && guildId.HasValue)
		{
			guild = new()
			{
				Id = guildId.Value,
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
	/// Handles the application command delete event.
	/// </summary>
	/// <param name="cmd">The application command.</param>
	/// <param name="guildId">The optional guild id.</param>

	internal async Task OnApplicationCommandDeleteAsync(DiscordApplicationCommand cmd, ulong? guildId)
	{
		cmd.Discord = this;

		var guild = this.InternalGetCachedGuild(guildId);

		if (guild == null && guildId.HasValue)
		{
			guild = new()
			{
				Id = guildId.Value,
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
	/// Handles the guild application command counts update event.
	/// </summary>
	/// <param name="chatInputCommandCount">The <see cref="ApplicationCommandType.ChatInput"/> count.</param>
	/// <param name="userContextMenuCommandCount">The <see cref="ApplicationCommandType.User"/> count.</param>
	/// <param name="messageContextMenuCount">The <see cref="ApplicationCommandType.Message"/> count.</param>
	/// <param name="guildId">The guild id.</param>
	/// <returns>Count of application commands.</returns>
	internal async Task OnGuildApplicationCommandCountsUpdateAsync(int chatInputCommandCount, int userContextMenuCommandCount, int messageContextMenuCount, ulong guildId)
	{
		var guild = this.InternalGetCachedGuild(guildId) ?? new DiscordGuild
		{
			Id = guildId,
			Discord = this
		};
		var ea = new GuildApplicationCommandCountEventArgs(this.ServiceProvider)
		{
			SlashCommands = chatInputCommandCount,
			UserContextMenuCommands = userContextMenuCommandCount,
			MessageContextMenuCommands = messageContextMenuCount,
			Guild = guild
		};

		await this._guildApplicationCommandCountUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the application command permissions update event.
	/// </summary>
	/// <param name="perms">The new permissions.</param>
	/// <param name="channelId">The command id.</param>
	/// <param name="guildId">The guild id.</param>
	/// <param name="applicationId">The application id.</param>
	internal async Task OnApplicationCommandPermissionsUpdateAsync(IEnumerable<DiscordApplicationCommandPermission> perms, ulong channelId, ulong guildId, ulong applicationId)
	{
		if (applicationId != this.CurrentApplication.Id)
			return;

		var guild = this.InternalGetCachedGuild(guildId);

		DiscordApplicationCommand cmd;
		try
		{
			cmd = await this.GetGuildApplicationCommandAsync(guildId, channelId).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			cmd = await this.GetGlobalApplicationCommandAsync(channelId).ConfigureAwait(false);
		}

		if (guild == null)
		{
			guild = new()
			{
				Id = guildId,
				Discord = this
			};
		}

		var ea = new ApplicationCommandPermissionsUpdateEventArgs(this.ServiceProvider)
		{
			Permissions = perms.ToList(),
			Command = cmd,
			ApplicationId = applicationId,
			Guild = guild
		};

		await this._applicationCommandPermissionsUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	#endregion

	#region Interaction

	/// <summary>
	/// Handles the interaction create event.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="user">The transport user.</param>
	/// <param name="member">The transport member.</param>
	/// <param name="interaction">The interaction.</param>
	/// <param name="rawInteraction">Debug.</param>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
	internal async Task OnInteractionCreateAsync(ulong? guildId, ulong channelId, TransportUser user, TransportMember member, DiscordInteraction interaction, string rawInteraction)
	{
		if (this.Configuration.EnableLibraryDeveloperMode)
		{
			//this.Logger.LogDebug("Interaction from {guild} on shard {shard}", guildId.HasValue ? guildId.Value : "dm", this.ShardId);
			//this.Logger.LogDebug("Interaction: {interaction}", rawInteraction);
		}
		var usr = new DiscordUser(user) { Discord = this };

		interaction.ChannelId = channelId;
		interaction.GuildId = guildId;
		interaction.Discord = this;
		interaction.Data.Discord = this;

		if (member != null)
		{
			usr = new DiscordMember(member) { GuildId = guildId.Value, Discord = this };
			if (interaction.Guild is not null)
				this.UpdateUser(usr, guildId, interaction.Guild, member);
		}
		else
		{
			this.UserCache.AddOrUpdate(usr.Id, usr, (old, @new) => @new);
		}

		usr.Locale = interaction.Locale;
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
					c.Value.GuildId = guildId.Value;
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
					{
						c.Value.GuildId = guildId.Value;
						try
						{
							if (this.Guilds.TryGetValue(guildId.Value, out var guild))
								if (guild.ChannelsInternal.TryGetValue(c.Key, out var channel) && channel.PermissionOverwritesInternal != null && channel.PermissionOverwritesInternal.Any())
									c.Value.PermissionOverwritesInternal = channel.PermissionOverwritesInternal;
						}
						catch (Exception) { }
					}
				}
			}

			if (resolved.Roles != null)
			{
				foreach (var c in resolved.Roles)
				{
					c.Value.Discord = this;

					if (guildId.HasValue)
						c.Value.GuildId = guildId.Value;
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


			if (resolved.Attachments != null)
				foreach (var a in resolved.Attachments)
					a.Value.Discord = this;
		}

		if (interaction.Type is InteractionType.Component || interaction.Type is InteractionType.ModalSubmit)
		{
			if (interaction.Message != null)
			{
				interaction.Message.Discord = this;
				interaction.Message.ChannelId = interaction.ChannelId;
			}
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

				var ea = new ContextMenuInteractionCreateEventArgs(this.ServiceProvider)
				{
					Interaction = interaction,
					TargetUser = targetMember ?? targetUser,
					TargetMessage = targetMessage,
					Type = interaction.Data.Type
				};
				await this._contextMenuInteractionCreated.InvokeAsync(this, ea).ConfigureAwait(false);
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
	/// Handles the entitlement create event.
	/// </summary>
	/// <param name="entitlement">The created entitlement</param>
	internal async Task OnEntitlementCreateAsync(DiscordEntitlement entitlement)
	{
		var ea = new EntitlementCreateEventArgs(this.ServiceProvider)
		{
			Entitlement = entitlement
		};
		await this._entitlementCreated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the entitlement update event.
	/// </summary>
	/// <param name="entitlement">The updated entitlement</param>
	internal async Task OnEntitlementUpdateAsync(DiscordEntitlement entitlement)
	{
		var ea = new EntitlementUpdateEventArgs(this.ServiceProvider)
		{
			Entitlement = entitlement
		};
		await this._entitlementUpdated.InvokeAsync(this, ea).ConfigureAwait(false);
	}

	/// <summary>
	/// Handles the entitlement delete event.
	/// </summary>
	/// <param name="entitlement">The deleted entitlement</param>
	internal async Task OnEntitlementDeleteAsync(DiscordEntitlement entitlement)
	{
		var ea = new EntitlementDeleteEventArgs(this.ServiceProvider)
		{
			Entitlement = entitlement
		};
		await this._entitlementDeleted.InvokeAsync(this, ea).ConfigureAwait(false);
	}

		/// <summary>
		/// Handles the typing start event.
		/// </summary>
		/// <param name="userId">The user id.</param>
		/// <param name="channelId">The channel id.</param>
		/// <param name="channel">The channel.</param>
		/// <param name="guildId">The optional guild id.</param>
		/// <param name="started">The time when the user started typing.</param>
		/// <param name="mbr">The transport member.</param>
		internal async Task OnTypingStartEventAsync(ulong userId, ulong channelId, DiscordChannel channel, ulong? guildId, DateTimeOffset started, TransportMember mbr)
		{
			if (channel == null)
			{
				channel = new()
				{
					Discord = this,
					Id = channelId,
					GuildId = guildId ?? default,
				};
			}

			var guild = this.InternalGetCachedGuild(guildId);
			var usr = this.UpdateUser(new() { Id = userId, Discord = this }, guildId, guild, mbr);
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
		/// Handles all unknown events.
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
