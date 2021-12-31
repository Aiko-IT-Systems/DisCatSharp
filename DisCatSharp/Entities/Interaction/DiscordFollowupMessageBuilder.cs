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

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Constructs a followup message to an interaction.
    /// </summary>
    public sealed class DiscordFollowupMessageBuilder
    {
        /// <summary>
        /// Whether this followup message is text-to-speech.
        /// </summary>
        public bool IsTts { get; set; }

        /// <summary>
        /// Whether this followup message should be ephemeral.
        /// </summary>
        public bool IsEphemeral { get; set; }

        /// <summary>
        /// Indicates this message is emphemeral.
        /// </summary>
        internal int? Flags
            => this.IsEphemeral ? 64 : null;

        /// <summary>
        /// Message to send on followup message.
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
        /// Embeds to send on followup message.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds => this._embeds;
        private readonly List<DiscordEmbed> _embeds = new();

        /// <summary>
        /// Files to send on this followup message.
        /// </summary>
        public IReadOnlyList<DiscordMessageFile> Files => this._files;
        private readonly List<DiscordMessageFile> _files = new();

        /// <summary>
        /// Components to send on this followup message.
        /// </summary>
        public IReadOnlyList<DiscordActionRowComponent> Components => this._components;
        private readonly List<DiscordActionRowComponent> _components = new();

        /// <summary>
        /// Mentions to send on this followup message.
        /// </summary>
        public IReadOnlyList<IMention> Mentions => this._mentions;
        private readonly List<IMention> _mentions = new();


        /// <summary>
        /// Appends a collection of components to the message.
        /// </summary>
        /// <param name="Components">The collection of components to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="Components"/> contained more than 5 components.</exception>
        public DiscordFollowupMessageBuilder AddComponents(params DiscordComponent[] Components)
            => this.AddComponents((IEnumerable<DiscordComponent>)Components);

        /// <summary>
        /// Appends several rows of components to the message
        /// </summary>
        /// <param name="Components">The rows of components to add, holding up to five each.</param>
        /// <returns></returns>
        public DiscordFollowupMessageBuilder AddComponents(IEnumerable<DiscordActionRowComponent> Components)
        {
            var ara = Components.ToArray();

            if (ara.Length + this._components.Count > 5)
                throw new ArgumentException("ActionRow count exceeds maximum of five.");

            foreach (var ar in ara)
                this._components.Add(ar);

            return this;
        }

        /// <summary>
        /// Appends a collection of components to the message.
        /// </summary>
        /// <param name="Components">The collection of components to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="Components"/> contained more than 5 components.</exception>
        public DiscordFollowupMessageBuilder AddComponents(IEnumerable<DiscordComponent> Components)
        {
            var compArr = Components.ToArray();
            var count = compArr.Length;

            if (count > 5)
                throw new ArgumentException("Cannot add more than 5 components per action row!");

            var arc = new DiscordActionRowComponent(compArr);
            this._components.Add(arc);
            return this;
        }
        /// <summary>
        /// Indicates if the followup message must use text-to-speech.
        /// </summary>
        /// <param name="Tts">Text-to-speech</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder WithTts(bool Tts)
        {
            this.IsTts = Tts;
            return this;
        }

        /// <summary>
        /// Sets the message to send with the followup message..
        /// </summary>
        /// <param name="Content">Message to send.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder WithContent(string Content)
        {
            this.Content = Content;
            return this;
        }

        /// <summary>
        /// Adds an embed to the followup message.
        /// </summary>
        /// <param name="Embed">Embed to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AddEmbed(DiscordEmbed Embed)
        {
            this._embeds.Add(Embed);
            return this;
        }

        /// <summary>
        /// Adds the given embeds to the followup message.
        /// </summary>
        /// <param name="Embeds">Embeds to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AddEmbeds(IEnumerable<DiscordEmbed> Embeds)
        {
            this._embeds.AddRange(Embeds);
            return this;
        }

        /// <summary>
        /// Adds a file to the followup message.
        /// </summary>
        /// <param name="Filename">Name of the file.</param>
        /// <param name="Data">File data.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <param name="Description">Description of the file.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AddFile(string Filename, Stream Data, bool ResetStreamPosition = false, string Description = null)
        {
            if (this.Files.Count >= 10)
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
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AddFile(FileStream Stream, bool ResetStreamPosition = false, string Description = null)
        {
            if (this.Files.Count >= 10)
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
        /// Adds the given files to the followup message.
        /// </summary>
        /// <param name="Files">Dictionary of file name and file data.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AddFiles(Dictionary<string, Stream> Files, bool ResetStreamPosition = false)
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
        /// Adds the mention to the mentions to parse, etc. with the followup message.
        /// </summary>
        /// <param name="Mention">Mention to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AddMention(IMention Mention)
        {
            this._mentions.Add(Mention);
            return this;
        }

        /// <summary>
        /// Adds the mentions to the mentions to parse, etc. with the followup message.
        /// </summary>
        /// <param name="Mentions">Mentions to add.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AddMentions(IEnumerable<IMention> Mentions)
        {
            this._mentions.AddRange(Mentions);
            return this;
        }

        /// <summary>
        /// Sets the followup message to be ephemeral.
        /// </summary>
        /// <param name="Ephemeral">Ephemeral.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordFollowupMessageBuilder AsEphemeral(bool Ephemeral)
        {
            this.IsEphemeral = Ephemeral;
            return this;
        }

        /// <summary>
        /// Clears all message components on this builder.
        /// </summary>
        public void ClearComponents()
            => this._components.Clear();

        /// <summary>
        /// Allows for clearing the Followup Message builder so that it can be used again to send a new message.
        /// </summary>
        public void Clear()
        {
            this.Content = "";
            this._embeds.Clear();
            this.IsTts = false;
            this._mentions.Clear();
            this._files.Clear();
            this.IsEphemeral = false;
            this._components.Clear();
        }

        /// <summary>
        /// Validates the builder.
        /// </summary>
        internal void Validate()
        {
            if (this.Files?.Count == 0 && string.IsNullOrEmpty(this.Content) && !this.Embeds.Any())
                throw new ArgumentException("You must specify content, an embed, or at least one file.");
        }
    }
}
