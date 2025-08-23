using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Adds methods to <see cref="DiscordGuild" />.
/// </summary>
public static class DiscordGuildMethodsHook
{
	/// <summary>
	///     Searches the guild for members through elasticsearch.
	///     If the guild isn't indexed yet, it'll return a 202 accepted response, we're gonna throw at this point.
	///     Access the <see cref="NotIndexedException" />'s <see cref="NotIndexedException.RetryAfter" /> field to know when to
	///     retry this request.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="searchParams">The guild member search params</param>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotIndexedException">Thrown if the elasticsearch endpoint has not finished indexing yet.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public static async Task<DiscordSearchGuildMembersResponse> SearchMembersAsync(this DiscordGuild guild, DiscordGuildMemberSearchParams searchParams)
	{
		DiscordApiClientHook hook = new(guild.Discord.ApiClient);
		return await hook.SearchGuildMembersAsync(guild.Id, searchParams);
	}

	/// <summary>
	///     Searches the guild for messages through elasticsearch.
	///     If the guild isn't indexed yet, it'll return a 202 accepted response, we're gonna throw at this point.
	///     Access the <see cref="NotIndexedException" />'s <see cref="NotIndexedException.RetryAfter" /> field to know when to
	///     retry this request.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="searchParams">The guild messages search params</param>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotIndexedException">Thrown if the elasticsearch endpoint has not finished indexing yet.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public static async Task<DiscordSearchGuildMessagesResponse?> SearchMessagesAsync(this DiscordGuild guild, DiscordGuildMessageSearchParams searchParams)
	{
		DiscordApiClientHook hook = new(guild.Discord.ApiClient);
		return await hook.SearchGuildMessagesAsync(guild.Id, searchParams);
	}
}
