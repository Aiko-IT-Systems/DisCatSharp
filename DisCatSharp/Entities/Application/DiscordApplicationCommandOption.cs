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
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a parameter for a <see cref="DiscordApplicationCommand"/>.
    /// </summary>
    public sealed class DiscordApplicationCommandOption
    {
        /// <summary>
        /// Gets the type of this command parameter.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandOptionType Type { get; internal set; }

        /// <summary>
        /// Gets the name of this command parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Sets the name localizations.
        /// </summary>
        [JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
        internal Dictionary<string, string> RawNameLocalizations { get; set; }

        /// <summary>
        /// Gets the name localizations.
        /// </summary>
        [JsonIgnore]
        public DiscordApplicationCommandLocalization NameLocalizations
            => new(this.RawNameLocalizations);

        /// <summary>
        /// Gets the description of this command parameter.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Sets the description localizations.
        /// </summary>
        [JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
        internal Dictionary<string, string> RawDescriptionLocalizations { get; set; }

        /// <summary>
        /// Gets the description localizations.
        /// </summary>
        [JsonIgnore]
        public DiscordApplicationCommandLocalization DescriptionLocalizations
            => new(this.RawDescriptionLocalizations);

        /// <summary>
        /// Gets whether this command parameter is required.
        /// </summary>
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Required { get; internal set; }

        /// <summary>
        /// Gets the optional choices for this command parameter.
        /// Not applicable for auto-complete options.
        /// </summary>
        [JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOptionChoice> Choices { get; internal set; }

        /// <summary>
        /// Gets the optional subcommand parameters for this parameter.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOption> Options { get; internal set; }

        /// <summary>
        /// Gets the optional allowed channel types.
        /// </summary>
        [JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<ChannelType> ChannelTypes { get; internal set; }

        /// <summary>
        /// Gets whether this option provides autocompletion.
        /// </summary>
        [JsonProperty("autocomplete", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AutoComplete { get; internal set; }

        /// <summary>
        /// Gets the minimum value for this slash command parameter.
        /// </summary>
        [JsonProperty("min_value", NullValueHandling = NullValueHandling.Ignore)]
        public object MinimumValue { get; internal set; }

        /// <summary>
        /// Gets the maximum value for this slash command parameter.
        /// </summary>
        [JsonProperty("max_value", NullValueHandling = NullValueHandling.Ignore)]
        public object MaximumValue { get; internal set; }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordApplicationCommandOption"/>.
        /// </summary>
        /// <param name="Name">The name of this parameter.</param>
        /// <param name="Description">The description of the parameter.</param>
        /// <param name="Type">The type of this parameter.</param>
        /// <param name="Required">Whether the parameter is required.</param>
        /// <param name="Choices">The optional choice selection for this parameter.</param>
        /// <param name="Options">The optional subcommands for this parameter.</param>
        /// <param name="Channeltypes">If the option is a channel type, the channels shown will be restricted to these types</param>
        /// <param name="Autocomplete">Whether this option provides autocompletion.</param>
        /// <param name="MinimumValue">The minimum value for this parameter. Only valid for types <see cref="ApplicationCommandOptionType.Integer"/> or <see cref="ApplicationCommandOptionType.Number"/>.</param>
        /// <param name="MaximumValue">The maximum value for this parameter. Only valid for types <see cref="ApplicationCommandOptionType.Integer"/> or <see cref="ApplicationCommandOptionType.Number"/>.</param>
        /// <param name="NameLocalizations">The localizations of the parameter name.</param>
        /// <param name="DescriptionLocalizations">The localizations of the parameter description.</param>
        public DiscordApplicationCommandOption(string Name, string Description, ApplicationCommandOptionType Type, bool? Required = null, IEnumerable<DiscordApplicationCommandOptionChoice> Choices = null, IEnumerable<DiscordApplicationCommandOption> Options = null, IEnumerable<ChannelType> Channeltypes = null, bool? Autocomplete = null, object MinimumValue = null, object MaximumValue = null, DiscordApplicationCommandLocalization NameLocalizations = null, DiscordApplicationCommandLocalization DescriptionLocalizations = null)
        {
            if (!Utilities.IsValidSlashCommandName(Name))
                throw new ArgumentException("Invalid application command option name specified. It must be below 32 characters and not contain any whitespace.", nameof(Name));
            if (Name.Any(Ch => char.IsUpper(Ch)))
                throw new ArgumentException("Application command option name cannot have any upper case characters.", nameof(Name));
            if (Description.Length > 100)
                throw new ArgumentException("Application command option description cannot exceed 100 characters.", nameof(Description));
            if ((Autocomplete ?? false) && (Choices?.Any() ?? false))
                throw new InvalidOperationException("Auto-complete slash command options cannot provide choices.");

            var choiceList = Choices != null ? new ReadOnlyCollection<DiscordApplicationCommandOptionChoice>(Choices.ToList()) : null;
            var optionList = Options != null ? new ReadOnlyCollection<DiscordApplicationCommandOption>(Options.ToList()) : null;
            var channelTypes = Channeltypes!= null ? new ReadOnlyCollection<ChannelType>(Channeltypes.ToList()) : null;

            this.Name = Name;
            this.Description = Description;
            this.Type = Type;
            this.Required = Required;
            this.Choices = choiceList;
            this.Options = optionList;
            this.ChannelTypes = channelTypes;
            this.AutoComplete = Autocomplete;
            this.MinimumValue = MinimumValue;
            this.MaximumValue = MaximumValue;
            this.RawNameLocalizations = NameLocalizations?.GetKeyValuePairs();
            this.RawDescriptionLocalizations = DescriptionLocalizations?.GetKeyValuePairs();
        }
    }
}
