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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

public partial class DiscordGuild
{
	// TODO: Rework audit logs!

	/// <summary>
	/// Gets audit log entries for this guild.
	/// </summary>
	/// <param name="limit">Maximum number of entries to fetch.</param>
	/// <param name="byMember">Filter by member responsible.</param>
	/// <param name="actionType">Filter by action type.</param>
	/// <returns>A collection of requested audit log entries.</returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewAuditLog"/> permission.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyList<DiscordAuditLogEntry>> GetAuditLogsAsync(int? limit = null, DiscordMember byMember = null, AuditLogActionType? actionType = null)
	{
		var alrs = new List<AuditLog>();
		int ac = 1, tc = 0, rmn = 100;
		var last = 0ul;
		while (ac > 0)
		{
			rmn = limit != null ? limit.Value - tc : 100;
			rmn = Math.Min(100, rmn);
			if (rmn <= 0) break;

			var alr = await this.Discord.ApiClient.GetAuditLogsAsync(this.Id, rmn, null, last == 0 ? null : last, byMember?.Id, (int?)actionType).ConfigureAwait(false);
			ac = alr.Entries.Count;
			tc += ac;
			if (ac > 0)
			{
				last = alr.Entries[alr.Entries.Count - 1].Id;
				alrs.Add(alr);
			}
		}

		var auditLogResult = await this.ProcessAuditLog(alrs);
		return auditLogResult;
	}

