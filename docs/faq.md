---
uid: faq
title: Frequently Asked Questions
---

# Frequently Asked Questions

### Code I copied from an article isn't compiling or working as expected. Why?
*Please use the code snippets as a reference; don't blindly copy-paste code!*

The snippets of code in the articles are meant to serve as examples to help you understand how to use a part of the library.<br/>
Although most will compile and work at the time of writing, changes to the library over time can make some snippets obsolete.<br/>
Many issues can be resolved with Intellisense by searching for similarly named methods and verifying method parameters.

### I'm targeting Mono and have exceptions, crashes, or other problems.
As mentioned in the [preamble](xref:preamble), the Mono runtime is inherently unstable and has numerous flaws.<br/>
Because of this we do not support Mono in any way, nor will we support any other projects which use it.

Instead, we recommend using either the latest LTS release or most recent stable version of [.NET Core](https://dotnet.microsoft.com/download).

### Connecting to a voice channel with VoiceNext will either hang or throw an exception.
To troubleshoot, please ensure that:

* You are using the latest version of DisCatSharp.
* You have properly enabled VoiceNext with your instance of @DisCatSharp.DiscordClient.
* You are *not* using VoiceNext in an event handler.
* You have [opus and libsodium](xref:voicenext_prerequisites) available in your target environment.


### Why am I getting *heartbeat skipped* message in my console?
There are two possible reasons:
#### Connection issue between your bot application and Discord.
Check your internet connection and ensure that the machine your bot is hosted on has a stable internet connection.<br/>
If your local network has no issues, the problem could be with either Discord or Cloudfare. In which case, it's out of your control.

#### Complex, long-running code in an event handler.
Any event handlers that have the potential to run for more than a few seconds could cause a deadlock, and cause several heartbeats to be skipped.
Please take a look at our short article on [handling DSharpPlus exceptions](xref:beyond_basics_events) to learn how to avoid this.


### Why am I getting a 4XX error and how can I fix it?
HTTP Error Code|Cause|Resolution
:---:|:---|:---
`401`|Invalid token.|Verify your token and make sure no errors were made.<br/>The client secret found on the 'general information' tab of your application page *is not* your token.
`403`|Not enough permissions.|Verify permissions and ensure your bot account has a role higher than the target user.<br/>Administrator permissions *do not* bypass the role hierarchy.
`404`|Requested object not found.|This usually means the entity does not exist. You should reattempt then inform your user.

### I cannot modify a specific user or role. Why is this?
In order to modify a user, the highest role of your bot account must be higher than the target user.<br/>
Changing the properties of a role requires that your bot account have a role higher than that role.

### Does CommandsNext support dependency injection?
It does! Please take a look at our [article](xref:commands_dependency_injection) on the subject.

### Can I use a user token?
Automating a user account is against Discord's [Terms of Service](https://dis.gd/terms) and is not supported by DisCatSharp.

### How can I set a custom status?
If you mean a *true* custom status like this:

![help](/images/faq_01.png)

No, you cannot. Discord does not allow bots to use custom statuses.

<br/>
However, if you meant an activity like this:

![Bot Presence](/images/faq_02.png)

You can use either of the following

* The overload for @DisCatSharp.DiscordClient.ConnectAsync(DiscordActivity,System.Nullable{UserStatus},System.Nullable{DateTimeOffset}) which accepts a @DisCatSharp.Entities.DiscordActivity.
* @DisCatSharp.DiscordClient.UpdateStatusAsync(DiscordActivity,System.Nullable{UserStatus},System.Nullable{DateTimeOffset}) OR @DisCatSharp.DiscordShardedClient.UpdateStatusAsync(DiscordActivity,System.Nullable{UserStatus},System.Nullable{DateTimeOffset}) (for the sharded client) at any point after `Ready` has been fired.

### Am I able to retrieve a @DisCatSharp.Entities.DiscordRole by name?
Yes. Use LINQ on the `Roles` property of your instance of @DisCatSharp.Entities.DiscordGuild and compare against the `Name` of  each @DisCatSharp.Entities.DiscordRole.

### Why are you using Newtonsoft.Json when System.Text.Json is available
Yes `System.Text.Json` is available to use but it still doesnt stand up to what we currently need which is why we still use Newtonsoft.Json.
Maybe in time we can switch to your favorite Json Deserializer but for right now we will be using Newtonsoft.Json for the forseeable future.

### Why the hell are my events not firing?
This is because in the Discord V8 API, they require @DisCatSharp.DiscordIntents to be enabled on @DisCatSharp.DiscordConfiguration and the
Discord Application Portal. We have an [article](xref:beyond_basics_intents) that covers all that has to be done to set this up.
