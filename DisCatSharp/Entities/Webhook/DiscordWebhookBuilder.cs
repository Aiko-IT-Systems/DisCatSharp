// This file is part of the DisCatSharp project, a fork of DSharpPlus.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Constructs ready-to-send webhook requests.
    /// </summary>
    public sealed class DiscordWebhookBuilder
    {
        /// <summary>
        /// Username to use for this webhook request.
        /// </summary>
        public Optional<string> Username { get; set; }

        /// <summary>
        /// Avatar url to use for this webhook request.
        /// </summary>
        public Optional<string> AvatarUrl { get; set; }

        /// <summary>
        /// Whether this webhook request is text-to-speech.
        /// </summary>
        public bool IsTts { get; set; }

        /// <summary>
        /// Message to send on this webhook request.
        /// </summary>
        public string Content
        {
            get => this._content;
            set
            {
                if (value != null && value.Length > 2000)
                    throw new ArgumentException("Content length cannot exceed 2000 characters.", nameof(value));
                this._content = value;
            }
        }
        private string _content;

        /// <summary>
        /// Whether to keep previous attachments.
        /// </summary>
        internal bool? _keepAttachments = null;

        /// <summary>
        /// Embeds to send on this webhook request.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds => this._embeds;
        private readonly List<DiscordEmbed> _embeds = new();

        /// <summary>
        /// Files to send on this webhook request.
        /// </summary>
        public IReadOnlyList<DiscordMessageFile> Files => this._files;
        private readonly List<DiscordMessageFile> _files = new();

        /// <summary>
        /// Mentions to send on this webhook request.
        /// </summary>
        public IReadOnlyList<IMention> Mentions => this._mentions;
        private readonly List<IMention> _mentions = new();

        /// <summary>
        /// Gets the components.
        /// </summary>
        public IReadOnlyList<DiscordActionRowComponent> Components => this._components;
        private readonly List<DiscordActionRowComponent> _components = new();


        /// <summary>
        /// Attachments to keep on this webhook request.
        /// </summary>
        public IEnumerable<DiscordAttachment> Attachments => this._attachments;
        internal readonly List<DiscordAttachment> _attachments = new();

        /// <summary>
        /// Constructs a new empty webhook request builder.
        /// </summary>
        public DiscordWebhookBuilder() { } // I still see no point in initializing collections with empty collections. //


        /// <summary>
        /// Adds a row of components to the builder, up to 5 components per row, and up to 5 rows per message.
        /// </summary>
        /// <param name="Components">The components to add to the builder.</param>
        /// <returns>The current builder to be chained.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">No components were passed.</exception>
        public DiscordWebhookBuilder AddComponents(params DiscordComponent[] Components)
            => this.AddComponents((IEnumerable<DiscordComponent>)Components);


        /// <summary>
        /// Appends several rows of components to the builder
        /// </summary>
        /// <param name="Components">The rows of components to add, holding up to five each.</param>
        /// <returns></returns>
        public DiscordWebhookBuilder AddComponents(IEnumerable<DiscordActionRowComponent> Components)
        {
            var ara = Components.ToArray();

            if (ara.Length + this._components.Count > 5)
                throw new ArgumentException("ActionRow count exceeds maximum of five.");

            foreach (var ar in ara)
                this._components.Add(ar);

            return this;
        }

        /// <summary>
        /// Adds a row of components to the builder, up to 5 components per row, and up to 5 rows per message.
        /// </summary>
        /// <param name="Components">The components to add to the builder.</param>
        /// <returns>The current builder to be chained.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">No components were passed.</exception>
        public DiscordWebhookBuilder AddComponents(IEnumerable<DiscordComponent> Components)
        {
            var cmpArr = Components.ToArray();
            var count = cmpArr.Length;

            if (!cmpArr.Any())
                throw new ArgumentOutOfRangeException(nameof(Components), "You must provide at least one component");

            if (count > 5)
                throw new ArgumentException("Cannot add more than 5 components per action row!");

            var comp = new DiscordActionRowComponent(cmpArr);
            this._components.Add(comp);

            return this;
        }

        /// <summary>
        /// Sets the username for this webhook builder.
        /// </summary>
        /// <param name="Username">Username of the webhook</param>
        public DiscordWebhookBuilder WithUsername(string Username)
        {
            this.Username = Username;
            return this;
        }

        /// <summary>
        /// Sets the avatar of this webhook builder from its url.
        /// </summary>
        /// <param name="AvatarUrl">Avatar url of the webhook</param>
        public DiscordWebhookBuilder WithAvatarUrl(string AvatarUrl)
        {
            this.AvatarUrl = AvatarUrl;
            return this;
        }

        /// <summary>
        /// Indicates if the webhook must use text-to-speech.
        /// </summary>
        /// <param name="Tts">Text-to-speech</param>
        public DiscordWebhookBuilder WithTts(bool Tts)
        {
            this.IsTts = Tts;
            return this;
        }

        /// <summary>
        /// Sets the message to send at the execution of the webhook.
        /// </summary>
        /// <param name="Content">Message to send.</param>
        public DiscordWebhookBuilder WithContent(string Content)
        {
            this.Content = Content;
            return this;
        }

        /// <summary>
        /// Adds an embed to send at the execution of the webhook.
        /// </summary>
        /// <param name="Embed">Embed to add.</param>
        public DiscordWebhookBuilder AddEmbed(DiscordEmbed Embed)
        {
            if (Embed != null)
                this._embeds.Add(Embed);

            return this;
        }

        /// <summary>
        /// Adds the given embeds to send at the execution of the webhook.
        /// </summary>
        /// <param name="Embeds">Embeds to add.</param>
        public DiscordWebhookBuilder AddEmbeds(IEnumerable<DiscordEmbed> Embeds)
        {
            this._embeds.AddRange(Embeds);
            return this;
        }

        /// <summary>
        /// Adds a file to send at the execution of the webhook.
        /// </summary>
        /// <param name="Filename">Name of the file.</param>
        /// <param name="Data">File data.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <param name="Description">Description of the file.</param>
        public DiscordWebhookBuilder AddFile(string Filename, Stream Data, bool ResetStreamPosition = false, string Description = null)
        {
            if (this.Files.Count() > 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            if (this._files.Any(X => X.FileName == Filename))
                throw new ArgumentException("A File with that filename already exists");

            if (ResetStreamPosition)
                this._files.Add(new DiscordMessageFile(Filename, Data, Data.Position, Description: Description));
            else
                this._files.Add(new DiscordMessageFile(Filename, Data, null, Description: Description));

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="Stream">The Stream to the file.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <param name="Description">Description of the file.</param>
        /// <returns></returns>
        public DiscordWebhookBuilder AddFile(FileStream Stream, bool ResetStreamPosition = false, string Description = null)
        {
            if (this.Files.Count() > 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            if (this._files.Any(X => X.FileName == Stream.Name))
                throw new ArgumentException("A File with that filename already exists");

            if (ResetStreamPosition)
                this._files.Add(new DiscordMessageFile(Stream.Name, Stream, Stream.Position, Description: Description));
            else
                this._files.Add(new DiscordMessageFile(Stream.Name, Stream, null, Description: Description));

            return this;
        }

        /// <summary>
        /// Adds the given files to send at the execution of the webhook.
        /// </summary>
        /// <param name="Files">Dictionary of file name and file data.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        public DiscordWebhookBuilder AddFiles(Dictionary<string, Stream> Files, bool ResetStreamPosition = false)
        {
            if (this.Files.Count() + Files.Count() > 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            foreach (var file in Files)
            {
                if (this._files.Any(X => X.FileName == file.Key))
                    throw new ArgumentException("A File with that filename already exists");

                if (ResetStreamPosition)
                    this._files.Add(new DiscordMessageFile(file.Key, file.Value, file.Value.Position));
                else
                    this._files.Add(new DiscordMessageFile(file.Key, file.Value, null));
            }


            return this;
        }

        /// <summary>
        /// Modifies the given attachments on edit.
        /// </summary>
        /// <param name="Attachments">Attachments to edit.</param>
        /// <returns></returns>
        public DiscordWebhookBuilder ModifyAttachments(IEnumerable<DiscordAttachment> Attachments)
        {
            this._attachments.AddRange(Attachments);
            return this;
        }

        /// <summary>
        /// Whether to keep the message attachments, if new ones are added.
        /// </summary>
        /// <returns></returns>
        public DiscordWebhookBuilder KeepAttachments(bool Keep)
        {
            this._keepAttachments = Keep;
            return this;
        }

        /// <summary>
        /// Adds the mention to the mentions to parse, etc. at the execution of the webhook.
        /// </summary>
        /// <param name="Mention">Mention to add.</param>
        public DiscordWebhookBuilder AddMention(IMention Mention)
        {
            this._mentions.Add(Mention);
            return this;
        }

        /// <summary>
        /// Adds the mentions to the mentions to parse, etc. at the execution of the webhook.
        /// </summary>
        /// <param name="Mentions">Mentions to add.</param>
        public DiscordWebhookBuilder AddMentions(IEnumerable<IMention> Mentions)
        {
            this._mentions.AddRange(Mentions);
            return this;
        }

        /// <summary>
        /// Executes a webhook.
        /// </summary>
        /// <param name="Webhook">The webhook that should be executed.</param>
        /// <returns>The message sent</returns>
        public async Task<DiscordMessage> SendAsync(DiscordWebhook Webhook) => await Webhook.Execute(this).ConfigureAwait(false);

        /// <summary>
        /// Executes a webhook.
        /// </summary>
        /// <param name="Webhook">The webhook that should be executed.</param>
        /// <param name="ThreadId">Target thread id.</param>
        /// <returns>The message sent</returns>
        public async Task<DiscordMessage> SendAsync(DiscordWebhook Webhook, ulong ThreadId) => await Webhook.Execute(this, ThreadId.ToString()).ConfigureAwait(false);

        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="Webhook">The webhook that should be executed.</param>
        /// <param name="Message">The message to modify.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook Webhook, DiscordMessage Message) => await this.ModifyAsync(Webhook, Message.Id).ConfigureAwait(false);

        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="Webhook">The webhook that should be executed.</param>
        /// <param name="MessageId">The id of the message to modify.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook Webhook, ulong MessageId) => await Webhook.EditMessageAsync(MessageId, this).ConfigureAwait(false);

        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="Webhook">The webhook that should be executed.</param>
        /// <param name="Message">The message to modify.</param>
        /// <param name="Thread">Target thread.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook Webhook, DiscordMessage Message, DiscordThreadChannel Thread) => await this.ModifyAsync(Webhook, Message.Id, Thread.Id).ConfigureAwait(false);

        /// <summary>
        /// Sends the modified webhook message.
        /// </summary>
        /// <param name="Webhook">The webhook that should be executed.</param>
        /// <param name="MessageId">The id of the message to modify.</param>
        /// <param name="ThreadId">Target thread id.</param>
        /// <returns>The modified message</returns>
        public async Task<DiscordMessage> ModifyAsync(DiscordWebhook Webhook, ulong MessageId, ulong ThreadId) => await Webhook.EditMessageAsync(MessageId, this, ThreadId.ToString()).ConfigureAwait(false);

        /// <summary>
        /// Clears all message components on this builder.
        /// </summary>
        public void ClearComponents()
            => this._components.Clear();

        /// <summary>
        /// Allows for clearing the Webhook Builder so that it can be used again to send a new message.
        /// </summary>
        public void Clear()
        {
            this.Content = "";
            this._embeds.Clear();
            this.IsTts = false;
            this._mentions.Clear();
            this._files.Clear();
            this._attachments.Clear();
            this._components.Clear();
            this._keepAttachments = false;
        }

        /// <summary>
        /// Does the validation before we send a the Create/Modify request.
        /// </summary>
        /// <param name="IsModify">Tells the method to perform the Modify Validation or Create Validation.</param>
        /// <param name="IsFollowup">Tells the method to perform the follow up message validation.</param>
        /// <param name="IsInteractionResponse">Tells the method to perform the interaction response validation.</param>
        internal void Validate(bool IsModify = false, bool IsFollowup = false, bool IsInteractionResponse = false)
        {
            if (IsModify)
            {
                if (this.Username.HasValue)
                    throw new ArgumentException("You cannot change the username of a message.");

                if (this.AvatarUrl.HasValue)
                    throw new ArgumentException("You cannot change the avatar of a message.");
            }
            else if (IsFollowup)
            {
                if (this.Username.HasValue)
                    throw new ArgumentException("You cannot change the username of a follow up message.");

                if (this.AvatarUrl.HasValue)
                    throw new ArgumentException("You cannot change the avatar of a follow up message.");
            }
            else if (IsInteractionResponse)
            {
                if (this.Username.HasValue)
                    throw new ArgumentException("You cannot change the username of an interaction response.");

                if (this.AvatarUrl.HasValue)
                    throw new ArgumentException("You cannot change the avatar of an interaction response.");
            }
            else
            {
                if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && !this.Embeds.Any())
                    throw new ArgumentException("You must specify content, an embed, or at least one file.");
            }
        }
    }
}
