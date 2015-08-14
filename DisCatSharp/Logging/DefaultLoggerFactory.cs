using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a default logger factory.
/// </summary>
internal class DefaultLoggerFactory : ILoggerFactory
{
	/// <summary>
	/// Gets the providers.
	/// </summary>
	private readonly List<ILoggerProvider> _providers = [];

	/// <summary>
	/// Gets whether this logger factory is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	/// Adds a provider.
	/// </summary>
	/// <param name="provider">The provider to be added.</param>
	public void AddProvider(ILoggerProvider provider)
		=> this._providers.Add(provider);

	/// <summary>
	/// Creates the logger.
	/// </summary>
	/// <param name="categoryName">The category name.</param>
	public ILogger CreateLogger(string categoryName)
		=> this._isDisposed
			? throw new ObjectDisposedException(nameof(DefaultLoggerFactory), "This logger factory is already disposed.")
			: categoryName != typeof(BaseDiscordClient).FullName && categoryName != typeof(DiscordWebhookClient).FullName && categoryName != typeof(DiscordOAuth2Client).FullName
				? throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseDiscordClient).FullName}, {typeof(DiscordWebhookClient).FullName} or {typeof(DiscordOAuth2Client).FullName}, not {categoryName}.", nameof(categoryName))
				: new CompositeDefaultLogger(this._providers);

	/// <summary>
	/// Disposes the logger.
	/// </summary>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(this._isDisposed, this);

		this._isDisposed = true;

		foreach (var provider in this._providers)
			provider.Dispose();

		this._providers.Clear();
	}
}
