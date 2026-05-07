using System;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities.OAuth2;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

internal sealed class DiscordOAuthTokenExchangeService(IOptions<DiscordOAuthIngressOptions> options, ILoggerFactory loggerFactory) : IDiscordOAuthTokenExchangeService
{
	private readonly ILoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
	private readonly IOptions<DiscordOAuthIngressOptions> _options = options ?? throw new ArgumentNullException(nameof(options));

	public async Task<DiscordAccessToken> ExchangeAccessTokenAsync(string code, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(code);

		var options = this._options.Value;
		if (!options.IsConfigured)
			throw new InvalidOperationException("Discord OAuth ingress has not been configured.");

		await using var client = new DisCatSharp.DiscordOAuth2Client(
			options.ClientId,
			options.ClientSecret!,
			options.RedirectUri!,
			loggerFactory: this._loggerFactory);

		return await client.ExchangeAccessTokenAsync(code, cancellationToken).ConfigureAwait(false);
	}
}
