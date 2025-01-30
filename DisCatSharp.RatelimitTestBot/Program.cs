using System.Net;

using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.RatelimitTestBot;

internal class Program
{
	private static void Main(string[] args)
	{
		Console.WriteLine("Hello, World!");
		var token = Environment.GetEnvironmentVariable("RatelimitTestBotToken") ?? throw new NullReferenceException();
		try
		{
			RatelimitTestBot bot = new(token, true);
			bot.SetupClients();
			bot.SetupCommands();
			bot.RunAsync().Wait();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			_ = Console.ReadKey();
		}
	}
}

internal sealed class RatelimitTestBot
{
	internal RatelimitTestBot(string token, bool enableProxy = false)
	{
		this.Configuration = new()
		{
			Token = token,
			TokenType = TokenType.Bot,
			AutoReconnect = true,
			MessageCacheSize = 1024,
			MinimumLogLevel = LogLevel.Debug,
			ShardCount = 1,
			ShardId = 0,
			Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContent,
			MobileStatus = true,
			ApiChannel = ApiChannel.Canary,
			GatewayCompressionLevel = GatewayCompressionLevel.None,
			AutoRefreshChannelCache = false,
			ReconnectIndefinitely = true,
			Proxy = enableProxy ? new WebProxy("127.0.0.1", 8004) : null,
			Override = "eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiRGlzY29yZCBDbGllbnQiLCJyZWxlYXNlX2NoYW5uZWwiOiJjYW5hcnkiLCJjbGllbnRfdmVyc2lvbiI6IjEuMC4yNDkiLCJvc192ZXJzaW9uIjoiMTAuMC4yMjYzNSIsIm9zX2FyY2giOiJ4NjQiLCJhcHBfYXJjaCI6Ing2NCIsInN5c3RlbV9sb2NhbGUiOiJlbi1VUyIsImJyb3dzZXJfdXNlcl9hZ2VudCI6Ik1vemlsbGEvNS4wIChXaW5kb3dzIE5UIDEwLjA7IFdpbjY0OyB4NjQpIEFwcGxlV2ViS2l0LzUzNy4zNiAoS0hUTUwsIGxpa2UgR2Vja28pIGRpc2NvcmQvMS4wLjI0OSBDaHJvbWUvMTA4LjAuNTM1OS4yMTUgRWxlY3Ryb24vMjIuMy4yNiBTYWZhcmkvNTM3LjM2IiwiYnJvd3Nlcl92ZXJzaW9uIjoiMjIuMy4yNiIsImNsaWVudF9idWlsZF9udW1iZXIiOjI1NjU2NSwibmF0aXZlX2J1aWxkX251bWJlciI6NDIxMjQsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGwsImRlc2lnbl9pZCI6MH0=",
			ReportMissingFields = false,
			EnableSentry = false,
			EnableLibraryDeveloperMode = true,
			SentryDebug = false,
			DisableExceptionFilter = true,
			HttpTimeout = TimeSpan.FromMinutes(4),
			DisableUpdateCheck = true,
			HasActivitiesEnabled = false
		};

		this.CommandsNextConfiguration = new()
		{
			EnableDms = false,
			StringPrefixes = ["rl!"],
			EnableDefaultHelp = true
		};
	}

	internal DiscordConfiguration Configuration { get; }

	internal CancellationTokenSource Cts { get; } = new();

	internal DiscordClient Client { get; set; }

	internal CommandsNextConfiguration CommandsNextConfiguration { get; }

	internal void SetupClients()
	{
		this.Client = new(this.Configuration);
		this.Client.UseCommandsNext(this.CommandsNextConfiguration);
	}

	internal void SetupCommands()
	{
		var cnext = this.Client.GetCommandsNext();
		cnext.RegisterCommands<RatelimitTestBotModule>();
	}

	internal async Task RunAsync()
	{
		await this.Client.ConnectAsync(new("Testing ratelimits", ActivityType.Custom), UserStatus.DoNotDisturb);

		while (!this.Cts.IsCancellationRequested)
			await Task.Delay(1000);

		await this.Client.DisconnectAsync();
	}
}

internal class RatelimitTestBotModule : BaseCommandModule
{
	[Command("ping")]
	public async Task PingAsync(CommandContext ctx)
		=> await ctx.RespondAsync("Pong!");

	[Command("test")]
	public async Task TestAsync(CommandContext ctx)
	{
		Parallel.For(0, 10, new()
		{
			MaxDegreeOfParallelism = 7
		}, async void (i) => await ctx.RespondAsync($"Meow {i}"));

		await ctx.RespondAsync("Done!");
	}
}
