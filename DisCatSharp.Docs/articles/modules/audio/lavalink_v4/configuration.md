---
uid: modules_audio_lavalink_v4_configuration
title: Lavalink V4 Configuration
author: DisCatSharp Team
---

# Setting up DisCatSharp.Lavalink

## Configuring Your Client

To begin using DisCatSharp's Lavalink client, you will need to add the `DisCatSharp.Lavalink` nuget package. Once installed, simply add these namespaces at the top of your bot file:
```cs
using DisCatSharp.Net;
using DisCatSharp.Lavalink;
```

After that, we will need to create a configuration for our extension to use. This is where the special values from the server configuration are used.
```cs
var endpoint = new ConnectionEndpoint
{
    Hostname = "127.0.0.1", // From your server configuration.
    Port = 2333 // From your server configuration
};

var lavalinkConfig = new LavalinkConfiguration
{
    Password = "youshallnotpass", // From your server configuration.
    RestEndpoint = endpoint,
    SocketEndpoint = endpoint
};
```
Finally, initialize the extension.
```cs
var lavalink = Discord.UseLavalink();
```

## Connecting with Lavalink

We are now ready to connect to the server. Call the Lavalink extension's connect method and pass the configuration. Make sure to call this **after** your Discord client connects. This can be called either directly after your client's connect method or in your client's ready event.

```cs
LavalinkNode = await Lavalink.ConnectAsync(lavalinkConfig);
```

Your main bot file should now look like this:

```cs
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DisCatSharp;
using DisCatSharp.Net;
using DisCatSharp.Lavalink;
namespace FirstLavalinkBot;

public class Program
{
	public static DiscordClient Discord { get; internal set; }

	public static void Main(string[] args = null)
	{
		MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
	}

	public static async Task MainAsync(string[] args)
	{
		Discord = new DiscordClient(new DiscordConfiguration
		{
			Token = "<token_here>",
			TokenType = TokenType.Bot,
			MinimumLogLevel = LogLevel.Debug
		});

		var endpoint = new ConnectionEndpoint
		{
			Hostname = "127.0.0.1", // From your server configuration.
			Port = 2333 // From your server configuration
		};

		var lavalinkConfig = new LavalinkConfiguration
		{
			Password = "youshallnotpass", // From your server configuration.
			RestEndpoint = endpoint,
			SocketEndpoint = endpoint
		};

		var lavalink = Discord.UseLavalink();

		await Discord.ConnectAsync();
		await lavalink.ConnectAsync(lavalinkConfig); // Make sure this is after Discord.ConnectAsync().

		await Task.Delay(-1);
	}
}
```
We are now ready to start the bot. If everything is configured properly, you should see a Lavalink connection appear in your DisCatSharp console:

```yml
[2020-10-10 17:56:07 -04:00] [403 /LavalinkSessionConnected] [Debug] Connection to Lavalink established UwU
```

And a client connection appear in your Lavalink console:

```yml
2023-06-25 09:05:28.757  INFO 4436 --- [  XNIO-1 task-1] io.undertow.servlet                      : Initializing Spring DispatcherServlet 'dispatcherServlet'
2023-06-25 09:05:28.758  INFO 4436 --- [  XNIO-1 task-1] o.s.web.servlet.DispatcherServlet        : Initializing Servlet 'dispatcherServlet'
2023-06-25 09:05:28.760  INFO 4436 --- [  XNIO-1 task-1] o.s.web.servlet.DispatcherServlet        : Completed initialization in 2 ms
2023-06-25 09:05:28.861  INFO 4436 --- [  XNIO-1 task-1] lavalink.server.io.RequestLoggingFilter  : GET /version?trace=false, client=127.0.0.1
2023-06-25 09:05:28.876  INFO 4436 --- [  XNIO-1 task-1] l.server.io.HandshakeInterceptorImpl     : Incoming connection from /127.0.0.1:54649
2023-06-25 09:05:28.918  INFO 4436 --- [  XNIO-1 task-1] lavalink.server.io.RequestLoggingFilter  : GET /v4/websocket, client=127.0.0.1
2023-06-25 09:05:29.048  INFO 4436 --- [  XNIO-1 task-1] lavalink.server.io.SocketServer          : Connection successfully established from DisCatSharp.Lavalink/10.4.1
```

We are now ready to set up some music commands!
