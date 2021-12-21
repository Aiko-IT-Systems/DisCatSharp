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

using System.Collections.Generic;
using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands
{
    /// <summary>
    /// Represents a application commands translation resolver.
    /// </summary>
    public class ApplicationCommandsTranslationResolver
    {
        #pragma warning disable IDE1006
        /// <summary>
        /// Gets the group translations.
        /// </summary>
        public IReadOnlyDictionary<string, GroupTranslation> GroupTranslations => this._groupTranslations;
        private Dictionary<string, GroupTranslation> _groupTranslations => new();

        /// <summary>
        /// Gets the sub group translations.
        /// </summary>
        public IReadOnlyDictionary<string, SubGroupTranslation> SubGroupTranslations => this._subGroupTranslations;
        private Dictionary<string, SubGroupTranslation> _subGroupTranslations => new();

        /// <summary>
        /// Gets the command translations.
        /// </summary>
        public IReadOnlyDictionary<string, CommandTranslation> CommandTranslations => this._commandTranslations;
        private Dictionary<string, CommandTranslation> _commandTranslations => new();

        #pragma warning restore IDE1006
    }

    public class GroupTranslation
    {
        /// <summary>
        /// Gets the group name.
        /// </summary>
        public string GroupName { get; set; }

        public DiscordApplicationCommandLocalization GroupNameLocalizations { get; set; }

        public DiscordApplicationCommandLocalization GroupDescriptionLocalizations { get; set; }

        /// <summary>
        /// Gets the sub group translations.
        /// </summary>
        public Dictionary<string, SubGroupTranslation> SubGroupTranslations { get; set; }

        /// <summary>
        /// Gets the command translations.
        /// </summary>
        public Dictionary<string, CommandTranslation> CommandTranslations { get; set; }
    }

    public class SubGroupTranslation
    {
        /// <summary>
        /// Gets the sub group name.
        /// </summary>
        public string SubGroupName { get; set; }

        public DiscordApplicationCommandLocalization SubGroupNameLocalizations { get; set; }

        public DiscordApplicationCommandLocalization SubGroupDescriptionLocalizations { get; set; }

        /// <summary>
        /// Gets the command translations.
        /// </summary>
        public Dictionary<string, CommandTranslation> CommandTranslations { get; set; }
    }

    public class CommandTranslation
    {
        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string CommandName { get; set; }

        public DiscordApplicationCommandLocalization CommandNameLocalizations { get; set; }

        public DiscordApplicationCommandLocalization CommandDescriptionLocalizations { get; set; }

        /// <summary>
        /// Gets the option translations.
        /// </summary>
        public Dictionary<string, OptionTranslation> OptionTranslations { get; set; }
    }

    public class OptionTranslation
    {
        /// <summary>
        /// Gets the option name.
        /// </summary>
        public string OptionName { get; set; }

        public DiscordApplicationCommandLocalization OptionNameLocalizations { get; set; }

        public DiscordApplicationCommandLocalization OptionDescriptionLocalizations { get; set; }

        /// <summary>
        /// Gets the choice translations
        /// </summary>
        public Dictionary<string, ChoiceTranslation> ChoiceTranslations { get; set; }

        /// <summary>
        /// Gets the auto complete choice translations
        /// </summary>
        public Dictionary<string, AutocompleteChoiceTranslation> AutocompleteChoiceTranslations { get; set; }
    }

    public class AutocompleteChoiceTranslation
    {
        /// <summary>
        /// Gets the autocomplete choice name.
        /// </summary>
        public string AutocompleteChoiceName { get; set; }

        public DiscordApplicationCommandLocalization AutocompleteNameLocalizations { get; set; }

    }

    public class ChoiceTranslation
    {
        /// <summary>
        /// Gets the choice name
        /// </summary>
        public string ChoiceName { get; set; }

        public DiscordApplicationCommandLocalization ChoiceNameLocalizations { get; set; }
    }
}
