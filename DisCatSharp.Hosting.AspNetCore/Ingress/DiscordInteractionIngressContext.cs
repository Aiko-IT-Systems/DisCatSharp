using System;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Provides the transport-neutral context for a Discord interaction delivered over HTTP ingress.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DiscordInteractionIngressContext" /> class.
/// </remarks>
/// <param name="request">The validated ingress request.</param>
/// <param name="interaction">The parsed interaction envelope.</param>
public sealed class DiscordInteractionIngressContext(DiscordIngressRequest request, DiscordInteractionIngressEnvelope interaction)
{

	/// <summary>
	///     Gets the original ingress request, including the preserved raw request body used for signature verification.
	/// </summary>
	public DiscordIngressRequest Request { get; } = request ?? throw new ArgumentNullException(nameof(request));

	/// <summary>
	///     Gets the parsed interaction envelope.
	/// </summary>
	public DiscordInteractionIngressEnvelope Interaction { get; } = interaction ?? throw new ArgumentNullException(nameof(interaction));
}
