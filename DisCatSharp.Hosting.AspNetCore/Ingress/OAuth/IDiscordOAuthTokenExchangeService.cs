using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities.OAuth2;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

/// <summary>
///     Exchanges Discord OAuth authorization codes for access tokens.
/// </summary>
public interface IDiscordOAuthTokenExchangeService
{
	/// <summary>
	///     Exchanges the specified authorization code for a Discord access token.
	/// </summary>
	/// <param name="code">The authorization code to exchange.</param>
	/// <param name="cancellationToken">A token used to cancel the operation.</param>
	/// <returns>The exchanged Discord access token.</returns>
	Task<DiscordAccessToken> ExchangeAccessTokenAsync(string code, CancellationToken cancellationToken = default);
}
