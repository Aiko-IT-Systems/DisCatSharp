using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Entities;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;

namespace DisCatSharp.Interactivity.Extensions;

/// <summary>
///     The interaction extensions.
/// </summary>
public static class InteractionExtensions
{
	/// <summary>
	///     Sends a paginated message in response to an interaction.
	///     <para>
	///         <b>Pass the interaction directly. Interactivity will ACK it.</b>
	///     </para>
	/// </summary>
	/// <param name="interaction">The interaction to create a response to.</param>
	/// <param name="deferred">Whether the interaction was deferred.</param>
	/// <param name="ephemeral">Whether the response should be ephemeral.</param>
	/// <param name="user">The user to listen for button presses from.</param>
	/// <param name="pages">The pages to paginate.</param>
	/// <param name="buttons">Optional: custom buttons</param>
	/// <param name="behaviour">Pagination behaviour.</param>
	/// <param name="deletion">Deletion behaviour</param>
	/// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
	public static Task SendPaginatedResponseAsync(this DiscordInteraction interaction, bool deferred, bool ephemeral, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons = null, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
		=> (interaction.Discord as DiscordClient)?.GetInteractivity()?.SendPaginatedResponseAsync(interaction, deferred, ephemeral, user, pages, buttons, behaviour, deletion, token);

	/// <summary>
	///     Sends multiple modals to the user with a prompt to open the next one.
	///     <para>
	///         <b>
	///             After the last modal, this method automatically responds with the thinking state. Use
	///             <see cref="DiscordInteraction.EditOriginalResponseAsync(DiscordWebhookBuilder, DisCatSharp.Enums.Core.ModifyMode)" /> to interact with the
	///             response.
	///         </b>
	///     </para>
	/// </summary>
	/// <param name="interaction">The interaction to create a response to.</param>
	/// <param name="modals">The modal pages.</param>
	/// <param name="timeOutOverride">A custom timeout. (Default: 15 minutes)</param>
	/// <returns>A read-only dictionary with the customid of the components as the key.</returns>
	/// <exception cref="ArgumentException">Is thrown when no modals are defined.</exception>
	/// <exception cref="InvalidOperationException">Is thrown when interactivity is not enabled for the client/shard.</exception>
	public static async Task<PaginatedModalResponse> CreatePaginatedModalResponseAsync(this DiscordInteraction interaction, IReadOnlyList<ModalPage> modals, TimeSpan? timeOutOverride = null)
	{
		if (modals is null || modals.Count == 0)
			throw new ArgumentException("You have to set at least one page");

		var client = (DiscordClient)interaction.Discord;
		var interactivity = client.GetInteractivity() ?? throw new InvalidOperationException($"Interactivity is not enabled for this {(client.IsShard ? "shard" : "client")}.");

		timeOutOverride ??= TimeSpan.FromMinutes(15);

		Dictionary<string, string> caughtResponses = [];
		Dictionary<string, string[]> caughtSelectResponses = [];

		var previousInteraction = interaction;

		foreach (var b in modals)
		{
			var modal = b.Modal.WithCustomId(Guid.NewGuid().ToString());

			if (previousInteraction.Type is InteractionType.Ping or InteractionType.ModalSubmit)
			{
				var originalResponse = (await previousInteraction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, b.OpenMessage.AddComponents(b.OpenButton)).ConfigureAwait(false)).Message;
				var modalOpen = await interactivity.WaitForButtonAsync(originalResponse, new List<DiscordButtonComponent>
				{
					b.OpenButton
				}, timeOutOverride).ConfigureAwait(false);

				if (modalOpen.TimedOut)
				{
					_ = previousInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(b.OpenMessage.Content).AddComponents(b.OpenButton.Disable()));
					return new()
					{
						TimedOut = true
					};
				}

				await modalOpen.Result.Interaction.CreateInteractionModalResponseAsync(modal).ConfigureAwait(false);
			}
			else
				await previousInteraction.CreateInteractionModalResponseAsync(modal).ConfigureAwait(false);

			_ = previousInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(b.OpenMessage.Content).AddComponents(b.OpenButton.Disable()));

			var modalResult = await interactivity.WaitForModalAsync(modal.CustomId, timeOutOverride).ConfigureAwait(false);

			if (modalResult.TimedOut)
				return new()
				{
					TimedOut = true
				};

			foreach (var submissions in modalResult.Result.Interaction.Data.ModalComponents.OfType<DiscordLabelComponent>().Where(x => x.Component is DiscordTextInputComponent).Select(s => s.Component as DiscordTextInputComponent))
				caughtResponses.Add(submissions.CustomId, submissions.Value);

			foreach (var submissions in modalResult.Result.Interaction.Data.ModalComponents.OfType<DiscordLabelComponent>().Where(x => x.Component is DiscordStringSelectComponent).Select(s => s.Component as DiscordStringSelectComponent))
				caughtSelectResponses.Add(submissions.CustomId, submissions.SelectedValues);

			previousInteraction = modalResult.Result.Interaction;
		}

		await previousInteraction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral()).ConfigureAwait(false);

		return new()
		{
			TimedOut = false,
			Responses = caughtResponses,
			SelectResponses = caughtSelectResponses,
			Interaction = previousInteraction
		};
	}
}
