using System;

using FluentAssertions;

using Xunit;

namespace DisCatSharp.SafetyTests;

public class HttpTests
{
	[Fact(DisplayName = "Ensure that no authorization header is set by DiscordClient in the public rest client")]
	public void BuiltInRestClientEnsureNoAuthorization()
	{
		DiscordClient client = new(new()
		{
			Token = "super_secret_bot_token"
		});
		var action = () => client.RestClient.DefaultRequestHeaders.GetValues("Authorization").ToString();
		action.Should()
			.Throw<InvalidOperationException>()
			.WithMessage("The given header was not found.");

		client.RestClient.DefaultRequestHeaders.Add("Authorization", "not_so_secret_manual_token");
		var action2 = () => client.RestClient.DefaultRequestHeaders.GetValues("Authorization").ToString();
		action2.Should()
			.NotThrow<InvalidOperationException>();
	}
}
