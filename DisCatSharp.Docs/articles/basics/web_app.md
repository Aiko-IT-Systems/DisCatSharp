---
uid: basics_web_app
title: Bot as Hosted Service
---

# Prerequisites
Install the following packages:
 - DisCatSharp
 - DisCatSharp.Hosting

# Bot.cs
Create a new class called `Bot` which inherits from `DiscordHostedService`.

```cs
public class Bot : DiscordHostedService
{
    public Bot(IConfiguration config, ILogger<DiscordHostedService> logger, IServiceProvider provider, IHostApplicationLifetime appLifetime) : base(config, logger, provider, appLifetime)
    {
    }
}
```

# Startup.cs
By using the `DisCatSharp.Hosting.DependencyInjection` module, this 1 line is enough to get
your basic bot running...

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDiscordHostedService<Bot>();
}
```

If you prefer another DI approach / the manual route -- the following two
lines are all you need!

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IDiscordHostedService, Bot>();
    services.AddHostedService(provider => provider.GetRequiredService<IDiscordHostedService>());
}
```

Singleton - we only want 1 instance of Bot to ever run during runtime. <br/>
Then we take the registered singleton to run as a `HostedService`.

# How to reference
Within a DI environment, when you want to reference your `Bot` all you have to do is add `IDiscordHostedService`
as a parameter in the constructor.

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
        "Using": [
            "DisCatSharp.Interactivity",
            "CommandsNext"
        ],

        "Discord": {
            "Token": "YOUR TOKEN HERE"
        },

        "Interactivity": {
            "PollBehaviour": "KeepEmojis"
        },

        "CommandsNext": {
            "StringPrefixes": [ "!" ]
        }
    }
}
```

Note: to configure an extension, you simply add a section for it under `DisCatSharp` in `appsettings.json`. You only have
to include values you **WISH TO OVERRIDE**. There is no need to include all config options if you only need to change 1 value.

For more info on which values are available checkout the following classes:
 - `ApplicationCommandsConfiguration`
 - `CommandsNextConfiguration`
 - `DiscordConfiguration`
 - `InteractivityConfiguration`
 - `LavalinkConfiguration`
 - `VoiceNextConfiguration`

For more information, you can also see the [example](https://github.com/Aiko-IT-Systems/DisCatSharp.Examples/tree/main/Hosting).

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

____
## Values
It's worth mentioning the required formats for certain value types

### Enum
- Single Flag/Value
  - "`Value`"
- Multiple Flags
  - "`Flag1|Flag2|Flag3`"

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
