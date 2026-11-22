---
uid: topics_social_sdk_bot_api
title: Social SDK Bot API
author: DisCatSharp Team
---

# Social SDK operations using a bot

`DiscordSocialClient` uses an existing `DiscordClient` and its bot token for Discord's server-side Social SDK operations. Access to game-stat widgets or Social SDK features can require enablement by Discord; profile and identity operations also require the user to authorize the application with `application_identities.write`.

```csharp
var social = new DiscordSocialClient(discord);
var data = new DiscordGameProfileData
{
    Primary = new DiscordGamePrimaryStats { RankName = "Silver", TotalWins = 57 },
    Dynamic = [DiscordGameDynamicField.Number("score", 123.5)]
};

await social.UpdateProfileAsync(applicationId, userId, externalUserId, data: data);
```

When an update includes `data`, Discord **replaces the entire existing data object**. Send all primary and dynamic values you want to keep. Omit `data` to leave the existing stats untouched. The first successful profile update can create a profile-only identity; later updates must use a matching external user ID. Use `GetUserIdentitiesAsync` or `GetIdentitiesByExternalIdAsync` to inspect the identities, and `GetProfileAsync` to read stats. `DeleteIdentityAsync` deletes the selected identity and its profile data; Discord prevents deletion of the last account-linking identity.

## Provisional accounts

`GetProvisionalTokenAsync` uses your own account system's user ID to create or retrieve a provisional account token. Keep the bot token on your backend and only return the provisional access token to the authenticated game client. Both returned tokens are secrets; the REST trace logger redacts the issuance response. `UnmergeProvisionalAccountAsync` uses the same external user ID, not the issued access token.

## Message moderation

Use `UpdateDirectMessageModerationAsync` to set application-scoped moderation metadata for a Social SDK direct message. Provide both participant IDs and the message ID; the order of participants does not matter. Discord's Social SDK webhook events are available through the ASP.NET Core webhook event ingress. Lobby message moderation is exposed separately by `DiscordLobbyClient`.

See Discord's [application identity profile reference](https://docs.discord.com/developers/resources/application-identity-profile), [bot-token provisional account guide](https://docs.discord.com/developers/discord-social-sdk/development-guides/provisional-accounts/bot-token-endpoint), and [moderation guide](https://docs.discord.com/developers/discord-social-sdk/how-to/integrate-moderation) for current access requirements and limits.
