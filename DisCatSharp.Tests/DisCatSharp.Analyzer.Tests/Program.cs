using DisCatSharp.Analyzer.Tests.Tests;

namespace DisCatSharp.Analyzer.Tests;

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
internal class Program
{
	internal static DiscordClient Client { get; set; }

	private static void Main(string[] args)
	{
		Client = new(new()
		{
			Token = string.Empty
		}); // DCS0201 should insert the property "Override" here
		_ = Task.Run(Dcs0201.TestAsync);
		Client = new DiscordClient(new DiscordConfiguration()
		{
			Token = string.Empty
		}); // DCS0201 should insert the property "Override" here
	}
}
