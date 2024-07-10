using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Entities;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a webhook-only client. This client can be used to execute Discord Webhooks.
/// </summary>
public class DiscordWebhookClient
{
	/// <summary>
	/// Gets the logger for this client.
	/// </summary>
	public ILogger<DiscordWebhookClient> Logger { get; }

	/// <summary>
	/// Gets the collection of registered webhooks.
	/// </summary>
	public IReadOnlyList<DiscordWebhook> Webhooks { get; }

	/// <summary>
	/// Gets or sets the username for registered webhooks. Note that this only takes effect when broadcasting.
	/// </summary>
	public string? Username { get; set; }

	/// <summary>
	/// Gets or set the avatar for registered webhooks. Note that this only takes effect when broadcasting.
	/// </summary>
	public string? AvatarUrl { get; set; }

	internal List<DiscordWebhook> Hooks;
	internal readonly DiscordApiClient ApiClient;

	internal readonly LogLevel MinimumLogLevel;
	internal readonly string LogTimestampFormat;

	/// <summary>
	/// Creates a new webhook client.
	/// </summary>
	public DiscordWebhookClient()
		: this(null!, null)
	{ }

	/// <summary>
	/// Creates a new webhook client, with specified HTTP proxy, timeout, and logging settings.
	/// </summary>
	/// <param name="proxy">The proxy to use for HTTP connections. Defaults to null.</param>
	/// <param name="timeout">The optional timeout to use for HTTP requests. Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts. Defaults to null.</param>
	/// <param name="useRelativeRateLimit">Whether to use the system clock for computing rate limit resets. See <see cref="DiscordConfiguration.UseRelativeRatelimit"/> for more details. Defaults to true.</param>
	/// <param name="loggerFactory">The optional logging factory to use for this client. Defaults to null.</param>
	/// <param name="minimumLogLevel">The minimum logging level for messages. Defaults to information.</param>
	/// <param name="logTimestampFormat">The timestamp format to use for the logger.</param>
	public DiscordWebhookClient(
		IWebProxy proxy = null!,
		TimeSpan? timeout = null,
		bool useRelativeRateLimit = true,
		ILoggerFactory loggerFactory = null!,
		LogLevel minimumLogLevel = LogLevel.Information,
		string logTimestampFormat = "yyyy-MM-dd HH:mm:ss zzz"
	)
	{
		this.MinimumLogLevel = minimumLogLevel;
		this.LogTimestampFormat = logTimestampFormat;

		if (loggerFactory == null!)
		{
			loggerFactory = new DefaultLoggerFactory();
			loggerFactory.AddProvider(new DefaultLoggerProvider(this));
		}

		this.Logger = loggerFactory.CreateLogger<DiscordWebhookClient>();

		var parsedTimeout = timeout ?? TimeSpan.FromSeconds(10);

		this.ApiClient = new(proxy!, parsedTimeout, useRelativeRateLimit, this.Logger);
		this.Hooks = [];
		this.Webhooks = new ReadOnlyCollection<DiscordWebhook>(this.Hooks);
	}

	/// <summary>
	/// Registers a webhook with this client. This retrieves a webhook based on the ID and token supplied.
	/// </summary>
	/// <param name="id">The ID of the webhook to add.</param>
	/// <param name="token">The token of the webhook to add.</param>
	/// <returns>The registered webhook.</returns>
	public async Task<DiscordWebhook> AddWebhookAsync(ulong id, string token)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentNullException(nameof(token));

		token = token.Trim();

		if (this.Hooks.Any(x => x.Id == id))
			throw new InvalidOperationException("This webhook is registered with this client.");

		var wh = await this.ApiClient.GetWebhookWithTokenAsync(id, token).ConfigureAwait(false);
		this.Hooks.Add(wh);

