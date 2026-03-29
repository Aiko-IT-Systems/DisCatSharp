using System;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.ApplicationCommands;

/// <summary>
///     Provides a shared, singleton empty <see cref="IServiceProvider" /> instance
///     to avoid allocating a new <see cref="ServiceCollection" /> and
///     <see cref="ServiceProvider" /> on every context or configuration creation.
/// </summary>
internal static class EmptyServiceProvider
{
	/// <summary>
	///     A shared empty service provider singleton.
	/// </summary>
	internal static readonly IServiceProvider Instance = new ServiceCollection().BuildServiceProvider(true);
}
