using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents a Discord OAuth authorization-code callback request.
/// </summary>
public sealed class DiscordOAuthCallbackRequest
{
	private static readonly IReadOnlyDictionary<string, string?> EmptyQueryParameters =
		new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordOAuthCallbackRequest" /> class.
	/// </summary>
	/// <param name="code">The authorization code received from Discord.</param>
	/// <param name="state">The callback state received from Discord.</param>
	/// <param name="error">The OAuth error returned by Discord, when available.</param>
	/// <param name="errorDescription">The OAuth error description returned by Discord, when available.</param>
	/// <param name="callbackUri">The absolute callback URI, when one is available.</param>
	/// <param name="queryParameters">The callback query parameters.</param>
	public DiscordOAuthCallbackRequest(
		string? code,
		string? state,
		string? error = null,
		string? errorDescription = null,
		Uri? callbackUri = null,
		IReadOnlyDictionary<string, string?>? queryParameters = null
	)
	{
		this.Code = Normalize(code);
		this.State = Normalize(state);
		this.Error = Normalize(error);
		this.ErrorDescription = Normalize(errorDescription);
		this.CallbackUri = callbackUri;
		this.QueryParameters = CreateQueryParameters(queryParameters);
	}

	/// <summary>
	///     Gets the authorization code received from Discord.
	/// </summary>
	public string? Code { get; }

	/// <summary>
	///     Gets the callback state received from Discord.
	/// </summary>
	public string? State { get; }

	/// <summary>
	///     Gets the OAuth error returned by Discord, when available.
	/// </summary>
	public string? Error { get; }

	/// <summary>
	///     Gets the OAuth error description returned by Discord, when available.
	/// </summary>
	public string? ErrorDescription { get; }

	/// <summary>
	///     Gets the absolute callback URI, when available.
	/// </summary>
	public Uri? CallbackUri { get; }

	/// <summary>
	///     Gets the callback query parameters.
	/// </summary>
	public IReadOnlyDictionary<string, string?> QueryParameters { get; }

	/// <summary>
	///     Attempts to resolve a callback query parameter.
	/// </summary>
	/// <param name="name">The query parameter name.</param>
	/// <param name="value">The resolved value.</param>
	/// <returns><see langword="true" /> when the parameter exists.</returns>
	public bool TryGetQueryValue(string name, out string? value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return this.QueryParameters.TryGetValue(name, out value);
	}

	private static IReadOnlyDictionary<string, string?> CreateQueryParameters(IReadOnlyDictionary<string, string?>? queryParameters)
	{
		if (queryParameters is null || queryParameters.Count == 0)
			return EmptyQueryParameters;

		Dictionary<string, string?> normalizedParameters = new(StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in queryParameters)
			normalizedParameters[key] = Normalize(value);

		return new ReadOnlyDictionary<string, string?>(normalizedParameters);
	}

	private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
