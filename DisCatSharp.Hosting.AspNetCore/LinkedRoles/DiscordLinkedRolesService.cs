using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DisCatSharp.Hosting.AspNetCore.LinkedRoles;

/// <summary>
///     Provides helper methods for linked-roles verification URLs, metadata synchronization, and OAuth role-connection updates.
/// </summary>
/// <remarks>
///     This helper keeps the ASP.NET Core ingress route configuration, Discord application metadata, and OAuth callback results aligned for linked-roles flows.
/// </remarks>
/// <param name="aspNetCoreOptions">The ingress route options.</param>
/// <param name="linkedRolesOptions">The linked-roles helper options.</param>
/// <param name="serviceProvider">The service provider.</param>
public sealed class DiscordLinkedRolesService(
	IOptions<DiscordAspNetCoreIngressOptions> aspNetCoreOptions,
	IOptions<DiscordLinkedRolesOptions> linkedRolesOptions,
	IServiceProvider serviceProvider
	)
{
	private readonly IOptions<DiscordAspNetCoreIngressOptions> _aspNetCoreOptions = aspNetCoreOptions ?? throw new ArgumentNullException(nameof(aspNetCoreOptions));
	private readonly IOptions<DiscordLinkedRolesOptions> _linkedRolesOptions = linkedRolesOptions ?? throw new ArgumentNullException(nameof(linkedRolesOptions));
	private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

	/// <summary>
	///     Computes the public linked-roles verification URL for the supplied application base URL.
	/// </summary>
	/// <param name="publicBaseUrl">The externally visible application base URL.</param>
	/// <returns>The public linked-roles verification URL.</returns>
	public Uri GetVerificationUrl(Uri publicBaseUrl)
		=> DiscordIngressPublicUrls.Create(publicBaseUrl, this._aspNetCoreOptions.Value, this._linkedRolesOptions.Value).RoleConnectionsVerificationUrl
		   ?? throw new InvalidOperationException("Linked-roles support did not produce a verification URL.");

	/// <summary>
	///     Updates the current Discord application's linked-roles verification URL to the computed public URL.
	/// </summary>
	/// <param name="client">The Discord client used to update the application.</param>
	/// <param name="publicBaseUrl">The externally visible application base URL.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The updated Discord application.</returns>
	public Task<DiscordApplication> UpdateVerificationUrlAsync(BaseDiscordClient client, Uri publicBaseUrl, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(client);

		var verificationUrl = this.GetVerificationUrl(publicBaseUrl);
		return client.UpdateCurrentApplicationInfoAsync(
			description: Optional.None,
			interactionsEndpointUrl: Optional.None,
			roleConnectionsVerificationUrl: verificationUrl.AbsoluteUri,
			customInstallUrl: Optional.None,
			tags: Optional.None,
			icon: Optional.None,
			coverImage: Optional.None,
			flags: Optional.None,
			integrationTypesConfig: Optional.None,
			cancellationToken: cancellationToken);
	}

	/// <summary>
	///     Gets a value indicating whether a callback result contains the required <c>role_connections.write</c> scope.
	/// </summary>
	/// <param name="callbackResult">The callback result to inspect.</param>
	/// <returns><see langword="true" /> when the granted scope contains <c>role_connections.write</c>.</returns>
	public bool HasRoleConnectionsWriteScope(DiscordOAuthCallbackResult callbackResult)
	{
		ArgumentNullException.ThrowIfNull(callbackResult);

		return callbackResult.AccessToken is not null
		       && callbackResult.AccessToken.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(static scope => scope == "role_connections.write");
	}

	/// <summary>
	///     Resolves the linked-roles metadata records from the registered provider.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The resolved metadata records.</returns>
	public async Task<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> GetMetadataAsync(CancellationToken cancellationToken = default)
	{
		var metadataProvider = this._serviceProvider.GetService<IDiscordLinkedRolesMetadataProvider>()
			?? throw new InvalidOperationException("No linked-roles metadata provider is registered. Use AddDiscordLinkedRolesMetadataProvider<TProvider>() to supply one.");

		var metadata = await metadataProvider.GetMetadataAsync(cancellationToken).ConfigureAwait(false);
		ValidateMetadata(metadata);
		return metadata;
	}

	/// <summary>
	///     Synchronizes the registered linked-roles metadata with Discord when the local schema differs from the remote application state.
	/// </summary>
	/// <param name="client">The Discord client used to manage application metadata.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The active metadata records after synchronization.</returns>
	public async Task<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> SynchronizeMetadataAsync(DiscordClient client, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(client);

		var desiredMetadata = await this.GetMetadataAsync(cancellationToken).ConfigureAwait(false);
		var currentMetadata = await client.GetRoleConnectionMetadata(cancellationToken).ConfigureAwait(false);
		return MetadataEquals(currentMetadata, desiredMetadata)
			? currentMetadata
			: await client.UpdateRoleConnectionMetadata(desiredMetadata, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	///     Publishes a linked-roles connection update using the access token captured by a successful OAuth callback.
	/// </summary>
	/// <param name="oauthClient">The OAuth2 client used to publish the role connection.</param>
	/// <param name="callbackResult">The completed OAuth callback result.</param>
	/// <param name="platformName">The external platform name to show in Discord.</param>
	/// <param name="platformUsername">The external platform username to show in Discord.</param>
	/// <param name="metadata">The role-connection metadata to publish.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The updated application role connection.</returns>
	public async Task<DiscordApplicationRoleConnection> PublishRoleConnectionAsync(
		DiscordOAuth2Client oauthClient,
		DiscordOAuthCallbackResult callbackResult,
		string platformName,
		string platformUsername,
		ApplicationRoleConnectionMetadata metadata,
		CancellationToken cancellationToken = default
	)
	{
		ArgumentNullException.ThrowIfNull(oauthClient);
		ArgumentNullException.ThrowIfNull(callbackResult);
		ArgumentNullException.ThrowIfNull(metadata);
		ArgumentException.ThrowIfNullOrWhiteSpace(platformName);
		ArgumentException.ThrowIfNullOrWhiteSpace(platformUsername);

		return !callbackResult.IsSuccess || callbackResult.AccessToken is null
			? throw new InvalidOperationException("The linked-roles connection can only be published from a successful OAuth callback result.")
			: !this.HasRoleConnectionsWriteScope(callbackResult)
			? throw new InvalidOperationException("The OAuth callback result does not include the required role_connections.write scope.")
			: await oauthClient.UpdateCurrentUserApplicationRoleConnectionAsync(
			callbackResult.AccessToken,
			platformName,
			platformUsername,
			metadata,
			cancellationToken).ConfigureAwait(false);
	}

	private static bool MetadataEquals(IReadOnlyList<DiscordApplicationRoleConnectionMetadata> current, IReadOnlyList<DiscordApplicationRoleConnectionMetadata> desired)
	{
		if (current.Count != desired.Count)
			return false;

		for (var index = 0; index < current.Count; index++)
		{
			var currentItem = current[index];
			var desiredItem = desired[index];
			if (currentItem.Type != desiredItem.Type
			    || !string.Equals(currentItem.Key, desiredItem.Key, StringComparison.Ordinal)
			    || !string.Equals(currentItem.Name, desiredItem.Name, StringComparison.Ordinal)
			    || !string.Equals(currentItem.Description, desiredItem.Description, StringComparison.Ordinal)
			    || !LocalizationEquals(currentItem.NameLocalizations, desiredItem.NameLocalizations)
			    || !LocalizationEquals(currentItem.DescriptionLocalizations, desiredItem.DescriptionLocalizations))
				return false;
		}

		return true;
	}

	private static bool LocalizationEquals(DiscordApplicationCommandLocalization? left, DiscordApplicationCommandLocalization? right)
	{
		var leftValues = left?.GetKeyValuePairs() ?? [];
		var rightValues = right?.GetKeyValuePairs() ?? [];
		return leftValues.Count == rightValues.Count
		       && leftValues.OrderBy(static pair => pair.Key, StringComparer.Ordinal)
		           .SequenceEqual(rightValues.OrderBy(static pair => pair.Key, StringComparer.Ordinal));
	}

	private static void ValidateMetadata(IReadOnlyList<DiscordApplicationRoleConnectionMetadata> metadata)
	{
		ArgumentNullException.ThrowIfNull(metadata);

		if (metadata.Count > 5)
			throw new InvalidOperationException("Discord linked-roles metadata supports at most 5 records.");

		HashSet<string> seenKeys = new(StringComparer.Ordinal);
		foreach (var entry in metadata)
		{
			ArgumentNullException.ThrowIfNull(entry);
			if (!seenKeys.Add(entry.Key))
				throw new InvalidOperationException($"Duplicate linked-roles metadata key '{entry.Key}' is not allowed.");
		}
	}
}
