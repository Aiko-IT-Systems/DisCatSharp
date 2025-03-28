namespace DisCatSharp.Analyzer.Tests.Tests;

internal static class Dcs0201
{
	internal static async Task TestAsync()
	{
		var invite = await Program.Client.GetInviteByCodeAsync("code");
		var profile = invite.Profile; // This should trigger DCS0201
	}
}
