using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Enums.Core;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity.Entities;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;

using NuGet.Packaging;

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
	/// <param name="timeoutOverride">A custom timeout. (Default: 15 minutes)</param>
	/// <returns>A read-only dictionary with the customid of the components as the key.</returns>
	/// <exception cref="ArgumentException">Is thrown when no modals are defined.</exception>
	/// <exception cref="InvalidOperationException">Is thrown when interactivity is not enabled for the client/shard.</exception>
	public static async Task<PaginatedModalResponse> CreatePaginatedModalResponseAsync(this DiscordInteraction interaction, IReadOnlyList<ModalPage> modals, TimeSpan? timeoutOverride = null)
	{
		if (modals is null || modals.Count is 0)
			throw new ArgumentException("You have to set at least one page");
		var client = (DiscordClient)interaction.Discord;
		var interactivity = client.GetInteractivity() ?? throw new InvalidOperationException($"Interactivity is not enabled for this {(client.IsShard ? "shard" : "client")}.");

		timeoutOverride ??= TimeSpan.FromMinutes(15);

		Dictionary<string, string> caughtResponses = [];
		Dictionary<string, string[]> caughtSelectResponses = [];

		var previousInteraction = interaction;
		DiscordInteraction ephemeralInteraction = null;
		DiscordMessage ephemeralMsg = null;

		var firstModalPage = modals[0];
		var firstModal = firstModalPage.Modal.WithCustomId(Guid.NewGuid().ToString());
		await interaction.CreateInteractionModalResponseAsync(firstModal).ConfigureAwait(false);
		var modalOpen = await interactivity.WaitForModalAsync(firstModal.CustomId, timeoutOverride);

		if (modalOpen.TimedOut)
		{
			return new()
			{
				TimedOut = true
			};
		}

		await modalOpen.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(firstModalPage.OpenMessage).AddComponents(firstModalPage.OpenButton).AsEphemeral()).ConfigureAwait(false);
		ephemeralInteraction = modalOpen.Result.Interaction;
		ephemeralMsg = await ephemeralInteraction.GetOriginalResponseAsync();

		var (firstResponses, firstSelectResponses) = GetResponsesFromInteraction(modalOpen.Result);
		caughtResponses.AddRange(firstResponses);
		caughtSelectResponses.AddRange(firstSelectResponses);

		foreach (var b in modals.Skip(1))
		{
			var waitingOnButton = await interactivity.WaitForButtonAsync(ephemeralMsg, interaction.User, timeoutOverride).ConfigureAwait(false);

			if (waitingOnButton.TimedOut)
			{
				if (ephemeralInteraction is not null && ephemeralMsg is not null)
					await ephemeralInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(firstModalPage.OpenMessage + "\nTimed out.").InjectComponentsFromMessage(ephemeralMsg).DisableAllComponents());
				return new()
				{
					TimedOut = true
				};
			}

			previousInteraction = waitingOnButton.Result.Interaction;
			var modal = b.Modal.WithCustomId(Guid.NewGuid().ToString());

			await previousInteraction.CreateInteractionModalResponseAsync(modal).ConfigureAwait(false);

			var modalResult = await interactivity.WaitForModalAsync(modal.CustomId, timeoutOverride).ConfigureAwait(false);

			if (modalResult.TimedOut)
			{
				if (ephemeralInteraction is not null && ephemeralMsg is not null)
					await ephemeralInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(b.OpenMessage + "\nTimed out.").InjectComponentsFromMessage(ephemeralMsg).DisableAllComponents());
				return new()
				{
					TimedOut = true
				};
			}

			await modalResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

			if (ephemeralInteraction is not null && ephemeralMsg is not null)
				ephemeralMsg = await ephemeralInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(b.OpenMessage).AddComponents(b.OpenButton));
			
			var (responses, selectResponses) = GetResponsesFromInteraction(modalResult.Result);
			caughtResponses.AddRange(responses);
			caughtSelectResponses.AddRange(selectResponses);

			previousInteraction = modalResult.Result.Interaction;
		}

		if (ephemeralInteraction is not null)
		{
			await ephemeralInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Done!").ClearComponents(), ModifyMode.Replace);
		}

		return new()
		{
			TimedOut = false,
			Responses = caughtResponses,
			SelectResponses = caughtSelectResponses,
			Interaction = ephemeralInteraction
		};
	}

	private static (Dictionary<string, string> responses, Dictionary<string, string[]> selectResponses) GetResponsesFromInteraction(ComponentInteractionCreateEventArgs args)
	{
		var caughtResponses = new Dictionary<string, string>();
		var caughtSelectResponses = new Dictionary<string, string[]>();

		foreach (var submissions in args.Interaction.Data.ModalComponents.OfType<DiscordLabelComponent>().Where(x => x.Component is DiscordTextInputComponent).Select(s => s.Component as DiscordTextInputComponent))
			caughtResponses.Add(submissions.CustomId, submissions.Value);

		foreach (var submissions in args.Interaction.Data.ModalComponents.OfType<DiscordLabelComponent>().Where(x => x.Component is DiscordStringSelectComponent).Select(s => s.Component as DiscordStringSelectComponent))
			caughtSelectResponses.Add(submissions.CustomId, submissions.SelectedValues);

		return (caughtResponses, caughtSelectResponses);
	}
}
