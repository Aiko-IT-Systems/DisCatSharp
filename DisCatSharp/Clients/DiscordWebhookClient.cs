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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using Microsoft.Extensions.Logging;

namespace DisCatSharp
{
    /// <summary>
    /// Represents a webhook-only client. This client can be used to execute Discord webhooks.
    /// </summary>
    public class DiscordWebhookClient
    {

        /// <summary>
        /// Gets the logger for this client.
        /// </summary>
        public ILogger<DiscordWebhookClient> Logger { get; }

        /// <summary>
        /// Gets the webhook regex.
        /// this regex has 2 named capture groups: "id" and "token".
        /// </summary>
        private static Regex WebhookRegex { get; } = new Regex(@"(?:https?:\/\/)?discord(?:app)?.com\/api\/(?:v\d\/)?webhooks\/(?<id>\d+)\/(?<token>[A-Za-z0-9_\-]+)", RegexOptions.ECMAScript);

        /// <summary>
        /// Gets the collection of registered webhooks.
        /// </summary>
        public IReadOnlyList<DiscordWebhook> Webhooks { get; }

        /// <summary>
        /// Gets or sets the username override for registered webhooks. Note that this only takes effect when broadcasting.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or set the avatar override for registered webhooks. Note that this only takes effect when broadcasting.
        /// </summary>
        public string AvatarUrl { get; set; }

        internal List<DiscordWebhook> _hooks;
        internal DiscordApiClient _apiclient;

        internal LogLevel _minimumLogLevel;
        internal string _logTimestampFormat;

        /// <summary>
        /// Creates a new webhook client.
        /// </summary>
        public DiscordWebhookClient()
            : this(null, null)
        { }

