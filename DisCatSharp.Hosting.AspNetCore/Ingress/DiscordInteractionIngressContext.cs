using System;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Provides the transport-neutral context for a Discord interaction delivered over HTTP ingress.
/// </summary>
public sealed class DiscordInteractionIngressContext
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordInteractionIngressContext" /> class.
	/// </summary>
	/// <param name="request">The validated ingress request.</param>
	/// <param name="interaction">The parsed interaction envelope.</param>
	public DiscordInteractionIngressContext(DiscordIngressRequest request, DiscordInteractionIngressEnvelope interaction)
	{
		this.Request = request ?? throw new ArgumentNullException(nameof(request));
		this.Interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
	}

	/// <summary>
	///     Gets the original ingress request, including the preserved raw request body used for signature verification.
	/// </summary>
	public DiscordIngressRequest Request { get; }

	/// <summary>
	///     Gets the parsed interaction envelope.
	/// </summary>
	public DiscordInteractionIngressEnvelope Interaction { get; }
}