	/// <summary>
	/// Proceesses audit log objects.
	/// </summary>
	/// <param name="auditLogApiResult">A list of raw audit log objects.</param>
	/// <returns>The processed audit log list as readonly.</returns>
	internal async Task<IReadOnlyList<DiscordAuditLogEntry>> ProcessAuditLog(List<AuditLog> auditLogApiResult)
	{
		List<AuditLogUser> amr = new();
		if (auditLogApiResult.Any(ar => ar.Users != null && ar.Users.Any()))
			amr = auditLogApiResult.SelectMany(xa => xa.Users)
				.GroupBy(xu => xu.Id)
				.Select(xgu => xgu.First()).ToList();

		if (amr.Any())
			foreach (var xau in amr)
			{
				if (this.Discord.UserCache.ContainsKey(xau.Id))
					continue;

				var xtu = new TransportUser
				{
					Id = xau.Id,
					Username = xau.Username,
					Discriminator = xau.Discriminator,
					AvatarHash = xau.AvatarHash
				};
				var xu = new DiscordUser(xtu) { Discord = this.Discord };
				xu = this.Discord.UserCache.AddOrUpdate(xu.Id, xu, (id, old) =>
				{
					old.Username = xu.Username;
					old.Discriminator = xu.Discriminator;
					old.AvatarHash = xu.AvatarHash;
					old.GlobalName = xu.GlobalName;
					return old;
				});
			}

		List<AuditLogGuildScheduledEvent> atgse = new();
		if (auditLogApiResult.Any(ar => ar.ScheduledEvents != null && ar.ScheduledEvents.Any()))
			atgse = auditLogApiResult.SelectMany(xa => xa.ScheduledEvents)
				.GroupBy(xse => xse.Id)
				.Select(xgse => xgse.First()).ToList();

		List<AuditLogThread> ath = new();
		if (auditLogApiResult.Any(ar => ar.Threads != null && ar.Threads.Any()))
			ath = auditLogApiResult.SelectMany(xa => xa.Threads)
				.GroupBy(xt => xt.Id)
				.Select(xgt => xgt.First()).ToList();

		List<AuditLogIntegration> aig = new();
		if (auditLogApiResult.Any(ar => ar.Integrations != null && ar.Integrations.Any()))
			aig = auditLogApiResult.SelectMany(xa => xa.Integrations)
				.GroupBy(xi => xi.Id)
				.Select(xgi => xgi.First()).ToList();

		List<AuditLogWebhook> ahr = new();
		if (auditLogApiResult.Any(ar => ar.Webhooks != null && ar.Webhooks.Any()))
			ahr = auditLogApiResult.SelectMany(xa => xa.Webhooks)
				.GroupBy(xh => xh.Id)
				.Select(xgh => xgh.First()).ToList();

		List<DiscordMember> ams = new();
		Dictionary<ulong, DiscordMember> amd = new();
		if (amr.Any())
			ams = amr.Select(xau => this.MembersInternal != null && this.MembersInternal.TryGetValue(xau.Id, out var member) ? member : new DiscordMember { Discord = this.Discord, Id = xau.Id, GuildId = this.Id }).ToList();
		if (ams.Any())
			amd = ams.ToDictionary(xm => xm.Id, xm => xm);

#pragma warning disable CS0219
		Dictionary<ulong, DiscordThreadChannel> dtc = null;
		Dictionary<ulong, DiscordIntegration> di = null;
		Dictionary<ulong, DiscordScheduledEvent> dse = null;
#pragma warning restore

		Dictionary<ulong, DiscordWebhook> ahd = null;
		if (ahr.Any())
		{
			var whr = await this.GetWebhooksAsync().ConfigureAwait(false);
			var whs = whr.ToDictionary(xh => xh.Id, xh => xh);

			var amh = ahr.Select(xah => whs.TryGetValue(xah.Id, out var webhook) ? webhook : new DiscordWebhook { Discord = this.Discord, Name = xah.Name, Id = xah.Id, AvatarHash = xah.AvatarHash, ChannelId = xah.ChannelId, GuildId = xah.GuildId, Token = xah.Token });
			ahd = amh.ToDictionary(xh => xh.Id, xh => xh);
		}

		var acs = auditLogApiResult.SelectMany(xa => xa.Entries).OrderByDescending(xa => xa.Id);
		var entries = new List<DiscordAuditLogEntry>();
		foreach (var xac in acs)
		{
			DiscordAuditLogEntry entry = null;
			ulong t1, t2;
			int t3, t4;
			long t5, t6;
			bool p1, p2;
			switch (xac.ActionType)
			{
				case AuditLogActionType.Invalid:
					break;

				case AuditLogActionType.GuildUpdate:
					entry = new DiscordAuditLogGuildEntry
					{
						Target = this
					};

					var entrygld = entry as DiscordAuditLogGuildEntry;
					foreach (var xc in xac.Changes)
					{
						PropertyChange<DiscordChannel> GetChannelChange()
						{
							ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
							ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

							return new PropertyChange<DiscordChannel>
							{
								Before = this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id },
								After = this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id }
							};
						}

						switch (xc.Key.ToLowerInvariant())
						{
							case "name":
								entrygld.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "owner_id":
								entrygld.OwnerChange = new PropertyChange<DiscordMember>
								{
									Before = this.MembersInternal != null && this.MembersInternal.TryGetValue(xc.OldValueUlong, out var oldMember) ? oldMember : await this.GetMemberAsync(xc.OldValueUlong).ConfigureAwait(false),
									After = this.MembersInternal != null && this.MembersInternal.TryGetValue(xc.NewValueUlong, out var newMember) ? newMember : await this.GetMemberAsync(xc.NewValueUlong).ConfigureAwait(false)
								};
								break;

							case "icon_hash":
								entrygld.IconChange = new PropertyChange<string>
								{
									Before = xc.OldValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ICONS}/{this.Id}/{xc.OldValueString}.webp" : null,
									After = xc.OldValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ICONS}/{this.Id}/{xc.NewValueString}.webp" : null
								};
								break;

							case "verification_level":
								entrygld.VerificationLevelChange = new PropertyChange<VerificationLevel>
								{
									Before = (VerificationLevel)(long)xc.OldValue,
									After = (VerificationLevel)(long)xc.NewValue
								};
								break;

							case "afk_channel_id":
								entrygld.AfkChannelChange = GetChannelChange();
								break;

							case "system_channel_flags":
								entrygld.SystemChannelFlagsChange = new PropertyChange<SystemChannelFlags>()
								{
									Before = (SystemChannelFlags)(long)xc.OldValue,
									After = (SystemChannelFlags)(long)xc.NewValue
								};
								break;

							case "widget_channel_id":
								entrygld.WidgetChannelChange = GetChannelChange();
								break;

							case "rules_channel_id":
								entrygld.RulesChannelChange = GetChannelChange();
								break;

							case "public_updates_channel_id":
								entrygld.PublicUpdatesChannelChange = GetChannelChange();
								break;

							case "splash_hash":
								entrygld.SplashChange = new PropertyChange<string>
								{
									Before = xc.OldValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.SPLASHES}/{this.Id}/{xc.OldValueString}.webp?size=2048" : null,
									After = xc.NewValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.SPLASHES}/{this.Id}/{xc.NewValueString}.webp?size=2048" : null
								};
								break;

							case "default_message_notifications":
								entrygld.NotificationSettingsChange = new PropertyChange<DefaultMessageNotifications>
								{
									Before = (DefaultMessageNotifications)(long)xc.OldValue,
									After = (DefaultMessageNotifications)(long)xc.NewValue
								};
								break;

							case "system_channel_id":
								entrygld.SystemChannelChange = GetChannelChange();
								break;

							case "explicit_content_filter":
								entrygld.ExplicitContentFilterChange = new PropertyChange<ExplicitContentFilter>
								{
									Before = (ExplicitContentFilter)(long)xc.OldValue,
									After = (ExplicitContentFilter)(long)xc.NewValue
								};
								break;

							case "mfa_level":
								entrygld.MfaLevelChange = new PropertyChange<MfaLevel>
								{
									Before = (MfaLevel)(long)xc.OldValue,
									After = (MfaLevel)(long)xc.NewValue
								};
								break;

							case "region":
								entrygld.RegionChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "vanity_url_code":
								entrygld.VanityUrlCodeChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "premium_progress_bar_enabled":
								entrygld.PremiumProgressBarChange = new PropertyChange<bool>
								{
									Before = (bool)xc.OldValue,
									After = (bool)xc.NewValue
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in guild update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.ChannelCreate:
				case AuditLogActionType.ChannelDelete:
				case AuditLogActionType.ChannelUpdate:
					entry = new DiscordAuditLogChannelEntry
					{
						Target = this.GetChannel(xac.TargetId.Value) ?? new DiscordChannel { Id = xac.TargetId.Value, Discord = this.Discord, GuildId = this.Id }
					};

					var entrychn = entry as DiscordAuditLogChannelEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "name":
								entrychn.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "type":
								p1 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entrychn.TypeChange = new PropertyChange<ChannelType?>
								{
									Before = p1 ? (ChannelType?)t1 : null,
									After = p2 ? (ChannelType?)t2 : null
								};
								break;

							case "flags":
								entrychn.ChannelFlagsChange = new PropertyChange<ChannelFlags>()
								{
									Before = (ChannelFlags)(long)(xc.OldValue ?? 0L),
									After = (ChannelFlags)(long)(xc.NewValue ?? 0L)
								};
								break;

							case "permission_overwrites":
								var olds = xc.OldValues?.OfType<JObject>()
									?.Select(xjo => xjo.ToObject<DiscordOverwrite>())
									?.Select(xo => { xo.Discord = this.Discord; return xo; });

								var news = xc.NewValues?.OfType<JObject>()
									?.Select(xjo => xjo.ToObject<DiscordOverwrite>())
									?.Select(xo => { xo.Discord = this.Discord; return xo; });

								entrychn.OverwriteChange = new PropertyChange<IReadOnlyList<DiscordOverwrite>>
								{
									Before = olds != null ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(olds)) : null,
									After = news != null ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(news)) : null
								};
								break;

							case "topic":
								entrychn.TopicChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "nsfw":
								entrychn.NsfwChange = new PropertyChange<bool?>
								{
									Before = (bool?)xc.OldValue,
									After = (bool?)xc.NewValue
								};
								break;

							case "rtc_region":
								entrychn.RtcRegionIdChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "bitrate":
								entrychn.BitrateChange = new PropertyChange<int?>
								{
									Before = (int?)(long?)xc.OldValue,
									After = (int?)(long?)xc.NewValue
								};
								break;

							case "user_limit":
								entrychn.UserLimitChange = new PropertyChange<int?>
								{
									Before = (int?)(long?)xc.OldValue,
									After = (int?)(long?)xc.NewValue
								};
								break;

							case "rate_limit_per_user":
								entrychn.PerUserRateLimitChange = new PropertyChange<int?>
								{
									Before = (int?)(long?)xc.OldValue,
									After = (int?)(long?)xc.NewValue
								};
								break;

							case "default_auto_archive_duration":
								entrychn.DefaultAutoArchiveDurationChange = new PropertyChange<ThreadAutoArchiveDuration?>
								{
									Before = (ThreadAutoArchiveDuration?)(long?)xc.OldValue,
									After = (ThreadAutoArchiveDuration?)(long?)xc.NewValue
								};
								break;
							case "available_tags":
								var old_tags = xc.OldValues?.OfType<JObject>()
									?.Select(xjo => xjo.ToObject<ForumPostTag>())
									?.Select(xo => { xo.Discord = this.Discord; return xo; });

								var new_tags = xc.NewValues?.OfType<JObject>()
									?.Select(xjo => xjo.ToObject<ForumPostTag>())
									?.Select(xo => { xo.Discord = this.Discord; return xo; });

								entrychn.AvailableTagsChange = new PropertyChange<List<ForumPostTag>>
								{
									Before = old_tags != null ? new List<ForumPostTag>(new List<ForumPostTag>(old_tags)) : null,
									After = new_tags != null ? new List<ForumPostTag>(new List<ForumPostTag>(new_tags)) : null
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in channel update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.OverwriteCreate:
				case AuditLogActionType.OverwriteDelete:
				case AuditLogActionType.OverwriteUpdate:
					entry = new DiscordAuditLogOverwriteEntry
					{
						Target = this.GetChannel(xac.TargetId.Value)?.PermissionOverwrites.FirstOrDefault(xo => xo.Id == xac.Options.Id),
						Channel = this.GetChannel(xac.TargetId.Value)
					};

					var entryovr = entry as DiscordAuditLogOverwriteEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "deny":
								p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entryovr.DenyChange = new PropertyChange<Permissions?>
								{
									Before = p1 ? (Permissions?)t1 : null,
									After = p2 ? (Permissions?)t2 : null
								};
								break;

							case "allow":
								p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entryovr.AllowChange = new PropertyChange<Permissions?>
								{
									Before = p1 ? (Permissions?)t1 : null,
									After = p2 ? (Permissions?)t2 : null
								};
								break;

							case "type":
								entryovr.TypeChange = new PropertyChange<OverwriteType?>
								{
									Before = xc.OldValue != null ? (OverwriteType)(long)xc.OldValue : null,
									After = xc.NewValue != null ? (OverwriteType)(long)xc.NewValue : null
								};
								break;

							case "id":
								p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entryovr.TargetIdChange = new PropertyChange<ulong?>
								{
									Before = p1 ? t1 : null,
									After = p2 ? t2 : null
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in overwrite update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.Kick:
					entry = new DiscordAuditLogKickEntry
					{
						Target = amd.TryGetValue(xac.TargetId.Value, out var kickMember) ? kickMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, GuildId = this.Id }
					};
					break;

				case AuditLogActionType.Prune:
					entry = new DiscordAuditLogPruneEntry
					{
						Days = xac.Options.DeleteMemberDays,
						Toll = xac.Options.MembersRemoved
					};
					break;

				case AuditLogActionType.Ban:
				case AuditLogActionType.Unban:
					entry = new DiscordAuditLogBanEntry
					{
						Target = amd.TryGetValue(xac.TargetId.Value, out var unbanMember) ? unbanMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, GuildId = this.Id }
					};
					break;

				case AuditLogActionType.MemberUpdate:
				case AuditLogActionType.MemberRoleUpdate:
					entry = new DiscordAuditLogMemberUpdateEntry
					{
						Target = amd.TryGetValue(xac.TargetId.Value, out var roleUpdMember) ? roleUpdMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, GuildId = this.Id }
					};

					var entrymbu = entry as DiscordAuditLogMemberUpdateEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "nick":
								entrymbu.NicknameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "deaf":
								entrymbu.DeafenChange = new PropertyChange<bool?>
								{
									Before = (bool?)xc.OldValue,
									After = (bool?)xc.NewValue
								};
								break;

							case "mute":
								entrymbu.MuteChange = new PropertyChange<bool?>
								{
									Before = (bool?)xc.OldValue,
									After = (bool?)xc.NewValue
								};
								break;
							case "communication_disabled_until":
								entrymbu.CommunicationDisabledUntilChange = new PropertyChange<DateTime?>
								{
									Before = (DateTime?)xc.OldValue,
									After = (DateTime?)xc.NewValue
								};
								break;

							case "$add":
								entrymbu.AddedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"]).Select(this.GetRole).ToList());
								break;

							case "$remove":
								entrymbu.RemovedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(xo => (ulong)xo["id"]).Select(this.GetRole).ToList());
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in member update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.RoleCreate:
				case AuditLogActionType.RoleDelete:
				case AuditLogActionType.RoleUpdate:
					entry = new DiscordAuditLogRoleUpdateEntry
					{
						Target = this.GetRole(xac.TargetId.Value) ?? new DiscordRole { Id = xac.TargetId.Value, Discord = this.Discord }
					};

					var entryrol = entry as DiscordAuditLogRoleUpdateEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "name":
								entryrol.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "color":
								p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
								p2 = int.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

								entryrol.ColorChange = new PropertyChange<int?>
								{
									Before = p1 ? t3 : null,
									After = p2 ? t4 : null
								};
								break;

							case "permissions":
								entryrol.PermissionChange = new PropertyChange<Permissions?>
								{
									Before = xc.OldValue != null ? (Permissions?)long.Parse((string)xc.OldValue) : null,
									After = xc.NewValue != null ? (Permissions?)long.Parse((string)xc.NewValue) : null
								};
								break;

							case "position":
								entryrol.PositionChange = new PropertyChange<int?>
								{
									Before = xc.OldValue != null ? (int?)(long)xc.OldValue : null,
									After = xc.NewValue != null ? (int?)(long)xc.NewValue : null,
								};
								break;

							case "mentionable":
								entryrol.MentionableChange = new PropertyChange<bool?>
								{
									Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
									After = xc.NewValue != null ? (bool?)xc.NewValue : null
								};
								break;

							case "hoist":
								entryrol.HoistChange = new PropertyChange<bool?>
								{
									Before = (bool?)xc.OldValue,
									After = (bool?)xc.NewValue
								};
								break;

							case "icon_hash":
								entryrol.IconHashChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in role update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.InviteCreate:
				case AuditLogActionType.InviteDelete:
				case AuditLogActionType.InviteUpdate:
					entry = new DiscordAuditLogInviteEntry();

					var inv = new DiscordInvite
					{
						Discord = this.Discord,
						Guild = new DiscordInviteGuild
						{
							Discord = this.Discord,
							Id = this.Id,
							Name = this.Name,
							SplashHash = this.SplashHash
						}
					};

					var entryinv = entry as DiscordAuditLogInviteEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "max_age":
								p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
								p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

								entryinv.MaxAgeChange = new PropertyChange<int?>
								{
									Before = p1 ? t3 : null,
									After = p2 ? t4 : null
								};
								break;

							case "code":
								inv.Code = xc.OldValueString ?? xc.NewValueString;

								entryinv.CodeChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "temporary":
								entryinv.TemporaryChange = new PropertyChange<bool?>
								{
									Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
									After = xc.NewValue != null ? (bool?)xc.NewValue : null
								};
								break;

							case "inviter_id":
								p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entryinv.InviterChange = new PropertyChange<DiscordMember>
								{
									Before = amd.TryGetValue(t1, out var propBeforeMember) ? propBeforeMember : new DiscordMember { Id = t1, Discord = this.Discord, GuildId = this.Id },
									After = amd.TryGetValue(t2, out var propAfterMember) ? propAfterMember : new DiscordMember { Id = t1, Discord = this.Discord, GuildId = this.Id },
								};
								break;

							case "channel_id":
								p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entryinv.ChannelChange = new PropertyChange<DiscordChannel>
								{
									Before = p1 ? this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null,
									After = p2 ? this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null
								};

								var ch = entryinv.ChannelChange.Before ?? entryinv.ChannelChange.After;
								var cht = ch?.Type;
								inv.Channel = new DiscordInviteChannel
								{
									Discord = this.Discord,
									Id = p1 ? t1 : t2,
									Name = ch?.Name,
									Type = cht != null ? cht.Value : ChannelType.Unknown
								};
								break;

							case "uses":
								p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
								p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

								entryinv.UsesChange = new PropertyChange<int?>
								{
									Before = p1 ? t3 : null,
									After = p2 ? t4 : null
								};
								break;

							case "max_uses":
								p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
								p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

								entryinv.MaxUsesChange = new PropertyChange<int?>
								{
									Before = p1 ? t3 : null,
									After = p2 ? t4 : null
								};
								break;

							// TODO: Add changes for target application

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in invite update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}

					entryinv.Target = inv;
					break;

				case AuditLogActionType.WebhookCreate:
				case AuditLogActionType.WebhookDelete:
				case AuditLogActionType.WebhookUpdate:
					entry = new DiscordAuditLogWebhookEntry
					{
						Target = ahd.TryGetValue(xac.TargetId.Value, out var webhook) ? webhook : new DiscordWebhook { Id = xac.TargetId.Value, Discord = this.Discord }
					};

					var entrywhk = entry as DiscordAuditLogWebhookEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "application_id": // ???
								p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entrywhk.IdChange = new PropertyChange<ulong?>
								{
									Before = p1 ? t1 : null,
									After = p2 ? t2 : null
								};
								break;

							case "name":
								entrywhk.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "channel_id":
								p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entrywhk.ChannelChange = new PropertyChange<DiscordChannel>
								{
									Before = p1 ? this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null,
									After = p2 ? this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null
								};
								break;

							case "type": // ???
								p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
								p2 = int.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

								entrywhk.TypeChange = new PropertyChange<int?>
								{
									Before = p1 ? t3 : null,
									After = p2 ? t4 : null
								};
								break;

							case "avatar_hash":
								entrywhk.AvatarHashChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in webhook update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.EmojiCreate:
				case AuditLogActionType.EmojiDelete:
				case AuditLogActionType.EmojiUpdate:
					entry = new DiscordAuditLogEmojiEntry
					{
						Target = this.EmojisInternal.TryGetValue(xac.TargetId.Value, out var target) ? target : new DiscordEmoji { Id = xac.TargetId.Value, Discord = this.Discord }
					};

					var entryemo = entry as DiscordAuditLogEmojiEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "name":
								entryemo.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in emote update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.StageInstanceCreate:
				case AuditLogActionType.StageInstanceDelete:
				case AuditLogActionType.StageInstanceUpdate:
					entry = new DiscordAuditLogStageEntry
					{
						Target = this.StageInstancesInternal.TryGetValue(xac.TargetId.Value, out var stage) ? stage : new DiscordStageInstance { Id = xac.TargetId.Value, Discord = this.Discord }
					};

					var entrysta = entry as DiscordAuditLogStageEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "topic":
								entrysta.TopicChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;
							case "privacy_level":
