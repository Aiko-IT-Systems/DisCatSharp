using DisCatSharp.Attributes;
using DisCatSharp.Entities;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents the type of interaction response
/// </summary>
public enum InteractionResponseType
{
	/// <summary>
	///     Acknowledges a Ping.
	/// </summary>
	Pong = 1,

	/// <summary>
	///     Responds to the interaction with a message.
	/// </summary>
	ChannelMessageWithSource = 4,

	/// <summary>
	///     Acknowledges an interaction to edit to a response later. The user sees a "thinking" state.
	/// </summary>
	DeferredChannelMessageWithSource = 5,

	/// <summary>
	///     Acknowledges a component interaction to allow a response later.
	/// </summary>
	DeferredMessageUpdate = 6,

	/// <summary>
	///     Responds to a component interaction by editing the message it's attached to.
	/// </summary>
	UpdateMessage = 7,

	/// <summary>
	///     Responds to an auto-complete request.
	/// </summary>
	AutoCompleteResult = 8,

	/// <summary>
	///     Responds to the interaction with a modal.
	/// </summary>
	Modal = 9,

	/// <summary>
	///     <para>Responds to the interaction with a message indicating that a premium subscription is required.</para>
	///     <para>
	///         <note type="warning">Can only be used if you have an associated application subscription sku.</note>
	///     </para>
	///     <para>
	///         <note type="warning">
	///             Deprecated. Create normal response instead and use
	///             <see cref="DiscordPremiumButtonComponent" /> with target sku id.
	///         </note>
	///     </para>
	/// </summary>
	[DiscordDeprecated("Replaced by DiscordPremiumButtonComponent."), Deprecated]
	PremiumRequired = 10,

	/// <summary>
	///     <para>Responds to the interaction with an iframe.</para>
	///     <para>
	///         <note type="warning">Can only be used if you are whitelisted..</note>
	///     </para>
	/// </summary>
	[DiscordInExperiment("Currently in closed beta."), Experimental("We provide this type but can't provide support.")]
	Iframe = 11,

	/// <summary>
	///     <para>Launch the Activity associated with the app.</para>
	///     <para>
	///         <note type="warning">Can only be used if the app has activities enabled.</note>
	///     </para>
	/// </summary>
	LaunchActivity = 12
}
