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
    /// Constructs a Message to be sent.
    /// </summary>
    public sealed class DiscordMessageBuilder
    {
        /// <summary>
        /// Gets or Sets the Message to be sent.
        /// </summary>
        public string Content
        {
            get => this._content;
            set
            {
                if (value != null && value.Length > 2000)
                    throw new ArgumentException("Content cannot exceed 2000 characters.", nameof(value));
                this._content = value;
            }
        }
        private string _content;

        /// <summary>
        /// Gets or sets the embed for the builder. This will always set the builder to have one embed.
        /// </summary>
        public DiscordEmbed Embed
        {
            get => this._embeds.Count > 0 ? this._embeds[0] : null;
            set
            {
                this._embeds.Clear();
                this._embeds.Add(value);
            }
        }

        /// <summary>
        /// Gets the Sticker to be send.
        /// </summary>
        public DiscordSticker Sticker { get; set; }

        /// <summary>
        /// Gets the Embeds to be sent.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds => this._embeds;
        private readonly List<DiscordEmbed> _embeds = new();

        /// <summary>
        /// Gets or Sets if the message should be TTS.
        /// </summary>
        public bool IsTts { get; set; } = false;

        /// <summary>
        /// Whether to keep previous attachments.
        /// </summary>
        internal bool? _keepAttachments = null;

        /// <summary>
        /// Gets the Allowed Mentions for the message to be sent.
        /// </summary>
        public List<IMention> Mentions { get; private set; } = null;

        /// <summary>
        /// Gets the Files to be sent in the Message.
        /// </summary>
        public IReadOnlyCollection<DiscordMessageFile> Files => this._files;
        internal readonly List<DiscordMessageFile> _files = new();

        /// <summary>
        /// Gets the components that will be attached to the message.
        /// </summary>
        public IReadOnlyList<DiscordActionRowComponent> Components => this._components;
        internal readonly List<DiscordActionRowComponent> _components = new(5);

        /// <summary>
        /// Gets the Attachments to be sent in the Message.
        /// </summary>
        public IReadOnlyList<DiscordAttachment> Attachments => this._attachments;
        internal readonly List<DiscordAttachment> _attachments = new();

        /// <summary>
        /// Gets the Reply Message ID.
        /// </summary>
        public ulong? ReplyId { get; private set; } = null;

        /// <summary>
        /// Gets if the Reply should mention the user.
        /// </summary>
        public bool MentionOnReply { get; private set; } = false;

        /// <summary>
        /// Gets if the embeds should be suppressed.
        /// </summary>
        public bool Suppressed { get; private set; } = false;

        /// <summary>
        /// Gets if the Reply will error if the Reply Message Id does not reference a valid message.
        /// <para>If set to false, invalid replies are send as a regular message.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool FailOnInvalidReply { get; set; }

        /// <summary>
        /// Sets the Content of the Message.
        /// </summary>
        /// <param name="Content">The content to be set.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithContent(string Content)
        {
            this.Content = Content;
            return this;
        }

        /// <summary>
        /// Adds a sticker to the message. Sticker must be from current guild.
        /// </summary>
        /// <param name="Sticker">The sticker to add.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithSticker(DiscordSticker Sticker)
        {
            this.Sticker = Sticker;
            return this;
        }

        /// <summary>
        /// Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
        /// </summary>
        /// <param name="Components">The components to add to the message.</param>
        /// <returns>The current builder to be chained.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">No components were passed.</exception>
        public DiscordMessageBuilder AddComponents(params DiscordComponent[] Components)
            => this.AddComponents((IEnumerable<DiscordComponent>)Components);


        /// <summary>
        /// Appends several rows of components to the message
        /// </summary>
        /// <param name="Components">The rows of components to add, holding up to five each.</param>
        /// <returns></returns>
        public DiscordMessageBuilder AddComponents(IEnumerable<DiscordActionRowComponent> Components)
        {
            var ara = Components.ToArray();

            if (ara.Length + this._components.Count > 5)
                throw new ArgumentException("ActionRow count exceeds maximum of five.");

            foreach (var ar in ara)
                this._components.Add(ar);

            return this;
        }

        /// <summary>
        /// Adds a row of components to a message, up to 5 components per row, and up to 5 rows per message.
        /// </summary>
        /// <param name="Components">The components to add to the message.</param>
        /// <returns>The current builder to be chained.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">No components were passed.</exception>
        public DiscordMessageBuilder AddComponents(IEnumerable<DiscordComponent> Components)
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
        /// Sets if the message should be TTS.
        /// </summary>
        /// <param name="IsTts">If TTS should be set.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder HasTts(bool IsTts)
        {
            this.IsTts = IsTts;
            return this;
        }

        /// <summary>
        /// Sets the embed for the current builder.
        /// </summary>
        /// <param name="Embed">The embed that should be set.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithEmbed(DiscordEmbed Embed)
        {
            if (Embed == null)
                return this;

            this.Embed = Embed;
            return this;
        }

        /// <summary>
        /// Appends an embed to the current builder.
        /// </summary>
        /// <param name="Embed">The embed that should be appended.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder AddEmbed(DiscordEmbed Embed)
        {
            if (Embed == null)
                return this; //Providing null embeds will produce a 400 response from Discord.//
            this._embeds.Add(Embed);
            return this;
        }

        /// <summary>
        /// Appends several embeds to the current builder.
        /// </summary>
        /// <param name="Embeds">The embeds that should be appended.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> Embeds)
        {
            this._embeds.AddRange(Embeds);
            return this;
        }

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="AllowedMention">The allowed Mention that should be sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithAllowedMention(IMention AllowedMention)
        {
            if (this.Mentions != null)
                this.Mentions.Add(AllowedMention);
            else
                this.Mentions = new List<IMention> { AllowedMention };

            return this;
        }

        /// <summary>
        /// Sets if the message has allowed mentions.
        /// </summary>
        /// <param name="AllowedMentions">The allowed Mentions that should be sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithAllowedMentions(IEnumerable<IMention> AllowedMentions)
        {
            if (this.Mentions != null)
                this.Mentions.AddRange(AllowedMentions);
            else
                this.Mentions = AllowedMentions.ToList();

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="FileName">The fileName that the file should be sent as.</param>
        /// <param name="Stream">The Stream to the file.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <param name="Description">Description of the file.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithFile(string FileName, Stream Stream, bool ResetStreamPosition = false, string Description = null)
        {
            if (this.Files.Count > 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            if (this._files.Any(X => X.FileName == FileName))
                throw new ArgumentException("A File with that filename already exists");

            if (ResetStreamPosition)
                this._files.Add(new DiscordMessageFile(FileName, Stream, Stream.Position, Description: Description));
            else
                this._files.Add(new DiscordMessageFile(FileName, Stream, null, Description: Description));

            return this;
        }

        /// <summary>
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="Stream">The Stream to the file.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <param name="Description">Description of the file.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithFile(FileStream Stream, bool ResetStreamPosition = false, string Description = null)
        {
            if (this.Files.Count > 10)
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
        /// Sets if the message has files to be sent.
        /// </summary>
        /// <param name="Files">The Files that should be sent.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithFiles(Dictionary<string, Stream> Files, bool ResetStreamPosition = false)
        {
            if (this.Files.Count + Files.Count > 10)
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
        public DiscordMessageBuilder ModifyAttachments(IEnumerable<DiscordAttachment> Attachments)
        {
            this._attachments.AddRange(Attachments);
            return this;
        }

        /// <summary>
        /// Whether to keep the message attachments, if new ones are added.
        /// </summary>
        /// <returns></returns>
        public DiscordMessageBuilder KeepAttachments(bool Keep)
        {
            this._keepAttachments = Keep;
            return this;
        }

        /// <summary>
        /// Sets if the message is a reply
        /// </summary>
        /// <param name="MessageId">The ID of the message to reply to.</param>
        /// <param name="Mention">If we should mention the user in the reply.</param>
        /// <param name="FailOnInvalidReply">Whether sending a reply that references an invalid message should be </param>
        /// <returns>The current builder to be chained.</returns>
        public DiscordMessageBuilder WithReply(ulong MessageId, bool Mention = false, bool FailOnInvalidReply = false)
        {
            this.ReplyId = MessageId;
            this.MentionOnReply = Mention;
            this.FailOnInvalidReply = FailOnInvalidReply;

            if (Mention)
            {
                this.Mentions ??= new List<IMention>();
                this.Mentions.Add(new RepliedUserMention());
            }

            return this;
        }


        /// <summary>
        /// Sends the Message to a specific channel
        /// </summary>
        /// <param name="Channel">The channel the message should be sent to.</param>
        /// <returns>The current builder to be chained.</returns>
        public Task<DiscordMessage> Send(DiscordChannel Channel) => Channel.SendMessage(this);

        /// <summary>
        /// Sends the modified message.
        /// <para>Note: Message replies cannot be modified. To clear the reply, simply pass <see langword="null"/> to <see cref="WithReply"/>.</para>
        /// </summary>
        /// <param name="Msg">The original Message to modify.</param>
        /// <returns>The current builder to be chained.</returns>
        public Task<DiscordMessage> Modify(DiscordMessage Msg) => Msg.ModifyAsync(this);

        /// <summary>
        /// Clears all message components on this builder.
        /// </summary>
        public void ClearComponents()
            => this._components.Clear();

        /// <summary>
        /// Allows for clearing the Message Builder so that it can be used again to send a new message.
        /// </summary>
        public void Clear()
        {
            this.Content = "";
            this._embeds.Clear();
            this.IsTts = false;
            this.Mentions = null;
            this._files.Clear();
            this.ReplyId = null;
            this.MentionOnReply = false;
            this._components.Clear();
            this.Suppressed = false;
            this.Sticker = null;
            this._attachments.Clear();
            this._keepAttachments = false;
        }

        /// <summary>
        /// Does the validation before we send a the Create/Modify request.
        /// </summary>
        /// <param name="IsModify">Tells the method to perform the Modify Validation or Create Validation.</param>
        internal void Validate(bool IsModify = false)
        {
            if (this._embeds.Count > 10)
                throw new ArgumentException("A message can only have up to 10 embeds.");

            if (!IsModify)
            {
                if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && (!this.Embeds?.Any() ?? true) && this.Sticker is null)
                    throw new ArgumentException("You must specify content, an embed, a sticker or at least one file.");

                if (this.Components.Count > 5)
                    throw new InvalidOperationException("You can only have 5 action rows per message.");

                if (this.Components.Any(C => C.Components.Count > 5))
                    throw new InvalidOperationException("Action rows can only have 5 components");
            }
        }
    }
}
