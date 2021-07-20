// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DisCatSharp.Entities;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.SlashCommands
{
    /// <summary>
    /// Represents a context for an interaction
    /// </summary>
    public sealed class InteractionContext
    {
        /// <summary>
        /// Gets the interaction that was created
        /// </summary>
        public DiscordInteraction Interaction { get; internal set; }

        /// <summary>
        /// Gets the client for this interaction
        /// </summary>
        public DiscordClient Client { get; internal set; }

        /// <summary>
        /// Gets the guild this interaction was executed in
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets the channel this interaction was executed in
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the user which executed this interaction
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the member which executed this interaction, or null if the command is in a DM
        /// </summary>
        public DiscordMember Member
            => this.User is DiscordMember member ? member : null;

        /// <summary>
        /// Gets the slash command module this interaction was created in
        /// </summary>
        public SlashCommandsExtension SlashCommandsExtension { get; internal set; }

        /// <summary>
        /// Gets the token for this interaction
        /// </summary>
        public string Token { get; internal set; }

        /// <summary>
        /// Gets the id for this interaction
        /// </summary>
        public ulong InteractionId { get; internal set; }

        /// <summary>
        /// Gets the name of the command
        /// </summary>
        public string CommandName { get; internal set; }

        /// <summary>
        /// <para>Gets the service provider.</para>
        /// <para>This allows passing data around without resorting to static members.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IServiceProvider Services { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

        /// <summary>
        /// Creates a response to this interaction
        /// <para>You must create a response within 3 seconds of this interaction being executed; if the command has the potential to take more than 3 seconds, create a <see cref="InteractionResponseType.DeferredChannelMessageWithSource"/> at the start, and edit the response later</para>
        /// </summary>
        /// <param name="type">The type of the response</param>
        /// <param name="builder">The data to be sent, if any</param>
        /// <returns></returns>
        public async Task CreateResponseAsync(InteractionResponseType type, DiscordInteractionResponseBuilder builder = null) => await this.Interaction.CreateResponseAsync(type, builder);

        /// <summary>
        /// Edits the interaction response
        /// </summary>
        /// <param name="builder">The data to edit the response with</param>
        /// <returns></returns>
        public async Task<DiscordMessage> EditResponseAsync(DiscordWebhookBuilder builder) => await this.Interaction.EditOriginalResponseAsync(builder);

        /// <summary>
        /// Deletes the interaction response
        /// </summary>
        /// <returns></returns>
        public async Task DeleteResponseAsync() => await this.Interaction.DeleteOriginalResponseAsync();

        /// <summary>
        /// Creates a follow up message to the interaction
        /// </summary>
        /// <param name="builder">The message to be sent, in the form of a webhook</param>
        /// <returns>The created message</returns>
        public async Task<DiscordMessage> FollowUpAsync(DiscordFollowupMessageBuilder builder) => await this.Interaction.CreateFollowupMessageAsync(builder);

        /// <summary>
        /// Edits a followup message
        /// </summary>
        /// <param name="followupMessageId">The id of the followup message to edit</param>
        /// <param name="builder">The webhook builder</param>
        /// <returns></returns>
        public async Task<DiscordMessage> EditFollowupAsync(ulong followupMessageId, DiscordWebhookBuilder builder) => await this.Interaction.EditFollowupMessageAsync(followupMessageId, builder);

        /// <summary>
        /// Deletes a followup message
        /// </summary>
        /// <param name="followupMessageId">The id of the followup message to delete</param>
        /// <returns></returns>
        public async Task DeleteFollowupAsync(ulong followupMessageId) => await this.Interaction.DeleteFollowupMessageAsync(followupMessageId);
    }
}
