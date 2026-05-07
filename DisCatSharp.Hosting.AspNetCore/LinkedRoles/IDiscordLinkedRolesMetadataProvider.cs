using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.Hosting.AspNetCore.LinkedRoles;

/// <summary>
///     Supplies linked-roles metadata records for synchronization with the Discord application.
/// </summary>
public interface IDiscordLinkedRolesMetadataProvider
{
	/// <summary>
	///     Resolves the desired linked-roles metadata records.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The metadata records that should be registered with Discord.</returns>
	ValueTask<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> GetMetadataAsync(CancellationToken cancellationToken = default);
}
