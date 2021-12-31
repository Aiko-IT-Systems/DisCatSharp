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
using DisCatSharp.Enums;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a command that is registered to an application.
    /// </summary>
    public sealed class DiscordApplicationCommand : SnowflakeObject, IEquatable<DiscordApplicationCommand>
    {
        /// <summary>
        /// Gets the type of this application command.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationCommandType Type { get; internal set; }

        /// <summary>
        /// Gets the unique ID of this command's application.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }

        /// <summary>
        /// Gets the name of this command.
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
        /// Gets the description of this command.
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
        /// Gets the potential parameters for this command.
        /// </summary>
        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyCollection<DiscordApplicationCommandOption> Options { get; internal set; }

        /// <summary>
        /// Gets whether the command is enabled by default when the app is added to a guild.
        /// </summary>
        [JsonProperty("default_permission", NullValueHandling = NullValueHandling.Ignore)]
        public bool DefaultPermission { get; internal set; }

        /// <summary>
        /// Gets the commands needed permissions.
        /// </summary>
        [JsonProperty("default_member_permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions? Permission { get; internal set; }

        /// <summary>
        /// Gets whether the command can be used in direct messages.
        /// </summary>
        [JsonProperty("dm_permission", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DmPermission { get; internal set; }

        /// <summary>
        /// Gets the version number for this command.
        /// </summary>
        [JsonProperty("version")]
        public ulong Version { get; internal set; }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordApplicationCommand"/>.
        /// </summary>
        /// <param name="Name">The name of the command.</param>
        /// <param name="Description">The description of the command.</param>
        /// <param name="Options">Optional parameters for this command.</param>
        /// <param name="default_permission">Optional default permission for this command.</param>
        /// <param name="Type">The type of the command. Defaults to ChatInput.</param>
        /// <param name="NameLocalizations">The localizations of the command name.</param>
        /// <param name="DescriptionLocalizations">The localizations of the command description.</param>
        public DiscordApplicationCommand(string Name, string Description, IEnumerable<DiscordApplicationCommandOption> Options = null, bool DefaultPermission = true, ApplicationCommandType Type = ApplicationCommandType.ChatInput, DiscordApplicationCommandLocalization NameLocalizations = null, DiscordApplicationCommandLocalization DescriptionLocalizations = null)
        {
            if (Type is ApplicationCommandType.ChatInput)
            {
                if (!Utilities.IsValidSlashCommandName(Name))
                    throw new ArgumentException("Invalid slash command name specified. It must be below 32 characters and not contain any whitespace.", nameof(Name));
                if (Name.Any(Ch => char.IsUpper(Ch)))
                    throw new ArgumentException("Slash command name cannot have any upper case characters.", nameof(Name));
                if (Description.Length > 100)
                    throw new ArgumentException("Slash command description cannot exceed 100 characters.", nameof(Description));

                this.RawNameLocalizations = NameLocalizations?.GetKeyValuePairs();
                this.RawDescriptionLocalizations = DescriptionLocalizations?.GetKeyValuePairs();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Description))
                    throw new ArgumentException("Context menus do not support descriptions.");
                if (Options?.Any() ?? false)
                    throw new ArgumentException("Context menus do not support options.");
                Description = string.Empty;

                this.RawNameLocalizations = NameLocalizations?.GetKeyValuePairs();
            }

            var optionsList = Options != null ? new ReadOnlyCollection<DiscordApplicationCommandOption>(Options.ToList()) : null;

            this.Type = Type;
            this.Name = Name;
            this.Description = Description;
            this.Options = optionsList;
            this.DefaultPermission = DefaultPermission;
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplicationCommand"/> object is equal to another object.
        /// </summary>
        /// <param name="Other">The command to compare to.</param>
        /// <returns>Whether the command is equal to this <see cref="DiscordApplicationCommand"/>.</returns>
        public bool Equals(DiscordApplicationCommand Other)
            => this.Id == Other.Id;

        /// <summary>
        /// Determines if two <see cref="DiscordApplicationCommand"/> objects are equal.
        /// </summary>
        /// <param name="E1">The first command object.</param>
        /// <param name="E2">The second command object.</param>
        /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are equal.</returns>
        public static bool operator ==(DiscordApplicationCommand E1, DiscordApplicationCommand E2)
            => E1.Equals(E2);

        /// <summary>
        /// Determines if two <see cref="DiscordApplicationCommand"/> objects are not equal.
        /// </summary>
        /// <param name="E1">The first command object.</param>
        /// <param name="E2">The second command object.</param>
        /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are not equal.</returns>
        public static bool operator !=(DiscordApplicationCommand E1, DiscordApplicationCommand E2)
            => !(E1 == E2);

        /// <summary>
        /// Determines if a <see cref="object"/> is equal to the current <see cref="DiscordApplicationCommand"/>.
        /// </summary>
        /// <param name="Other">The object to compare to.</param>
        /// <returns>Whether the two <see cref="DiscordApplicationCommand"/> objects are not equal.</returns>
        public override bool Equals(object Other)
            => Other is DiscordApplicationCommand dac && this.Equals(dac);

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordApplicationCommand"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordApplicationCommand"/>.</returns>
        public override int GetHashCode()
            => this.Id.GetHashCode();
    }
}
