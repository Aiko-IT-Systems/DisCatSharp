---
uid: getting_started_web_app
title: Bot as Hosted Service
author: DisCatSharp Team
---

# Prerequisites

Install the following packages:

-   DisCatSharp
-   DisCatSharp.Hosting

> [!IMPORTANT]
> Please be aware that this approach relies on Dependency Injection. You can either use one of Microsoft's default project templates for .Net Core Web App, or get a head start by using the
> `DisCatSharp.Hosting.ProjectTemplates` pack which contains a Bot Template to jumpstart your development. If you do the latter, majority of this is done for you.

# Bot.cs

For the sake of example, create a new class called `Bot` which inherits from `DiscordHostedService`. You're welcome to replace `Bot` with whatever you want.

> [!NOTE]
> If you want to host a variety of bots it is important to provide a custom name into the `base` constructor. This indicates the `Key` within `IConfiguration` that will be used for
> configuring your bot.

## Default

`DisCatSharp` is the default key used when configuring the bot.

```cs
public class Bot : DiscordHostedService
{
    public Bot(IConfiguration config,
            ILogger<Bot> logger,
            IServiceProvider provider,
            IHostApplicationLifetime appLifetime) : base(config, logger, provider, appLifetime)
    {
    }
}
```

## Custom

For example’s sake the custom bot name is "Bot", so replace it with whatever you want.

```cs
public class Bot : DiscordHostedService
{
    public Bot(IConfiguration config,
            ILogger<Bot> logger,
            IServiceProvider provider,
            IHostApplicationLifetime appLifetime) : base(config, logger, provider, appLifetime, "Bot")
    {
    }
}
```

# Startup.cs

## DisCatSharp.Hosting.DependencyInjection

By using the `DisCatSharp.Hosting.DependencyInjection` module, this 1 line is enough to get
your basic bot running...

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDiscordHostedService<Bot>();
}
```

## Manual Registration

If you prefer another DI approach / the manual route -- the following two
lines are all you need! For example sake, this bot doesn't have anything fancy going on.
You're welcome to create your own interface which inherits from `IDiscordHostedService`.

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IDiscordHostedService, Bot>();
    services.AddHostedService(provider => provider.GetRequiredService<IDiscordHostedService>());
}
```

Singleton - we only want 1 instance of Bot to ever run during runtime. <br>
Then we take the registered singleton to run as a `HostedService`.

# How to reference

Within a DI environment, whether it's via constructor or an `IServiceProvider`

## If explicitly registered as `Bot`

You either put `Bot` as part of your constructor. Or from a provider you do

```cs
Bot bot = provider.GetRequiredService<Bot>();
```

## Interface + Bot

This approach means you are mapping the Interface to your `Bot`. However, you might notice that

```cs
Bot bot = provider.GetRequiredService<Bot>();
```

or via constructor - you will get an exception indicating that `Bot` has not been registered. Well... it's true. It's looking for a key within the collection that matches the type you asked for.
When you use the Interface/Implementation combination it behaves **almost** like a dictionary -- `Bot` is not a valid key in this scenario.

So to retrieve your `Bot` reference you have to use the interface.

```cs
IBot bot = provider.GetRequiredService<IBot>();
```

If you go down this path of mapping interface to implementation you shouldn't be casting your interface to Bot, or whatever. You'd be better off just using the explicitly registered type.
The reasoning behind this approach is to allow you to swap out the implementation type in **ONE** place, and **NOT** have to update any other code.

For instance, logging... there are SO many ways to do logging. You might be familiar with [ILogger](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger). So long as something implements this interface it doesn't matter. It could be Serilog,
or a custom logger you created, or another package from the internet. If later in a project you are dissatisfied with your custom-built logger (which inherits from `ILogger`) you could
easily swap it out with `Serilog` in one place. This makes swapping between packages extremely easy - a simple 1 to 2 line change compared to a project-wide impact.

# How to Configure

You must provide a token in order for the bot to work.

Add the following to `appsettings.json`

