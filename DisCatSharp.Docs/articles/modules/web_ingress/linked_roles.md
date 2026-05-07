---
uid: modules_web_ingress_linked_roles
title: Linked Roles
author: DisCatSharp Team
---

# Linked Roles

`DisCatSharp.Hosting.AspNetCore` includes a small helper layer for Discord linked roles.

Those helpers live under `DisCatSharp.Hosting.AspNetCore.LinkedRoles`.

The package does **not** own your app-specific verification page or your account-linking business logic. Instead, it helps with the parts that are repetitive across apps:

- computing the public verification URL
- syncing linked-role metadata records with Discord
- publishing a user's role connection from a successful OAuth callback result

## Feature boundary

The linked-roles verification URL is **your** public app route. It is not one of the ingress endpoints created by `MapDisCatSharpIngress()`.

That split is intentional:

- ingress endpoints handle Discord-owned HTTP surfaces such as OAuth callbacks and interactions
- linked roles help you keep your own verification route and Discord's application metadata in sync

## Register the helper

```cs
using DisCatSharp.Hosting.AspNetCore;

builder.Services
    .AddDisCatSharpAspNetCore(configureOAuth: options =>
    {
        options.ClientId = 123456789012345678;
        options.ClientSecret = "YOUR_CLIENT_SECRET";
        options.RedirectUri = "https://bot.example.com/discord/oauth/callback";
    })
    .AddDiscordLinkedRolesSupport(options =>
    {
        options.VerificationPath = "role-connections/verify";
    });
```

## Compute the verification URL

Discord's **Linked Roles Verification URL** is your app's public entry point for the linked-role flow.

```cs
using DisCatSharp.Hosting.AspNetCore.LinkedRoles;

var linkedRoles = app.Services.GetRequiredService<DiscordLinkedRolesService>();
var verificationUrl = linkedRoles.GetVerificationUrl(new Uri("https://bot.example.com"));
```

With the default ingress settings and `VerificationPath = "role-connections/verify"`, the computed URL becomes:

```text
https://bot.example.com/role-connections/verify
```

Use that value in the Developer Portal's **Linked Roles Verification URL** field.

If you want DisCatSharp to update the application setting for you:

```cs
await linkedRoles.UpdateVerificationUrlAsync(discordClient, new Uri("https://bot.example.com"));
```

Because the verification URL belongs to your app, you still need to expose that route yourself:

```cs
app.MapGet("/role-connections/verify", () => Results.Text("Render your verification experience here."));
```

## Sync metadata records

Register a metadata provider:

```cs
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.LinkedRoles;

builder.Services.AddDiscordLinkedRolesMetadataProvider<MyLinkedRolesMetadataProvider>();

public sealed class MyLinkedRolesMetadataProvider : IDiscordLinkedRolesMetadataProvider
{
    public ValueTask<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> GetMetadataAsync(CancellationToken cancellationToken = default)
        => new((IReadOnlyList<DiscordApplicationRoleConnectionMetadata>)
        [
            new DiscordApplicationRoleConnectionMetadata(
                ApplicationRoleConnectionMetadataType.BooleanEqual,
                "verified",
                "Verified Account",
                "Whether the account is verified")
        ]);
}
```

Then synchronize the schema:

```cs
await linkedRoles.SynchronizeMetadataAsync(discordClient);
```

The helper compares the current metadata with the provider output and only sends an update when the schema differs.

## Publish a user's role connection from OAuth

After your OAuth callback succeeds, use the exchanged access token captured by [DiscordOAuthCallbackResult](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.DiscordOAuthCallbackResult):

```cs
var callbackResult = await oauthCallbackHandler.HandleAsync(callbackRequest);

await linkedRoles.PublishRoleConnectionAsync(
    oauth2Client,
    callbackResult,
    platformName: "Example",
    platformUsername: "lala",
    metadata: new ApplicationRoleConnectionMetadata()
        .AddMetadata("verified", "1"));
```

The callback result must be successful and the granted scope must include `role_connections.write`.

You can check that requirement ahead of time with `linkedRoles.HasRoleConnectionsWriteScope(callbackResult)`.

## Existing app vs self-host

The helper service is hosting-model agnostic.

- existing ASP.NET Core app: register the helper, map your verification route, and optionally use the OAuth callback endpoint
- self-host mode: register the same helper services, but your linked-roles verification page still belongs to your own app surface outside the self-hosted ingress routes

## What you still own

The helper does not replace your application logic. You still need to provide:

1. the verification page or route at the configured verification URL
2. any user/account lookup required by your external service
3. the metadata values you want to publish for each user

To validate the resulting verification URL against the Developer Portal, see [Proxy and Validation Helpers](proxy_and_validation.md).