#pragma warning disable CS0612 // Type or member is obsolete
								entrysta.PrivacyLevelChange = new PropertyChange<StagePrivacyLevel?>
								{
									Before = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5) ? (StagePrivacyLevel?)t5 : null,
									After = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6) ? (StagePrivacyLevel?)t6 : null,
								};
#pragma warning restore CS0612 // Type or member is obsolete
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in stage instance update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.StickerCreate:
				case AuditLogActionType.StickerDelete:
				case AuditLogActionType.StickerUpdate:
					entry = new DiscordAuditLogStickerEntry
					{
						Target = this.StickersInternal.TryGetValue(xac.TargetId.Value, out var sticker) ? sticker : new DiscordSticker { Id = xac.TargetId.Value, Discord = this.Discord }
					};

					var entrysti = entry as DiscordAuditLogStickerEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "name":
								entrysti.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;
							case "description":
								entrysti.DescriptionChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;
							case "tags":
								entrysti.TagsChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;
							case "guild_id":
								entrysti.GuildIdChange = new PropertyChange<ulong?>
								{
									Before = ulong.TryParse(xc.OldValueString, out var ogid) ? ogid : null,
									After = ulong.TryParse(xc.NewValueString, out var ngid) ? ngid : null
								};
								break;
							case "available":
								entrysti.AvailabilityChange = new PropertyChange<bool?>
								{
									Before = (bool?)xc.OldValue,
									After = (bool?)xc.NewValue,
								};
								break;
							case "asset":
								entrysti.AssetChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;
							case "id":
								entrysti.IdChange = new PropertyChange<ulong?>
								{
									Before = ulong.TryParse(xc.OldValueString, out var oid) ? oid : null,
									After = ulong.TryParse(xc.NewValueString, out var nid) ? nid : null
								};
								break;
							case "type":
								p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
								p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);
								entrysti.TypeChange = new PropertyChange<StickerType?>
								{
									Before = p1 ? (StickerType?)t5 : null,
									After = p2 ? (StickerType?)t6 : null
								};
								break;
							case "format_type":
								p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
								p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);
								entrysti.FormatChange = new PropertyChange<StickerFormat?>
								{
									Before = p1 ? (StickerFormat?)t5 : null,
									After = p2 ? (StickerFormat?)t6 : null
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in sticker update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;



				case AuditLogActionType.MessageDelete:
				case AuditLogActionType.MessageBulkDelete:
				{
					entry = new DiscordAuditLogMessageEntry();

					var entrymsg = entry as DiscordAuditLogMessageEntry;

					if (xac.Options != null)
					{
						entrymsg.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
						entrymsg.MessageCount = xac.Options.Count;
					}

					if (entrymsg.Channel != null)
					{
						entrymsg.Target = this.Discord is DiscordClient dc
							&& dc.MessageCache != null
							&& dc.MessageCache.TryGet(xm => xm.Id == xac.TargetId.Value && xm.ChannelId == entrymsg.Channel.Id, out var msg)
							? msg
							: new DiscordMessage { Discord = this.Discord, Id = xac.TargetId.Value };
					}
					break;
				}

				case AuditLogActionType.MessagePin:
				case AuditLogActionType.MessageUnpin:
				{
					entry = new DiscordAuditLogMessagePinEntry();

					var entrypin = entry as DiscordAuditLogMessagePinEntry;

					if (this.Discord is not DiscordClient dc)
					{
						break;
					}

					if (xac.Options != null)
					{
						DiscordMessage message = default;
						dc.MessageCache?.TryGet(x => x.Id == xac.Options.MessageId && x.ChannelId == xac.Options.ChannelId, out message);

						entrypin.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
						entrypin.Message = message ?? new DiscordMessage { Id = xac.Options.MessageId, Discord = this.Discord };
					}

					if (xac.TargetId.HasValue)
					{
						dc.UserCache.TryGetValue(xac.TargetId.Value, out var user);
						entrypin.Target = user ?? new DiscordUser { Id = user.Id, Discord = this.Discord };
					}

					break;
				}

				case AuditLogActionType.BotAdd:
				{
					entry = new DiscordAuditLogBotAddEntry();

					if (!(this.Discord is DiscordClient dc && xac.TargetId.HasValue))
					{
						break;
					}

					dc.UserCache.TryGetValue(xac.TargetId.Value, out var bot);
					(entry as DiscordAuditLogBotAddEntry).TargetBot = bot ?? new DiscordUser { Id = xac.TargetId.Value, Discord = this.Discord };

					break;
				}

				case AuditLogActionType.MemberMove:
					entry = new DiscordAuditLogMemberMoveEntry();

					if (xac.Options == null)
					{
						break;
					}

					var moveentry = entry as DiscordAuditLogMemberMoveEntry;

					moveentry.UserCount = xac.Options.Count;
					moveentry.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
					break;

				case AuditLogActionType.MemberDisconnect:
					entry = new DiscordAuditLogMemberDisconnectEntry
					{
						UserCount = xac.Options?.Count ?? 0
					};
					break;

				case AuditLogActionType.IntegrationCreate:
				case AuditLogActionType.IntegrationDelete:
				case AuditLogActionType.IntegrationUpdate:
					entry = new DiscordAuditLogIntegrationEntry();

					var integentry = entry as DiscordAuditLogIntegrationEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "type":
								integentry.Type = new PropertyChange<string>()
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;
							case "enable_emoticons":
								integentry.EnableEmoticons = new PropertyChange<bool?>
								{
									Before = (bool?)xc.OldValue,
									After = (bool?)xc.NewValue
								};
								break;
							case "expire_behavior":
								integentry.ExpireBehavior = new PropertyChange<int?>
								{
									Before = (int?)xc.OldValue,
									After = (int?)xc.NewValue
								};
								break;
							case "expire_grace_period":
								integentry.ExpireBehavior = new PropertyChange<int?>
								{
									Before = (int?)xc.OldValue,
									After = (int?)xc.NewValue
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in integration update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				case AuditLogActionType.ThreadCreate:
				case AuditLogActionType.ThreadDelete:
				case AuditLogActionType.ThreadUpdate:
					entry = new DiscordAuditLogThreadEntry
					{
						Target = this.ThreadsInternal.TryGetValue(xac.TargetId.Value, out var thread) ? thread : new DiscordThreadChannel { Id = xac.TargetId.Value, Discord = this.Discord }
					};

					var entrythr = entry as DiscordAuditLogThreadEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "name":
								entrythr.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "type":
								p1 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
								p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

								entrythr.TypeChange = new PropertyChange<ChannelType?>
								{
									Before = p1 ? (ChannelType?)t1 : null,
									After = p2 ? (ChannelType?)t2 : null
								};
								break;

							case "archived":
								entrythr.ArchivedChange = new PropertyChange<bool?>
								{
									Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
									After = xc.NewValue != null ? (bool?)xc.NewValue : null
								};
								break;

							case "locked":
								entrythr.LockedChange = new PropertyChange<bool?>
								{
									Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
									After = xc.NewValue != null ? (bool?)xc.NewValue : null
								};
								break;

							case "invitable":
								entrythr.InvitableChange = new PropertyChange<bool?>
								{
									Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
									After = xc.NewValue != null ? (bool?)xc.NewValue : null
								};
								break;

							case "auto_archive_duration":
								p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
								p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

								entrythr.AutoArchiveDurationChange = new PropertyChange<ThreadAutoArchiveDuration?>
								{
									Before = p1 ? (ThreadAutoArchiveDuration?)t5 : null,
									After = p2 ? (ThreadAutoArchiveDuration?)t6 : null
								};
								break;

							case "rate_limit_per_user":
								entrythr.PerUserRateLimitChange = new PropertyChange<int?>
								{
									Before = (int?)(long?)xc.OldValue,
									After = (int?)(long?)xc.NewValue
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in thread update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;


				case AuditLogActionType.GuildScheduledEventCreate:
				case AuditLogActionType.GuildScheduledEventDelete:
				case AuditLogActionType.GuildScheduledEventUpdate:
					entry = new DiscordAuditLogGuildScheduledEventEntry
					{
						Target = this.ScheduledEventsInternal.TryGetValue(xac.TargetId.Value, out var scheduledEvent) ? scheduledEvent : new DiscordScheduledEvent { Id = xac.TargetId.Value, Discord = this.Discord }
					};

					var entryse = entry as DiscordAuditLogGuildScheduledEventEntry;
					foreach (var xc in xac.Changes)
					{
						switch (xc.Key.ToLowerInvariant())
						{
							case "channel_id":
								entryse.ChannelIdChange = new PropertyChange<ulong?>
								{
									Before = ulong.TryParse(xc.OldValueString, out var ogid) ? ogid : null,
									After = ulong.TryParse(xc.NewValueString, out var ngid) ? ngid : null
								};
								break;

							case "name":
								entryse.NameChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "description":
								entryse.DescriptionChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "location":
								entryse.LocationChange = new PropertyChange<string>
								{
									Before = xc.OldValueString,
									After = xc.NewValueString
								};
								break;

							case "privacy_level":
								p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
								p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

								entryse.PrivacyLevelChange = new PropertyChange<ScheduledEventPrivacyLevel?>
								{
									Before = p1 ? (ScheduledEventPrivacyLevel?)t5 : null,
									After = p2 ? (ScheduledEventPrivacyLevel?)t6 : null
								};
								break;

							case "entity_type":
								p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
								p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

								entryse.EntityTypeChange = new PropertyChange<ScheduledEventEntityType?>
								{
									Before = p1 ? (ScheduledEventEntityType?)t5 : null,
									After = p2 ? (ScheduledEventEntityType?)t6 : null
								};
								break;

							case "status":
								p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
								p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

								entryse.StatusChange = new PropertyChange<ScheduledEventStatus?>
								{
									Before = p1 ? (ScheduledEventStatus?)t5 : null,
									After = p2 ? (ScheduledEventStatus?)t6 : null
								};
								break;

							default:
								this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in scheduled event update: {0} - this should be reported to library developers", xc.Key);
								break;
						}
					}
					break;

				// TODO: Handle ApplicationCommandPermissionUpdate
				case AuditLogActionType.ApplicationCommandPermissionUpdate:
					break;

				// TODO: Implement auto mod audit log
				case AuditLogActionType.AutoModerationRuleCreate:
				case AuditLogActionType.AutoModerationRuleUpdate:
				case AuditLogActionType.AutoModerationRuleDelete:
					break;

				case AuditLogActionType.AutoModerationBlockMessage:
					break;

				case AuditLogActionType.AUtoModerationFlagMessage:
					break;

				default:
					this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown audit log action type: {0} - this should be reported to library developers", (int)xac.ActionType);
					break;
			}

			if (entry == null)
				continue;

			entry.ActionCategory = xac.ActionType switch
			{
				AuditLogActionType.ChannelCreate or AuditLogActionType.EmojiCreate or AuditLogActionType.InviteCreate or AuditLogActionType.OverwriteCreate or AuditLogActionType.RoleCreate or AuditLogActionType.WebhookCreate or AuditLogActionType.IntegrationCreate or AuditLogActionType.StickerCreate or AuditLogActionType.StageInstanceCreate or AuditLogActionType.ThreadCreate or AuditLogActionType.GuildScheduledEventCreate or AuditLogActionType.AutoModerationRuleCreate => AuditLogActionCategory.Create,
				AuditLogActionType.ChannelDelete or AuditLogActionType.EmojiDelete or AuditLogActionType.InviteDelete or AuditLogActionType.MessageDelete or AuditLogActionType.MessageBulkDelete or AuditLogActionType.OverwriteDelete or AuditLogActionType.RoleDelete or AuditLogActionType.WebhookDelete or AuditLogActionType.IntegrationDelete or AuditLogActionType.StickerDelete or AuditLogActionType.StageInstanceDelete or AuditLogActionType.ThreadDelete or AuditLogActionType.GuildScheduledEventDelete or AuditLogActionType.AutoModerationRuleDelete => AuditLogActionCategory.Delete,
				AuditLogActionType.ChannelUpdate or AuditLogActionType.EmojiUpdate or AuditLogActionType.InviteUpdate or AuditLogActionType.MemberRoleUpdate or AuditLogActionType.MemberUpdate or AuditLogActionType.OverwriteUpdate or AuditLogActionType.RoleUpdate or AuditLogActionType.WebhookUpdate or AuditLogActionType.IntegrationUpdate or AuditLogActionType.StickerUpdate or AuditLogActionType.StageInstanceUpdate or AuditLogActionType.ThreadUpdate or AuditLogActionType.GuildScheduledEventUpdate or AuditLogActionType.AutoModerationRuleUpdate => AuditLogActionCategory.Update,
				_ => AuditLogActionCategory.Other,
			};
			entry.Discord = this.Discord;
			entry.ActionType = xac.ActionType;
			entry.Id = xac.Id;
			entry.Reason = xac.Reason;
			entry.UserResponsible = amd.Any() && amd.TryGetValue(xac.UserId, out var resp) ? resp : this.MembersInternal[xac.UserId];
			entries.Add(entry);
		}

		return new ReadOnlyCollection<DiscordAuditLogEntry>(entries);
	}
}
