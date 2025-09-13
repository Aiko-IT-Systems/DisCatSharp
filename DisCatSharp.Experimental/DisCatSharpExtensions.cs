using System.Threading.Tasks;

using DisCatSharp.Attributes;

namespace DisCatSharp.Experimental;

/// <summary>
///     Represents experimental extension methods for DisCatSharp.
/// </summary>
public static class DisCatSharpExtensions
{
	/// <summary>
	/// This is a test method to test the functionality of DisCatSharp.Experimental and the DisCatSharp.Analyzer.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="id">The id of the user to get the username of.</param>
	/// <returns>The username of the user with the given id.</returns>
	[Experimental("This function is being tested and might change at any time."), RequiresFeature(Features.MonetizedApplication)]
	public static async Task<string> GetUsernameAsync(this DiscordClient client, ulong id)
	{
		var user = await client.ApiClient.GetUserAsync(id);
		return user.UsernameWithDiscriminator;
	}
}
