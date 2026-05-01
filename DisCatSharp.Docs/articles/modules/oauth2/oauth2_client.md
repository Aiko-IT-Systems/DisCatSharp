---
uid: modules_oauth2_oauth2_client
title: Discord OAuth2 Client
author: DisCatSharp Team
---

# DiscordOAuth2Client

We support integrating discords OAuth2 directly into DisCatSharp bots.
For this we've added the [DiscordOAuth2Client](xref:DisCatSharp.DiscordOAuth2Client).

For ASP.NET Core and self-hosted HTTP ingress, the recommended first-party package is `DisCatSharp.Hosting.AspNetCore`.

> [!IMPORTANT]
> `DisCatSharp.Extensions.OAuth2Web` is deprecated in favor of `DisCatSharp.Hosting.AspNetCore`.
> New applications should use the first-party ASP.NET Core ingress package for OAuth callbacks, signed interaction ingress, webhook events, proxy helpers, and validation helpers.

## Example

```cs
// Client registration
internal DiscordClient Client { get; private set; } // We assume you registered it somewhere
internal static DiscordOAuth2Client OAuth2Client { get; private set; }
internal static Uri RequestUri { get; set; }

// ...
OAuth2Client = new(3218382190382813, "thisistotallylegitsecret", "http://127.0.0.1:42069/oauth/");

// Event registration
OAuth2Client.OAuth2ClientErrored += (sender, args) =>
{
	Client.Logger.LogError(args.Exception, "OAuth2 Client error in {Event}. Exception: {Exception}", args.EventName, args.Exception.Message);
	return Task.CompletedTask;
};

// Some command class
[SlashCommand("test_oauth2", "Tests OAuth2")]
public static async Task TestOAuth2Async(InteractionContext ctx)
{
	await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Please wait.."));

	var state = OAuth2Client.GenerateState();

    RequestUri = OAuth2Client.GenerateOAuth2Url("identify connections", state);

	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(RequestUri.AbsoluteUri));
}

// Somewhere were you handle web stuff
var uri; // we assume you got the uri in your http handler

if (!OAuth2Client.ValidateState(RequestUri, uri)) // Validate the state
	return;

var token = await OAuth2Client.ExchangeAccessTokenAsync(OAuth2Client.GetCodeFromUri(uri)); // Exchange the code for an access token
var user = await OAuth2Client.GetCurrentUserAsync(token); // Get the user

// Work with your token. I.e. get the users connections
var connections = await OAuth2Client.GetCurrentUserConnectionsAsync(token);

```

## Example with DisCatSharp.Hosting.AspNetCore

```cs
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDisCatSharpAspNetCore(
    configureOAuth: options =>
    {
        options.ClientId = 3218382190382813;
        options.ClientSecret = "thisistotallylegitsecret";
        options.RedirectUri = "https://example.com/discord/oauth/callback";
    });

var app = builder.Build();

app.MapDisCatSharpIngress();
app.Run();
```

For bots that do not already own an ASP.NET Core app, the same package also exposes a self-hosted mode:

```cs
using DisCatSharp.Hosting.AspNetCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddDisCatSharpAspNetCoreSelfHost(options =>
{
    options.ListenPort = 42069;
    options.BaseUrl = new Uri("https://example.com");
});
```

## Migration from OAuth2Web

If you currently use `DisCatSharp.Extensions.OAuth2Web`, migrate as follows:

1. Replace `UseOAuth2Web(...)` / `UseOAuth2WebAsync(...)` with service registration via `AddDisCatSharpAspNetCore(...)` or `AddDisCatSharpAspNetCoreSelfHost(...)`.
2. Replace manual `Start()` / `StopAsync()` calls with:
   - `app.MapDisCatSharpIngress()` in an existing ASP.NET Core app, or
   - host-managed self-hosted ingress via `AddDisCatSharpAspNetCoreSelfHost(...)`.
3. Replace Apache-only proxy generation with the new helper APIs in `DisCatSharp.Hosting.AspNetCore`, which cover:
   - NGINX
   - Apache
   - common Docker-oriented reverse proxies
4. Replace old redirect/proxy checks with the new validation helpers in `DisCatSharp.Hosting.AspNetCore.Validation`.

The old extension remains a useful reference for historical behavior, but it is no longer the recommended path.

## Limitations

`DiscordOAuth2Client` remains the protocol client.
The ASP.NET Core ingress package provides the HTTP-facing integration and first-party ingress helpers, but you can still use the access token and build additional calls yourself if needed.
