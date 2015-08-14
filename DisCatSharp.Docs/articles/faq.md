---
uid: faq
title: Frequently Asked Questions
author: DisCatSharp Team
---

# Frequently Asked Questions

## Why don't you provide light mode for the documentation?

Because light attracts bugs.

Jokes aside, I mainly use dark mode, and I don't want to maintain two versions of the documentation. CSS is painful enough.

## What is a snowflake

Discord calls their IDs snowflakes.

A snowflake explanation can be seen in the following image:

![snowflake](/images/snowflake.png)

## What is a gateway

Discord uses a websocket connection to send and receive data. This is called the gateway. The gateway is used to send and receive events, and to send and receive messages.

![gateway](/images/gateway.png)

## Code I copied from an article isn't compiling or working as expected. Why?

_Please use the code snippets as a reference; don't blindly copy-paste code!_

The snippets of code in the articles are meant to serve as examples to help you understand how to use a part of the library.

Although most will compile and work at the time of writing, changes to the library over time can make some snippets obsolete.

Many issues can be resolved with Intellisense by searching for similarly named methods and verifying method parameters.

## Connecting to a voice channel with VoiceNext will either hang or throw an exception.

To troubleshoot, please ensure that:

-   You are using the latest version of DisCatSharp.
-   You have properly enabled VoiceNext with your instance of @DisCatSharp.DiscordClient.
-   You are _not_ using VoiceNext in an event handler.
-   You have [opus and libsodium](xref:modules_audio_voicenext_prerequisites) available in your target environment.

## Why am I getting _heartbeat skipped_ message in my console?

There are two possible reasons:

### Connection issue between your bot application and Discord.

Check your internet connection and ensure that the machine your bot is hosted on has a stable internet connection.<br/>
If your local network has no issues, the problem could be with either Discord or Cloudflare. In which case, it's out of your control.

### Complex, long-running code in an event handler.

Any event handlers that have the potential to run for more than a few seconds could cause a deadlock, and cause several heartbeats to be skipped.
Please take a look at our short article on [handling exceptions](xref:topics_events) to learn how to avoid this.

## Why am I getting a 4XX error and how can I fix it?

| HTTP Error Code | Cause                       | Resolution                                                                                                                                                      |
| :-------------: | :-------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|      `401`      | Invalid token.              | Verify your token and make sure no errors were made.<br/>The client secret found on the 'general information' tab of your application page _is not_ your token. |
|      `403`      | Not enough permissions.     | Verify permissions and ensure your bot account has a role higher than the target user.<br/>Administrator permissions _do not_ bypass the role hierarchy.        |
|      `404`      | Requested object not found. | This usually means the entity does not exist. You should reattempt then inform your user.                                                                       |

## I cannot modify a specific user or role. Why is this?

In order to modify a user, the highest role of your bot account must be higher than the target user.<br/>
Changing the properties of a role requires that your bot account have a role higher than that role.

## Does CommandsNext support dependency injection?

It does! Please take a look at our [article](xref:modules_commandsnext_dependency_injection) on the subject.

Basically every module of DisCatSharp supports dependency injection.

## Can I use a user token?

Automating a user account is against Discord's [Terms of Service](https://dis.gd/terms) and is not supported by DisCatSharp.

## How can I set a custom status?

You can use either of the following

-   The overload for [ConnectAsync](xref:DisCatSharp.DiscordClient.ConnectAsync*)(@DisCatSharp.Entities.DiscordActivity, @DisCatSharp.Entities.UserStatus?, [](xref:System.DateTimeOffset)?) which accepts a [DiscordActivity](xref:DisCatSharp.Entities.DiscordActivity).
-   [UpdateStatusAsync](xref:DisCatSharp.DiscordClient.UpdateStatusAsync*)(@DisCatSharp.Entities.DiscordActivity, @DisCatSharp.Entities.UserStatus?, [](xref:System.DateTimeOffset)?) OR [UpdateStatusAsync](xref:DisCatSharp.DiscordShardedClient.UpdateStatusAsync*)(@DisCatSharp.Entities.DiscordActivity, @DisCatSharp.Entities.UserStatus?, [](xref:System.DateTimeOffset)?) (for the sharded client) at any point after `Ready` has been fired.

## Am I able to retrieve a @DisCatSharp.Entities.DiscordRole by name?

Yes. Use LINQ on the `Roles` property of your instance of [DiscordGuild](xref:DisCatSharp.Entities.DiscordGuild) and compare against the `Name` of each [DiscordRole](xref:DisCatSharp.Entities.DiscordRole).

## Why are my events not firing?

This is because in the Discord V8+ API, they require @DisCatSharp.Enums.DiscordIntents to be enabled on [DiscordConfiguration](xref:DisCatSharp.DiscordConfiguration) and the
Discord Application Portal. We have an [article](xref:topics_intents) that covers all that has to be done to set this up.
