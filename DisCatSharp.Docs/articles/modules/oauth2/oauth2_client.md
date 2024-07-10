---
uid: modules_oauth2_oauth2_client
title: Discord OAuth2 Client
author: DisCatSharp Team
---

# DiscordOAuth2Client

We support integrating discords OAuth2 directly into DisCatSharp bots.
For this we've added the [DiscordOAuth2Client](xref:DisCatSharp.DiscordOAuth2Client).

Additionally, we provide a new extension called [OAuth2WebExtention](xref:DisCatSharp.Extensions.OAuth2Web.OAuth2WebExtension) in the package `DisCatSharp.Extensions.OAuth2Web`, which provides you with a web server implementation and certain events.

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

## Example with OAuth2Web Extension

```cs
// Client registration
internal DiscordClient Client { get; private set; } // We assume you registered it somewhere
internal OAuth2WebExtension OAuth2WebService { get; private set; }

// ...
this.OAuth2WebService = this.Client.UseOAuth2Web(new OAuth2WebConfiguration()
{
	ClientId = 3218382190382813, // Your client id
	ClientSecret = "thisistotallylegitsecret", // Your client secret
	RedirectUri = "http://127.0.0.1:42069/oauth/", // The redirect url, must be public accessible. Possible proxy it.
	ListenAll = true,
	SecureStates = true
});

// Event registration
var oauth2 = this.Client.GetOAuth2Web();
oauth2.AuthorizationCodeReceived += (sender, args) =>
{
	Client.Logger.LogDebug("Auth code received for {State}", args.ReceivedState);
	return Task.CompletedTask;
};
oauth2.AuthorizationCodeExchanged += (sender, args) =>
{
	Client.Logger.LogDebug("Auth code exchanged for {User} with state {State}", args.UserId, args.ReceivedState);
	return Task.CompletedTask;
};
oauth2.AccessTokenRefreshed += (sender, args) =>
{
	Client.Logger.LogDebug("Access token refresh for {User}", args.UserId);
	return Task.CompletedTask;
};
oauth2.AccessTokenRevoked += (sender, args) =>
{
	Client.Logger.LogDebug("Access token revoked for {User}", args.UserId);
	return Task.CompletedTask;
};
oauth2.OAuth2Client.OAuth2ClientErrored += (sender, args) =>
{
	Client.Logger.LogError(args.Exception, "OAuth2 Client error in {Event}. Exception: {Exception}", args.EventName, args.Exception.Message);
	return Task.CompletedTask;
};

// Client start
await this.Client.ConnectAsync();
this.Client.Logger.LogInformation("Starting OAuth2 Web");
this.OAuth2WebService.StartAsync();

// Some command class
[SlashCommand("test_oauth2", "Tests OAuth2")]
public static async Task TestOAuth2Async(InteractionContext ctx)
{
	await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Please wait.."));

	var web = ctx.Client.GetOAuth2Web();

	var uri = web.OAuth2Client.GenerateOAuth2Url("identify connections", web.OAuth2Client.GenerateSecureState(ctx.User.Id));
	web.SubmitPendingOAuth2Url(uri);

	await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(uri.AbsoluteUri));

	var res = await web.WaitForAccessTokenAsync(ctx.User, uri, TimeSpan.FromMinutes(1));
	if (!res.TimedOut)
	{
		var testData = await web.OAuth2Client.GetCurrentUserConnectionsAsync(res.Result.DiscordAccessToken);
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{testData.Count} total connections. First connection username: {testData.First().Name}"));
		await web.RevokeAccessTokenAsync(ctx.User);
	}
	else
		await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out :("));
}
```

## Limitations

We only implemented methods and routes which all users can use.
You can use the access token and build calls yourself if needed.