		return wh;
	}

	/// <summary>
	/// Registers a webhook with this client. This retrieves a webhook from webhook URL.
	/// </summary>
	/// <param name="url">URL of the webhook to retrieve. This URL must contain both ID and token.</param>
	/// <returns>The registered webhook.</returns>
	public Task<DiscordWebhook> AddWebhookAsync(Uri url)
	{
		ArgumentNullException.ThrowIfNull(url);

		var m = DiscordRegEx.WebhookRegex().Match(url.ToString());
		if (!m.Success)
			throw new ArgumentException("Invalid webhook URL supplied.", nameof(url));

		var idRaw = m.Groups["id"];
		var tokenRaw = m.Groups["token"];
		if (!ulong.TryParse(idRaw.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
			throw new ArgumentException("Invalid webhook URL supplied.", nameof(url));

		var token = tokenRaw.Value;
		return this.AddWebhookAsync(id, token);
	}

	/// <summary>
	/// Registers a webhook with this client. This retrieves a webhook using the supplied full discord client.
	/// </summary>
	/// <param name="id">ID of the webhook to register.</param>
	/// <param name="client">Discord client to which the webhook will belong.</param>
	/// <returns>The registered webhook.</returns>
	public async Task<DiscordWebhook> AddWebhookAsync(ulong id, BaseDiscordClient client)
	{
		ArgumentNullException.ThrowIfNull(client);

		if (this.Hooks.Any(x => x.Id == id))
			throw new ArgumentException("This webhook is already registered with this client.");

		var wh = await client.ApiClient.GetWebhookAsync(id).ConfigureAwait(false);

		this.Hooks.Add(wh);

		return wh;
	}

	/// <summary>
	/// Registers a webhook with this client. This reuses the supplied webhook object.
	/// </summary>
	/// <param name="webhook">Webhook to register.</param>
	/// <returns>The registered webhook.</returns>
	public DiscordWebhook AddWebhook(DiscordWebhook webhook)
	{
		ArgumentNullException.ThrowIfNull(webhook);

		if (this.Hooks.Any(x => x.Id == webhook.Id))
			throw new ArgumentException("This webhook is already registered with this client.");

		webhook.ApiClient = this.ApiClient;
		this.Hooks.Add(webhook);

		return webhook;
	}

	/// <summary>
	/// Unregisters a webhook with this client.
	/// </summary>
	/// <param name="id">ID of the webhook to unregister.</param>
	/// <returns>The unregistered webhook.</returns>
	public DiscordWebhook RemoveWebhook(ulong id)
	{
		if (this.Hooks.All(x => x.Id != id))
			throw new ArgumentException("This webhook is not registered with this client.");

		var wh = this.GetRegisteredWebhook(id);
		this.Hooks.Remove(wh!);
		return wh!;
	}

	/// <summary>
	/// Gets a registered webhook with specified ID.
	/// </summary>
	/// <param name="id">ID of the registered webhook to retrieve.</param>
	/// <returns>The requested webhook.</returns>
	public DiscordWebhook? GetRegisteredWebhook(ulong id)
		=> this.Hooks.FirstOrDefault(xw => xw.Id == id);

	/// <summary>
	/// Broadcasts a message to all registered webhooks.
	/// </summary>
	/// <param name="builder">Webhook builder filled with data to send.</param>
	/// <returns>A dictionary of <see cref="DisCatSharp.Entities.DiscordWebhook"/>s and <see cref="DisCatSharp.Entities.DiscordMessage"/>s.</returns>
	public async Task<Dictionary<DiscordWebhook, DiscordMessage>> BroadcastMessageAsync(DiscordWebhookBuilder builder)
	{
		var deadHooks = new List<DiscordWebhook>();
		var messages = new Dictionary<DiscordWebhook, DiscordMessage>();

		foreach (var hook in this.Hooks)
			try
			{
				if (this.Username != null)
					builder = builder.WithUsername(this.Username);
				if (this.AvatarUrl != null)
					builder = builder.WithAvatarUrl(this.AvatarUrl);
				messages.Add(hook, await hook.ExecuteAsync(builder).ConfigureAwait(false));
			}
			catch (NotFoundException)
			{
				deadHooks.Add(hook);
			}

		// Removing dead webhooks from collection
		foreach (var xwh in deadHooks)
			this.Hooks.Remove(xwh);

		return messages;
	}

	~DiscordWebhookClient()
	{
		this.Hooks.Clear();
		this.Hooks = null!;
		this.ApiClient.Rest.Dispose();
	}
}