```json
{
	"DisCatSharp": {
		"Discord": {
			"Token": "YOUR TOKEN HERE"
		}
	}
}
```

## Dependency Injection

The ServiceProvider where you register the `DiscordHostedService` is automatically copied to the DiscordClient.
Therefore, if you want to use any services in your [event handlers](xref:topics_events), you can simply register them before the `DiscordHostedService`:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<YourService>();

    services.AddDiscordHostedService<Bot>();
}
```

In this case, `YourService` will be available in all your Discord event handlers.

## Initialization errors handling

During the initialization of bots, various exceptions can be thrown. For example: invalid token.
By default, the exception will be displayed in the console, after which the application will shutdown.
You can handle exceptions by overriding method `OnInitializationError` in your `DiscordHostedService`.

```cs
protected override void OnInitializationError(Exception ex)
{
    // your code here

    base.OnInitializationError(ex);
}
```

## Extensions

If you wish to add additional modules/extensions you can do so one of two ways.

1. Use the full namespace name
2. Namespace without the `DisCatSharp` prefix - because we assume the extension starts with DisCatSharp.

To add the extensions `Interactivity` and `CommandsNext`:

```json
{
	"DisCatSharp": {
		"Using": ["DisCatSharp.Interactivity", "CommandsNext"],

		"Discord": {
			"Token": "YOUR TOKEN HERE"
		},

		"Interactivity": {
			"PollBehaviour": "KeepEmojis"
		},

		"CommandsNext": {
			"StringPrefixes": ["!"]
		}
	}
}
```

> [!NOTE]
> To configure an extension, you simply add a section for it under `DisCatSharp` in `appsettings.json`. You only have
> to include values you **WISH TO OVERRIDE**. There is no need to include all config options if you only need to change 1 value.
> For more info on which values are available checkout the following classes:
>
> -   `ApplicationCommandsConfiguration`
> -   `CommandsNextConfiguration`
> -   `DiscordConfiguration`
> -   `InteractivityConfiguration`
> -   `LavalinkConfiguration`
> -   `VoiceNextConfiguration`
>
> For more information, you can also see the [example](https://github.com/Aiko-IT-Systems/DisCatSharp.Examples/tree/main/Hosting).

## Multiple bots

In case you need to use multiple bots in one application, you need to use different names for them in the `appsettings.json`:

```json
{
	"BotOne": {
		"Discord": {
			"Token": "YOUR TOKEN HERE"
		}
	},
	"BotTwo": {
		"Discord": {
			"Token": "YOUR TOKEN HERE"
		}
	}
}
```

Next, you need to create a new `DiscordHostedService` for each of the bots.

```cs
public class BotOne : DiscordHostedService
{
    public BotOne(IConfiguration config, ILogger<DiscordHostedService> logger, IServiceProvider provider,
        IHostApplicationLifetime appLifetime) : base(config, logger, provider, appLifetime, "BotOne")
    {
    }
}

public class BotTwo : DiscordHostedService
{
    public BotTwo(IConfiguration config, ILogger<DiscordHostedService> logger, IServiceProvider provider,
        IHostApplicationLifetime appLifetime) : base(config, logger, provider, appLifetime, "BotTwo")
    {
    }
}
```

Note: you must also specify the name of the bot in the constructor, which must match the one specified in the config.

Now, you can simply register them in the usual way:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDiscordHostedService<BotOne>();
    services.AddDiscordHostedService<BotTwo>();
}
```

---

## Values

It's worth mentioning the required formats for certain value types

### Enum

-   Single Flag/Value
    -   "`Value`"
-   Multiple Flags
    -   "`Flag1|Flag2|Flag3`"

#### Example

```json
{
	"DisCatSharp": {
		"Discord": {
			"Intents": "GuildMembers|GuildsBans"
		}
	}
}
```

### TimeSpan

Hours:Minutes:Seconds "`HH:mm:ss`"

#### Example

HttpTimeout of 5 minutes

```json
{
	"DisCatSharp": {
		"Discord": {
			"HttpTimeout": "00:05:00"
		}
	}
}
```
