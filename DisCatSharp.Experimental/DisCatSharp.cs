using System.Threading.Tasks;

using DisCatSharp.Attributes;

namespace DisCatSharp.Experimental;

public static class DisCatSharp
{
	[Experimental("This function is being tested and might change at any time.")]
	public static async Task<string> GetUsernameAsync(this DiscordClient client, ulong id)
	{
		var user = await client.ApiClient.GetUserAsync(id);
		return user.UsernameWithDiscriminator;
	}
}
