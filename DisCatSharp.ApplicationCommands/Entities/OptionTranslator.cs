// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands
{
	/// <summary>
	/// Represents a option translator.
	/// </summary>
	internal class OptionTranslator
	{
		/// <summary>
		/// Gets the option name.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets the option name translations.
		/// </summary>
		[JsonProperty("name_translations")]
		internal Dictionary<string, string> NameTranslationsDictionary { get; set; }
		[JsonIgnore]
		public DiscordApplicationCommandLocalization NameTranslations
			=> new(this.NameTranslationsDictionary);

		/// <summary>
		/// Gets the option description translations.
		/// </summary>
		[JsonProperty("description_translations")]
		internal Dictionary<string, string> DescriptionTranslationsDictionary { get; set; }
		[JsonIgnore]
		public DiscordApplicationCommandLocalization DescriptionTranslations
			=> new(this.DescriptionTranslationsDictionary);

		/// <summary>
		/// Gets the choice translators, if applicable.
		/// </summary>
		[JsonProperty("choices")]
		public List<ChoiceTranslator> Choices { get; set; }
	}
}
