// This file is part of the DisCatSharp project.
//
// Copyright (c) 2021-2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
/// The application commands translation context.
/// </summary>
public class ApplicationCommandsTranslationContext
{
	/// <summary>
	/// Gets the type.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the group translation json.
	/// </summary>
	internal string GroupTranslations { get; set; }

	/// <summary>
	/// Gets the single translation json.
	/// </summary>
	internal string SingleTranslations { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationCommandsTranslationContext"/> class.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="name">The name.</param>
	internal ApplicationCommandsTranslationContext(Type type, string name)
	{
		this.Type = type;
		this.Name = name;
	}

	public void AddGroupTranslation(string translationJson)
		=> this.GroupTranslations = translationJson;

	public void AddSingleTranslation(string translationJson)
		=> this.SingleTranslations = translationJson;
}
