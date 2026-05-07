using System;

namespace DisCatSharp.Hosting.AspNetCore.Routing;

/// <summary>
///     Describes a mapped DisCatSharp ingress endpoint.
/// </summary>
/// <remarks>
///     Instances of this type are attached as endpoint metadata so applications can inspect the logical
///     DisCatSharp ingress surface without relying on route templates or display names.
/// </remarks>
public sealed class DiscordIngressEndpointMetadata
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressEndpointMetadata" /> class.
	/// </summary>
	/// <param name="module">The logical ingress module that owns the endpoint.</param>
	/// <param name="endpointName">The stable endpoint name.</param>
	/// <param name="relativePath">The relative route path inside the ingress surface.</param>
	public DiscordIngressEndpointMetadata(string module, string endpointName, string relativePath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(module);
		ArgumentException.ThrowIfNullOrWhiteSpace(endpointName);
		ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

		this.Module = module;
		this.EndpointName = endpointName;
		this.RelativePath = relativePath;
	}

	/// <summary>
	///     Gets the logical ingress module that owns the endpoint.
	/// </summary>
	public string Module { get; }

	/// <summary>
	///     Gets the stable endpoint name.
	/// </summary>
	public string EndpointName { get; }

	/// <summary>
	///     Gets the route path relative to the ingress root.
	/// </summary>
	public string RelativePath { get; }
}