        /// <summary>
        /// Creates a new webhook client, with specified HTTP proxy, timeout, and logging settings.
        /// </summary>
        /// <param name="Proxy">Proxy to use for HTTP connections.</param>
        /// <param name="Timeout">Timeout to use for HTTP requests. Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts.</param>
        /// <param name="UseRelativeRateLimit">Whether to use the system clock for computing rate limit resets. See <see cref="DiscordConfiguration.UseRelativeRatelimit"/> for more details.</param>
        /// <param name="LoggerFactory">The optional logging factory to use for this client.</param>
        /// <param name="MinimumLogLevel">The minimum logging level for messages.</param>
        /// <param name="LogTimestampFormat">The timestamp format to use for the logger.</param>
        public DiscordWebhookClient(IWebProxy Proxy = null, TimeSpan? Timeout = null, bool UseRelativeRateLimit = true,
            ILoggerFactory LoggerFactory = null, LogLevel MinimumLogLevel = LogLevel.Information, string LogTimestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
        {
            this._minimumLogLevel = MinimumLogLevel;
            this._logTimestampFormat = LogTimestampFormat;

            if (LoggerFactory == null)
            {
                LoggerFactory = new DefaultLoggerFactory();
                LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
            }

            this.Logger = LoggerFactory.CreateLogger<DiscordWebhookClient>();

            var parsedTimeout = Timeout ?? TimeSpan.FromSeconds(10);

            this._apiclient = new DiscordApiClient(Proxy, parsedTimeout, UseRelativeRateLimit, this.Logger);
            this._hooks = new List<DiscordWebhook>();
            this.Webhooks = new ReadOnlyCollection<DiscordWebhook>(this._hooks);
        }

        /// <summary>
        /// Registers a webhook with this client. This retrieves a webhook based on the ID and token supplied.
        /// </summary>
        /// <param name="Id">The ID of the webhook to add.</param>
        /// <param name="Token">The token of the webhook to add.</param>
        /// <returns>The registered webhook.</returns>
        public async Task<DiscordWebhook> AddWebhookAsync(ulong Id, string Token)
        {
            if (string.IsNullOrWhiteSpace(Token))
                throw new ArgumentNullException(nameof(Token));
            Token = Token.Trim();

            if (this._hooks.Any(X => X.Id == Id))
                throw new InvalidOperationException("This webhook is registered with this client.");

            var wh = await this._apiclient.GetWebhookWithTokenAsync(Id, Token).ConfigureAwait(false);
            this._hooks.Add(wh);

            return wh;
        }

        /// <summary>
        /// Registers a webhook with this client. This retrieves a webhook from webhook URL.
        /// </summary>
        /// <param name="Url">URL of the webhook to retrieve. This URL must contain both ID and token.</param>
        /// <returns>The registered webhook.</returns>
        public Task<DiscordWebhook> AddWebhook(Uri Url)
        {
            if (Url == null)
                throw new ArgumentNullException(nameof(Url));

            var m = WebhookRegex.Match(Url.ToString());
            if (!m.Success)
                throw new ArgumentException("Invalid webhook URL supplied.", nameof(Url));

            var idraw = m.Groups["id"];
            var tokenraw = m.Groups["token"];
            if (!ulong.TryParse(idraw.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                throw new ArgumentException("Invalid webhook URL supplied.", nameof(Url));

            var token = tokenraw.Value;
            return this.AddWebhookAsync(id, token);
        }

        /// <summary>
        /// Registers a webhook with this client. This retrieves a webhook using the supplied full discord client.
        /// </summary>
        /// <param name="Id">ID of the webhook to register.</param>
        /// <param name="Client">Discord client to which the webhook will belong.</param>
        /// <returns>The registered webhook.</returns>
        public async Task<DiscordWebhook> AddWebhookAsync(ulong Id, BaseDiscordClient Client)
        {
            if (Client == null)
                throw new ArgumentNullException(nameof(Client));

            if (this._hooks.Any(X => X.Id == Id))
                throw new ArgumentException("This webhook is already registered with this client.");

            var wh = await Client.ApiClient.GetWebhookAsync(Id).ConfigureAwait(false);
            // personally I don't think we need to override anything.
            // it would even make sense to keep the hook as-is, in case
            // it's returned without a token for some bizarre reason
            // remember -- discord is not really consistent
            //var nwh = new DiscordWebhook()
            //{
            //    ApiClient = _apiclient,
            //    AvatarHash = wh.AvatarHash,
            //    ChannelId = wh.ChannelId,
            //    GuildId = wh.GuildId,
            //    Id = wh.Id,
            //    Name = wh.Name,
            //    Token = wh.Token,
            //    User = wh.User,
            //    Discord = null
            //};
            this._hooks.Add(wh);

            return wh;
        }

        /// <summary>
        /// Registers a webhook with this client. This reuses the supplied webhook object.
        /// </summary>
        /// <param name="Webhook">Webhook to register.</param>
        /// <returns>The registered webhook.</returns>
        public DiscordWebhook AddWebhook(DiscordWebhook Webhook)
        {
            if (Webhook == null)
                throw new ArgumentNullException(nameof(Webhook));

            if (this._hooks.Any(X => X.Id == Webhook.Id))
                throw new ArgumentException("This webhook is already registered with this client.");

            // see line 128-131 for explanation
            // For christ's sake, update the line numbers if they change.
            //var nwh = new DiscordWebhook()
            //{
            //    ApiClient = _apiclient,
            //    AvatarHash = webhook.AvatarHash,
            //    ChannelId = webhook.ChannelId,
            //    GuildId = webhook.GuildId,
            //    Id = webhook.Id,
            //    Name = webhook.Name,
            //    Token = webhook.Token,
            //    User = webhook.User,
            //    Discord = null
            //};
            this._hooks.Add(Webhook);

            return Webhook;
        }

        /// <summary>
        /// Unregisters a webhook with this client.
        /// </summary>
        /// <param name="Id">ID of the webhook to unregister.</param>
        /// <returns>The unregistered webhook.</returns>
        public DiscordWebhook RemoveWebhook(ulong Id)
        {
            if (!this._hooks.Any(X => X.Id == Id))
                throw new ArgumentException("This webhook is not registered with this client.");

            var wh = this.GetRegisteredWebhook(Id);
            this._hooks.Remove(wh);
            return wh;
        }

        /// <summary>
        /// Gets a registered webhook with specified ID.
        /// </summary>
        /// <param name="Id">ID of the registered webhook to retrieve.</param>
        /// <returns>The requested webhook.</returns>
        public DiscordWebhook GetRegisteredWebhook(ulong Id)
            => this._hooks.FirstOrDefault(Xw => Xw.Id == Id);

        /// <summary>
        /// Broadcasts a message to all registered webhooks.
        /// </summary>
        /// <param name="Builder">Webhook builder filled with data to send.</param>
        /// <returns></returns>
        public async Task<Dictionary<DiscordWebhook, DiscordMessage>> BroadcastMessageAsync(DiscordWebhookBuilder Builder)
        {
            var deadhooks = new List<DiscordWebhook>();
            var messages = new Dictionary<DiscordWebhook, DiscordMessage>();

            foreach (var hook in this._hooks)
            {
                try
                {
                    messages.Add(hook, await hook.Execute(Builder).ConfigureAwait(false));
                }
                catch (NotFoundException)
                {
                    deadhooks.Add(hook);
                }
            }

            // Removing dead webhooks from collection
            foreach (var xwh in deadhooks) this._hooks.Remove(xwh);

            return messages;
        }

        ~DiscordWebhookClient()
        {
            this._hooks.Clear();
            this._hooks = null;
            this._apiclient.Rest.Dispose();
        }
    }
}
