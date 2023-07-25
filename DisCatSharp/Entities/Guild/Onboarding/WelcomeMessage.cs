// This file is part of the DisCatSharp project, based off DSharpPlus.
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

using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a welcome message for the server guide.
/// </summary>
public sealed class WelcomeMessage : ObservableApiObject
{
	/// <summary>
	/// Gets the welcome message author ids.
	/// Can only be <c>1</c> and target must have write permission.
	/// </summary>
	[JsonProperty("author_ids", NullValueHandling = NullValueHandling.Ignore)]
	internal List<ulong> AuthorIds { get; set; } = new();

	/// <summary>
	/// Gets the author id.
	/// </summary>
	[JsonIgnore]
	public ulong AuthorId
		=> this.AuthorIds.First();

	/// <summary>
	/// Gets the welcome message.
	/// <para> <c>[@username]</c> is used to mention the new member.</para>
	/// </summary>
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public string Message { get; internal set; }

	/// <summary>
	/// Constructs a new welcome message for the server guide.
	/// </summary>
	/// <param name="authorId">The author id.</param>
	/// <param name="message">The message. Use <c>[@username]</c> to mention the new member. Required.</param>
	public WelcomeMessage(ulong authorId, string message)
	{
		this.AuthorIds = new() { authorId };
		this.Message = message;
	}

	/// <summary>
	/// Constructs a welcome message for the server guide.
	/// </summary>
	internal WelcomeMessage()
	{ }
}
