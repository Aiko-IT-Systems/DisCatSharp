---
uid: modules_oauth2_oauth2_client
title: Discord OAuth2 Client
author: DisCatSharp Team
---

# DiscordOAuth2Client

`DiscordOAuth2Client` is the protocol client for Discord OAuth2 inside DisCatSharp.

Use it when you want to:

- generate authorization URLs
- generate and validate OAuth state
- exchange authorization codes
- refresh or revoke tokens
- call Discord OAuth-protected APIs

> [!IMPORTANT]
> `DiscordOAuth2Client` is **not deprecated**.
> Only the old `DisCatSharp.Extensions.OAuth2Web` package is deprecated.

If you need built-in callback endpoints, signed HTTP interactions, or signed webhook events, combine the client with `DisCatSharp.Hosting.AspNetCore`.

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

## When to add the ingress package

Add `DisCatSharp.Hosting.AspNetCore` when you want DisCatSharp to own the callback endpoint inside ASP.NET Core.

See:

- [OAuth2 Overview](xref:modules_oauth2_overview)
- `Web Ingress > Overview`
- `Web Ingress > OAuth Callbacks`
- `Web Ingress > Migrating from OAuth2Web`

## Limitations

`DiscordOAuth2Client` intentionally does **not** create or host a web server by itself.
If you need hosted callback routing, use `DisCatSharp.Hosting.AspNetCore` or your own web stack around the client.
