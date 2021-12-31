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
    /// Constructs an interaction response.
    /// </summary>
    public sealed class DiscordInteractionResponseBuilder
    {
        /// <summary>
        /// Whether this interaction response is text-to-speech.
        /// </summary>
        public bool IsTts { get; set; }

        /// <summary>
        /// Whether this interaction response should be ephemeral.
        /// </summary>
        public bool IsEphemeral { get; set; }

        /// <summary>
        /// Content of the message to send.
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
        /// Embeds to send on this interaction response.
        /// </summary>
        public IReadOnlyList<DiscordEmbed> Embeds => this._embeds;
        private readonly List<DiscordEmbed> _embeds = new();


        /// <summary>
        /// Files to send on this interaction response.
        /// </summary>
        public IReadOnlyList<DiscordMessageFile> Files => this._files;
        private readonly List<DiscordMessageFile> _files = new();

        /// <summary>
        /// Components to send on this interaction response.
        /// </summary>
        public IReadOnlyList<DiscordActionRowComponent> Components => this._components;
        private readonly List<DiscordActionRowComponent> _components = new();

        /// <summary>
        /// The choices to send on this interaction response.
        /// Mutually exclusive with content, embed, and components.
        /// </summary>
        public IReadOnlyList<DiscordApplicationCommandAutocompleteChoice> Choices => this._choices;
        private readonly List<DiscordApplicationCommandAutocompleteChoice> _choices = new();


        /// <summary>
        /// Mentions to send on this interaction response.
        /// </summary>
        public IEnumerable<IMention> Mentions => this._mentions;
        private readonly List<IMention> _mentions = new();

        /// <summary>
        /// Constructs a new empty interaction response builder.
        /// </summary>
        public DiscordInteractionResponseBuilder() { }


        /// <summary>
        /// Constructs a new <see cref="DiscordInteractionResponseBuilder"/> based on an existing <see cref="DisCatSharp.Entities.DiscordMessageBuilder"/>.
        /// </summary>
        /// <param name="Builder">The builder to copy.</param>
        public DiscordInteractionResponseBuilder(DiscordMessageBuilder Builder)
        {
            this._content = Builder.Content;
            this._mentions = Builder.Mentions;
            this._embeds.AddRange(Builder.Embeds);
            this._components.AddRange(Builder.Components);
        }


        /// <summary>
        /// Appends a collection of components to the builder. Each call will append to a new row.
        /// </summary>
        /// <param name="Components">The components to append. Up to five.</param>
        /// <returns>The current builder to chain calls with.</returns>
        /// <exception cref="System.ArgumentException">Thrown when passing more than 5 components.</exception>
        public DiscordInteractionResponseBuilder AddComponents(params DiscordComponent[] Components)
            => this.AddComponents((IEnumerable<DiscordComponent>)Components);

        /// <summary>
        /// Appends several rows of components to the message
        /// </summary>
        /// <param name="Components">The rows of components to add, holding up to five each.</param>
        /// <returns></returns>
        public DiscordInteractionResponseBuilder AddComponents(IEnumerable<DiscordActionRowComponent> Components)
        {
            var ara = Components.ToArray();

            if (ara.Length + this._components.Count > 5)
                throw new ArgumentException("ActionRow count exceeds maximum of five.");

            foreach (var ar in ara)
                this._components.Add(ar);

            return this;
        }

        /// <summary>
        /// Appends a collection of components to the builder. Each call will append to a new row.
        /// </summary>
        /// <param name="Components">The components to append. Up to five.</param>
        /// <returns>The current builder to chain calls with.</returns>
        /// <exception cref="System.ArgumentException">Thrown when passing more than 5 components.</exception>
        public DiscordInteractionResponseBuilder AddComponents(IEnumerable<DiscordComponent> Components)
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
        /// Indicates if the interaction response will be text-to-speech.
        /// </summary>
        /// <param name="Tts">Text-to-speech</param>
        public DiscordInteractionResponseBuilder WithTts(bool Tts)
        {
            this.IsTts = Tts;
            return this;
        }

        /// <summary>
        /// Sets the interaction response to be ephemeral.
        /// </summary>
        /// <param name="Ephemeral">Ephemeral.</param>
        public DiscordInteractionResponseBuilder AsEphemeral(bool Ephemeral)
        {
            this.IsEphemeral = Ephemeral;
            return this;
        }

        /// <summary>
        /// Sets the content of the message to send.
        /// </summary>
        /// <param name="Content">Content to send.</param>
        public DiscordInteractionResponseBuilder WithContent(string Content)
        {
            this.Content = Content;
            return this;
        }

        /// <summary>
        /// Adds an embed to send with the interaction response.
        /// </summary>
        /// <param name="Embed">Embed to add.</param>
        public DiscordInteractionResponseBuilder AddEmbed(DiscordEmbed Embed)
        {
            if (Embed != null)
                this._embeds.Add(Embed); // Interactions will 400 silently //
            return this;
        }

        /// <summary>
        /// Adds the given embeds to send with the interaction response.
        /// </summary>
        /// <param name="Embeds">Embeds to add.</param>
        public DiscordInteractionResponseBuilder AddEmbeds(IEnumerable<DiscordEmbed> Embeds)
        {
            this._embeds.AddRange(Embeds);
            return this;
        }

        /// <summary>
        /// Adds a file to the interaction response.
        /// </summary>
        /// <param name="Filename">Name of the file.</param>
        /// <param name="Data">File data.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <param name="Description">Description of the file.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddFile(string Filename, Stream Data, bool ResetStreamPosition = false, string Description = null)
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
        public DiscordInteractionResponseBuilder AddFile(FileStream Stream, bool ResetStreamPosition = false, string Description = null)
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
        /// Adds the given files to the interaction response builder.
        /// </summary>
        /// <param name="Files">Dictionary of file name and file data.</param>
        /// <param name="ResetStreamPosition">Tells the API Client to reset the stream position to what it was after the file is sent.</param>
        /// <returns>The builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddFiles(Dictionary<string, Stream> Files, bool ResetStreamPosition = false)
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
        /// Adds the mention to the mentions to parse, etc. with the interaction response.
        /// </summary>
        /// <param name="Mention">Mention to add.</param>
        public DiscordInteractionResponseBuilder AddMention(IMention Mention)
        {
            this._mentions.Add(Mention);
            return this;
        }

        /// <summary>
        /// Adds the mentions to the mentions to parse, etc. with the interaction response.
        /// </summary>
        /// <param name="Mentions">Mentions to add.</param>
        public DiscordInteractionResponseBuilder AddMentions(IEnumerable<IMention> Mentions)
        {
            this._mentions.AddRange(Mentions);
            return this;
        }

        /// <summary>
        /// Adds a single auto-complete choice to the builder.
        /// </summary>
        /// <param name="Choice">The choice to add.</param>
        /// <returns>The current builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddAutoCompleteChoice(DiscordApplicationCommandAutocompleteChoice Choice)
        {
            this._choices.Add(Choice);
            return this;
        }

        /// <summary>
        /// Adds auto-complete choices to the builder.
        /// </summary>
        /// <param name="Choices">The choices to add.</param>
        /// <returns>The current builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddAutoCompleteChoices(IEnumerable<DiscordApplicationCommandAutocompleteChoice> Choices)
        {
            this._choices.AddRange(Choices);
            return this;
        }

        /// <summary>
        /// Adds auto-complete choices to the builder.
        /// </summary>
        /// <param name="Choices">The choices to add.</param>
        /// <returns>The current builder to chain calls with.</returns>
        public DiscordInteractionResponseBuilder AddAutoCompleteChoices(params DiscordApplicationCommandAutocompleteChoice[] Choices)
            => this.AddAutoCompleteChoices((IEnumerable<DiscordApplicationCommandAutocompleteChoice>)Choices);

        /// <summary>
        /// Clears all message components on this builder.
        /// </summary>
        public void ClearComponents()
            => this._components.Clear();

        /// <summary>
        /// Allows for clearing the Interaction Response Builder so that it can be used again to send a new response.
        /// </summary>
        public void Clear()
        {
            this.Content = "";
            this._embeds.Clear();
            this.IsTts = false;
            this.IsEphemeral = false;
            this._mentions.Clear();
            this._components.Clear();
            this._choices.Clear();
            this._files.Clear();
        }
    }
}
