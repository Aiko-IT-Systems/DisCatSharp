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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net
{
    /// <summary>
    /// Represents a discord api client.
    /// </summary>
    public sealed class DiscordApiClient
    {
        /// <summary>
        /// The audit log reason header name.
        /// </summary>
        private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

        /// <summary>
        /// Gets the discord client.
        /// </summary>
        internal BaseDiscordClient Discord { get; }
        /// <summary>
        /// Gets the rest client.
        /// </summary>
        internal RestClient Rest { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordApiClient"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        internal DiscordApiClient(BaseDiscordClient client)
        {
            this.Discord = client;
            this.Rest = new RestClient(client);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordApiClient"/> class.
        /// </summary>
        /// <param name="proxy">The proxy.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="useRelativeRateLimit">If true, use relative rate limit.</param>
        /// <param name="logger">The logger.</param>
        internal DiscordApiClient(IWebProxy proxy, TimeSpan timeout, bool useRelativeRateLimit, ILogger logger) // This is for meta-clients, such as the webhook client
        {
            this.Rest = new RestClient(proxy, timeout, useRelativeRateLimit, logger);
        }

        /// <summary>
        /// Builds the query string.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="post">If true, post.</param>
        /// <returns>A string.</returns>
        private static string BuildQueryString(IDictionary<string, string> values, bool post = false)
        {
            if (values == null || values.Count == 0)
                return string.Empty;

            var vals_collection = values.Select(xkvp =>
                $"{WebUtility.UrlEncode(xkvp.Key)}={WebUtility.UrlEncode(xkvp.Value)}");
            var vals = string.Join("&", vals_collection);

            return !post ? $"?{vals}" : vals;
        }

        /// <summary>
        /// Prepares the message.
        /// </summary>
        /// <param name="msg_raw">The msg_raw.</param>
        /// <returns>A DiscordMessage.</returns>
        private DiscordMessage PrepareMessage(JToken msg_raw)
        {
            var author = msg_raw["author"].ToObject<TransportUser>();
            var ret = msg_raw.ToDiscordObject<DiscordMessage>();
            ret.Discord = this.Discord;

            this.PopulateMessage(author, ret);

            var referencedMsg = msg_raw["referenced_message"];
            if (ret.MessageType == MessageType.Reply && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
            {
                author = referencedMsg["author"].ToObject<TransportUser>();
                ret.ReferencedMessage.Discord = this.Discord;
                this.PopulateMessage(author, ret.ReferencedMessage);
            }

            if (ret.Channel != null)
                return ret;

            var channel = !ret.GuildId.HasValue
                ? new DiscordDmChannel
                {
                    Id = ret.ChannelId,
                    Discord = this.Discord,
                    Type = ChannelType.Private
                }
                : new DiscordChannel
                {
                    Id = ret.ChannelId,
                    GuildId = ret.GuildId,
                    Discord = this.Discord
                };

            ret.Channel = channel;

            return ret;
        }

        /// <summary>
        /// Populates the message.
        /// </summary>
        /// <param name="author">The author.</param>
        /// <param name="ret">The ret.</param>
        private void PopulateMessage(TransportUser author, DiscordMessage ret)
        {
            var guild = ret.Channel?.Guild;

            //If this is a webhook, it shouldn't be in the user cache.
            if (author.IsBot && int.Parse(author.Discriminator) == 0)
            {
                ret.Author = new DiscordUser(author) { Discord = this.Discord };
            }
            else
            {
                if (!this.Discord.UserCache.TryGetValue(author.Id, out var usr))
                {
                    this.Discord.UserCache[author.Id] = usr = new DiscordUser(author) { Discord = this.Discord };
                }

                if (guild != null)
                {
                    if (!guild.Members.TryGetValue(author.Id, out var mbr))
                        mbr = new DiscordMember(usr) { Discord = this.Discord, _guild_id = guild.Id };
                    ret.Author = mbr;
                }
                else
                {
                    ret.Author = usr;
                }
            }

            ret.PopulateMentions();

            if (ret._reactions == null)
                ret._reactions = new List<DiscordReaction>();
            foreach (var xr in ret._reactions)
                xr.Emoji.Discord = this.Discord;
        }

        /// <summary>
        /// Dos the request async.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="bucket">The bucket.</param>
        /// <param name="url">The url.</param>
        /// <param name="method">The method.</param>
        /// <param name="route">The route.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
        /// <returns>A Task.</returns>
        private Task<RestResponse> DoRequestAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, string payload = null, double? ratelimitWaitOverride = null)
        {
            var req = new RestRequest(client, bucket, url, method, route, headers, payload, ratelimitWaitOverride);

            if (this.Discord != null)
                this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequestAsync(req);

            return req.WaitForCompletionAsync();
        }

        /// <summary>
        /// Dos the multipart async.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="bucket">The bucket.</param>
        /// <param name="url">The url.</param>
        /// <param name="method">The method.</param>
        /// <param name="route">The route.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="values">The values.</param>
        /// <param name="files">The files.</param>
        /// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
        /// <returns>A Task.</returns>
        private Task<RestResponse> DoMultipartAsync(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
            IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimitWaitOverride = null)
        {
            var req = new MultipartWebRequest(client, bucket, url, method, route, headers, values, files, ratelimitWaitOverride);

            if (this.Discord != null)
                this.Rest.ExecuteRequestAsync(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequestAsync(req);

            return req.WaitForCompletionAsync();
        }

        #region Guild

        /// <summary>
        /// Searches the members async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(ulong guild_id, string name, int? limit)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.SEARCH}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);
            var querydict = new Dictionary<string, string>
            {
                ["query"] = name,
                ["limit"] = limit.ToString()
            };
            var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JArray.Parse(res.Response);
            var tms = json.ToObject<IReadOnlyList<TransportMember>>();

            var mbrs = new List<DiscordMember>();
            foreach (var xtm in tms)
            {
                var usr = new DiscordUser(xtm.User) { Discord = this.Discord };

                this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
                {
                    old.Username = usr.Username;
                    old.Discord = usr.Discord;
                    old.AvatarHash = usr.AvatarHash;

                    return old;
                });

                mbrs.Add(new DiscordMember(xtm) { Discord = this.Discord, _guild_id = guild_id });
            }

            return mbrs;
        }

        /// <summary>
        /// Gets the guild ban async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordBan> GetGuildBanAsync(ulong guild_id, ulong user_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id, user_id}, out var url);
            var uri = Utilities.GetApiUriFor(url, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, uri, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JObject.Parse(res.Response);

            var ban = json.ToObject<DiscordBan>();

            return ban;
        }

        /// <summary>
        /// Creates the guild async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="region_id">The region_id.</param>
        /// <param name="iconb64">The iconb64.</param>
        /// <param name="verification_level">The verification_level.</param>
        /// <param name="default_message_notifications">The default_message_notifications.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuild> CreateGuildAsync(string name, string region_id, Optional<string> iconb64, VerificationLevel? verification_level,
            DefaultMessageNotifications? default_message_notifications)
        {
            var pld = new RestGuildCreatePayload
            {
                Name = name,
                RegionId = region_id,
                DefaultMessageNotifications = default_message_notifications,
                VerificationLevel = verification_level,
                IconBase64 = iconb64
            };

            var route = $"{Endpoints.GUILDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildCreateEventAsync(guild, raw_members, null).ConfigureAwait(false);
            return guild;
        }

        /// <summary>
        /// Creates the guild from template async.
        /// </summary>
        /// <param name="template_code">The template_code.</param>
        /// <param name="name">The name.</param>
        /// <param name="iconb64">The iconb64.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuild> CreateGuildFromTemplateAsync(string template_code, string name, Optional<string> iconb64)
        {
            var pld = new RestGuildCreateFromTemplatePayload
            {
                Name = name,
                IconBase64 = iconb64
            };

            var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { template_code }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var raw_members = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildCreateEventAsync(guild, raw_members, null).ConfigureAwait(false);
            return guild;
        }

        /// <summary>
        /// Deletes the guild async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task DeleteGuildAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

            if (this.Discord is DiscordClient dc)
            {
                var gld = dc._guilds[guild_id];
                await dc.OnGuildDeleteEventAsync(gld).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Modifies the guild async.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="name">The name.</param>
        /// <param name="region">The region.</param>
        /// <param name="verificationLevel">The verification level.</param>
        /// <param name="defaultMessageNotifications">The default message notifications.</param>
        /// <param name="mfaLevel">The mfa level.</param>
        /// <param name="explicitContentFilter">The explicit content filter.</param>
        /// <param name="afkChannelId">The afk channel id.</param>
        /// <param name="afkTimeout">The afk timeout.</param>
        /// <param name="iconb64">The iconb64.</param>
        /// <param name="ownerId">The owner id.</param>
        /// <param name="splashb64">The splashb64.</param>
        /// <param name="systemChannelId">The system channel id.</param>
        /// <param name="systemChannelFlags">The system channel flags.</param>
        /// <param name="description">The description.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuild> ModifyGuildAsync(ulong guildId, Optional<string> name,
            Optional<string> region, Optional<VerificationLevel> verificationLevel,
            Optional<DefaultMessageNotifications> defaultMessageNotifications, Optional<MfaLevel> mfaLevel,
            Optional<ExplicitContentFilter> explicitContentFilter, Optional<ulong?> afkChannelId,
            Optional<int> afkTimeout, Optional<string> iconb64, Optional<ulong> ownerId, Optional<string> splashb64,
            Optional<ulong?> systemChannelId, Optional<SystemChannelFlags> systemChannelFlags, Optional<string> description, string reason)
        {
            var pld = new RestGuildModifyPayload
            {
                Name = name,
                RegionId = region,
                VerificationLevel = verificationLevel,
                DefaultMessageNotifications = defaultMessageNotifications,
                MfaLevel = mfaLevel,
                ExplicitContentFilter = explicitContentFilter,
                AfkChannelId = afkChannelId,
                AfkTimeout = afkTimeout,
                IconBase64 = iconb64,
                SplashBase64 = splashb64,
                OwnerId = ownerId,
                SystemChannelId = systemChannelId,
                SystemChannelFlags = systemChannelFlags,
                Description = description
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id = guildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();
            foreach (var r in guild._roles.Values)
                r._guild_id = guild.Id;

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildUpdateEventAsync(guild, rawMembers).ConfigureAwait(false);
            return guild;
        }

        /// <summary>
        /// Gets the guild bans async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var bans_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordBan>>(res.Response).Select(xb =>
            {
                if (!this.Discord.TryGetCachedUserInternal(xb.RawUser.Id, out var usr))
                {
                    usr = new DiscordUser(xb.RawUser) { Discord = this.Discord };
                    usr = this.Discord.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
                    {
                        old.Username = usr.Username;
                        old.Discriminator = usr.Discriminator;
                        old.AvatarHash = usr.AvatarHash;
                        return old;
                    });
                }

                xb.User = usr;
                return xb;
            });
            var bans = new ReadOnlyCollection<DiscordBan>(new List<DiscordBan>(bans_raw));

            return bans;
        }

        /// <summary>
        /// Creates the guild ban async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="delete_message_days">The delete_message_days.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task CreateGuildBanAsync(ulong guild_id, ulong user_id, int delete_message_days, string reason)
        {
            if (delete_message_days < 0 || delete_message_days > 7)
                throw new ArgumentException("Delete message days must be a number between 0 and 7.", nameof(delete_message_days));

            var urlparams = new Dictionary<string, string>
            {
                ["delete_message_days"] = delete_message_days.ToString(CultureInfo.InvariantCulture)
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams), this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
        }

        /// <summary>
        /// Removes the guild ban async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGuildBanAsync(ulong guild_id, ulong user_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.BANS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Leaves the guild async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal Task LeaveGuildAsync(ulong guild_id)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Adds the guild member async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="access_token">The access_token.</param>
        /// <param name="nick">The nick.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="muted">If true, muted.</param>
        /// <param name="deafened">If true, deafened.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMember> AddGuildMemberAsync(ulong guild_id, ulong user_id, string access_token, string nick, IEnumerable<DiscordRole> roles, bool muted, bool deafened)
        {
            var pld = new RestGuildMemberAddPayload
            {
                AccessToken = access_token,
                Nickname = nick ?? "",
                Roles = roles ?? new List<DiscordRole>(),
                Deaf = deafened,
                Mute = muted
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm) { Discord = this.Discord, _guild_id = guild_id };
        }

        /// <summary>
        /// Lists the guild members async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="after">The after.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<TransportMember>> ListGuildMembersAsync(ulong guild_id, int? limit, ulong? after)
        {
            var urlparams = new Dictionary<string, string>();
            if (limit != null && limit > 0)
                urlparams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);
            if (after != null)
                urlparams["after"] = after.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var members_raw = JsonConvert.DeserializeObject<List<TransportMember>>(res.Response);
            return new ReadOnlyCollection<TransportMember>(members_raw);
        }

        /// <summary>
        /// Adds the guild member role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="role_id">The role_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task AddGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, user_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers);
        }

        /// <summary>
        /// Removes the guild member role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="role_id">The role_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGuildMemberRoleAsync(ulong guild_id, ulong user_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Modifies the guild channel position async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="pld">The pld.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildChannelPositionAsync(ulong guild_id, IEnumerable<RestGuildChannelReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Modifies the guild channel parent async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="pld">The pld.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildChannelParentAsync(ulong guild_id, IEnumerable<RestGuildChannelNewParentPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Detaches the guild channel parent async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="pld">The pld.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DetachGuildChannelParentAsync(ulong guild_id, IEnumerable<RestGuildChannelNoParentPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Modifies the guild role position async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="pld">The pld.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildRolePositionAsync(ulong guild_id, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets the audit logs async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="after">The after.</param>
        /// <param name="before">The before.</param>
        /// <param name="responsible">The responsible.</param>
        /// <param name="action_type">The action_type.</param>
        /// <returns>A Task.</returns>
        internal async Task<AuditLog> GetAuditLogsAsync(ulong guild_id, int limit, ulong? after, ulong? before, ulong? responsible, int? action_type)
        {
            var urlparams = new Dictionary<string, string>
            {
                ["limit"] = limit.ToString(CultureInfo.InvariantCulture)
            };
            if (after != null)
                urlparams["after"] = after?.ToString(CultureInfo.InvariantCulture);
            if (before != null)
                urlparams["before"] = before?.ToString(CultureInfo.InvariantCulture);
            if (responsible != null)
                urlparams["user_id"] = responsible?.ToString(CultureInfo.InvariantCulture);
            if (action_type != null)
                urlparams["action_type"] = action_type?.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.AUDIT_LOGS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var audit_log_data_raw = JsonConvert.DeserializeObject<AuditLog>(res.Response);

            return audit_log_data_raw;
        }

        /// <summary>
        /// Gets the guild vanity url async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> GetGuildVanityUrlAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VANITY_URL}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invite = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);

            return invite;
        }

        /// <summary>
        /// Gets the guild widget async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWidget> GetGuildWidgetAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET_JSON}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawChannels = (JArray)json["channels"];

            var ret = json.ToDiscordObject<DiscordWidget>();
            ret.Discord = this.Discord;
            ret.Guild = this.Discord.Guilds[guild_id];

            ret.Channels = ret.Guild == null
                ? rawChannels.Select(r => new DiscordChannel
                {
                    Id = (ulong)r["id"],
                    Name = r["name"].ToString(),
                    Position = (int)r["position"]
                }).ToList()
                : rawChannels.Select(r =>
                {
                    var c = ret.Guild.GetChannel((ulong)r["id"]);
                    c.Position = (int)r["position"];
                    return c;
                }).ToList();

            return ret;
        }

        /// <summary>
        /// Gets the guild widget settings async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[guild_id];

            return ret;
        }

        /// <summary>
        /// Modifies the guild widget settings async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="isEnabled">If true, is enabled.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong guild_id, bool? isEnabled, ulong? channelId, string reason)
        {
            var pld = new RestGuildWidgetSettingsPayload
            {
                Enabled = isEnabled,
                ChannelId = channelId
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WIDGET}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[guild_id];

            return ret;
        }

        /// <summary>
        /// Gets the guild templates async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var templates_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response);

            return new ReadOnlyCollection<DiscordGuildTemplate>(new List<DiscordGuildTemplate>(templates_raw));
        }

        /// <summary>
        /// Creates the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guild_id, string name, string description)
        {
            var pld = new RestGuildTemplateCreateOrModifyPayload
            {
                Name = name,
                Description = description
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return ret;
        }

        /// <summary>
        /// Syncs the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="template_code">The template_code.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guild_id, string template_code)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        /// <summary>
        /// Modifies the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="template_code">The template_code.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guild_id, string template_code, string name, string description)
        {
            var pld = new RestGuildTemplateCreateOrModifyPayload
            {
                Name = name,
                Description = description
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        /// <summary>
        /// Deletes the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="template_code">The template_code.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guild_id, string template_code)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, template_code }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

            var template_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return template_raw;
        }

        /// <summary>
        /// Gets the guild membership screening form async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var screening_raw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

            return screening_raw;
        }

        /// <summary>
        /// Modifies the guild membership screening form async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="enabled">The enabled.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong guild_id, Optional<bool> enabled, Optional<DiscordGuildMembershipScreeningField[]> fields, Optional<string> description)
        {
            var pld = new RestGuildMembershipScreeningFormModifyPayload
            {
                Enabled = enabled,
                Description = description,
                Fields = fields
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBER_VERIFICATION}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var screening_raw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

            return screening_raw;
        }

        /// <summary>
        /// Gets the guild welcome screen async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        /// <summary>
        /// Modifies the guild welcome screen async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="enabled">The enabled.</param>
        /// <param name="welcomeChannels">The welcome channels.</param>
        /// <param name="description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong guild_id, Optional<bool> enabled, Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> welcomeChannels, Optional<string> description)
        {
            var pld = new RestGuildWelcomeScreenModifyPayload
            {
                Enabled = enabled,
                WelcomeChannels = welcomeChannels,
                Description = description
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WELCOME_SCREEN}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        /// <summary>
        /// Updates the current user voice state async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="suppress">If true, suppress.</param>
        /// <param name="requestToSpeakTimestamp">The request to speak timestamp.</param>
        /// <returns>A Task.</returns>
        internal async Task UpdateCurrentUserVoiceStateAsync(ulong guild_id, ulong channelId, bool? suppress, DateTimeOffset? requestToSpeakTimestamp)
        {
            var pld = new RestGuildUpdateCurrentUserVoiceStatePayload
            {
                ChannelId = channelId,
                Suppress = suppress,
                RequestToSpeakTimestamp = requestToSpeakTimestamp
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/@me";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Updates the user voice state async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="suppress">If true, suppress.</param>
        /// <returns>A Task.</returns>
        internal async Task UpdateUserVoiceStateAsync(ulong guild_id, ulong user_id, ulong channelId, bool? suppress)
        {
            var pld = new RestGuildUpdateUserVoiceStatePayload
            {
                ChannelId = channelId,
                Suppress = suppress
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
        }
        #endregion

        #region Channel
        /// <summary>
        /// Creates the guild channel async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="topic">The topic.</param>
        /// <param name="bitrate">The bitrate.</param>
        /// <param name="user_limit">The user_limit.</param>
        /// <param name="overwrites">The overwrites.</param>
        /// <param name="nsfw">If true, nsfw.</param>
        /// <param name="perUserRateLimit">The per user rate limit.</param>
        /// <param name="qualityMode">The quality mode.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordChannel> CreateGuildChannelAsync(ulong guild_id, string name, ChannelType type, ulong? parent, Optional<string> topic, int? bitrate, int? user_limit, IEnumerable<DiscordOverwriteBuilder> overwrites, bool? nsfw, Optional<int?> perUserRateLimit, VideoQualityMode? qualityMode, string reason)
        {
            var restoverwrites = new List<DiscordRestOverwrite>();
            if (overwrites != null)
                foreach (var ow in overwrites)
                    restoverwrites.Add(ow.Build());

            var pld = new RestChannelCreatePayload
            {
                Name = name,
                Type = type,
                Parent = parent,
                Topic = topic,
                Bitrate = bitrate,
                UserLimit = user_limit,
                PermissionOverwrites = restoverwrites,
                Nsfw = nsfw,
                PerUserRateLimit = perUserRateLimit,
                QualityMode = qualityMode
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;
            foreach (var xo in ret._permissionOverwrites)
            {
                xo.Discord = this.Discord;
                xo._channel_id = ret.Id;
            }

            return ret;
        }

        /// <summary>
        /// Modifies the channel async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="position">The position.</param>
        /// <param name="topic">The topic.</param>
        /// <param name="nsfw">If true, nsfw.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="bitrate">The bitrate.</param>
        /// <param name="user_limit">The user_limit.</param>
        /// <param name="perUserRateLimit">The per user rate limit.</param>
        /// <param name="rtcRegion">The rtc region.</param>
        /// <param name="qualityMode">The quality mode.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyChannelAsync(ulong channel_id, string name, int? position, Optional<string> topic, bool? nsfw, Optional<ulong?> parent, int? bitrate, int? user_limit, Optional<int?> perUserRateLimit, Optional<string> rtcRegion, VideoQualityMode? qualityMode, string reason)
        {
            var pld = new RestChannelModifyPayload
            {
                Name = name,
                Position = position,
                Topic = topic,
                Nsfw = nsfw,
                Parent = parent,
                Bitrate = bitrate,
                UserLimit = user_limit,
                PerUserRateLimit = perUserRateLimit,
                RtcRegion = rtcRegion,
                QualityMode = qualityMode,
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets the channel async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordChannel> GetChannelAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;
            foreach (var xo in ret._permissionOverwrites)
            {
                xo.Discord = this.Discord;
                xo._channel_id = ret.Id;
            }

            return ret;
        }

        /// <summary>
        /// Deletes the channel async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteChannelAsync(ulong channel_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Gets the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> GetMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Creates the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="content">The content.</param>
        /// <param name="embeds">The embeds.</param>
        /// <param name="sticker">The sticker.</param>
        /// <param name="replyMessageId">The reply message id.</param>
        /// <param name="mentionReply">If true, mention reply.</param>
        /// <param name="failOnInvalidReply">If true, fail on invalid reply.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, string content, IEnumerable<DiscordEmbed> embeds, DiscordSticker sticker, ulong? replyMessageId, bool mentionReply, bool failOnInvalidReply)
        {
            if (content != null && content.Length > 2000)
                throw new ArgumentException("Message content length cannot exceed 2000 characters.");

            if (!embeds?.Any() ?? true)
            {
                if (content == null && sticker == null)
                    throw new ArgumentException("You must specify message content, a sticker or an embed.");
                if (content.Length == 0)
                    throw new ArgumentException("Message content must not be empty.");
            }

            if (embeds != null)
                foreach (var embed in embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = content != null,
                Content = content,
                StickersIds = sticker is null ? Array.Empty<ulong>() : new[] {sticker.Id},
                IsTTS = false,
                HasEmbed = embeds?.Any() ?? false,
                Embeds = embeds
            };

            if (replyMessageId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = replyMessageId, FailIfNotExists = failOnInvalidReply };

            if (replyMessageId != null)
                pld.Mentions = new DiscordMentions(Mentions.All, true, mentionReply);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Creates the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CreateMessageAsync(ulong channel_id, DiscordMessageBuilder builder)
        {
            builder.Validate();

            if (builder.Embeds != null)
                foreach (var embed in builder.Embeds)
                    if (embed?.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = builder.Content != null,
                Content = builder.Content,
                StickersIds = builder.Sticker is null ? Array.Empty<ulong>() : new[] {builder.Sticker.Id},
                IsTTS = builder.IsTTS,
                HasEmbed = builder.Embeds != null,
                Embeds = builder.Embeds,
                Components = builder.Components
            };

            if (builder.ReplyId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = builder.ReplyId, FailIfNotExists = builder.FailOnInvalidReply };

            if (builder.Mentions != null || builder.ReplyId != null)
                pld.Mentions = new DiscordMentions(builder.Mentions ?? Mentions.All, builder.Mentions?.Any() ?? false, builder.MentionOnReply);

            if (builder.Files.Count == 0)
            {
                var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
                var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

                var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
                var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));
                return ret;
            }
            else
            {
                var values = new Dictionary<string, string>
                {
                    ["payload_json"] = DiscordJson.SerializeObject(pld)
                };

                var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
                var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

                var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
                var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));

                foreach (var file in builder._files.Where(x => x.ResetPositionTo.HasValue))
                {
                    file.Stream.Position = file.ResetPositionTo.Value;
                }

                return ret;
            }
        }

        /// <summary>
        /// Gets the guild channels async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var channels_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            foreach (var ret in channels_raw)
                foreach (var xo in ret._permissionOverwrites)
                {
                    xo.Discord = this.Discord;
                    xo._channel_id = ret.Id;
                }

            return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channels_raw));
        }

        /// <summary>
        /// Creates the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="topic">The topic.</param>
        /// <param name="privacy_level">The privacy_level.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channel_id, string topic, StagePrivacyLevel privacy_level, string reason)
        {
            var pld = new RestStageInstanceCreatePayload
            {
                ChannelId = channel_id,
                Topic = topic,
                PrivacyLevel = privacy_level
            };

            var route = $"{Endpoints.STAGE_INSTANCES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

            return stageInstance;
        }

        /// <summary>
        /// Gets the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordStageInstance> GetStageInstanceAsync(ulong channel_id)
        {
            var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

            return stageInstance;
        }

        /// <summary>
        /// Modifies the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="topic">The topic.</param>
        /// <param name="privacy_level">The privacy_level.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyStageInstanceAsync(ulong channel_id, Optional<string> topic, Optional<StagePrivacyLevel> privacy_level, string reason)
        {
            var pld = new RestStageInstanceModifyPayload
            {
                Topic = topic,
                PrivacyLevel = privacy_level
            };

            var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id }, out var path);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Deletes the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteStageInstanceAsync(ulong channel_id, string reason)
        {
            var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id }, out var path);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Gets the channel messages async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="before">The before.</param>
        /// <param name="after">The after.</param>
        /// <param name="around">The around.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channel_id, int limit, ulong? before, ulong? after, ulong? around)
        {
            var urlparams = new Dictionary<string, string>();
            if (around != null)
                urlparams["around"] = around?.ToString(CultureInfo.InvariantCulture);
            if (before != null)
                urlparams["before"] = before?.ToString(CultureInfo.InvariantCulture);
            if (after != null)
                urlparams["after"] = after?.ToString(CultureInfo.InvariantCulture);
            if (limit > 0)
                urlparams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var msgs_raw = JArray.Parse(res.Response);
            var msgs = new List<DiscordMessage>();
            foreach (var xj in msgs_raw)
                msgs.Add(this.PrepareMessage(xj));

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        /// <summary>
        /// Gets the channel message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> GetChannelMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Edits the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="content">The content.</param>
        /// <param name="embeds">The embeds.</param>
        /// <param name="mentions">The mentions.</param>
        /// <param name="components">The components.</param>
        /// <param name="suppress_embed">The suppress_embed.</param>
        /// <param name="files">The files.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> EditMessageAsync(ulong channel_id, ulong message_id, Optional<string> content, Optional<IEnumerable<DiscordEmbed>> embeds, IEnumerable<IMention> mentions, IReadOnlyList<DiscordActionRowComponent> components, Optional<bool> suppress_embed, IReadOnlyCollection<DiscordMessageFile> files)
        {
            if (embeds.HasValue && embeds.Value != null)
                foreach (var embed in embeds.Value)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageEditPayload
            {
                HasContent = content.HasValue,
                Content = content.HasValue ? (string)content : null,
                HasEmbed = embeds.HasValue && (embeds.Value?.Any() ?? false),
                Embeds = embeds.HasValue && (embeds.Value?.Any() ?? false) ? embeds.Value : null,
                Components = components,
                Flags = suppress_embed.HasValue ? (bool)suppress_embed ? MessageFlags.SuppressedEmbeds : null : null
            };

            if (mentions != null)
                pld.Mentions = new DiscordMentions(mentions);

            var values = new Dictionary<string, string>
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: files).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            foreach (var file in files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo.Value;
            }

            return ret;
        }

        /// <summary>
        /// Deletes the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteMessageAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Deletes the messages async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_ids">The message_ids.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteMessagesAsync(ulong channel_id, IEnumerable<ulong> message_ids, string reason)
        {
            var pld = new RestChannelMessageBulkDeletePayload
            {
                Messages = message_ids
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}{Endpoints.BULK_DELETE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets the channel invites async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }

        /// <summary>
        /// Creates the channel invite async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="max_age">The max_age.</param>
        /// <param name="max_uses">The max_uses.</param>
        /// <param name="target_type">The target_type.</param>
        /// <param name="target_application">The target_application.</param>
        /// <param name="temporary">If true, temporary.</param>
        /// <param name="unique">If true, unique.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> CreateChannelInviteAsync(ulong channel_id, int max_age, int max_uses, TargetType? target_type, TargetActivity? target_application, bool temporary, bool unique, string reason)
        {
            var pld = new RestChannelInviteCreatePayload
            {
                MaxAge = max_age,
                MaxUses = max_uses,
                TargetType = target_type,
                TargetApplication = target_application,
                Temporary = temporary,
                Unique = unique
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the channel permission async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="overwrite_id">The overwrite_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteChannelPermissionAsync(ulong channel_id, ulong overwrite_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, overwrite_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Edits the channel permissions async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="overwrite_id">The overwrite_id.</param>
        /// <param name="allow">The allow.</param>
        /// <param name="deny">The deny.</param>
        /// <param name="type">The type.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task EditChannelPermissionsAsync(ulong channel_id, ulong overwrite_id, Permissions allow, Permissions deny, string type, string reason)
        {
            var pld = new RestChannelPermissionEditPayload
            {
                Type = type,
                Allow = allow & PermissionMethods.FULL_PERMS,
                Deny = deny & PermissionMethods.FULL_PERMS
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PERMISSIONS}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, overwrite_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Triggers the typing async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal Task TriggerTypingAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.TYPING}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
        }

        /// <summary>
        /// Gets the pinned messages async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var msgs_raw = JArray.Parse(res.Response);
            var msgs = new List<DiscordMessage>();
            foreach (var xj in msgs_raw)
                msgs.Add(this.PrepareMessage(xj));

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        /// <summary>
        /// Pins the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task PinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        /// <summary>
        /// Unpins the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task UnpinMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.PINS}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Adds the group dm recipient async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="access_token">The access_token.</param>
        /// <param name="nickname">The nickname.</param>
        /// <returns>A Task.</returns>
        internal Task AddGroupDmRecipientAsync(ulong channel_id, ulong user_id, string access_token, string nickname)
        {
            var pld = new RestChannelGroupDmRecipientAddPayload
            {
                AccessToken = access_token,
                Nickname = nickname
            };

            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Removes the group dm recipient async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGroupDmRecipientAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}/:channel_id{Endpoints.RECIPIENTS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Creates the group dm async.
        /// </summary>
        /// <param name="access_tokens">The access_tokens.</param>
        /// <param name="nicks">The nicks.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> access_tokens, IDictionary<ulong, string> nicks)
        {
            var pld = new RestUserGroupDmCreatePayload
            {
                AccessTokens = access_tokens,
                Nicknames = nicks
            };

            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Creates the dm async.
        /// </summary>
        /// <param name="recipient_id">The recipient_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordDmChannel> CreateDmAsync(ulong recipient_id)
        {
            var pld = new RestUserDmCreatePayload
            {
                Recipient = recipient_id
            };

            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CHANNELS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Follows the channel async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="webhook_channel_id">The webhook_channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordFollowedChannel> FollowChannelAsync(ulong channel_id, ulong webhook_channel_id)
        {
            var pld = new FollowedChannelAddPayload
            {
                WebhookChannelId = webhook_channel_id
            };

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.FOLLOWERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<DiscordFollowedChannel>(response.Response);
        }

        /// <summary>
        /// Crossposts the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CrosspostMessageAsync(ulong channel_id, ulong message_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.CROSSPOST}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var response = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        #endregion

        #region Member
        /// <summary>
        /// Gets the current user async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal Task<DiscordUser> GetCurrentUserAsync()
            => this.GetUserAsync("@me");

        /// <summary>
        /// Gets the user async.
        /// </summary>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordUser> GetUserAsync(ulong user_id)
            => this.GetUserAsync(user_id.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the user async.
        /// </summary>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordUser> GetUserAsync(string user_id)
        {
            var route = $"{Endpoints.USERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var duser = new DiscordUser(user_raw) { Discord = this.Discord };

            return duser;
        }

        /// <summary>
        /// Gets the guild member async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMember> GetGuildMemberAsync(ulong guild_id, ulong user_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            var usr = new DiscordUser(tm.User) { Discord = this.Discord };
            usr = this.Discord.UserCache.AddOrUpdate(tm.User.Id, usr, (id, old) =>
            {
                old.Username = usr.Username;
                old.Discriminator = usr.Discriminator;
                old.AvatarHash = usr.AvatarHash;
                return old;
            });

            return new DiscordMember(tm)
            {
                Discord = this.Discord,
                _guild_id = guild_id
            };
        }

        /// <summary>
        /// Removes the guild member async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGuildMemberAsync(ulong guild_id, ulong user_id, string reason)
        {
            var urlparams = new Dictionary<string, string>();
            if (reason != null)
                urlparams["reason"] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams), this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Modifies the current user async.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <returns>A Task.</returns>
        internal async Task<TransportUser> ModifyCurrentUserAsync(string username, Optional<string> base64_avatar)
        {
            var pld = new RestUserUpdateCurrentPayload
            {
                Username = username,
                AvatarBase64 = base64_avatar.HasValue ? base64_avatar.Value : null,
                AvatarSet = base64_avatar.HasValue
            };

            var route = $"{Endpoints.USERS}{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var user_raw = JsonConvert.DeserializeObject<TransportUser>(res.Response);

            return user_raw;
        }

        /// <summary>
        /// Gets the current user guilds async.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="before">The before.</param>
        /// <param name="after">The after.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}";

            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path)
                .AddParameter($"limit", limit.ToString(CultureInfo.InvariantCulture));

            if (before != null)
                url.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
            if (after != null)
                url.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));

            var res = await this.DoRequestAsync(this.Discord, bucket, url.Build(), RestRequestMethod.GET, route).ConfigureAwait(false);

            if (this.Discord is DiscordClient)
            {
                var guilds_raw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response);
                var glds = guilds_raw.Select(xug => (this.Discord as DiscordClient)?._guilds[xug.Id]);
                return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(glds));
            }
            else
            {
                return new ReadOnlyCollection<DiscordGuild>(JsonConvert.DeserializeObject<List<DiscordGuild>>(res.Response));
            }
        }

        /// <summary>
        /// Modifies the guild member async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="nick">The nick.</param>
        /// <param name="role_ids">The role_ids.</param>
        /// <param name="mute">The mute.</param>
        /// <param name="deaf">The deaf.</param>
        /// <param name="voice_channel_id">The voice_channel_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildMemberAsync(ulong guild_id, ulong user_id, Optional<string> nick,
            Optional<IEnumerable<ulong>> role_ids, Optional<bool> mute, Optional<bool> deaf,
            Optional<ulong?> voice_channel_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick,
                RoleIds = role_ids,
                Deafen = deaf,
                Mute = mute,
                VoiceChannelId = voice_channel_id
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Modifies the current member nickname async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="nick">The nick.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyCurrentMemberNicknameAsync(ulong guild_id, string nick, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = nick
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.MEMBERS}{Endpoints.ME}{Endpoints.NICK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, payload: DiscordJson.SerializeObject(pld));
        }
        #endregion

        #region Roles
        /// <summary>
        /// Gets the guild roles async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var roles_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = this.Discord; xr._guild_id = guild_id; return xr; });

            return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(roles_raw));
        }

        /// <summary>
        /// Gets the guild async.
        /// </summary>
        /// <param name="guildId">The guild id.</param>
        /// <param name="with_counts">If true, with_counts.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuild> GetGuildAsync(ulong guildId, bool? with_counts)
        {
            var urlparams = new Dictionary<string, string>();
            if (with_counts.HasValue)
                urlparams["with_counts"] = with_counts?.ToString();

            var route = $"{Endpoints.GUILDS}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id = guildId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, urlparams).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guildRest = json.ToDiscordObject<DiscordGuild>();
            foreach (var r in guildRest._roles.Values)
                r._guild_id = guildRest.Id;

            if (this.Discord is DiscordClient dc)
            {
                await dc.OnGuildUpdateEventAsync(guildRest, rawMembers).ConfigureAwait(false);
                return dc._guilds[guildRest.Id];
            }
            else
            {
                guildRest.Discord = this.Discord;
                return guildRest;
            }
        }

        /// <summary>
        /// Modifies the guild role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="role_id">The role_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="permissions">The permissions.</param>
        /// <param name="color">The color.</param>
        /// <param name="hoist">If true, hoist.</param>
        /// <param name="mentionable">If true, mentionable.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong guild_id, ulong role_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions & PermissionMethods.FULL_PERMS,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;
            ret._guild_id = guild_id;

            return ret;
        }

        /// <summary>
        /// Deletes the role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="role_id">The role_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteRoleAsync(ulong guild_id, ulong role_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, role_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Creates the guild role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="permissions">The permissions.</param>
        /// <param name="color">The color.</param>
        /// <param name="hoist">If true, hoist.</param>
        /// <param name="mentionable">If true, mentionable.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordRole> CreateGuildRoleAsync(ulong guild_id, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = name,
                Permissions = permissions & PermissionMethods.FULL_PERMS,
                Color = color,
                Hoist = hoist,
                Mentionable = mentionable
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;
            ret._guild_id = guild_id;

            return ret;
        }
        #endregion

        #region Prune
        /// <summary>
        /// Gets the guild prune count async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="days">The days.</param>
        /// <param name="include_roles">The include_roles.</param>
        /// <returns>A Task.</returns>
        internal async Task<int> GetGuildPruneCountAsync(ulong guild_id, int days, IEnumerable<ulong> include_roles)
        {
            if (days < 0 || days > 30)
                throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));

            var urlparams = new Dictionary<string, string>
            {
                ["days"] = days.ToString(CultureInfo.InvariantCulture)
            };

            var sb = new StringBuilder();

            if (include_roles != null)
            {
                var roleArray = include_roles.ToArray();
                var roleArrayCount = roleArray.Count();

                for (var i = 0; i < roleArrayCount; i++)
                    sb.Append($"&include_roles={roleArray[i]}");
            }

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PRUNE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);
            var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlparams)}{sb}", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned.Value;
        }

        /// <summary>
        /// Begins the guild prune async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="days">The days.</param>
        /// <param name="compute_prune_count">If true, compute_prune_count.</param>
        /// <param name="include_roles">The include_roles.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<int?> BeginGuildPruneAsync(ulong guild_id, int days, bool compute_prune_count, IEnumerable<ulong> include_roles, string reason)
        {
            if (days < 0 || days > 30)
                throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(days));

            var urlparams = new Dictionary<string, string>
            {
                ["days"] = days.ToString(CultureInfo.InvariantCulture),
                ["compute_prune_count"] = compute_prune_count.ToString()
            };

            var sb = new StringBuilder();

            if (include_roles != null)
            {
                var roleArray = include_roles.ToArray();
                var roleArrayCount = roleArray.Count();

                for (var i = 0; i < roleArrayCount; i++)
                    sb.Append($"&include_roles={roleArray[i]}");
            }

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PRUNE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlparams)}{sb}", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers).ConfigureAwait(false);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        #endregion

        #region GuildVarious
        /// <summary>
        /// Gets the template async.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
        {
            var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { code }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var templates_raw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return templates_raw;
        }

        /// <summary>
        /// Gets the guild integrations async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var integrations_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrations_raw));
        }

        /// <summary>
        /// Gets the guild preview async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.PREVIEW}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildPreview>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Creates the guild integration async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="type">The type.</param>
        /// <param name="id">The id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guild_id, string type, ulong id)
        {
            var pld = new RestGuildIntegrationAttachPayload
            {
                Type = type,
                Id = id
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Modifies the guild integration async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="integration_id">The integration_id.</param>
        /// <param name="expire_behaviour">The expire_behaviour.</param>
        /// <param name="expire_grace_period">The expire_grace_period.</param>
        /// <param name="enable_emoticons">If true, enable_emoticons.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guild_id, ulong integration_id, int expire_behaviour, int expire_grace_period, bool enable_emoticons)
        {
            var pld = new RestGuildIntegrationModifyPayload
            {
                ExpireBehavior = expire_behaviour,
                ExpireGracePeriod = expire_grace_period,
                EnableEmoticons = enable_emoticons
            };

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, integration_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the guild integration async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="integration">The integration.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteGuildIntegrationAsync(ulong guild_id, DiscordIntegration integration)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, integration_id = integration.Id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, payload: DiscordJson.SerializeObject(integration));
        }

        /// <summary>
        /// Syncs the guild integration async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="integration_id">The integration_id.</param>
        /// <returns>A Task.</returns>
        internal Task SyncGuildIntegrationAsync(ulong guild_id, ulong integration_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id{Endpoints.SYNC}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id, integration_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route);
        }

        /// <summary>
        /// Gets the guild voice regions async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REGIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var regions_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions_raw));
        }

        /// <summary>
        /// Gets the guild invites async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INVITES}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var invites_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invites_raw));
        }
        #endregion

        #region Invite
        /// <summary>
        /// Gets the invite async.
        /// </summary>
        /// <param name="invite_code">The invite_code.</param>
        /// <param name="with_counts">If true, with_counts.</param>
        /// <param name="with_expiration">If true, with_expiration.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> GetInviteAsync(string invite_code, bool? with_counts, bool? with_expiration)
        {
            var urlparams = new Dictionary<string, string>();
            if (with_counts.HasValue)
                urlparams["with_counts"] = with_counts?.ToString();
            if (with_expiration.HasValue)
                urlparams["with_expiration"] = with_expiration?.ToString();

            var route = $"{Endpoints.INVITES}/:invite_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { invite_code }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the invite async.
        /// </summary>
        /// <param name="invite_code">The invite_code.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> DeleteInviteAsync(string invite_code, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.INVITES}/:invite_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { invite_code }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /*
         * Disabled due to API restrictions
         *
         * internal async Task<DiscordInvite> InternalAcceptInvite(string invite_code)
         * {
         *     this.Discord.DebugLogger.LogMessage(LogLevel.Warning, "REST API", "Invite accept endpoint was used; this account is now likely unverified", DateTime.Now);
         *
         *     var url = new Uri($"{Utils.GetApiBaseUri(), Endpoints.INVITES}/{invite_code));
         *     var bucket = this.Rest.GetBucket(0, MajorParameterType.Unbucketed, url, HttpRequestMethod.POST);
         *     var res = await this.DoRequestAsync(this.Discord, bucket, url, HttpRequestMethod.POST).ConfigureAwait(false);
         *
         *     var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
         *     ret.Discord = this.Discord;
         *
         *     return ret;
         * }
         */
        #endregion

        #region Connections
        /// <summary>
        /// Gets the users connections async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordConnection>> GetUsersConnectionsAsync()
        {
            var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CONNECTIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var connections_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

            return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connections_raw));
        }
        #endregion

        #region Voice
        /// <summary>
        /// Lists the voice regions async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        {
            var route = $"{Endpoints.VOICE}{Endpoints.REGIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
        }
        #endregion

        #region Webhooks
        /// <summary>
        /// Creates the webhook async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> CreateWebhookAsync(ulong channel_id, string name, Optional<string> base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar.HasValue ? base64_avatar.Value : null,
                AvatarSet = base64_avatar.HasValue
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Gets the channel webhooks async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        /// <summary>
        /// Gets the guild webhooks async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.WEBHOOKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var webhooks_raw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(xw => { xw.Discord = this.Discord; xw.ApiClient = this; return xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooks_raw));
        }

        /// <summary>
        /// Gets the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> GetWebhookAsync(ulong webhook_id)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        // Auth header not required
        /// <summary>
        /// Gets the webhook with token async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong webhook_id, string webhook_token)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Token = webhook_token;
            ret.Id = webhook_id;
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Modifies the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="name">The name.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, ulong channelId, string name, Optional<string> base64_avatar, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar.HasValue ? base64_avatar.Value : null,
                AvatarSet = base64_avatar.HasValue,
                ChannelId = channelId
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Modifies the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong webhook_id, string name, string base64_avatar, string webhook_token, string reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = name,
                AvatarBase64 = base64_avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Deletes the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteWebhookAsync(ulong webhook_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Deletes the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteWebhookAsync(ulong webhook_id, string webhook_token, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        /// <summary>
        /// Executes the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> ExecuteWebhookAsync(ulong webhook_id, string webhook_token, DiscordWebhookBuilder builder, string thread_id)
        {
            builder.Validate();

            if (builder.Embeds != null)
                foreach (var embed in builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestWebhookExecutePayload
            {
                Content = builder.Content,
                Username = builder.Username.HasValue ? builder.Username.Value : null,
                AvatarUrl = builder.AvatarUrl.HasValue ? builder.AvatarUrl.Value : null,
                IsTTS = builder.IsTTS,
                Embeds = builder.Embeds
            };

            if (builder.Mentions != null)
                pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

            if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count() > 0 || builder.IsTTS == true || builder.Mentions != null)
                values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var qub = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true");
            if (thread_id != null)
                qub.AddParameter("thread_id", thread_id);
            var url = qub.Build();

            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

            foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo.Value;
            }

            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Executes the webhook slack async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="json_payload">The json_payload.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> ExecuteWebhookSlackAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.SLACK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: json_payload).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Executes the webhook github async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="json_payload">The json_payload.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> ExecuteWebhookGithubAsync(ulong webhook_id, string webhook_token, string json_payload)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.GITHUB}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { webhook_id, webhook_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: json_payload).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Edits the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> EditWebhookMessageAsync(ulong webhook_id, string webhook_token, string message_id, DiscordWebhookBuilder builder)
        {
            builder.Validate(true);

            var pld = new RestWebhookMessageEditPayload
            {
                Content = builder.Content,
                Embeds = builder.Embeds,
                Mentions = builder.Mentions,
                Components = builder.Components,
                Attachments = builder.Attachments
            };

            var values = new Dictionary<string, string>
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { webhook_id, webhook_token, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, values: values, files: builder.Files);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;

            foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue)) {
                file.Stream.Position = file.ResetPositionTo.Value;
            }

            return ret;
        }

        /// <summary>
        /// Edits the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> EditWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id, DiscordWebhookBuilder builder) =>
            this.EditWebhookMessageAsync(webhook_id, webhook_token, message_id.ToString(), builder);

        /// <summary>
        /// Gets the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> GetWebhookMessageAsync(ulong webhook_id, string webhook_token, string message_id)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { webhook_id, webhook_token, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);
            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Gets the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> GetWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id) =>
            this.GetWebhookMessageAsync(webhook_id, webhook_token, message_id.ToString());

        /// <summary>
        /// Deletes the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task DeleteWebhookMessageAsync(ulong webhook_id, string webhook_token, string message_id)
        {
            var route = $"{Endpoints.WEBHOOKS}/:webhook_id/:webhook_token{Endpoints.MESSAGES}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { webhook_id, webhook_token, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }
        /// <summary>
        /// Deletes the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteWebhookMessageAsync(ulong webhook_id, string webhook_token, ulong message_id) =>
            this.DeleteWebhookMessageAsync(webhook_id, webhook_token, message_id.ToString());
        #endregion

        #region Reactions
        /// <summary>
        /// Creates the reaction async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="emoji">The emoji.</param>
        /// <returns>A Task.</returns>
        internal Task CreateReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Deletes the own reaction async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="emoji">The emoji.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteOwnReactionAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Deletes the user reaction async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="emoji">The emoji.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteUserReactionAsync(ulong channel_id, ulong message_id, ulong user_id, string emoji, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Gets the reactions async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="emoji">The emoji.</param>
        /// <param name="after_id">The after_id.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channel_id, ulong message_id, string emoji, ulong? after_id = null, int limit = 25)
        {
            var urlparams = new Dictionary<string, string>();
            if (after_id.HasValue)
                urlparams["after"] = after_id.Value.ToString(CultureInfo.InvariantCulture);

            urlparams["limit"] = limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams), this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var reacters_raw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response);
            var reacters = new List<DiscordUser>();
            foreach (var xr in reacters_raw)
            {
                var usr = new DiscordUser(xr) { Discord = this.Discord };
                usr = this.Discord.UserCache.AddOrUpdate(xr.Id, usr, (id, old) =>
                {
                    old.Username = usr.Username;
                    old.Discriminator = usr.Discriminator;
                    old.AvatarHash = usr.AvatarHash;
                    return old;
                });

                reacters.Add(usr);
            }

            return new ReadOnlyCollection<DiscordUser>(new List<DiscordUser>(reacters));
        }

        /// <summary>
        /// Deletes the all reactions async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteAllReactionsAsync(ulong channel_id, ulong message_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Deletes the reactions emoji async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="emoji">The emoji.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteReactionsEmojiAsync(ulong channel_id, ulong message_id, string emoji)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.REACTIONS}/:emoji";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, message_id, emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, ratelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }
        #endregion

        #region Threads

        /// <summary>
        /// Creates the thread with message.
        /// </summary>
        /// <param name="channel_id">The channel id to create the thread in.</param>
        /// <param name="message_id">The message id to create the thread from.</param>
        /// <param name="name">The name of the thread.</param>
        /// <param name="auto_archive_duration">The auto_archive_duration for the thread.</param>
        /// <param name="reason">The reason.</param>
        internal async Task<DiscordThreadChannel> CreateThreadWithMessageAsync(ulong channel_id, ulong message_id, string name, ThreadAutoArchiveDuration auto_archive_duration, string reason)
        {
            var pld = new RestThreadChannelCreatePayload
            {
                Name = name,
                AutoArchiveDuration = auto_archive_duration
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.MESSAGES}/:message_id{Endpoints.THREADS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id, message_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));

            var thread_channel = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);

            return thread_channel;
        }

        /// <summary>
        /// Creates the thread without a message.
        /// </summary>
        /// <param name="channel_id">The channel id to create the thread in.</param>
        /// <param name="name">The name of the thread.</param>
        /// <param name="auto_archive_duration">The auto_archive_duration for the thread.</param>
        /// <param name="reason">The reason.</param>
        internal async Task<DiscordThreadChannel> CreateThreadWithoutMessageAsync(ulong channel_id, string name, ThreadAutoArchiveDuration auto_archive_duration, string reason)
        {
            var pld = new RestThreadChannelCreatePayload
            {
                Name = name,
                AutoArchiveDuration = auto_archive_duration
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld));

            var thread_channel = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);

            return thread_channel;
        }

        /// <summary>
        /// Gets the thread.
        /// </summary>
        /// <param name="thread_id">The thread id.</param>
        internal async Task<DiscordThreadChannel> GetThreadAsync(ulong thread_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { thread_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Joins the thread.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        internal async Task JoinThreadAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        /// <summary>
        /// Leaves the thread.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        internal async Task LeaveThreadAsync(ulong channel_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}{Endpoints.ME}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Adds the thread member.
        /// </summary>
        /// <param name="channel_id">The channel id to add the member to.</param>
        /// <param name="user_id">The user id to add.</param>
        internal async Task AddThreadMemberAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route);
        }

        /// <summary>
        /// Removes the thread member.
        /// </summary>
        /// <param name="channel_id">The channel id to remove the member from.</param>
        /// <param name="user_id">The user id to remove.</param>
        internal async Task RemoveThreadMemberAsync(ulong channel_id, ulong user_id)
        {
            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREAD_MEMBERS}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { channel_id, user_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Gets the thread members.
        /// </summary>
        /// <param name="thread_id">The thread id.</param>
        internal async Task<IReadOnlyList<DiscordThreadChannelMember>> GetThreadMembersAsync(ulong thread_id)
        {
            var route = $"{Endpoints.CHANNELS}/:thread_id{Endpoints.THREAD_MEMBERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { thread_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var thread_members_raw = JsonConvert.DeserializeObject<List<DiscordThreadChannelMember>>(res.Response);

            return new ReadOnlyCollection<DiscordThreadChannelMember>(thread_members_raw);
        }

        /// <summary>
        /// Gets the active threads in a guild.
        /// </summary>
        /// <param name="guild_id">The guild id.</param>
        internal async Task<DiscordThreadResult> GetActiveThreadsAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.THREADS}{Endpoints.THREAD_ACTIVE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var thread_return = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return thread_return;
        }

        /// <summary>
        /// Gets the joined private archived threads in a channel.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="before">Get threads before snowflake.</param>
        /// <param name="limit">Limit the results.</param>
        internal async Task<DiscordThreadResult> GetJoinedPrivateArchivedThreadsAsync(ulong channel_id, ulong? before, int? limit)
        {
            var urlparams = new Dictionary<string, string>();
            if (before != null)
                urlparams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
            if (limit != null && limit > 0)
                urlparams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.USERS}{Endpoints.ME}{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PRIVATE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var thread_return = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return thread_return;
        }

        /// <summary>
        /// Gets the public archived threads in a channel.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="before">Get threads before snowflake.</param>
        /// <param name="limit">Limit the results.</param>
        internal async Task<DiscordThreadResult> GetPublicArchivedThreadsAsync(ulong channel_id, ulong? before, int? limit)
        {
            var urlparams = new Dictionary<string, string>();
            if (before != null)
                urlparams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
            if (limit != null && limit > 0)
                urlparams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PUBLIC}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var thread_return = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return thread_return;
        }

        /// <summary>
        /// Gets the private archived threads in a channel.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="before">Get threads before snowflake.</param>
        /// <param name="limit">Limit the results.</param>
        internal async Task<DiscordThreadResult> GetPrivateArchivedThreadsAsync(ulong channel_id, ulong? before, int? limit)
        {
            var urlparams = new Dictionary<string, string>();
            if (before != null)
                urlparams["before"] = before.Value.ToString(CultureInfo.InvariantCulture);
            if (limit != null && limit > 0)
                urlparams["limit"] = limit.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.CHANNELS}/:channel_id{Endpoints.THREADS}{Endpoints.THREAD_ARCHIVED}{Endpoints.THREAD_PRIVATE}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { channel_id }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var thread_return = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return thread_return;
        }

        /// <summary>
        /// Modifies a thread.
        /// </summary>
        /// <param name="thread_id">The thread to modify.</param>
        /// <param name="name">The new name.</param>
        /// <param name="locked">The new locked state.</param>
        /// <param name="archived">The new archived state.</param>
        /// <param name="autoArchiveDuration">The new auto archive duration.</param>
        /// <param name="perUserRateLimit">The new per user rate limit.</param>
        /// <param name="reason">The reason for the modification.</param>
        internal Task ModifyThreadAsync(ulong thread_id, string name, Optional<bool?> locked, Optional<bool?> archived, Optional<ThreadAutoArchiveDuration?> autoArchiveDuration, Optional<int?> perUserRateLimit, string reason)
        {
            var pld = new RestThreadChannelModifyPayload
            {
                Name = name,
                Archived = archived,
                AutoArchiveDuration = autoArchiveDuration,
                Locked = locked,
                PerUserRateLimit = perUserRateLimit
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:thread_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { thread_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Deletes a thread.
        /// </summary>
        /// <param name="thread_id">The thread to delete.</param>
        /// <param name="reason">The reason for deletion.</param>
        internal Task DeleteThreadAsync(ulong thread_id, string reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var route = $"{Endpoints.CHANNELS}/:thread_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { thread_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }

        #endregion

        #region Emoji
        /// <summary>
        /// Gets the guild emojis async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);
            var users = new Dictionary<ulong, DiscordUser>();
            var emojis = new List<DiscordGuildEmoji>();
            foreach (var rawEmoji in emojisRaw)
            {
                var xge = rawEmoji.ToObject<DiscordGuildEmoji>();
                xge.Guild = gld;

                var xtu = rawEmoji["user"]?.ToObject<TransportUser>();
                if (xtu != null)
                {
                    if (!users.ContainsKey(xtu.Id))
                    {
                        var user = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);
                        users[user.Id] = user;
                    }

                    xge.User = users[xtu.Id];
                }

                emojis.Add(xge);
            }

            return new ReadOnlyCollection<DiscordGuildEmoji>(emojis);
        }

        /// <summary>
        /// Gets the guild emoji async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="emoji_id">The emoji_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guild_id, ulong emoji_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { guild_id, emoji_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);

            var emoji_raw = JObject.Parse(res.Response);
            var emoji = emoji_raw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        /// <summary>
        /// Creates the guild emoji async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="imageb64">The imageb64.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guild_id, string name, string imageb64, IEnumerable<ulong> roles, string reason)
        {
            var pld = new RestGuildEmojiCreatePayload
            {
                Name = name,
                ImageB64 = imageb64,
                Roles = roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);

            var emoji_raw = JObject.Parse(res.Response);
            var emoji = emoji_raw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToObject<TransportUser>();
            emoji.User = xtu != null
                ? gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu)
                : this.Discord.CurrentUser;

            return emoji;
        }

        /// <summary>
        /// Modifies the guild emoji async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="emoji_id">The emoji_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guild_id, ulong emoji_id, string name, IEnumerable<ulong> roles, string reason)
        {
            var pld = new RestGuildEmojiModifyPayload
            {
                Name = name,
                Roles = roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { guild_id, emoji_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(guild_id, out var gld);

            var emoji_raw = JObject.Parse(res.Response);
            var emoji = emoji_raw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emoji_raw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        /// <summary>
        /// Deletes the guild emoji async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="emoji_id">The emoji_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteGuildEmojiAsync(ulong guild_id, ulong emoji_id, string reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(reason))
                headers[REASON_HEADER_NAME] = reason;

            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, emoji_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }
        #endregion

        #region Stickers

        /// <summary>
        /// Gets the sticker async.
        /// </summary>
        /// <param name="sticker_id">The sticker_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordSticker> GetStickerAsync(ulong sticker_id)
        {
            var route = $"{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {sticker_id}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Gets the sticker packs async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
        {
            var route = $"{Endpoints.STICKERPACKS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response)["sticker_packs"] as JArray;
            var ret = json.ToDiscordObject<DiscordStickerPack[]>();

            return ret.ToList();
        }

        /// <summary>
        /// Gets the guild stickers async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordSticker>> GetGuildStickersAsync(ulong guild_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
            var json = JArray.Parse(res.Response);
            var ret = json.ToDiscordObject<DiscordSticker[]>();

            for (var i = 0; i < ret.Length; i++)
            {
                var stkr = ret[i];
                stkr.Discord = this.Discord;

                if (json[i]["user"] is JObject obj) // Null = Missing stickers perm //
                {
                    var tsr = obj.ToDiscordObject<TransportUser>();
                    var usr = new DiscordUser(tsr) {Discord = this.Discord};
                    usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
                    {
                        old.Username = usr.Username;
                        old.Discriminator = usr.Discriminator;
                        old.AvatarHash = usr.AvatarHash;
                        return old;
                    });
                    stkr.User = usr;
                }
            }

            return ret.ToList();
        }

        /// <summary>
        /// Gets the guild sticker async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="sticker_id">The sticker_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordSticker> GetGuildStickerAsync(ulong guild_id, ulong sticker_id)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id, sticker_id}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var ret = json.ToDiscordObject<DiscordSticker>();
            if (json["user"] is not null) // Null = Missing stickers perm //
            {
                var tsr = json["user"].ToDiscordObject<TransportUser>();
                var usr = new DiscordUser(tsr) {Discord = this.Discord};
                usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
                {
                    old.Username = usr.Username;
                    old.Discriminator = usr.Discriminator;
                    old.AvatarHash = usr.AvatarHash;
                    return old;
                });
                ret.User = usr;
            }
            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Creates the guild sticker async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="file">The file.</param>
        /// <param name="reason">The reason.</param>
        internal async Task<DiscordSticker> CreateGuildStickerAsync(ulong guild_id, string name, Optional<string> description, string tags, DiscordMessageFile file, string reason)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);
            /*
            var pld = new RestStickerCreatePayload()
            {
                Name = name,
                Description = description,
                Tags = tags
            };
            */
            var values = new Dictionary<string, string>
            {
                { "name", name },
                { "description", description.HasValue ? description.Value : null },
                { "tags", tags }
            };

            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, values, new[] {file});
            
            var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Modifies the guild sticker async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="sticker_id">The sticker_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordSticker> ModifyGuildStickerAsync(ulong guild_id, ulong sticker_id, Optional<string> name, Optional<string> description, Optional<string> tags, string reason)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id, sticker_id}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            var pld = new RestStickerModifyPayload()
            {
                Name = name,
                Description = description,
                Tags = tags
            };

            var values = new Dictionary<string, string>
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route);
            var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();
            ret.Discord = this.Discord;

            return null;
        }

        /// <summary>
        /// Deletes the guild sticker async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="sticker_id">The sticker_id.</param>
        /// <param name="reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task DeleteGuildStickerAsync(ulong guild_id, ulong sticker_id, string reason)
        {
            var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { guild_id, sticker_id }, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(reason))
                headers.Add(REASON_HEADER_NAME, reason);

            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
        }
        #endregion

        #region Slash Commands
        /// <summary>
        /// Gets the global application commands async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(ulong application_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        /// <summary>
        /// Bulks the overwrite global application commands async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="commands">The commands.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ulong application_id, IEnumerable<DiscordApplicationCommand> commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options,
                    DefaultPermission = command.DefaultPermission
                });
            }

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        /// <summary>
        /// Creates the global application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="command">The command.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(ulong application_id, DiscordApplicationCommand command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Name = command.Name,
                Description = command.Description,
                Options = command.Options,
                DefaultPermission = command.DefaultPermission
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Gets the global application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="command_id">The command_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong application_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Edits the global application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="command_id">The command_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="options">The options.</param>
        /// <param name="default_permission">The default_permission.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong application_id, ulong command_id, Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options, Optional<bool> default_permission)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = name,
                Description = description,
                Options = options,
                DefaultPermission = default_permission
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the global application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="command_id">The command_id.</param>
        /// <returns>A Task.</returns>
        internal async Task DeleteGlobalApplicationCommandAsync(ulong application_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { application_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Gets the guild application commands async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong application_id, ulong guild_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        /// <summary>
        /// Bulks the overwrite guild application commands async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="commands">The commands.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong application_id, ulong guild_id, IEnumerable<DiscordApplicationCommand> commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options,
                    DefaultPermission = command.DefaultPermission
                });
            }

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PUT, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        /// <summary>
        /// Creates the guild application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="command">The command.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong application_id, ulong guild_id, DiscordApplicationCommand command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Name = command.Name,
                Description = command.Description,
                Options = command.Options,
                DefaultPermission = command.DefaultPermission

            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id, guild_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Gets the guild application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="command_id">The command_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Edits the guild application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="command_id">The command_id.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="options">The options.</param>
        /// <param name="default_permission">The default_permission.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id, Optional<string> name, Optional<string> description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> options, Optional<bool> default_permission)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = name,
                Description = description,
                Options = options,
                DefaultPermission = default_permission
            };

            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the guild application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="command_id">The command_id.</param>
        /// <returns>A Task.</returns>
        internal async Task DeleteGuildApplicationCommandAsync(ulong application_id, ulong guild_id, ulong command_id)
        {
            var route = $"{Endpoints.APPLICATIONS}/:application_id{Endpoints.GUILDS}/:guild_id{Endpoints.COMMANDS}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new { application_id, guild_id, command_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route);
        }

        /// <summary>
        /// Creates the interaction response async.
        /// </summary>
        /// <param name="interaction_id">The interaction_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="type">The type.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task CreateInteractionResponseAsync(ulong interaction_id, string interaction_token, InteractionResponseType type, DiscordInteractionResponseBuilder builder)
        {
            var pld = new RestInteractionResponsePayload
            {
                Type = type,
                Data = builder != null ? new DiscordInteractionApplicationCommandCallbackData
                {
                    Content = builder.Content,
                    Embeds = builder.Embeds,
                    IsTTS = builder.IsTTS,
                    Mentions = builder.Mentions,
                    Flags = builder.IsEphemeral ? MessageFlags.Ephemeral : 0,
                    Components = builder.Components
                } : null
            };

            var route = $"{Endpoints.INTERACTIONS}/:interaction_id/:interaction_token{Endpoints.CALLBACK}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { interaction_id, interaction_token }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets the original interaction response async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> GetOriginalInteractionResponseAsync(ulong application_id, string interaction_token) =>
            this.GetWebhookMessageAsync(application_id, interaction_token, Endpoints.ORIGINAL);

        /// <summary>
        /// Edits the original interaction response async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> EditOriginalInteractionResponseAsync(ulong application_id, string interaction_token, DiscordWebhookBuilder builder) =>
            this.EditWebhookMessageAsync(application_id, interaction_token, Endpoints.ORIGINAL, builder);

        /// <summary>
        /// Deletes the original interaction response async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteOriginalInteractionResponseAsync(ulong application_id, string interaction_token) =>
            this.DeleteWebhookMessageAsync(application_id, interaction_token, Endpoints.ORIGINAL);

        /// <summary>
        /// Creates the followup message async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CreateFollowupMessageAsync(ulong application_id, string interaction_token, DiscordFollowupMessageBuilder builder)
        {
            builder.Validate();

            if (builder.Embeds != null)
                foreach (var embed in builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestFollowupMessageCreatePayload
            {
                Content = builder.Content,
                IsTTS = builder.IsTTS,
                Embeds = builder.Embeds,
                Flags = builder.Flags,
                Components = builder.Components
            };

            if (builder.Mentions != null)
                pld.Mentions = new DiscordMentions(builder.Mentions, builder.Mentions.Any());

            if (!string.IsNullOrEmpty(builder.Content) || builder.Embeds?.Count() > 0 || builder.IsTTS == true || builder.Mentions != null)
                values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.WEBHOOKS}/:application_id/:interaction_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { application_id, interaction_token }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path).AddParameter("wait", "true").Build();
            var res = await this.DoMultipartAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, values: values, files: builder.Files).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

            foreach (var file in builder.Files.Where(x => x.ResetPositionTo.HasValue))
            {
                file.Stream.Position = file.ResetPositionTo.Value;
            }

            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Gets the followup message async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> GetFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id) =>
            this.GetWebhookMessageAsync(application_id, interaction_token, message_id);

        /// <summary>
        /// Edits the followup message async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> EditFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id, DiscordWebhookBuilder builder) =>
            this.EditWebhookMessageAsync(application_id, interaction_token, message_id, builder);

        /// <summary>
        /// Deletes the followup message async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteFollowupMessageAsync(ulong application_id, string interaction_token, ulong message_id) =>
            this.DeleteWebhookMessageAsync(application_id, interaction_token, message_id);

        //TODO: edit, delete, follow up
        #endregion

        #region Misc
        /// <summary>
        /// Gets the current application info async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal Task<TransportApplication> GetCurrentApplicationInfoAsync()
            => this.GetApplicationInfoAsync("@me");

        /// <summary>
        /// Gets the application info async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <returns>A Task.</returns>
        internal Task<TransportApplication> GetApplicationInfoAsync(ulong application_id)
            => this.GetApplicationInfoAsync(application_id.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the application info async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <returns>A Task.</returns>
        private async Task<TransportApplication> GetApplicationInfoAsync(string application_id)
        {
            var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TransportApplication>(res.Response);
        }

        /// <summary>
        /// Gets the application assets async.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication application)
        {
            var route = $"{Endpoints.OAUTH2}{Endpoints.APPLICATIONS}/:application_id{Endpoints.ASSETS}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { application_id = application.Id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

            var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
            foreach (var asset in assets)
            {
                asset.Discord = application.Discord;
                asset.Application = application;
            }

            return new ReadOnlyCollection<DiscordApplicationAsset>(new List<DiscordApplicationAsset>(assets));
        }

        /// <summary>
        /// Gets the gateway info async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal async Task<GatewayInfo> GetGatewayInfoAsync()
        {
            var headers = Utilities.GetBaseHeaders();
            var route = Endpoints.GATEWAY;
            if (this.Discord.Configuration.TokenType == TokenType.Bot)
                route += Endpoints.BOT;
            var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration.UseCanary);
            var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route, headers).ConfigureAwait(false);

            var info = JObject.Parse(res.Response).ToObject<GatewayInfo>();
            info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.resetAfter);
            return info;
        }
        #endregion

    }
}
