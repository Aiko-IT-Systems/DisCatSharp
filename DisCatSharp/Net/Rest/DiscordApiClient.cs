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
        private const string ReasonHeaderName = "X-Audit-Log-Reason";

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
        /// <param name="Client">The client.</param>
        internal DiscordApiClient(BaseDiscordClient Client)
        {
            this.Discord = Client;
            this.Rest = new RestClient(Client);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordApiClient"/> class.
        /// </summary>
        /// <param name="Proxy">The proxy.</param>
        /// <param name="Timeout">The timeout.</param>
        /// <param name="UseRelativeRateLimit">If true, use relative rate limit.</param>
        /// <param name="Logger">The logger.</param>
        internal DiscordApiClient(IWebProxy Proxy, TimeSpan Timeout, bool UseRelativeRateLimit, ILogger Logger) // This is for meta-clients, such as the webhook client
        {
            this.Rest = new RestClient(Proxy, Timeout, UseRelativeRateLimit, Logger);
        }

        /// <summary>
        /// Builds the query string.
        /// </summary>
        /// <param name="Values">The values.</param>
        /// <param name="Post">If true, post.</param>
        /// <returns>A string.</returns>
        private static string BuildQueryString(IDictionary<string, string> Values, bool Post = false)
        {
            if (Values == null || Values.Count == 0)
                return string.Empty;

            var valsCollection = Values.Select(Xkvp =>
                $"{WebUtility.UrlEncode(Xkvp.Key)}={WebUtility.UrlEncode(Xkvp.Value)}");
            var vals = string.Join("&", valsCollection);

            return !Post ? $"?{vals}" : vals;
        }

        /// <summary>
        /// Prepares the message.
        /// </summary>
        /// <param name="msg_raw">The msg_raw.</param>
        /// <returns>A DiscordMessage.</returns>
        private DiscordMessage PrepareMessage(JToken MsgRaw)
        {
            var author = MsgRaw["author"].ToObject<TransportUser>();
            var ret = MsgRaw.ToDiscordObject<DiscordMessage>();
            ret.Discord = this.Discord;

            this.PopulateMessage(author, ret);

            var referencedMsg = MsgRaw["referenced_message"];
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
        /// <param name="Author">The author.</param>
        /// <param name="Ret">The ret.</param>
        private void PopulateMessage(TransportUser Author, DiscordMessage Ret)
        {
            var guild = Ret.Channel?.Guild;

            //If this is a webhook, it shouldn't be in the user cache.
            if (Author.IsBot && int.Parse(Author.Discriminator) == 0)
            {
                Ret.Author = new DiscordUser(Author) { Discord = this.Discord };
            }
            else
            {
                if (!this.Discord.UserCache.TryGetValue(Author.Id, out var usr))
                {
                    this.Discord.UserCache[Author.Id] = usr = new DiscordUser(Author) { Discord = this.Discord };
                }

                if (guild != null)
                {
                    if (!guild.Members.TryGetValue(Author.Id, out var mbr))
                        mbr = new DiscordMember(usr) { Discord = this.Discord, _guildId = guild.Id };
                    Ret.Author = mbr;
                }
                else
                {
                    Ret.Author = usr;
                }
            }

            Ret.PopulateMentions();

            if (Ret._reactions == null)
                Ret._reactions = new List<DiscordReaction>();
            foreach (var xr in Ret._reactions)
                xr.Emoji.Discord = this.Discord;
        }

        /// <summary>
        /// Executes a rest request.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="Url">The url.</param>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="Headers">The headers.</param>
        /// <param name="Payload">The payload.</param>
        /// <param name="RatelimitWaitOverride">The ratelimit wait override.</param>
        internal Task<RestResponse> DoRequest(BaseDiscordClient Client, RateLimitBucket Bucket, Uri Url, RestRequestMethod Method, string Route, IReadOnlyDictionary<string, string> Headers = null, string Payload = null, double? RatelimitWaitOverride = null)
        {
            var req = new RestRequest(Client, Bucket, Url, Method, Route, Headers, Payload, RatelimitWaitOverride);

            if (this.Discord != null)
                this.Rest.ExecuteRequest(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequest(req);

            return req.WaitForCompletion();
        }

        /// <summary>
        /// Executes a multipart rest request for stickers.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="Url">The url.</param>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="Headers">The headers.</param>
        /// <param name="File">The file.</param>
        /// <param name="Name">The sticker name.</param>
        /// <param name="Tags">The sticker tag.</param>
        /// <param name="Description">The sticker description.</param>
        /// <param name="RatelimitWaitOverride">The ratelimit wait override.</param>
        private Task<RestResponse> DoStickerMultipart(BaseDiscordClient Client, RateLimitBucket Bucket, Uri Url, RestRequestMethod Method, string Route, IReadOnlyDictionary<string, string> Headers = null,
            DiscordMessageFile File = null, string Name = "", string Tags = "", string Description = "", double? RatelimitWaitOverride = null)
        {
            var req = new MultipartStickerWebRequest(Client, Bucket, Url, Method, Route, Headers, File, Name, Tags, Description, RatelimitWaitOverride);

            if (this.Discord != null)
                this.Rest.ExecuteRequest(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequest(req);

            return req.WaitForCompletion();
        }

        /// <summary>
        /// Executes a multipart request.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="Bucket">The bucket.</param>
        /// <param name="Url">The url.</param>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="Headers">The headers.</param>
        /// <param name="Values">The values.</param>
        /// <param name="Files">The files.</param>
        /// <param name="RatelimitWaitOverride">The ratelimit wait override.</param>
        private Task<RestResponse> DoMultipart(BaseDiscordClient Client, RateLimitBucket Bucket, Uri Url, RestRequestMethod Method, string Route, IReadOnlyDictionary<string, string> Headers = null, IReadOnlyDictionary<string, string> Values = null,
            IReadOnlyCollection<DiscordMessageFile> Files = null, double? RatelimitWaitOverride = null)
        {
            var req = new MultipartWebRequest(Client, Bucket, Url, Method, Route, Headers, Values, Files, RatelimitWaitOverride);

            if (this.Discord != null)
                this.Rest.ExecuteRequest(req).LogTaskFault(this.Discord.Logger, LogLevel.Error, LoggerEvents.RestError, "Error while executing request");
            else
                _ = this.Rest.ExecuteRequest(req);

            return req.WaitForCompletion();
        }

        #region Guild

        /// <summary>
        /// Searches the members async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Limit">The limit.</param>
        internal async Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(ulong GuildId, string Name, int? Limit)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}{Endpoints.Search}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);
            var querydict = new Dictionary<string, string>
            {
                ["query"] = Name,
                ["limit"] = Limit.ToString()
            };
            var url = Utilities.GetApiUriFor(path, BuildQueryString(querydict), this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);
            var json = JArray.Parse(res.Response);
            var tms = json.ToObject<IReadOnlyList<TransportMember>>();

            var mbrs = new List<DiscordMember>();
            foreach (var xtm in tms)
            {
                var usr = new DiscordUser(xtm.User) { Discord = this.Discord };

                this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (Id, Old) =>
                {
                    Old.Username = usr.Username;
                    Old.Discord = usr.Discord;
                    Old.AvatarHash = usr.AvatarHash;

                    return Old;
                });

                mbrs.Add(new DiscordMember(xtm) { Discord = this.Discord, _guildId = GuildId });
            }

            return mbrs;
        }

        /// <summary>
        /// Gets the guild ban async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        internal async Task<DiscordBan> GetGuildBanAsync(ulong GuildId, ulong UserId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Bans}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId, user_id = UserId}, out var path);
            var uri = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, uri, RestRequestMethod.Get, route).ConfigureAwait(false);
            var json = JObject.Parse(res.Response);

            var ban = json.ToObject<DiscordBan>();

            return ban;
        }

        /// <summary>
        /// Creates the guild async.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="region_id">The region_id.</param>
        /// <param name="Iconb64">The iconb64.</param>
        /// <param name="verification_level">The verification_level.</param>
        /// <param name="default_message_notifications">The default_message_notifications.</param>
        /// <param name="system_channel_flags">The system_channel_flags.</param>
        internal async Task<DiscordGuild> CreateGuildAsync(string Name, string RegionId, Optional<string> Iconb64, VerificationLevel? VerificationLevel,
            DefaultMessageNotifications? DefaultMessageNotifications, SystemChannelFlags? SystemChannelFlags)
        {
            var pld = new RestGuildCreatePayload
            {
                Name = Name,
                RegionId = RegionId,
                DefaultMessageNotifications = DefaultMessageNotifications,
                VerificationLevel = VerificationLevel,
                IconBase64 = Iconb64,
                SystemChannelFlags = SystemChannelFlags

            };

            var route = $"{Endpoints.Guilds}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildCreateEventAsync(guild, rawMembers, null).ConfigureAwait(false);
            return guild;
        }

        /// <summary>
        /// Creates the guild from template async.
        /// </summary>
        /// <param name="template_code">The template_code.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Iconb64">The iconb64.</param>
        internal async Task<DiscordGuild> CreateGuildFromTemplateAsync(string TemplateCode, string Name, Optional<string> Iconb64)
        {
            var pld = new RestGuildCreateFromTemplatePayload
            {
                Name = Name,
                IconBase64 = Iconb64
            };

            var route = $"{Endpoints.Guilds}{Endpoints.Templates}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { template_code = TemplateCode }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildCreateEventAsync(guild, rawMembers, null).ConfigureAwait(false);
            return guild;
        }

        /// <summary>
        /// Deletes the guild async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        internal async Task DeleteGuildAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route).ConfigureAwait(false);

            if (this.Discord is DiscordClient dc)
            {
                var gld = dc._guilds[GuildId];
                await dc.OnGuildDeleteEventAsync(gld).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Modifies the guild.
        /// </summary>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="VerificationLevel">The verification level.</param>
        /// <param name="DefaultMessageNotifications">The default message notifications.</param>
        /// <param name="MfaLevel">The mfa level.</param>
        /// <param name="ExplicitContentFilter">The explicit content filter.</param>
        /// <param name="AfkChannelId">The afk channel id.</param>
        /// <param name="AfkTimeout">The afk timeout.</param>
        /// <param name="Iconb64">The iconb64.</param>
        /// <param name="OwnerId">The owner id.</param>
        /// <param name="Splashb64">The splashb64.</param>
        /// <param name="SystemChannelId">The system channel id.</param>
        /// <param name="SystemChannelFlags">The system channel flags.</param>
        /// <param name="PublicUpdatesChannelId">The public updates channel id.</param>
        /// <param name="RulesChannelId">The rules channel id.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Bannerb64">The banner base64.</param>
        /// <param name="DiscorverySplashb64">The discovery base64.</param>
        /// <param name="PreferredLocale">The preferred locale.</param>
        /// <param name="PremiumProgressBarEnabled">Whether the premium progress bar should be enabled.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordGuild> ModifyGuildAsync(ulong GuildId, Optional<string> Name, Optional<VerificationLevel> VerificationLevel,
            Optional<DefaultMessageNotifications> DefaultMessageNotifications, Optional<MfaLevel> MfaLevel,
            Optional<ExplicitContentFilter> ExplicitContentFilter, Optional<ulong?> AfkChannelId,
            Optional<int> AfkTimeout, Optional<string> Iconb64, Optional<ulong> OwnerId, Optional<string> Splashb64,
            Optional<ulong?> SystemChannelId, Optional<SystemChannelFlags> SystemChannelFlags,
            Optional<ulong?> PublicUpdatesChannelId, Optional<ulong?> RulesChannelId, Optional<string> Description,
            Optional<string> Bannerb64, Optional<string> DiscorverySplashb64, Optional<string> PreferredLocale, Optional<bool> PremiumProgressBarEnabled, string Reason)
        {
            var pld = new RestGuildModifyPayload
            {
                Name = Name,
                VerificationLevel = VerificationLevel,
                DefaultMessageNotifications = DefaultMessageNotifications,
                MfaLevel = MfaLevel,
                ExplicitContentFilter = ExplicitContentFilter,
                AfkChannelId = AfkChannelId,
                AfkTimeout = AfkTimeout,
                IconBase64 = Iconb64,
                SplashBase64 = Splashb64,
                BannerBase64 = Bannerb64,
                DiscoverySplashBase64 = DiscorverySplashb64,
                OwnerId = OwnerId,
                SystemChannelId = SystemChannelId,
                SystemChannelFlags = SystemChannelFlags,
                RulesChannelId = RulesChannelId,
                PublicUpdatesChannelId = PublicUpdatesChannelId,
                PreferredLocale = PreferredLocale,
                Description = Description,
                PremiumProgressBarEnabled = PremiumProgressBarEnabled
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();
            foreach (var r in guild._roles.Values)
                r._guildId = guild.Id;

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildUpdateEventAsync(guild, rawMembers).ConfigureAwait(false);
            return guild;
        }



        /// <summary>
        /// Modifies the guild community settings.
        /// </summary>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="Features">The guild features.</param>
        /// <param name="RulesChannelId">The rules channel id.</param>
        /// <param name="PublicUpdatesChannelId">The public updates channel id.</param>
        /// <param name="PreferredLocale">The preferred locale.</param>
        /// <param name="Description">The description.</param>
        /// <param name="DefaultMessageNotifications">The default message notifications.</param>
        /// <param name="ExplicitContentFilter">The explicit content filter.</param>
        /// <param name="VerificationLevel">The verification level.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordGuild> ModifyGuildCommunitySettingsAsync(ulong GuildId, List<string> Features, Optional<ulong?> RulesChannelId, Optional<ulong?> PublicUpdatesChannelId, string PreferredLocale, string Description, DefaultMessageNotifications DefaultMessageNotifications, ExplicitContentFilter ExplicitContentFilter, VerificationLevel VerificationLevel, string Reason)
        {
            var pld = new RestGuildCommunityModifyPayload
            {
                VerificationLevel = VerificationLevel,
                DefaultMessageNotifications = DefaultMessageNotifications,
                ExplicitContentFilter = ExplicitContentFilter,
                RulesChannelId = RulesChannelId,
                PublicUpdatesChannelId = PublicUpdatesChannelId,
                PreferredLocale = PreferredLocale,
                Description = Description ?? Optional.FromNoValue<string>(),
                Features = Features
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guild = json.ToDiscordObject<DiscordGuild>();
            foreach (var r in guild._roles.Values)
                r._guildId = guild.Id;

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildUpdateEventAsync(guild, rawMembers).ConfigureAwait(false);
            return guild;
        }

        /// <summary>
        /// Gets the guild bans async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordBan>> GetGuildBansAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Bans}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var bansRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordBan>>(res.Response).Select(Xb =>
            {
                if (!this.Discord.TryGetCachedUserInternal(Xb.RawUser.Id, out var usr))
                {
                    usr = new DiscordUser(Xb.RawUser) { Discord = this.Discord };
                    usr = this.Discord.UserCache.AddOrUpdate(usr.Id, usr, (Id, Old) =>
                    {
                        Old.Username = usr.Username;
                        Old.Discriminator = usr.Discriminator;
                        Old.AvatarHash = usr.AvatarHash;
                        return Old;
                    });
                }

                Xb.User = usr;
                return Xb;
            });
            var bans = new ReadOnlyCollection<DiscordBan>(new List<DiscordBan>(bansRaw));

            return bans;
        }

        /// <summary>
        /// Creates the guild ban async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="delete_message_days">The delete_message_days.</param>
        /// <param name="Reason">The reason.</param>
        internal Task CreateGuildBan(ulong GuildId, ulong UserId, int DeleteMessageDays, string Reason)
        {
            if (DeleteMessageDays < 0 || DeleteMessageDays > 7)
                throw new ArgumentException("Delete message days must be a number between 0 and 7.", nameof(DeleteMessageDays));

            var urlparams = new Dictionary<string, string>
            {
                ["delete_message_days"] = DeleteMessageDays.ToString(CultureInfo.InvariantCulture)
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Bans}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams), this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, headers);
        }

        /// <summary>
        /// Removes the guild ban async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGuildBan(ulong GuildId, ulong UserId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Bans}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Leaves the guild async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal Task LeaveGuild(ulong GuildId)
        {
            var route = $"{Endpoints.Users}{Endpoints.Me}{Endpoints.Guilds}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Adds the guild member async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="access_token">The access_token.</param>
        /// <param name="Nick">The nick.</param>
        /// <param name="Roles">The roles.</param>
        /// <param name="Muted">If true, muted.</param>
        /// <param name="Deafened">If true, deafened.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMember> AddGuildMemberAsync(ulong GuildId, ulong UserId, string AccessToken, string Nick, IEnumerable<DiscordRole> Roles, bool Muted, bool Deafened)
        {
            var pld = new RestGuildMemberAddPayload
            {
                AccessToken = AccessToken,
                Nickname = Nick ?? "",
                Roles = Roles ?? new List<DiscordRole>(),
                Deaf = Deafened,
                Mute = Muted
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            return new DiscordMember(tm) { Discord = this.Discord, _guildId = GuildId };
        }

        /// <summary>
        /// Lists the guild members async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Limit">The limit.</param>
        /// <param name="After">The after.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<TransportMember>> ListGuildMembersAsync(ulong GuildId, int? Limit, ulong? After)
        {
            var urlparams = new Dictionary<string, string>();
            if (Limit != null && Limit > 0)
                urlparams["limit"] = Limit.Value.ToString(CultureInfo.InvariantCulture);
            if (After != null)
                urlparams["after"] = After.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var membersRaw = JsonConvert.DeserializeObject<List<TransportMember>>(res.Response);
            return new ReadOnlyCollection<TransportMember>(membersRaw);
        }

        /// <summary>
        /// Adds the guild member role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="role_id">The role_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task AddGuildMemberRole(ulong GuildId, ulong UserId, ulong RoleId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}/:user_id{Endpoints.Roles}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { guild_id = GuildId, user_id = UserId, role_id = RoleId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, headers);
        }

        /// <summary>
        /// Removes the guild member role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="role_id">The role_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGuildMemberRole(ulong GuildId, ulong UserId, ulong RoleId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}/:user_id{Endpoints.Roles}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, user_id = UserId, role_id = RoleId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Modifies the guild channel position async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Pld">The pld.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildChannelPosition(ulong GuildId, IEnumerable<RestGuildChannelReorderPayload> Pld, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Channels}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(Pld));
        }

        /// <summary>
        /// Modifies the guild channel parent async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Pld">The pld.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildChannelParent(ulong GuildId, IEnumerable<RestGuildChannelNewParentPayload> Pld, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Channels}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(Pld));
        }

        /// <summary>
        /// Detaches the guild channel parent async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Pld">The pld.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DetachGuildChannelParent(ulong GuildId, IEnumerable<RestGuildChannelNoParentPayload> Pld, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Channels}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(Pld));
        }

        /// <summary>
        /// Modifies the guild role position async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Pld">The pld.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildRolePosition(ulong GuildId, IEnumerable<RestGuildRoleReorderPayload> Pld, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Roles}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(Pld));
        }

        /// <summary>
        /// Gets the audit logs async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Limit">The limit.</param>
        /// <param name="After">The after.</param>
        /// <param name="Before">The before.</param>
        /// <param name="Responsible">The responsible.</param>
        /// <param name="action_type">The action_type.</param>
        /// <returns>A Task.</returns>
        internal async Task<AuditLog> GetAuditLogsAsync(ulong GuildId, int Limit, ulong? After, ulong? Before, ulong? Responsible, int? ActionType)
        {
            var urlparams = new Dictionary<string, string>
            {
                ["limit"] = Limit.ToString(CultureInfo.InvariantCulture)
            };
            if (After != null)
                urlparams["after"] = After?.ToString(CultureInfo.InvariantCulture);
            if (Before != null)
                urlparams["before"] = Before?.ToString(CultureInfo.InvariantCulture);
            if (Responsible != null)
                urlparams["user_id"] = Responsible?.ToString(CultureInfo.InvariantCulture);
            if (ActionType != null)
                urlparams["action_type"] = ActionType?.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.AuditLogs}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var auditLogDataRaw = JsonConvert.DeserializeObject<AuditLog>(res.Response);

            return auditLogDataRaw;
        }

        /// <summary>
        /// Gets the guild vanity url async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> GetGuildVanityUrlAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.VanityUrl}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var invite = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);

            return invite;
        }

        /// <summary>
        /// Gets the guild widget async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWidget> GetGuildWidgetAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.WidgetJson}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawChannels = (JArray)json["channels"];

            var ret = json.ToDiscordObject<DiscordWidget>();
            ret.Discord = this.Discord;
            ret.Guild = this.Discord.Guilds[GuildId];

            ret.Channels = ret.Guild == null
                ? rawChannels.Select(R => new DiscordChannel
                {
                    Id = (ulong)R["id"],
                    Name = R["name"].ToString(),
                    Position = (int)R["position"]
                }).ToList()
                : rawChannels.Select(R =>
                {
                    var c = ret.Guild.GetChannel((ulong)R["id"]);
                    c.Position = (int)R["position"];
                    return c;
                }).ToList();

            return ret;
        }

        /// <summary>
        /// Gets the guild widget settings async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWidgetSettings> GetGuildWidgetSettingsAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Widget}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[GuildId];

            return ret;
        }

        /// <summary>
        /// Modifies the guild widget settings async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="IsEnabled">If true, is enabled.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWidgetSettings> ModifyGuildWidgetSettingsAsync(ulong GuildId, bool? IsEnabled, ulong? ChannelId, string Reason)
        {
            var pld = new RestGuildWidgetSettingsPayload
            {
                Enabled = IsEnabled,
                ChannelId = ChannelId
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Widget}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWidgetSettings>(res.Response);
            ret.Guild = this.Discord.Guilds[GuildId];

            return ret;
        }

        /// <summary>
        /// Gets the guild templates async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Templates}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var templatesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response);

            return new ReadOnlyCollection<DiscordGuildTemplate>(new List<DiscordGuildTemplate>(templatesRaw));
        }

        /// <summary>
        /// Creates the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong GuildId, string Name, string Description)
        {
            var pld = new RestGuildTemplateCreateOrModifyPayload
            {
                Name = Name,
                Description = Description
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Templates}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return ret;
        }

        /// <summary>
        /// Syncs the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="template_code">The template_code.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong GuildId, string TemplateCode)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Templates}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { guild_id = GuildId, template_code = TemplateCode }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route).ConfigureAwait(false);

            var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return templateRaw;
        }

        /// <summary>
        /// Modifies the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="template_code">The template_code.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong GuildId, string TemplateCode, string Name, string Description)
        {
            var pld = new RestGuildTemplateCreateOrModifyPayload
            {
                Name = Name,
                Description = Description
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Templates}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, template_code = TemplateCode }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return templateRaw;
        }

        /// <summary>
        /// Deletes the guild template async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="template_code">The template_code.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong GuildId, string TemplateCode)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Templates}/:template_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, template_code = TemplateCode }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route).ConfigureAwait(false);

            var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return templateRaw;
        }

        /// <summary>
        /// Gets the guild membership screening form async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildMembershipScreening> GetGuildMembershipScreeningFormAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.MemberVerification}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var screeningRaw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

            return screeningRaw;
        }

        /// <summary>
        /// Modifies the guild membership screening form async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Enabled">The enabled.</param>
        /// <param name="Fields">The fields.</param>
        /// <param name="Description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildMembershipScreening> ModifyGuildMembershipScreeningFormAsync(ulong GuildId, Optional<bool> Enabled, Optional<DiscordGuildMembershipScreeningField[]> Fields, Optional<string> Description)
        {
            var pld = new RestGuildMembershipScreeningFormModifyPayload
            {
                Enabled = Enabled,
                Description = Description,
                Fields = Fields
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.MemberVerification}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var screeningRaw = JsonConvert.DeserializeObject<DiscordGuildMembershipScreening>(res.Response);

            return screeningRaw;
        }

        /// <summary>
        /// Gets the guild welcome screen async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildWelcomeScreen> GetGuildWelcomeScreenAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.WelcomeScreen}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        /// <summary>
        /// Modifies the guild welcome screen async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Enabled">The enabled.</param>
        /// <param name="WelcomeChannels">The welcome channels.</param>
        /// <param name="Description">The description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildWelcomeScreen> ModifyGuildWelcomeScreenAsync(ulong GuildId, Optional<bool> Enabled, Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> WelcomeChannels, Optional<string> Description)
        {
            var pld = new RestGuildWelcomeScreenModifyPayload
            {
                Enabled = Enabled,
                WelcomeChannels = WelcomeChannels,
                Description = Description
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.WelcomeScreen}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordGuildWelcomeScreen>(res.Response);
            return ret;
        }

        /// <summary>
        /// Updates the current user voice state async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="Suppress">If true, suppress.</param>
        /// <param name="RequestToSpeakTimestamp">The request to speak timestamp.</param>
        /// <returns>A Task.</returns>
        internal async Task UpdateCurrentUserVoiceStateAsync(ulong GuildId, ulong ChannelId, bool? Suppress, DateTimeOffset? RequestToSpeakTimestamp)
        {
            var pld = new RestGuildUpdateCurrentUserVoiceStatePayload
            {
                ChannelId = ChannelId,
                Suppress = Suppress,
                RequestToSpeakTimestamp = RequestToSpeakTimestamp
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.VoiceStates}/@me";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Updates the user voice state async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="Suppress">If true, suppress.</param>
        /// <returns>A Task.</returns>
        internal async Task UpdateUserVoiceStateAsync(ulong GuildId, ulong UserId, ulong ChannelId, bool? Suppress)
        {
            var pld = new RestGuildUpdateUserVoiceStatePayload
            {
                ChannelId = ChannelId,
                Suppress = Suppress
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.VoiceStates}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld));
        }
        #endregion

        #region Guild Scheduled Events

        /// <summary>
        /// Creates a scheduled event.
        /// </summary>
        internal async Task<DiscordScheduledEvent> CreateGuildScheduledEventAsync(ulong GuildId, ulong? ChannelId, DiscordScheduledEventEntityMetadata Metadata, string Name, DateTimeOffset ScheduledStartTime, DateTimeOffset? ScheduledEndTime, string Description, ScheduledEventEntityType Type, string Reason = null)
        {
            var pld = new RestGuildScheduledEventCreatePayload
            {
                ChannelId = ChannelId,
                EntityMetadata = Metadata,
                Name = Name,
                ScheduledStartTime = ScheduledStartTime,
                ScheduledEndTime = ScheduledEndTime,
                Description = Description,
                EntityType = Type
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.ScheduledEvents}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld));

            var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
            var guild = this.Discord.Guilds[GuildId];

            scheduledEvent.Discord = this.Discord;

            if (scheduledEvent.Creator != null)
                scheduledEvent.Creator.Discord = this.Discord;

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildScheduledEventCreateEventAsync(scheduledEvent, guild);

            return scheduledEvent;
        }

        /// <summary>
        /// Modifies a scheduled event.
        /// </summary>
        internal async Task<DiscordScheduledEvent> ModifyGuildScheduledEventAsync(ulong GuildId, ulong ScheduledEventId, Optional<ulong?> ChannelId, Optional<DiscordScheduledEventEntityMetadata> Metadata, Optional<string> Name, Optional<DateTimeOffset> ScheduledStartTime, Optional<DateTimeOffset> ScheduledEndTime, Optional<string> Description, Optional<ScheduledEventEntityType> Type, Optional<ScheduledEventStatus> Status, string Reason = null)
        {
            var pld = new RestGuildSheduledEventModifyPayload
            {
                ChannelId = ChannelId,
                EntityMetadata = Metadata,
                Name = Name,
                ScheduledStartTime = ScheduledStartTime,
                ScheduledEndTime = ScheduledEndTime,
                Description = Description,
                EntityType = Type,
                Status = Status
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.ScheduledEvents}/:scheduled_event_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, scheduled_event_id = ScheduledEventId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld));

            var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
            var guild = this.Discord.Guilds[GuildId];

            scheduledEvent.Discord = this.Discord;

            if (scheduledEvent.Creator != null)
            {
                scheduledEvent.Creator.Discord = this.Discord;
                this.Discord.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (Id, Old) =>
                {
                    Old.Username = scheduledEvent.Creator.Username;
                    Old.Discriminator = scheduledEvent.Creator.Discriminator;
                    Old.AvatarHash = scheduledEvent.Creator.AvatarHash;
                    Old.Flags = scheduledEvent.Creator.Flags;
                    return Old;
                });
            }

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildScheduledEventUpdateEventAsync(scheduledEvent, guild);

            return scheduledEvent;
        }

        /// <summary>
        /// Modifies a scheduled event.
        /// </summary>
        internal async Task<DiscordScheduledEvent> ModifyGuildScheduledEventStatusAsync(ulong GuildId, ulong ScheduledEventId, ScheduledEventStatus Status, string Reason = null)
        {
            var pld = new RestGuildSheduledEventModifyPayload
            {
                Status = Status
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.ScheduledEvents}/:scheduled_event_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, scheduled_event_id = ScheduledEventId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld));

            var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
            var guild = this.Discord.Guilds[GuildId];

            scheduledEvent.Discord = this.Discord;

            if (scheduledEvent.Creator != null)
            {
                scheduledEvent.Creator.Discord = this.Discord;
                this.Discord.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (Id, Old) =>
                {
                    Old.Username = scheduledEvent.Creator.Username;
                    Old.Discriminator = scheduledEvent.Creator.Discriminator;
                    Old.AvatarHash = scheduledEvent.Creator.AvatarHash;
                    Old.Flags = scheduledEvent.Creator.Flags;
                    return Old;
                });
            }

            if (this.Discord is DiscordClient dc)
                await dc.OnGuildScheduledEventUpdateEventAsync(scheduledEvent, guild);

            return scheduledEvent;
        }

        /// <summary>
        /// Gets a scheduled event.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="scheduled_event_id">The event id.</param>
        /// <param name="with_user_count">Whether to include user count.</param>
        internal async Task<DiscordScheduledEvent> GetGuildScheduledEventAsync(ulong GuildId, ulong ScheduledEventId, bool? WithUserCount)
        {
            var urlparams = new Dictionary<string, string>();
            if (WithUserCount.HasValue)
                urlparams["with_user_count"] = WithUserCount?.ToString();

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.ScheduledEvents}/:scheduled_event_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId, scheduled_event_id = ScheduledEventId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);

            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var scheduledEvent = JsonConvert.DeserializeObject<DiscordScheduledEvent>(res.Response);
            var guild = this.Discord.Guilds[GuildId];

            scheduledEvent.Discord = this.Discord;

            if (scheduledEvent.Creator != null)
            {
                scheduledEvent.Creator.Discord = this.Discord;
                this.Discord.UserCache.AddOrUpdate(scheduledEvent.Creator.Id, scheduledEvent.Creator, (Id, Old) =>
                {
                    Old.Username = scheduledEvent.Creator.Username;
                    Old.Discriminator = scheduledEvent.Creator.Discriminator;
                    Old.AvatarHash = scheduledEvent.Creator.AvatarHash;
                    Old.Flags = scheduledEvent.Creator.Flags;
                    return Old;
                });
            }

            return scheduledEvent;
        }

        /// <summary>
        /// Gets the guilds scheduled events.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="with_user_count">Whether to include the count of users subscribed to the scheduled event.</param>
        internal async Task<IReadOnlyDictionary<ulong, DiscordScheduledEvent>> ListGuildScheduledEventsAsync(ulong GuildId, bool? WithUserCount)
        {
            var urlparams = new Dictionary<string, string>();
            if (WithUserCount.HasValue)
                urlparams["with_user_count"] = WithUserCount?.ToString();

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.ScheduledEvents}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var events = new Dictionary<ulong, DiscordScheduledEvent>();
            var eventsRaw = JsonConvert.DeserializeObject<List<DiscordScheduledEvent>>(res.Response);
            var guild = this.Discord.Guilds[GuildId];

            foreach (var ev in eventsRaw)
            {
                ev.Discord = this.Discord;
                if (ev.Creator != null)
                {
                    ev.Creator.Discord = this.Discord;
                    this.Discord.UserCache.AddOrUpdate(ev.Creator.Id, ev.Creator, (Id, Old) =>
                    {
                        Old.Username = ev.Creator.Username;
                        Old.Discriminator = ev.Creator.Discriminator;
                        Old.AvatarHash = ev.Creator.AvatarHash;
                        Old.Flags = ev.Creator.Flags;
                        return Old;
                    });
                }

                events.Add(ev.Id, ev);
            }

            return new ReadOnlyDictionary<ulong, DiscordScheduledEvent>(new Dictionary<ulong, DiscordScheduledEvent>(events));
        }

        /// <summary>
        /// Deletes a guild sheduled event.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="scheduled_event_id">The sheduled event id.</param>
        /// <param name="Reason">The reason.</param>
        internal Task DeleteGuildScheduledEvent(ulong GuildId, ulong ScheduledEventId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.ScheduledEvents}/:scheduled_event_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, scheduled_event_id = ScheduledEventId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Gets the users who RSVP'd to a sheduled event.
        /// Optional with member objects.
        /// This endpoint is paginated.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="scheduled_event_id">The sheduled event id.</param>
        /// <param name="Limit">The limit how many users to receive from the event.</param>
        /// <param name="Before">Get results before the given id.</param>
        /// <param name="After">Get results after the given id.</param>
        /// <param name="with_member">Wether to include guild member data. attaches guild_member property to the user object.</param>
        internal async Task<IReadOnlyDictionary<ulong, DiscordScheduledEventUser>> GetGuildScheduledEventRspvUsersAsync(ulong GuildId, ulong ScheduledEventId, int? Limit, ulong? Before, ulong? After, bool? WithMember)
        {
            var urlparams = new Dictionary<string, string>();
            if (Limit != null && Limit > 0)
                urlparams["limit"] = Limit.Value.ToString(CultureInfo.InvariantCulture);
            if (Before != null)
                urlparams["before"] = Before.Value.ToString(CultureInfo.InvariantCulture);
            if (After != null)
                urlparams["after"] = After.Value.ToString(CultureInfo.InvariantCulture);
            if (WithMember != null)
                urlparams["with_member"] = WithMember.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.ScheduledEvents}/:scheduled_event_id{Endpoints.Users}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId, scheduled_event_id = ScheduledEventId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var rspvUsers = JsonConvert.DeserializeObject<IEnumerable<DiscordScheduledEventUser>>(res.Response);
            Dictionary<ulong, DiscordScheduledEventUser> rspv = new();

            foreach (var rspvUser in rspvUsers)
            {

                rspvUser.Discord = this.Discord;
                rspvUser.GuildId = GuildId;

                rspvUser.User.Discord = this.Discord;
                rspvUser.User = this.Discord.UserCache.AddOrUpdate(rspvUser.User.Id, rspvUser.User, (Id, Old) =>
                {
                    Old.Username = rspvUser.User.Username;
                    Old.Discriminator = rspvUser.User.Discriminator;
                    Old.AvatarHash = rspvUser.User.AvatarHash;
                    Old.BannerHash = rspvUser.User.BannerHash;
                    Old._bannerColor = rspvUser.User._bannerColor;
                    return Old;
                });

                /*if (with_member.HasValue && with_member.Value && rspv_user.Member != null)
                {
                    rspv_user.Member.Discord = this.Discord;
                }*/

                rspv.Add(rspvUser.User.Id, rspvUser);
            }

            return new ReadOnlyDictionary<ulong, DiscordScheduledEventUser>(new Dictionary<ulong, DiscordScheduledEventUser>(rspv));
        }
        #endregion

        #region Channel
        /// <summary>
        /// Creates the guild channel async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Type">The type.</param>
        /// <param name="Parent">The parent.</param>
        /// <param name="Topic">The topic.</param>
        /// <param name="Bitrate">The bitrate.</param>
        /// <param name="user_limit">The user_limit.</param>
        /// <param name="Overwrites">The overwrites.</param>
        /// <param name="Nsfw">If true, nsfw.</param>
        /// <param name="PerUserRateLimit">The per user rate limit.</param>
        /// <param name="QualityMode">The quality mode.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordChannel> CreateGuildChannelAsync(ulong GuildId, string Name, ChannelType Type, ulong? Parent, Optional<string> Topic, int? Bitrate, int? UserLimit, IEnumerable<DiscordOverwriteBuilder> Overwrites, bool? Nsfw, Optional<int?> PerUserRateLimit, VideoQualityMode? QualityMode, string Reason)
        {
            var restoverwrites = new List<DiscordRestOverwrite>();
            if (Overwrites != null)
                foreach (var ow in Overwrites)
                    restoverwrites.Add(ow.Build());

            var pld = new RestChannelCreatePayload
            {
                Name = Name,
                Type = Type,
                Parent = Parent,
                Topic = Topic,
                Bitrate = Bitrate,
                UserLimit = UserLimit,
                PermissionOverwrites = restoverwrites,
                Nsfw = Nsfw,
                PerUserRateLimit = PerUserRateLimit,
                QualityMode = QualityMode
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Channels}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;
            foreach (var xo in ret._permissionOverwrites)
            {
                xo.Discord = this.Discord;
                xo._channelId = ret.Id;
            }

            return ret;
        }

        /// <summary>
        /// Modifies the channel async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Position">The position.</param>
        /// <param name="Topic">The topic.</param>
        /// <param name="Nsfw">If true, nsfw.</param>
        /// <param name="Parent">The parent.</param>
        /// <param name="Bitrate">The bitrate.</param>
        /// <param name="user_limit">The user_limit.</param>
        /// <param name="PerUserRateLimit">The per user rate limit.</param>
        /// <param name="RtcRegion">The rtc region.</param>
        /// <param name="QualityMode">The quality mode.</param>
        /// <param name="AutoArchiveDuration">The default auto archive duration.</param>
        /// <param name="Type">The type.</param>
        /// <param name="PermissionOverwrites">The permission overwrites.</param>
        /// <param name="Bannerb64">The banner.</param>
        /// <param name="Reason">The reason.</param>
        internal Task ModifyChannel(ulong ChannelId, string Name, int? Position, Optional<string> Topic, bool? Nsfw, Optional<ulong?> Parent, int? Bitrate, int? UserLimit, Optional<int?> PerUserRateLimit, Optional<string> RtcRegion, VideoQualityMode? QualityMode, ThreadAutoArchiveDuration? AutoArchiveDuration, Optional<ChannelType> Type, IEnumerable<DiscordOverwriteBuilder> PermissionOverwrites, Optional<string> Bannerb64, string Reason)
        {

            List<DiscordRestOverwrite> restoverwrites = null;
            if (PermissionOverwrites != null)
            {
                restoverwrites = new List<DiscordRestOverwrite>();
                foreach (var ow in PermissionOverwrites)
                    restoverwrites.Add(ow.Build());
            }

            var pld = new RestChannelModifyPayload
            {
                Name = Name,
                Position = Position,
                Topic = Topic,
                Nsfw = Nsfw,
                Parent = Parent,
                Bitrate = Bitrate,
                UserLimit = UserLimit,
                PerUserRateLimit = PerUserRateLimit,
                RtcRegion = RtcRegion,
                QualityMode = QualityMode,
                DefaultAutoArchiveDuration = AutoArchiveDuration,
                Type = Type,
                PermissionOverwrites = restoverwrites,
                BannerBase64 = Bannerb64
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets the channel async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordChannel> GetChannelAsync(ulong ChannelId)
        {
            var route = $"{Endpoints.Channels}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordChannel>(res.Response);
            ret.Discord = this.Discord;
            foreach (var xo in ret._permissionOverwrites)
            {
                xo.Discord = this.Discord;
                xo._channelId = ret.Id;
            }

            return ret;
        }

        /// <summary>
        /// Deletes the channel async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteChannel(ulong ChannelId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Gets the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> GetMessageAsync(ulong ChannelId, ulong MessageId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Creates the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Content">The content.</param>
        /// <param name="Embeds">The embeds.</param>
        /// <param name="Sticker">The sticker.</param>
        /// <param name="ReplyMessageId">The reply message id.</param>
        /// <param name="MentionReply">If true, mention reply.</param>
        /// <param name="FailOnInvalidReply">If true, fail on invalid reply.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CreateMessageAsync(ulong ChannelId, string Content, IEnumerable<DiscordEmbed> Embeds, DiscordSticker Sticker, ulong? ReplyMessageId, bool MentionReply, bool FailOnInvalidReply)
        {
            if (Content != null && Content.Length > 2000)
                throw new ArgumentException("Message content length cannot exceed 2000 characters.");

            if (!Embeds?.Any() ?? true)
            {
                if (Content == null && Sticker == null)
                    throw new ArgumentException("You must specify message content, a sticker or an embed.");
                if (Content.Length == 0)
                    throw new ArgumentException("Message content must not be empty.");
            }

            if (Embeds != null)
                foreach (var embed in Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = Content != null,
                Content = Content,
                StickersIds = Sticker is null ? Array.Empty<ulong>() : new[] {Sticker.Id},
                IsTts = false,
                HasEmbed = Embeds?.Any() ?? false,
                Embeds = Embeds
            };

            if (ReplyMessageId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = ReplyMessageId, FailIfNotExists = FailOnInvalidReply };

            if (ReplyMessageId != null)
                pld.Mentions = new DiscordMentions(Mentions.All, true, MentionReply);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Creates the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CreateMessageAsync(ulong ChannelId, DiscordMessageBuilder Builder)
        {
            Builder.Validate();

            if (Builder.Embeds != null)
                foreach (var embed in Builder.Embeds)
                    if (embed?.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageCreatePayload
            {
                HasContent = Builder.Content != null,
                Content = Builder.Content,
                StickersIds = Builder.Sticker is null ? Array.Empty<ulong>() : new[] {Builder.Sticker.Id},
                IsTts = Builder.IsTts,
                HasEmbed = Builder.Embeds != null,
                Embeds = Builder.Embeds,
                Components = Builder.Components
            };

            if (Builder.ReplyId != null)
                pld.MessageReference = new InternalDiscordMessageReference { MessageId = Builder.ReplyId, FailIfNotExists = Builder.FailOnInvalidReply };

            pld.Mentions = new DiscordMentions(Builder.Mentions ?? Mentions.All, Builder.Mentions?.Any() ?? false, Builder.MentionOnReply);

            if (Builder.Files.Count == 0)
            {
                var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}";
                var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

                var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
                var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));
                return ret;
            }
            else
            {
                ulong fileId = 0;
                List<DiscordAttachment> attachments = new(Builder.Files.Count);
                foreach (var file in Builder.Files)
                {
                    DiscordAttachment att = new()
                    {
                        Id = fileId,
                        Discord = this.Discord,
                        Description = file.Description,
                        FileName = file.FileName
                    };
                    attachments.Add(att);
                    fileId++;
                }
                pld.Attachments = attachments;

                var values = new Dictionary<string, string>
                {
                    ["payload_json"] = DiscordJson.SerializeObject(pld)
                };

                var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}";
                var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

                var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
                var res = await this.DoMultipart(this.Discord, bucket, url, RestRequestMethod.Post, route, Values: values, Files: Builder.Files).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));

                foreach (var file in Builder._files.Where(X => X.ResetPositionTo.HasValue))
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
        internal async Task<IReadOnlyList<DiscordChannel>> GetGuildChannelsAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Channels}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var channelsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordChannel>>(res.Response).Select(Xc => { Xc.Discord = this.Discord; return Xc; });

            foreach (var ret in channelsRaw)
                foreach (var xo in ret._permissionOverwrites)
                {
                    xo.Discord = this.Discord;
                    xo._channelId = ret.Id;
                }

            return new ReadOnlyCollection<DiscordChannel>(new List<DiscordChannel>(channelsRaw));
        }

        /// <summary>
        /// Creates the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Topic">The topic.</param>
        /// <param name="send_start_notification">Whether everyone should be notified about the stage.</param>
        /// <param name="privacy_level">The privacy_level.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordStageInstance> CreateStageInstanceAsync(ulong ChannelId, string Topic, bool SendStartNotification, StagePrivacyLevel PrivacyLevel, string Reason)
        {
            var pld = new RestStageInstanceCreatePayload
            {
                ChannelId = ChannelId,
                Topic = Topic,
                PrivacyLevel = PrivacyLevel,
                SendStartNotification = SendStartNotification
            };

            var route = $"{Endpoints.StageInstances}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { }, out var path);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

            return stageInstance;
        }

        /// <summary>
        /// Gets the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        internal async Task<DiscordStageInstance> GetStageInstanceAsync(ulong ChannelId)
        {
            var route = $"{Endpoints.StageInstances}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

            return stageInstance;
        }

        /// <summary>
        /// Modifies the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Topic">The topic.</param>
        /// <param name="privacy_level">The privacy_level.</param>
        /// <param name="Reason">The reason.</param>
        internal Task ModifyStageInstance(ulong ChannelId, Optional<string> Topic, Optional<StagePrivacyLevel> PrivacyLevel, string Reason)
        {
            var pld = new RestStageInstanceModifyPayload
            {
                Topic = Topic,
                PrivacyLevel = PrivacyLevel
            };

            var route = $"{Endpoints.StageInstances}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { channel_id = ChannelId }, out var path);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Deletes the stage instance async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Reason">The reason.</param>
        internal Task DeleteStageInstance(ulong ChannelId, string Reason)
        {
            var route = $"{Endpoints.StageInstances}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId }, out var path);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Gets the channel messages async.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="Limit">The limit.</param>
        /// <param name="Before">The before.</param>
        /// <param name="After">The after.</param>
        /// <param name="Around">The around.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong ChannelId, int Limit, ulong? Before, ulong? After, ulong? Around)
        {
            var urlparams = new Dictionary<string, string>();
            if (Around != null)
                urlparams["around"] = Around?.ToString(CultureInfo.InvariantCulture);
            if (Before != null)
                urlparams["before"] = Before?.ToString(CultureInfo.InvariantCulture);
            if (After != null)
                urlparams["after"] = After?.ToString(CultureInfo.InvariantCulture);
            if (Limit > 0)
                urlparams["limit"] = Limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var msgsRaw = JArray.Parse(res.Response);
            var msgs = new List<DiscordMessage>();
            foreach (var xj in msgsRaw)
                msgs.Add(this.PrepareMessage(xj));

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        /// <summary>
        /// Gets the channel message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> GetChannelMessageAsync(ulong ChannelId, ulong MessageId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = this.PrepareMessage(JObject.Parse(res.Response));

            return ret;
        }

        /// <summary>
        /// Edits the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Content">The content.</param>
        /// <param name="Embeds">The embeds.</param>
        /// <param name="Mentions">The mentions.</param>
        /// <param name="Components">The components.</param>
        /// <param name="suppress_embed">The suppress_embed.</param>
        /// <param name="Files">The files.</param>
        /// <param name="Attachments">The attachments to keep.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> EditMessageAsync(ulong ChannelId, ulong MessageId, Optional<string> Content, Optional<IEnumerable<DiscordEmbed>> Embeds, IEnumerable<IMention> Mentions, IReadOnlyList<DiscordActionRowComponent> Components, Optional<bool> SuppressEmbed, IReadOnlyCollection<DiscordMessageFile> Files, Optional<IEnumerable<DiscordAttachment>> Attachments)
        {
            if (Embeds.HasValue && Embeds.Value != null)
                foreach (var embed in Embeds.Value)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var pld = new RestChannelMessageEditPayload
            {
                HasContent = Content.HasValue,
                Content = Content.HasValue ? (string)Content : null,
                HasEmbed = Embeds.HasValue && (Embeds.Value?.Any() ?? false),
                Embeds = Embeds.HasValue && (Embeds.Value?.Any() ?? false) ? Embeds.Value : null,
                Components = Components ?? null,
                Flags = SuppressEmbed.HasValue ? (bool)SuppressEmbed ? MessageFlags.SuppressedEmbeds : null : null
            };

            pld.Mentions = new DiscordMentions(Mentions ?? Entities.Mentions.None, false, Mentions?.OfType<RepliedUserMention>().Any() ?? false);

            if (Files?.Count > 0)
            {
                ulong fileId = 0;
                List<DiscordAttachment> attachmentsNew = new();
                foreach (var file in Files)
                {
                    DiscordAttachment att = new()
                    {
                        Id = fileId,
                        Discord = this.Discord,
                        Description = file.Description,
                        FileName = file.FileName
                    };
                    attachmentsNew.Add(att);
                    fileId++;
                }
                if (Attachments.HasValue && Attachments.Value.Any())
                    attachmentsNew.AddRange(Attachments.Value);

                pld.Attachments = attachmentsNew;

                var values = new Dictionary<string, string>
                {
                    ["payload_json"] = DiscordJson.SerializeObject(pld)
                };

                var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id";
                var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

                var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
                var res = await this.DoMultipart(this.Discord, bucket, url, RestRequestMethod.Patch, route, Values: values, Files: Files).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));

                foreach (var file in Files.Where(X => X.ResetPositionTo.HasValue))
                {
                    file.Stream.Position = file.ResetPositionTo.Value;
                }

                return ret;
            }
            else
            {
                pld.Attachments = Attachments.HasValue ? Attachments.Value : null;

                var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id";
                var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

                var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
                var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

                var ret = this.PrepareMessage(JObject.Parse(res.Response));

                return ret;
            }
        }

        /// <summary>
        /// Deletes the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteMessage(ulong ChannelId, ulong MessageId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Deletes the messages async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_ids">The message_ids.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteMessages(ulong ChannelId, IEnumerable<ulong> MessageIds, string Reason)
        {
            var pld = new RestChannelMessageBulkDeletePayload
            {
                Messages = MessageIds
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}{Endpoints.BulkDelete}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets the channel invites async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordInvite>> GetChannelInvitesAsync(ulong ChannelId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Invites}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(Xi => { Xi.Discord = this.Discord; return Xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
        }

        /// <summary>
        /// Creates the channel invite async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="max_age">The max_age.</param>
        /// <param name="max_uses">The max_uses.</param>
        /// <param name="target_type">The target_type.</param>
        /// <param name="target_application">The target_application.</param>
        /// <param name="target_user">The target_user.</param>
        /// <param name="Temporary">If true, temporary.</param>
        /// <param name="Unique">If true, unique.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> CreateChannelInviteAsync(ulong ChannelId, int MaxAge, int MaxUses, TargetType? TargetType, TargetActivity? TargetApplication, ulong? TargetUser, bool Temporary, bool Unique, string Reason)
        {
            var pld = new RestChannelInviteCreatePayload
            {
                MaxAge = MaxAge,
                MaxUses = MaxUses,
                TargetType = TargetType,
                TargetApplication = TargetApplication,
                TargetUserId = TargetUser,
                Temporary = Temporary,
                Unique = Unique
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Invites}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the channel permission async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="overwrite_id">The overwrite_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteChannelPermission(ulong ChannelId, ulong OverwriteId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Permissions}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, overwrite_id = OverwriteId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Edits the channel permissions async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="overwrite_id">The overwrite_id.</param>
        /// <param name="Allow">The allow.</param>
        /// <param name="Deny">The deny.</param>
        /// <param name="Type">The type.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task EditChannelPermissions(ulong ChannelId, ulong OverwriteId, Permissions Allow, Permissions Deny, string Type, string Reason)
        {
            var pld = new RestChannelPermissionEditPayload
            {
                Type = Type,
                Allow = Allow & PermissionMethods.FullPerms,
                Deny = Deny & PermissionMethods.FullPerms
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Permissions}/:overwrite_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = ChannelId, overwrite_id = OverwriteId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Triggers the typing async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal Task TriggerTyping(ulong ChannelId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Typing}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route);
        }

        /// <summary>
        /// Gets the pinned messages async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong ChannelId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Pins}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var msgsRaw = JArray.Parse(res.Response);
            var msgs = new List<DiscordMessage>();
            foreach (var xj in msgsRaw)
                msgs.Add(this.PrepareMessage(xj));

            return new ReadOnlyCollection<DiscordMessage>(new List<DiscordMessage>(msgs));
        }

        /// <summary>
        /// Pins the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task PinMessage(ulong ChannelId, ulong MessageId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Pins}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route);
        }

        /// <summary>
        /// Unpins the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task UnpinMessage(ulong ChannelId, ulong MessageId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Pins}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Adds the group dm recipient async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="access_token">The access_token.</param>
        /// <param name="Nickname">The nickname.</param>
        /// <returns>A Task.</returns>
        internal Task AddGroupDmRecipient(ulong ChannelId, ulong UserId, string AccessToken, string Nickname)
        {
            var pld = new RestChannelGroupDmRecipientAddPayload
            {
                AccessToken = AccessToken,
                Nickname = Nickname
            };

            var route = $"{Endpoints.Users}{Endpoints.Me}{Endpoints.Channels}/:channel_id{Endpoints.Recipients}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = ChannelId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, Payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Removes the group dm recipient async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGroupDmRecipient(ulong ChannelId, ulong UserId)
        {
            var route = $"{Endpoints.Users}{Endpoints.Me}{Endpoints.Channels}/:channel_id{Endpoints.Recipients}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Creates the group dm async.
        /// </summary>
        /// <param name="access_tokens">The access_tokens.</param>
        /// <param name="Nicks">The nicks.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordDmChannel> CreateGroupDmAsync(IEnumerable<string> AccessTokens, IDictionary<ulong, string> Nicks)
        {
            var pld = new RestUserGroupDmCreatePayload
            {
                AccessTokens = AccessTokens,
                Nicknames = Nicks
            };

            var route = $"{Endpoints.Users}{Endpoints.Me}{Endpoints.Channels}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordDmChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Creates the dm async.
        /// </summary>
        /// <param name="recipient_id">The recipient_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordDmChannel> CreateDmAsync(ulong RecipientId)
        {
            var pld = new RestUserDmCreatePayload
            {
                Recipient = RecipientId
            };

            var route = $"{Endpoints.Users}{Endpoints.Me}{Endpoints.Channels}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
        internal async Task<DiscordFollowedChannel> FollowChannelAsync(ulong ChannelId, ulong WebhookChannelId)
        {
            var pld = new FollowedChannelAddPayload
            {
                WebhookChannelId = WebhookChannelId
            };

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Followers}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var response = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<DiscordFollowedChannel>(response.Response);
        }

        /// <summary>
        /// Crossposts the message async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CrosspostMessageAsync(ulong ChannelId, ulong MessageId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Crosspost}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var response = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<DiscordMessage>(response.Response);
        }

        #endregion

        #region Member
        /// <summary>
        /// Gets the current user async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal Task<DiscordUser> GetCurrentUser()
            => this.GetUserAsync("@me");

        /// <summary>
        /// Gets the user async.
        /// </summary>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordUser> GetUser(ulong UserId)
            => this.GetUserAsync(UserId.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the user async.
        /// </summary>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordUser> GetUserAsync(string UserId)
        {
            var route = $"{Endpoints.Users}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
            var duser = new DiscordUser(userRaw) { Discord = this.Discord };

            return duser;
        }

        /// <summary>
        /// Gets the guild member async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMember> GetGuildMemberAsync(ulong GuildId, ulong UserId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var tm = JsonConvert.DeserializeObject<TransportMember>(res.Response);

            var usr = new DiscordUser(tm.User) { Discord = this.Discord };
            usr = this.Discord.UserCache.AddOrUpdate(tm.User.Id, usr, (Id, Old) =>
            {
                Old.Username = usr.Username;
                Old.Discriminator = usr.Discriminator;
                Old.AvatarHash = usr.AvatarHash;
                return Old;
            });

            return new DiscordMember(tm)
            {
                Discord = this.Discord,
                _guildId = GuildId
            };
        }

        /// <summary>
        /// Removes the guild member async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task RemoveGuildMember(ulong GuildId, ulong UserId, string Reason)
        {
            var urlparams = new Dictionary<string, string>();
            if (Reason != null)
                urlparams["reason"] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams), this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Modifies the current user async.
        /// </summary>
        /// <param name="Username">The username.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <returns>A Task.</returns>
        internal async Task<TransportUser> ModifyCurrentUserAsync(string Username, Optional<string> Base64Avatar)
        {
            var pld = new RestUserUpdateCurrentPayload
            {
                Username = Username,
                AvatarBase64 = Base64Avatar.HasValue ? Base64Avatar.Value : null,
                AvatarSet = Base64Avatar.HasValue
            };

            var route = $"{Endpoints.Users}{Endpoints.Me}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);

            return userRaw;
        }

        /// <summary>
        /// Gets the current user guilds async.
        /// </summary>
        /// <param name="Limit">The limit.</param>
        /// <param name="Before">The before.</param>
        /// <param name="After">The after.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int Limit = 100, ulong? Before = null, ulong? After = null)
        {
            var route = $"{Endpoints.Users}{Endpoints.Me}{Endpoints.Guilds}";

            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration)
                .AddParameter($"limit", Limit.ToString(CultureInfo.InvariantCulture));

            if (Before != null)
                url.AddParameter("before", Before.Value.ToString(CultureInfo.InvariantCulture));
            if (After != null)
                url.AddParameter("after", After.Value.ToString(CultureInfo.InvariantCulture));

            var res = await this.DoRequest(this.Discord, bucket, url.Build(), RestRequestMethod.Get, route).ConfigureAwait(false);

            if (this.Discord is DiscordClient)
            {
                var guildsRaw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response);
                var glds = guildsRaw.Select(Xug => (this.Discord as DiscordClient)?._guilds[Xug.Id]);
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
        /// <param name="Nick">The nick.</param>
        /// <param name="role_ids">The role_ids.</param>
        /// <param name="Mute">The mute.</param>
        /// <param name="Deaf">The deaf.</param>
        /// <param name="voice_channel_id">The voice_channel_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyGuildMember(ulong GuildId, ulong UserId, Optional<string> Nick,
            Optional<IEnumerable<ulong>> RoleIds, Optional<bool> Mute, Optional<bool> Deaf,
            Optional<ulong?> VoiceChannelId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = Nick,
                RoleIds = RoleIds,
                Deafen = Deaf,
                Mute = Mute,
                VoiceChannelId = VoiceChannelId
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, Payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Modifies the time out of a guild member.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="Until">Datetime offset.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyTimeout(ulong GuildId, ulong UserId, DateTimeOffset? Until, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var pld = new RestGuildMemberTimeoutModifyPayload
            {
                CommunicationDisabledUntil = Until
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, Payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Modifies the current member nickname async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Nick">The nick.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task ModifyCurrentMemberNickname(ulong GuildId, string Nick, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var pld = new RestGuildMemberModifyPayload
            {
                Nickname = Nick
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Members}{Endpoints.Me}{Endpoints.Nick}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, Payload: DiscordJson.SerializeObject(pld));
        }
        #endregion

        #region Roles
        /// <summary>
        /// Gets the guild roles async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Roles}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var rolesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(Xr => { Xr.Discord = this.Discord; Xr._guildId = GuildId; return Xr; });

            return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(rolesRaw));
        }

        /// <summary>
        /// Gets the guild async.
        /// </summary>
        /// <param name="GuildId">The guild id.</param>
        /// <param name="with_counts">If true, with_counts.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuild> GetGuildAsync(ulong GuildId, bool? WithCounts)
        {
            var urlparams = new Dictionary<string, string>();
            if (WithCounts.HasValue)
                urlparams["with_counts"] = WithCounts?.ToString();

            var route = $"{Endpoints.Guilds}/:guild_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route, urlparams).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var rawMembers = (JArray)json["members"];
            var guildRest = json.ToDiscordObject<DiscordGuild>();
            foreach (var r in guildRest._roles.Values)
                r._guildId = guildRest.Id;

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
        /// <param name="Name">The name.</param>
        /// <param name="Permissions">The permissions.</param>
        /// <param name="Color">The color.</param>
        /// <param name="Hoist">If true, hoist.</param>
        /// <param name="Mentionable">If true, mentionable.</param>
        /// <param name="Iconb64">The icon.</param>
        /// <param name="Emoji">The unicode emoji icon.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong GuildId, ulong RoleId, string Name, Permissions? Permissions, int? Color, bool? Hoist, bool? Mentionable, Optional<string> Iconb64, Optional<string> Emoji, string Reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = Name,
                Permissions = Permissions & PermissionMethods.FullPerms,
                Color = Color,
                Hoist = Hoist,
                Mentionable = Mentionable,
            };

            if (Emoji.HasValue && !Iconb64.HasValue)
                pld.UnicodeEmoji = Emoji;

            if (Emoji.HasValue && Iconb64.HasValue)
            {
                pld.IconBase64 = null;
                pld.UnicodeEmoji = Emoji;
            }

            if (Iconb64.HasValue)
                pld.IconBase64 = Iconb64;

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Roles}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, role_id = RoleId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;
            ret._guildId = GuildId;

            return ret;
        }

        /// <summary>
        /// Deletes the role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="role_id">The role_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteRole(ulong GuildId, ulong RoleId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Roles}/:role_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, role_id = RoleId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Creates the guild role async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Permissions">The permissions.</param>
        /// <param name="Color">The color.</param>
        /// <param name="Hoist">If true, hoist.</param>
        /// <param name="Mentionable">If true, mentionable.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordRole> CreateGuildRoleAsync(ulong GuildId, string Name, Permissions? Permissions, int? Color, bool? Hoist, bool? Mentionable, string Reason)
        {
            var pld = new RestGuildRolePayload
            {
                Name = Name,
                Permissions = Permissions & PermissionMethods.FullPerms,
                Color = Color,
                Hoist = Hoist,
                Mentionable = Mentionable
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Roles}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
            ret.Discord = this.Discord;
            ret._guildId = GuildId;

            return ret;
        }
        #endregion

        #region Prune
        /// <summary>
        /// Gets the guild prune count async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Days">The days.</param>
        /// <param name="include_roles">The include_roles.</param>
        /// <returns>A Task.</returns>
        internal async Task<int> GetGuildPruneCountAsync(ulong GuildId, int Days, IEnumerable<ulong> IncludeRoles)
        {
            if (Days < 0 || Days > 30)
                throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(Days));

            var urlparams = new Dictionary<string, string>
            {
                ["days"] = Days.ToString(CultureInfo.InvariantCulture)
            };

            var sb = new StringBuilder();

            if (IncludeRoles != null)
            {
                var roleArray = IncludeRoles.ToArray();
                var roleArrayCount = roleArray.Count();

                for (var i = 0; i < roleArrayCount; i++)
                    sb.Append($"&include_roles={roleArray[i]}");
            }

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Prune}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);
            var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlparams)}{sb}", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned.Value;
        }

        /// <summary>
        /// Begins the guild prune async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Days">The days.</param>
        /// <param name="compute_prune_count">If true, compute_prune_count.</param>
        /// <param name="include_roles">The include_roles.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<int?> BeginGuildPruneAsync(ulong GuildId, int Days, bool ComputePruneCount, IEnumerable<ulong> IncludeRoles, string Reason)
        {
            if (Days < 0 || Days > 30)
                throw new ArgumentException("Prune inactivity days must be a number between 0 and 30.", nameof(Days));

            var urlparams = new Dictionary<string, string>
            {
                ["days"] = Days.ToString(CultureInfo.InvariantCulture),
                ["compute_prune_count"] = ComputePruneCount.ToString()
            };

            var sb = new StringBuilder();

            if (IncludeRoles != null)
            {
                var roleArray = IncludeRoles.ToArray();
                var roleArrayCount = roleArray.Count();

                for (var i = 0; i < roleArrayCount; i++)
                    sb.Append($"&include_roles={roleArray[i]}");
            }

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Prune}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, $"{BuildQueryString(urlparams)}{sb}", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers).ConfigureAwait(false);

            var pruned = JsonConvert.DeserializeObject<RestGuildPruneResultPayload>(res.Response);

            return pruned.Pruned;
        }
        #endregion

        #region GuildVarious
        /// <summary>
        /// Gets the template async.
        /// </summary>
        /// <param name="Code">The code.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildTemplate> GetTemplateAsync(string Code)
        {
            var route = $"{Endpoints.Guilds}{Endpoints.Templates}/:code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { code = Code }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var templatesRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

            return templatesRaw;
        }

        /// <summary>
        /// Gets the guild integrations async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Integrations}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var integrationsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(Xi => { Xi.Discord = this.Discord; return Xi; });

            return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrationsRaw));
        }

        /// <summary>
        /// Gets the guild preview async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Preview}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordGuildPreview>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Creates the guild integration async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Type">The type.</param>
        /// <param name="Id">The id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong GuildId, string Type, ulong Id)
        {
            var pld = new RestGuildIntegrationAttachPayload
            {
                Type = Type,
                Id = Id
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Integrations}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
        internal async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong GuildId, ulong IntegrationId, int ExpireBehaviour, int ExpireGracePeriod, bool EnableEmoticons)
        {
            var pld = new RestGuildIntegrationModifyPayload
            {
                ExpireBehavior = ExpireBehaviour,
                ExpireGracePeriod = ExpireGracePeriod,
                EnableEmoticons = EnableEmoticons
            };

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Integrations}/:integration_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, integration_id = IntegrationId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the guild integration async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Integration">The integration.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteGuildIntegration(ulong GuildId, DiscordIntegration Integration)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Integrations}/:integration_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, integration_id = Integration.Id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, Payload: DiscordJson.SerializeObject(Integration));
        }

        /// <summary>
        /// Syncs the guild integration async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="integration_id">The integration_id.</param>
        /// <returns>A Task.</returns>
        internal Task SyncGuildIntegration(ulong GuildId, ulong IntegrationId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Integrations}/:integration_id{Endpoints.Sync}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId, integration_id = IntegrationId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route);
        }

        /// <summary>
        /// Gets the guild voice regions async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Regions}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var regionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regionsRaw));
        }

        /// <summary>
        /// Gets the guild invites async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordInvite>> GetGuildInvitesAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Invites}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var invitesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordInvite>>(res.Response).Select(Xi => { Xi.Discord = this.Discord; return Xi; });

            return new ReadOnlyCollection<DiscordInvite>(new List<DiscordInvite>(invitesRaw));
        }
        #endregion

        #region Invite
        /// <summary>
        /// Gets the invite async.
        /// </summary>
        /// <param name="invite_code">The invite_code.</param>
        /// <param name="with_counts">If true, with_counts.</param>
        /// <param name="with_expiration">If true, with_expiration.</param>
        /// <param name="guild_scheduled_event_id">The scheduled event id to get.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> GetInviteAsync(string InviteCode, bool? WithCounts, bool? WithExpiration, ulong? GuildScheduledEventId)
        {
            var urlparams = new Dictionary<string, string>();
            if (WithCounts.HasValue)
                urlparams["with_counts"] = WithCounts?.ToString();
            if (WithExpiration.HasValue)
                urlparams["with_expiration"] = WithExpiration?.ToString();
            if (GuildScheduledEventId.HasValue)
                urlparams["guild_scheduled_event_id"] = GuildScheduledEventId?.ToString();

            var route = $"{Endpoints.Invites}/:invite_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { invite_code = InviteCode }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordInvite>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Deletes the invite async.
        /// </summary>
        /// <param name="invite_code">The invite_code.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordInvite> DeleteInviteAsync(string InviteCode, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Invites}/:invite_code";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { invite_code = InviteCode }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers).ConfigureAwait(false);

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
         *     var url = new Uri($"{Utils.GetApiBaseUri(this.Configuration), Endpoints.INVITES}/{invite_code));
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
            var route = $"{Endpoints.Users}{Endpoints.Me}{Endpoints.Connections}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var connectionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(Xc => { Xc.Discord = this.Discord; return Xc; });

            return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connectionsRaw));
        }
        #endregion

        #region Voice
        /// <summary>
        /// Lists the voice regions async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        {
            var route = $"{Endpoints.Voice}{Endpoints.Regions}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

            return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
        }
        #endregion

        #region Webhooks
        /// <summary>
        /// Creates the webhook async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> CreateWebhookAsync(ulong ChannelId, string Name, Optional<string> Base64Avatar, string Reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = Name,
                AvatarBase64 = Base64Avatar.HasValue ? Base64Avatar.Value : null,
                AvatarSet = Base64Avatar.HasValue
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Webhooks}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

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
        internal async Task<IReadOnlyList<DiscordWebhook>> GetChannelWebhooksAsync(ulong ChannelId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Webhooks}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(Xw => { Xw.Discord = this.Discord; Xw.ApiClient = this; return Xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
        }

        /// <summary>
        /// Gets the guild webhooks async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordWebhook>> GetGuildWebhooksAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Webhooks}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var webhooksRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordWebhook>>(res.Response).Select(Xw => { Xw.Discord = this.Discord; Xw.ApiClient = this; return Xw; });

            return new ReadOnlyCollection<DiscordWebhook>(new List<DiscordWebhook>(webhooksRaw));
        }

        /// <summary>
        /// Gets the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> GetWebhookAsync(ulong WebhookId)
        {
            var route = $"{Endpoints.Webhooks}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { webhook_id = WebhookId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Gets the webhook with token async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong WebhookId, string WebhookToken)
        {
            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { webhook_id = WebhookId, webhook_token = WebhookToken }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Token = WebhookToken;
            ret.Id = WebhookId;
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Modifies the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="ChannelId">The channel id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong WebhookId, ulong ChannelId, string Name, Optional<string> Base64Avatar, string Reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = Name,
                AvatarBase64 = Base64Avatar.HasValue ? Base64Avatar.Value : null,
                AvatarSet = Base64Avatar.HasValue,
                ChannelId = ChannelId
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Webhooks}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { webhook_id = WebhookId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Modifies the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="base64_avatar">The base64_avatar.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordWebhook> ModifyWebhookAsync(ulong WebhookId, string Name, string Base64Avatar, string WebhookToken, string Reason)
        {
            var pld = new RestWebhookPayload
            {
                Name = Name,
                AvatarBase64 = Base64Avatar
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { webhook_id = WebhookId, webhook_token = WebhookToken }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordWebhook>(res.Response);
            ret.Discord = this.Discord;
            ret.ApiClient = this;

            return ret;
        }

        /// <summary>
        /// Deletes the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteWebhook(ulong WebhookId, string Reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Webhooks}/:webhook_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { webhook_id = WebhookId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Deletes the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteWebhook(ulong WebhookId, string WebhookToken, string Reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { webhook_id = WebhookId, webhook_token = WebhookToken }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        /// <summary>
        /// Executes the webhook async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="Builder">The builder.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> ExecuteWebhookAsync(ulong WebhookId, string WebhookToken, DiscordWebhookBuilder Builder, string ThreadId)
        {
            Builder.Validate();

            if (Builder.Embeds != null)
                foreach (var embed in Builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestWebhookExecutePayload
            {
                Content = Builder.Content,
                Username = Builder.Username.HasValue ? Builder.Username.Value : null,
                AvatarUrl = Builder.AvatarUrl.HasValue ? Builder.AvatarUrl.Value : null,
                IsTts = Builder.IsTts,
                Embeds = Builder.Embeds,
                Components = Builder.Components
            };

            if (Builder.Mentions != null)
                pld.Mentions = new DiscordMentions(Builder.Mentions, Builder.Mentions.Any());

            if (Builder.Files?.Count > 0)
            {
                ulong fileId = 0;
                List<DiscordAttachment> attachments = new();
                foreach (var file in Builder.Files)
                {
                    DiscordAttachment att = new()
                    {
                        Id = fileId,
                        Discord = this.Discord,
                        Description = file.Description,
                        FileName = file.FileName,
                        FileSize = null
                    };
                    attachments.Add(att);
                    fileId++;
                }
                pld.Attachments = attachments;
            }

            if (!string.IsNullOrEmpty(Builder.Content) || Builder.Embeds?.Count() > 0 || Builder.Files?.Count > 0 || Builder.IsTts == true || Builder.Mentions != null)
                values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { webhook_id = WebhookId, webhook_token = WebhookToken }, out var path);

            var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
            if (ThreadId != null)
                qub.AddParameter("thread_id", ThreadId);

            var url = qub.Build();

            var res = await this.DoMultipart(this.Discord, bucket, url, RestRequestMethod.Post, route, Values: values, Files: Builder.Files).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

            foreach (var att in ret.Attachments)
                att.Discord = this.Discord;

            foreach (var file in Builder.Files.Where(X => X.ResetPositionTo.HasValue))
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
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> ExecuteWebhookSlackAsync(ulong WebhookId, string WebhookToken, string JsonPayload, string ThreadId)
        {
            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token{Endpoints.Slack}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { webhook_id = WebhookId, webhook_token = WebhookToken }, out var path);

            var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
            if (ThreadId != null)
                qub.AddParameter("thread_id", ThreadId);
            var url = qub.Build();
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: JsonPayload).ConfigureAwait(false);
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
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> ExecuteWebhookGithubAsync(ulong WebhookId, string WebhookToken, string JsonPayload, string ThreadId)
        {
            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token{Endpoints.Github}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { webhook_id = WebhookId, webhook_token = WebhookToken }, out var path);

            var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true");
            if (ThreadId != null)
                qub.AddParameter("thread_id", ThreadId);
            var url = qub.Build();
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: JsonPayload).ConfigureAwait(false);
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
        /// <param name="Builder">The builder.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> EditWebhookMessageAsync(ulong WebhookId, string WebhookToken, string MessageId, DiscordWebhookBuilder Builder, string ThreadId)
        {
            Builder.Validate(true);

            var pld = new RestWebhookMessageEditPayload
            {
                Content = Builder.Content,
                Embeds = Builder.Embeds,
                Mentions = Builder.Mentions,
                Components = Builder.Components,
            };

            if (Builder.Files?.Count > 0)
            {
                ulong fileId = 0;
                List<DiscordAttachment> attachments = new();
                foreach (var file in Builder.Files)
                {
                    DiscordAttachment att = new()
                    {
                        Id = fileId,
                        Discord = this.Discord,
                        Description = file.Description,
                        FileName = file.FileName,
                        FileSize = null
                    };
                    attachments.Add(att);
                    fileId++;
                }
                if (Builder.Attachments != null && Builder.Attachments?.Count() > 0)
                    attachments.AddRange(Builder.Attachments);

                pld.Attachments = attachments;

                var values = new Dictionary<string, string>
                {
                    ["payload_json"] = DiscordJson.SerializeObject(pld)
                };
                var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token{Endpoints.Messages}/:message_id";
                var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { webhook_id = WebhookId, webhook_token = WebhookToken, message_id = MessageId }, out var path);

                var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
                if (ThreadId != null)
                    qub.AddParameter("thread_id", ThreadId);

                var url = qub.Build();
                var res = await this.DoMultipart(this.Discord, bucket, url, RestRequestMethod.Patch, route, Values: values, Files: Builder.Files);

                var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

                ret.Discord = this.Discord;

                foreach (var att in ret._attachments)
                    att.Discord = this.Discord;

                foreach (var file in Builder.Files.Where(X => X.ResetPositionTo.HasValue))
                {
                    file.Stream.Position = file.ResetPositionTo.Value;
                }

                return ret;
            }
            else
            {
                pld.Attachments = Builder.Attachments;

                var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token{Endpoints.Messages}/:message_id";
                var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { webhook_id = WebhookId, webhook_token = WebhookToken, message_id = MessageId }, out var path);

                var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
                if (ThreadId != null)
                    qub.AddParameter("thread_id", ThreadId);

                var url = qub.Build();
                var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld));

                var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

                ret.Discord = this.Discord;

                foreach (var att in ret._attachments)
                    att.Discord = this.Discord;

                return ret;
            }
        }

        /// <summary>
        /// Edits the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Builder">The builder.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> EditWebhookMessage(ulong WebhookId, string WebhookToken, ulong MessageId, DiscordWebhookBuilder Builder, ulong ThreadId) =>
            this.EditWebhookMessageAsync(WebhookId, WebhookToken, MessageId.ToString(), Builder, ThreadId.ToString());

        /// <summary>
        /// Gets the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> GetWebhookMessageAsync(ulong WebhookId, string WebhookToken, string MessageId, string ThreadId)
        {
            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token{Endpoints.Messages}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { webhook_id = WebhookId, webhook_token = WebhookToken, message_id = MessageId }, out var path);

            var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
            if (ThreadId != null)
                qub.AddParameter("thread_id", ThreadId);
            var url = qub.Build();
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

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
        internal Task<DiscordMessage> GetWebhookMessage(ulong WebhookId, string WebhookToken, ulong MessageId) =>
            this.GetWebhookMessageAsync(WebhookId, WebhookToken, MessageId.ToString(), null);

        /// <summary>
        /// Gets the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> GetWebhookMessage(ulong WebhookId, string WebhookToken, ulong MessageId, ulong ThreadId) =>
            this.GetWebhookMessageAsync(WebhookId, WebhookToken, MessageId.ToString(), ThreadId.ToString());

        /// <summary>
        /// Deletes the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal async Task DeleteWebhookMessageAsync(ulong WebhookId, string WebhookToken, string MessageId, string ThreadId)
        {
            var route = $"{Endpoints.Webhooks}/:webhook_id/:webhook_token{Endpoints.Messages}/:message_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { webhook_id = WebhookId, webhook_token = WebhookToken, message_id = MessageId }, out var path);

            var qub = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration);
            if (ThreadId != null)
                qub.AddParameter("thread_id", ThreadId);
            var url = qub.Build();
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Deletes the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteWebhookMessage(ulong WebhookId, string WebhookToken, ulong MessageId) =>
            this.DeleteWebhookMessageAsync(WebhookId, WebhookToken, MessageId.ToString(), null);

        /// <summary>
        /// Deletes the webhook message async.
        /// </summary>
        /// <param name="webhook_id">The webhook_id.</param>
        /// <param name="webhook_token">The webhook_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="thread_id">The thread_id.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteWebhookMessage(ulong WebhookId, string WebhookToken, ulong MessageId, ulong ThreadId) =>
            this.DeleteWebhookMessageAsync(WebhookId, WebhookToken, MessageId.ToString(), ThreadId.ToString());
        #endregion

        #region Reactions
        /// <summary>
        /// Creates the reaction async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Emoji">The emoji.</param>
        /// <returns>A Task.</returns>
        internal Task CreateReaction(ulong ChannelId, ulong MessageId, string Emoji)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Reactions}/:emoji{Endpoints.Me}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = ChannelId, message_id = MessageId, emoji = Emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, RatelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Deletes the own reaction async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Emoji">The emoji.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteOwnReaction(ulong ChannelId, ulong MessageId, string Emoji)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Reactions}/:emoji{Endpoints.Me}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, message_id = MessageId, emoji = Emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, RatelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Deletes the user reaction async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="user_id">The user_id.</param>
        /// <param name="Emoji">The emoji.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteUserReaction(ulong ChannelId, ulong MessageId, ulong UserId, string Emoji, string Reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Reactions}/:emoji/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, message_id = MessageId, emoji = Emoji, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers, RatelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Gets the reactions async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Emoji">The emoji.</param>
        /// <param name="after_id">The after_id.</param>
        /// <param name="Limit">The limit.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong ChannelId, ulong MessageId, string Emoji, ulong? AfterId = null, int Limit = 25)
        {
            var urlparams = new Dictionary<string, string>();
            if (AfterId.HasValue)
                urlparams["after"] = AfterId.Value.ToString(CultureInfo.InvariantCulture);

            urlparams["limit"] = Limit.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Reactions}/:emoji";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId, message_id = MessageId, emoji = Emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, BuildQueryString(urlparams), this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var reactersRaw = JsonConvert.DeserializeObject<IEnumerable<TransportUser>>(res.Response);
            var reacters = new List<DiscordUser>();
            foreach (var xr in reactersRaw)
            {
                var usr = new DiscordUser(xr) { Discord = this.Discord };
                usr = this.Discord.UserCache.AddOrUpdate(xr.Id, usr, (Id, Old) =>
                {
                    Old.Username = usr.Username;
                    Old.Discriminator = usr.Discriminator;
                    Old.AvatarHash = usr.AvatarHash;
                    return Old;
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
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteAllReactions(ulong ChannelId, ulong MessageId, string Reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Reactions}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers, RatelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }

        /// <summary>
        /// Deletes the reactions emoji async.
        /// </summary>
        /// <param name="channel_id">The channel_id.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Emoji">The emoji.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteReactionsEmoji(ulong ChannelId, ulong MessageId, string Emoji)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Reactions}/:emoji";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, message_id = MessageId, emoji = Emoji }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, RatelimitWaitOverride: this.Discord.Configuration.UseRelativeRatelimit ? null : (double?)0.26);
        }
        #endregion

        #region Threads

        /// <summary>
        /// Creates the thread with message.
        /// </summary>
        /// <param name="channel_id">The channel id to create the thread in.</param>
        /// <param name="message_id">The message id to create the thread from.</param>
        /// <param name="Name">The name of the thread.</param>
        /// <param name="auto_archive_duration">The auto_archive_duration for the thread.</param>
        /// <param name="rate_limit_per_user">The rate limit per user.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordThreadChannel> CreateThreadWithMessageAsync(ulong ChannelId, ulong MessageId, string Name, ThreadAutoArchiveDuration AutoArchiveDuration, int? RateLimitPerUser, string Reason = null)
        {
            var pld = new RestThreadChannelCreatePayload
            {
                Name = Name,
                AutoArchiveDuration = AutoArchiveDuration,
                PerUserRateLimit = RateLimitPerUser
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Messages}/:message_id{Endpoints.Threads}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId, message_id = MessageId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld));

            var threadChannel = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);

            return threadChannel;
        }

        /// <summary>
        /// Creates the thread without a message.
        /// </summary>
        /// <param name="channel_id">The channel id to create the thread in.</param>
        /// <param name="Name">The name of the thread.</param>
        /// <param name="auto_archive_duration">The auto_archive_duration for the thread.</param>
        /// <param name="Type">Can be either <see cref="ChannelType.PublicThread"/> or <see cref="ChannelType.PrivateThread"/>.</param>
        /// <param name="rate_limit_per_user">The rate limit per user.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordThreadChannel> CreateThreadWithoutMessageAsync(ulong ChannelId, string Name, ThreadAutoArchiveDuration AutoArchiveDuration, ChannelType Type = ChannelType.PublicThread, int? RateLimitPerUser = null, string Reason = null)
        {
            var pld = new RestThreadChannelCreatePayload
            {
                Name = Name,
                AutoArchiveDuration = AutoArchiveDuration,
                PerUserRateLimit = RateLimitPerUser,
                Type = Type
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Threads}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld));

            var threadChannel = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);

            return threadChannel;
        }

        /// <summary>
        /// Gets the thread.
        /// </summary>
        /// <param name="thread_id">The thread id.</param>
        internal async Task<DiscordThreadChannel> GetThreadAsync(ulong ThreadId)
        {
            var route = $"{Endpoints.Channels}/:channel_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { thread_id = ThreadId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var ret = JsonConvert.DeserializeObject<DiscordThreadChannel>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Joins the thread.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        internal async Task JoinThreadAsync(ulong ChannelId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.ThreadMembers}{Endpoints.Me}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route);
        }

        /// <summary>
        /// Leaves the thread.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        internal async Task LeaveThreadAsync(ulong ChannelId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.ThreadMembers}{Endpoints.Me}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Adds a thread member.
        /// </summary>
        /// <param name="channel_id">The channel id to add the member to.</param>
        /// <param name="user_id">The user id to add.</param>
        internal async Task AddThreadMemberAsync(ulong ChannelId, ulong UserId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.ThreadMembers}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { channel_id = ChannelId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route);
        }

        /// <summary>
        /// Gets a thread member.
        /// </summary>
        /// <param name="channel_id">The channel id to get the member from.</param>
        /// <param name="user_id">The user id to get.</param>
        internal async Task<DiscordThreadChannelMember> GetThreadMemberAsync(ulong ChannelId, ulong UserId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.ThreadMembers}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var threadMember = JsonConvert.DeserializeObject<DiscordThreadChannelMember>(res.Response);

            return threadMember;
        }

        /// <summary>
        /// Removes a thread member.
        /// </summary>
        /// <param name="channel_id">The channel id to remove the member from.</param>
        /// <param name="user_id">The user id to remove.</param>
        internal async Task RemoveThreadMemberAsync(ulong ChannelId, ulong UserId)
        {
            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.ThreadMembers}/:user_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { channel_id = ChannelId, user_id = UserId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Gets the thread members.
        /// </summary>
        /// <param name="thread_id">The thread id.</param>
        internal async Task<IReadOnlyList<DiscordThreadChannelMember>> GetThreadMembersAsync(ulong ThreadId)
        {
            var route = $"{Endpoints.Channels}/:thread_id{Endpoints.ThreadMembers}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { thread_id = ThreadId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var threadMembersRaw = JsonConvert.DeserializeObject<List<DiscordThreadChannelMember>>(res.Response);

            return new ReadOnlyCollection<DiscordThreadChannelMember>(threadMembersRaw);
        }

        /// <summary>
        /// Gets the active threads in a guild.
        /// </summary>
        /// <param name="guild_id">The guild id.</param>
        internal async Task<DiscordThreadResult> GetActiveThreadsAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Threads}{Endpoints.ThreadActive}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return threadReturn;
        }

        /// <summary>
        /// Gets the joined private archived threads in a channel.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="Before">Get threads before snowflake.</param>
        /// <param name="Limit">Limit the results.</param>
        internal async Task<DiscordThreadResult> GetJoinedPrivateArchivedThreadsAsync(ulong ChannelId, ulong? Before, int? Limit)
        {
            var urlparams = new Dictionary<string, string>();
            if (Before != null)
                urlparams["before"] = Before.Value.ToString(CultureInfo.InvariantCulture);
            if (Limit != null && Limit > 0)
                urlparams["limit"] = Limit.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Users}{Endpoints.Me}{Endpoints.Threads}{Endpoints.ThreadArchived}{Endpoints.ThreadPrivate}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return threadReturn;
        }

        /// <summary>
        /// Gets the public archived threads in a channel.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="Before">Get threads before snowflake.</param>
        /// <param name="Limit">Limit the results.</param>
        internal async Task<DiscordThreadResult> GetPublicArchivedThreadsAsync(ulong ChannelId, ulong? Before, int? Limit)
        {
            var urlparams = new Dictionary<string, string>();
            if (Before != null)
                urlparams["before"] = Before.Value.ToString(CultureInfo.InvariantCulture);
            if (Limit != null && Limit > 0)
                urlparams["limit"] = Limit.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Threads}{Endpoints.ThreadArchived}{Endpoints.ThreadPublic}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return threadReturn;
        }

        /// <summary>
        /// Gets the private archived threads in a channel.
        /// </summary>
        /// <param name="channel_id">The channel id.</param>
        /// <param name="Before">Get threads before snowflake.</param>
        /// <param name="Limit">Limit the results.</param>
        internal async Task<DiscordThreadResult> GetPrivateArchivedThreadsAsync(ulong ChannelId, ulong? Before, int? Limit)
        {
            var urlparams = new Dictionary<string, string>();
            if (Before != null)
                urlparams["before"] = Before.Value.ToString(CultureInfo.InvariantCulture);
            if (Limit != null && Limit > 0)
                urlparams["limit"] = Limit.Value.ToString(CultureInfo.InvariantCulture);

            var route = $"{Endpoints.Channels}/:channel_id{Endpoints.Threads}{Endpoints.ThreadArchived}{Endpoints.ThreadPrivate}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { channel_id = ChannelId }, out var path);

            var url = Utilities.GetApiUriFor(path, urlparams.Any() ? BuildQueryString(urlparams) : "", this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var threadReturn = JsonConvert.DeserializeObject<DiscordThreadResult>(res.Response);

            return threadReturn;
        }

        /// <summary>
        /// Modifies a thread.
        /// </summary>
        /// <param name="thread_id">The thread to modify.</param>
        /// <param name="Name">The new name.</param>
        /// <param name="Locked">The new locked state.</param>
        /// <param name="Archived">The new archived state.</param>
        /// <param name="AutoArchiveDuration">The new auto archive duration.</param>
        /// <param name="PerUserRateLimit">The new per user rate limit.</param>
        /// <param name="Invitable">The new user invitable state.</param>
        /// <param name="Reason">The reason for the modification.</param>
        internal Task ModifyThread(ulong ThreadId, string Name, Optional<bool?> Locked, Optional<bool?> Archived, Optional<ThreadAutoArchiveDuration?> AutoArchiveDuration, Optional<int?> PerUserRateLimit, Optional<bool?> Invitable, string Reason)
        {
            var pld = new RestThreadChannelModifyPayload
            {
                Name = Name,
                Archived = Archived,
                AutoArchiveDuration = AutoArchiveDuration,
                Locked = Locked,
                PerUserRateLimit = PerUserRateLimit,
                Invitable = Invitable
            };

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:thread_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { thread_id = ThreadId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Deletes a thread.
        /// </summary>
        /// <param name="thread_id">The thread to delete.</param>
        /// <param name="Reason">The reason for deletion.</param>
        internal Task DeleteThread(ulong ThreadId, string Reason)
        {
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var route = $"{Endpoints.Channels}/:thread_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { thread_id = ThreadId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }

        #endregion

        #region Emoji
        /// <summary>
        /// Gets the guild emojis async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Emojis}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response);

            this.Discord.Guilds.TryGetValue(GuildId, out var gld);
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
        internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong GuildId, ulong EmojiId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Emojis}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId, emoji_id = EmojiId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(GuildId, out var gld);

            var emojiRaw = JObject.Parse(res.Response);
            var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        /// <summary>
        /// Creates the guild emoji async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Imageb64">The imageb64.</param>
        /// <param name="Roles">The roles.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong GuildId, string Name, string Imageb64, IEnumerable<ulong> Roles, string Reason)
        {
            var pld = new RestGuildEmojiCreatePayload
            {
                Name = Name,
                ImageB64 = Imageb64,
                Roles = Roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Emojis}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(GuildId, out var gld);

            var emojiRaw = JObject.Parse(res.Response);
            var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
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
        /// <param name="Name">The name.</param>
        /// <param name="Roles">The roles.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong GuildId, ulong EmojiId, string Name, IEnumerable<ulong> Roles, string Reason)
        {
            var pld = new RestGuildEmojiModifyPayload
            {
                Name = Name,
                Roles = Roles?.ToArray()
            };

            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Emojis}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, emoji_id = EmojiId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

            this.Discord.Guilds.TryGetValue(GuildId, out var gld);

            var emojiRaw = JObject.Parse(res.Response);
            var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
            emoji.Guild = gld;

            var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
            if (xtu != null)
                emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

            return emoji;
        }

        /// <summary>
        /// Deletes the guild emoji async.
        /// </summary>
        /// <param name="guild_id">The guild_id.</param>
        /// <param name="emoji_id">The emoji_id.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteGuildEmoji(ulong GuildId, ulong EmojiId, string Reason)
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers[ReasonHeaderName] = Reason;

            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Emojis}/:emoji_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, emoji_id = EmojiId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            return this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }
        #endregion

        #region Stickers

        /// <summary>
        /// Gets a sticker.
        /// </summary>
        /// <param name="sticker_id">The sticker id.</param>
        internal async Task<DiscordSticker> GetStickerAsync(ulong StickerId)
        {
            var route = $"{Endpoints.Stickers}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { sticker_id = StickerId}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);
            var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Gets the sticker packs.
        /// </summary>
        internal async Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
        {
            var route = $"{Endpoints.Stickerpacks}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response)["sticker_packs"] as JArray;
            var ret = json.ToDiscordObject<DiscordStickerPack[]>();

            return ret.ToList();
        }

        /// <summary>
        /// Gets the guild stickers.
        /// </summary>
        /// <param name="guild_id">The guild id.</param>
        internal async Task<IReadOnlyList<DiscordSticker>> GetGuildStickersAsync(ulong GuildId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Stickers}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);
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
                    usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (Id, Old) =>
                    {
                        Old.Username = usr.Username;
                        Old.Discriminator = usr.Discriminator;
                        Old.AvatarHash = usr.AvatarHash;
                        return Old;
                    });
                    stkr.User = usr;
                }
            }

            return ret.ToList();
        }

        /// <summary>
        /// Gets a guild sticker.
        /// </summary>
        /// <param name="guild_id">The guild id.</param>
        /// <param name="sticker_id">The sticker id.</param>
        internal async Task<DiscordSticker> GetGuildStickerAsync(ulong GuildId, ulong StickerId)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Stickers}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { guild_id = GuildId, sticker_id = StickerId}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var json = JObject.Parse(res.Response);
            var ret = json.ToDiscordObject<DiscordSticker>();
            if (json["user"] is not null) // Null = Missing stickers perm //
            {
                var tsr = json["user"].ToDiscordObject<TransportUser>();
                var usr = new DiscordUser(tsr) {Discord = this.Discord};
                usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (Id, Old) =>
                {
                    Old.Username = usr.Username;
                    Old.Discriminator = usr.Discriminator;
                    Old.AvatarHash = usr.AvatarHash;
                    return Old;
                });
                ret.User = usr;
            }
            ret.Discord = this.Discord;
            return ret;
        }

        /// <summary>
        /// Creates the guild sticker.
        /// </summary>
        /// <param name="guild_id">The guild id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Tags">The tags.</param>
        /// <param name="File">The file.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordSticker> CreateGuildStickerAsync(ulong GuildId, string Name, string Description, string Tags, DiscordMessageFile File, string Reason)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Stickers}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { guild_id = GuildId}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var res = await this.DoStickerMultipart(this.Discord, bucket, url, RestRequestMethod.Post, route, headers, File, Name, Tags, Description);

            var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Modifies the guild sticker.
        /// </summary>
        /// <param name="guild_id">The guild id.</param>
        /// <param name="sticker_id">The sticker id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Tags">The tags.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task<DiscordSticker> ModifyGuildStickerAsync(ulong GuildId, ulong StickerId, Optional<string> Name, Optional<string> Description, Optional<string> Tags, string Reason)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Stickers}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { guild_id = GuildId, sticker_id = StickerId}, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            var pld = new RestStickerModifyPayload()
            {
                Name = Name,
                Description = Description,
                Tags = Tags
            };

            var values = new Dictionary<string, string>
            {
                ["payload_json"] = DiscordJson.SerializeObject(pld)
            };

            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route);
            var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();
            ret.Discord = this.Discord;

            return null;
        }

        /// <summary>
        /// Deletes the guild sticker async.
        /// </summary>
        /// <param name="guild_id">The guild id.</param>
        /// <param name="sticker_id">The sticker id.</param>
        /// <param name="Reason">The reason.</param>
        internal async Task DeleteGuildStickerAsync(ulong GuildId, ulong StickerId, string Reason)
        {
            var route = $"{Endpoints.Guilds}/:guild_id{Endpoints.Stickers}/:sticker_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { guild_id = GuildId, sticker_id = StickerId }, out var path);
            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var headers = Utilities.GetBaseHeaders();
            if (!string.IsNullOrWhiteSpace(Reason))
                headers.Add(ReasonHeaderName, Reason);

            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route, headers);
        }
        #endregion

        #region Application Commands
        /// <summary>
        /// Gets the global application commands async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(ulong ApplicationId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Commands}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = ApplicationId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        /// <summary>
        /// Bulks the overwrite global application commands async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="Commands">The commands.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(ulong ApplicationId, IEnumerable<DiscordApplicationCommand> Commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in Commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Type = command.Type,
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options,
                    DefaultPermission = command.DefaultPermission,
                    NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
                    DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs()
                });
            }

            this.Discord.Logger.LogDebug(DiscordJson.SerializeObject(pld));

            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Commands}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { application_id = ApplicationId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, Payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationCommand>>(res.Response);
            foreach (var app in ret)
                app.Discord = this.Discord;
            return ret.ToList();
        }

        /// <summary>
        /// Creates the global application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="Command">The command.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(ulong ApplicationId, DiscordApplicationCommand Command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Type = Command.Type,
                Name = Command.Name,
                Description = Command.Description,
                Options = Command.Options,
                DefaultPermission = Command.DefaultPermission,
                NameLocalizations = Command.NameLocalizations.GetKeyValuePairs(),
                DescriptionLocalizations = Command.DescriptionLocalizations.GetKeyValuePairs()
            };

            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Commands}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { application_id = ApplicationId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld));

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
        internal async Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong ApplicationId, ulong CommandId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Commands}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = ApplicationId, command_id = CommandId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var ret = JsonConvert.DeserializeObject<DiscordApplicationCommand>(res.Response);
            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Edits the global application command async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="command_id">The command_id.</param>
        /// <param name="Name">The name.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Options">The options.</param>
        /// <param name="default_permission">The default_permission.</param>
        /// <param name="name_localization">The localizations of the name.</param>
        /// <param name="description_localization">The localizations of the description.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong ApplicationId, ulong CommandId, Optional<string> Name, Optional<string> Description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> Options, Optional<bool> DefaultPermission, Optional<DiscordApplicationCommandLocalization> NameLocalization, Optional<DiscordApplicationCommandLocalization> DescriptionLocalization)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = Name,
                Description = Description,
                Options = Options,
                DefaultPermission = DefaultPermission,
                NameLocalizations = NameLocalization.HasValue ? NameLocalization.Value.GetKeyValuePairs() : null,
                DescriptionLocalizations = DescriptionLocalization.HasValue ? DescriptionLocalization.Value.GetKeyValuePairs() : null
            };

            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Commands}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { application_id = ApplicationId, command_id = CommandId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld));

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
        internal async Task DeleteGlobalApplicationCommandAsync(ulong ApplicationId, ulong CommandId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Commands}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { application_id = ApplicationId, command_id = CommandId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Gets the guild application commands async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="guild_id">The guild_id.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong ApplicationId, ulong GuildId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = ApplicationId, guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

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
        /// <param name="Commands">The commands.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong ApplicationId, ulong GuildId, IEnumerable<DiscordApplicationCommand> Commands)
        {
            var pld = new List<RestApplicationCommandCreatePayload>();
            foreach (var command in Commands)
            {
                pld.Add(new RestApplicationCommandCreatePayload
                {
                    Type = command.Type,
                    Name = command.Name,
                    Description = command.Description,
                    Options = command.Options,
                    DefaultPermission = command.DefaultPermission,
                    NameLocalizations = command.NameLocalizations?.GetKeyValuePairs(),
                    DescriptionLocalizations = command.DescriptionLocalizations?.GetKeyValuePairs()
                });
            }
            this.Discord.Logger.LogDebug(DiscordJson.SerializeObject(pld));

            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { application_id = ApplicationId, guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, Payload: DiscordJson.SerializeObject(pld));

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
        /// <param name="Command">The command.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong ApplicationId, ulong GuildId, DiscordApplicationCommand Command)
        {
            var pld = new RestApplicationCommandCreatePayload
            {
                Type = Command.Type,
                Name = Command.Name,
                Description = Command.Description,
                Options = Command.Options,
                DefaultPermission = Command.DefaultPermission,
                NameLocalizations = Command.NameLocalizations.GetKeyValuePairs(),
                DescriptionLocalizations = Command.DescriptionLocalizations.GetKeyValuePairs()

            };

            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { application_id = ApplicationId, guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld));

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
        internal async Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong ApplicationId, ulong GuildId, ulong CommandId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = ApplicationId, guild_id = GuildId, command_id = CommandId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

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
        /// <param name="Name">The name.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Options">The options.</param>
        /// <param name="default_permission">The default_permission.</param>
        /// <param name="name_localization">The localizations of the name.</param>
        /// <param name="description_localization">The localizations of the description.</param>
        internal async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong ApplicationId, ulong GuildId, ulong CommandId, Optional<string> Name, Optional<string> Description, Optional<IReadOnlyCollection<DiscordApplicationCommandOption>> Options, Optional<bool> DefaultPermission, Optional<DiscordApplicationCommandLocalization> NameLocalization, Optional<DiscordApplicationCommandLocalization> DescriptionLocalization)
        {
            var pld = new RestApplicationCommandEditPayload
            {
                Name = Name,
                Description = Description,
                Options = Options,
                DefaultPermission = DefaultPermission,
                NameLocalizations = NameLocalization.HasValue ? NameLocalization.Value.GetKeyValuePairs() : null,
                DescriptionLocalizations = DescriptionLocalization.HasValue ? DescriptionLocalization.Value.GetKeyValuePairs() : null
            };

            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Patch, route, new { application_id = ApplicationId, guild_id = GuildId, command_id = CommandId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Patch, route, Payload: DiscordJson.SerializeObject(pld));

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
        internal async Task DeleteGuildApplicationCommandAsync(ulong ApplicationId, ulong GuildId, ulong CommandId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}/:command_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Delete, route, new { application_id = ApplicationId, guild_id = GuildId, command_id = CommandId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Delete, route);
        }

        /// <summary>
        /// Gets the guild application command permissions.
        /// </summary>
        /// <param name="application_id">The target application id.</param>
        /// <param name="guild_id">The target guild id.</param>
        internal async Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> GetGuildApplicationCommandPermissionsAsync(ulong ApplicationId, ulong GuildId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}{Endpoints.Permissions}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = ApplicationId, guild_id = GuildId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermission>>(res.Response);

            foreach (var app in ret)
                app.Discord = this.Discord;

            return ret.ToList();
        }

        /// <summary>
        /// Gets the application command permission.
        /// </summary>
        /// <param name="application_id">The target application id.</param>
        /// <param name="guild_id">The target guild id.</param>
        /// <param name="command_id">The target command id.</param>
        internal async Task<DiscordGuildApplicationCommandPermission> GetApplicationCommandPermissionAsync(ulong ApplicationId, ulong GuildId, ulong CommandId)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}/:command_id{Endpoints.Permissions}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = ApplicationId, guild_id = GuildId, command_id = CommandId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route);

            var ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermission>(res.Response);

            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Overwrites the guild application command permissions.
        /// </summary>
        /// <param name="application_id">The target application id.</param>
        /// <param name="guild_id">The target guild id.</param>
        /// <param name="command_id">The target command id.</param>
        /// <param name="Permissions">Array of permissions.</param>
        internal async Task<DiscordGuildApplicationCommandPermission> OverwriteGuildApplicationCommandPermissionsAsync(ulong ApplicationId, ulong GuildId, ulong CommandId, IEnumerable<DiscordApplicationCommandPermission> Permissions)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}/:command_id{Endpoints.Permissions}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { application_id = ApplicationId, guild_id = GuildId, command_id = CommandId }, out var path);

            if (Permissions.ToArray().Length > 10)
                throw new NotSupportedException("You can add only up to 10 permission overwrites per command.");

            var pld = new RestApplicationCommandPermissionEditPayload
            {
                Permissions = Permissions
            };

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, Payload: DiscordJson.SerializeObject(pld));

            var ret = JsonConvert.DeserializeObject<DiscordGuildApplicationCommandPermission>(res.Response);

            ret.Discord = this.Discord;

            return ret;
        }

        /// <summary>
        /// Bulks overwrite the application command permissions.
        /// </summary>
        /// <param name="application_id">The target application id.</param>
        /// <param name="guild_id">The target guild id.</param>
        /// <param name="permission_overwrites"></param>
        internal async Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> BulkOverwriteApplicationCommandPermissionsAsync(ulong ApplicationId, ulong GuildId, IEnumerable<DiscordGuildApplicationCommandPermission> PermissionOverwrites)
        {
            var route = $"{Endpoints.Applications}/:application_id{Endpoints.Guilds}/:guild_id{Endpoints.Commands}{Endpoints.Permissions}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Put, route, new { application_id = ApplicationId, guild_id = GuildId }, out var path);

            var pld = new List<RestGuildApplicationCommandPermissionEditPayload>();
            foreach (var overwrite in PermissionOverwrites)
            {
                if (overwrite.Permissions.Count > 10)
                    throw new NotSupportedException("You can add only up to 10 permission overwrites per command.");

                pld.Add(new RestGuildApplicationCommandPermissionEditPayload
                {
                    CommandId = overwrite.Id,
                    Permissions = overwrite.Permissions
                });
            }

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Put, route, Payload: DiscordJson.SerializeObject(pld.ToArray()));

            var ret = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildApplicationCommandPermission>>(res.Response);

            foreach (var app in ret)
                app.Discord = this.Discord;

            return ret.ToList();
        }

        /// <summary>
        /// Creates the interaction response async.
        /// </summary>
        /// <param name="interaction_id">The interaction_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="Type">The type.</param>
        /// <param name="Builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task CreateInteractionResponseAsync(ulong InteractionId, string InteractionToken, InteractionResponseType Type, DiscordInteractionResponseBuilder Builder)
        {
            if (Builder?.Embeds != null)
                foreach (var embed in Builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            RestInteractionResponsePayload pld;

            if (Type != InteractionResponseType.AutoCompleteResult)
            {
                var data = Builder != null ? new DiscordInteractionApplicationCommandCallbackData
                {
                    Content = Builder.Content ?? null,
                    Embeds = Builder.Embeds ?? null,
                    IsTts = Builder.IsTts,
                    Mentions = Builder.Mentions ?? null,
                    Flags = Builder.IsEphemeral ? MessageFlags.Ephemeral : null,
                    Components = Builder.Components ?? null,
                    Choices = null
                } : null;


                pld = new RestInteractionResponsePayload
                {
                    Type = Type,
                    Data = data
                };


                if (Builder != null && Builder.Files != null && Builder.Files.Count > 0)
                {
                    ulong fileId = 0;
                    List<DiscordAttachment> attachments = new();
                    foreach (var file in Builder.Files)
                    {
                        DiscordAttachment att = new()
                        {
                            Id = fileId,
                            Discord = this.Discord,
                            Description = file.Description,
                            FileName = file.FileName,
                            FileSize = null
                        };
                        attachments.Add(att);
                        fileId++;
                    }
                    pld.Attachments = attachments;
                    pld.Data.Attachments = attachments;
                }
            }
            else
            {
                pld = new RestInteractionResponsePayload
                {
                    Type = Type,
                    Data = new DiscordInteractionApplicationCommandCallbackData
                    {
                        Content = null,
                        Embeds = null,
                        IsTts = null,
                        Mentions = null,
                        Flags = null,
                        Components = null,
                        Choices = Builder.Choices,
                        Attachments = null
                    },
                    Attachments = null
                };
            }

            var values = new Dictionary<string, string>();

            if (Builder != null)
                if (!string.IsNullOrEmpty(Builder.Content) || Builder.Embeds?.Count() > 0 || Builder.IsTts == true || Builder.Mentions != null || Builder.Files?.Count > 0)
                    values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.Interactions}/:interaction_id/:interaction_token{Endpoints.Callback}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { interaction_id = InteractionId, interaction_token = InteractionToken }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "false").Build();
            if (Builder != null)
            {
                await this.DoMultipart(this.Discord, bucket, url, RestRequestMethod.Post, route, Values: values, Files: Builder.Files);

                foreach (var file in Builder.Files.Where(X => X.ResetPositionTo.HasValue))
                {
                    file.Stream.Position = file.ResetPositionTo.Value;
                }
            }
            else
            {
                await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld));
            }
        }

        /// <summary>
        /// Creates the interaction response async.
        /// </summary>
        /// <param name="interaction_id">The interaction_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="Type">The type.</param>
        /// <param name="Builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task CreateInteractionModalResponseAsync(ulong InteractionId, string InteractionToken, InteractionResponseType Type, DiscordInteractionModalBuilder Builder)
        {
            if (Builder.ModalComponents.Any(Mc => Mc.Components.Any(C => C.Type != Enums.ComponentType.InputText)))
                throw new NotSupportedException("Can't send any other type then Input Text as Modal Component.");

            var pld = new RestInteractionModalResponsePayload
            {
                Type = Type,
                Data = new DiscordInteractionApplicationCommandModalCallbackData
                {
                    Title = Builder.Title,
                    CustomId = Builder.CustomId,
                    ModalComponents = Builder.ModalComponents
                }
            };

            var values = new Dictionary<string, string>();

            var route = $"{Endpoints.Interactions}/:interaction_id/:interaction_token{Endpoints.Callback}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { interaction_id = InteractionId, interaction_token = InteractionToken }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
            await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Post, route, Payload: DiscordJson.SerializeObject(pld));
        }

        /// <summary>
        /// Gets the original interaction response async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> GetOriginalInteractionResponse(ulong ApplicationId, string InteractionToken) =>
            this.GetWebhookMessageAsync(ApplicationId, InteractionToken, Endpoints.Original, null);

        /// <summary>
        /// Edits the original interaction response async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="Builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> EditOriginalInteractionResponse(ulong ApplicationId, string InteractionToken, DiscordWebhookBuilder Builder) =>
            this.EditWebhookMessageAsync(ApplicationId, InteractionToken, Endpoints.Original, Builder, null);

        /// <summary>
        /// Deletes the original interaction response async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteOriginalInteractionResponse(ulong ApplicationId, string InteractionToken) =>
            this.DeleteWebhookMessageAsync(ApplicationId, InteractionToken, Endpoints.Original, null);

        /// <summary>
        /// Creates the followup message async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="Builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal async Task<DiscordMessage> CreateFollowupMessageAsync(ulong ApplicationId, string InteractionToken, DiscordFollowupMessageBuilder Builder)
        {
            Builder.Validate();

            if (Builder.Embeds != null)
                foreach (var embed in Builder.Embeds)
                    if (embed.Timestamp != null)
                        embed.Timestamp = embed.Timestamp.Value.ToUniversalTime();

            var values = new Dictionary<string, string>();
            var pld = new RestFollowupMessageCreatePayload
            {
                Content = Builder.Content,
                IsTts = Builder.IsTts,
                Embeds = Builder.Embeds,
                Flags = Builder.Flags,
                Components = Builder.Components
            };


            if (Builder.Files != null && Builder.Files.Count > 0)
            {
                ulong fileId = 0;
                List<DiscordAttachment> attachments = new();
                foreach (var file in Builder.Files)
                {
                    DiscordAttachment att = new()
                    {
                        Id = fileId,
                        Discord = this.Discord,
                        Description = file.Description,
                        FileName = file.FileName,
                        FileSize = null
                    };
                    attachments.Add(att);
                    fileId++;
                }
                pld.Attachments = attachments;
            }

            if (Builder.Mentions != null)
                pld.Mentions = new DiscordMentions(Builder.Mentions, Builder.Mentions.Any());

            if (!string.IsNullOrEmpty(Builder.Content) || Builder.Embeds?.Count() > 0 || Builder.IsTts == true || Builder.Mentions != null || Builder.Files?.Count > 0)
                values["payload_json"] = DiscordJson.SerializeObject(pld);

            var route = $"{Endpoints.Webhooks}/:application_id/:interaction_token";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Post, route, new { application_id = ApplicationId, interaction_token = InteractionToken }, out var path);

            var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration).AddParameter("wait", "true").Build();
            var res = await this.DoMultipart(this.Discord, bucket, url, RestRequestMethod.Post, route, Values: values, Files: Builder.Files).ConfigureAwait(false);
            var ret = JsonConvert.DeserializeObject<DiscordMessage>(res.Response);

            foreach (var att in ret._attachments)
            {
                att.Discord = this.Discord;
            }

            foreach (var file in Builder.Files.Where(X => X.ResetPositionTo.HasValue))
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
        internal Task<DiscordMessage> GetFollowupMessage(ulong ApplicationId, string InteractionToken, ulong MessageId) =>
            this.GetWebhookMessage(ApplicationId, InteractionToken, MessageId);

        /// <summary>
        /// Edits the followup message async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <param name="Builder">The builder.</param>
        /// <returns>A Task.</returns>
        internal Task<DiscordMessage> EditFollowupMessage(ulong ApplicationId, string InteractionToken, ulong MessageId, DiscordWebhookBuilder Builder) =>
            this.EditWebhookMessageAsync(ApplicationId, InteractionToken, MessageId.ToString(), Builder, null);

        /// <summary>
        /// Deletes the followup message async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <param name="interaction_token">The interaction_token.</param>
        /// <param name="message_id">The message_id.</param>
        /// <returns>A Task.</returns>
        internal Task DeleteFollowupMessage(ulong ApplicationId, string InteractionToken, ulong MessageId) =>
            this.DeleteWebhookMessage(ApplicationId, InteractionToken, MessageId);
        #endregion

        #region Misc
        /// <summary>
        /// Gets the current application info async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal Task<TransportApplication> GetCurrentApplicationInfo()
            => this.GetApplicationInfo("@me");

        /// <summary>
        /// Gets the application info async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <returns>A Task.</returns>
        internal Task<TransportApplication> GetApplicationInfo(ulong ApplicationId)
            => this.GetApplicationInfo(ApplicationId.ToString(CultureInfo.InvariantCulture));

        /// <summary>
        /// Gets the application info async.
        /// </summary>
        /// <param name="application_id">The application_id.</param>
        /// <returns>A Task.</returns>
        private async Task<TransportApplication> GetApplicationInfo(string ApplicationId)
        {
            var route = $"{Endpoints.Oauth2}{Endpoints.Applications}/:application_id";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = ApplicationId }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TransportApplication>(res.Response);
        }

        /// <summary>
        /// Gets the application assets async.
        /// </summary>
        /// <param name="Application">The application.</param>
        /// <returns>A Task.</returns>
        internal async Task<IReadOnlyList<DiscordApplicationAsset>> GetApplicationAssetsAsync(DiscordApplication Application)
        {
            var route = $"{Endpoints.Oauth2}{Endpoints.Applications}/:application_id{Endpoints.Assets}";
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { application_id = Application.Id }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route).ConfigureAwait(false);

            var assets = JsonConvert.DeserializeObject<IEnumerable<DiscordApplicationAsset>>(res.Response);
            foreach (var asset in assets)
            {
                asset.Discord = Application.Discord;
                asset.Application = Application;
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
            var route = Endpoints.Gateway;
            if (this.Discord.Configuration.TokenType == TokenType.Bot)
                route += Endpoints.Bot;
            var bucket = this.Rest.GetBucket(RestRequestMethod.Get, route, new { }, out var path);

            var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
            var res = await this.DoRequest(this.Discord, bucket, url, RestRequestMethod.Get, route, headers).ConfigureAwait(false);

            var info = JObject.Parse(res.Response).ToObject<GatewayInfo>();
            info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.resetAfter);
            return info;
        }
        #endregion

        #region DCS Internals
        /// <summary>
        /// Gets the DisCatSharp team.
        /// </summary>>
        internal async Task<DisCatSharpTeam> GetDisCatSharpTeamAsync()
        {
            try
            {
                var wc = new WebClient();
                var dcs = await wc.DownloadStringTaskAsync(new Uri("https://dcs.aitsys.dev/api/devs/"));
                var dcsGuild = await wc.DownloadStringTaskAsync(new Uri("https://dcs.aitsys.dev/api/guild/"));

                var app = JsonConvert.DeserializeObject<TransportApplication>(dcs);
                var guild = JsonConvert.DeserializeObject<DiscordGuild>(dcsGuild);

                var dcst = new DisCatSharpTeam
                {
                    IconHash = app.Team.IconHash,
                    TeamName = app.Team.Name,
                    PrivacyPolicyUrl = app.PrivacyPolicyUrl,
                    TermsOfServiceUrl = app.TermsOfServiceUrl,
                    RepoUrl = "https://github.com/Aiko-IT-Systems/DisCatSharp",
                    DocsUrl = "https://docs.dcs.aitsys.dev",
                    Id = app.Team.Id,
                    BannerHash = guild.BannerHash,
                    LogoHash = guild.IconHash,
                    GuildId = guild.Id,
                    Guild = guild,
                    SupportInvite = await this.GetInviteAsync("discatsharp", true, true, null)
                };
                List<DisCatSharpTeamMember> team = new();
                DisCatSharpTeamMember owner = new();
                foreach (var mb in app.Team.Members.OrderBy(M => M.User.Username))
                {
                    var tuser = await this.GetUser(mb.User.Id);
                    var user = mb.User;
                    if (mb.User.Id == 856780995629154305)
                    {
                        owner.Id = user.Id;
                        owner.Username = user.Username;
                        owner.Discriminator = user.Discriminator;
                        owner.AvatarHash = user.AvatarHash;
                        owner.BannerHash = tuser.BannerHash;
                        owner._bannerColor = tuser._bannerColor;
                        team.Add(owner);
                    }
                    else
                    {
                        team.Add(new()
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Discriminator = user.Discriminator,
                            AvatarHash = user.AvatarHash,
                            BannerHash = tuser.BannerHash,
                            _bannerColor = tuser._bannerColor
                        });
                    }
                }
                dcst.Owner = owner;
                dcst.Developers = team;

                return dcst;
            }
            catch (Exception ex)
            {
                this.Discord.Logger.LogDebug(ex.Message);
                this.Discord.Logger.LogDebug(ex.StackTrace);
                return null;
            }
        }
        #endregion
    }
}
