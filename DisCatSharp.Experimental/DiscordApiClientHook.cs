using DisCatSharp.Net;

namespace DisCatSharp.Experimental;

internal class DiscordApiClientHook
{
	internal DiscordApiClient ApiClient { get; set; }

	public DiscordApiClientHook(DiscordApiClient apiClient)
	{
		this.ApiClient = apiClient;
	}
}
