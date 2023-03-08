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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an action which will execute when a rule is triggered.
/// </summary>
public class AutomodAction
{
	/// <summary>
	/// The type of action.
	/// </summary>
	[JsonProperty("type")]
	public AutomodActionType ActionType { get; internal set; }

	/// <summary>
	/// The additional meta data needed during execution for this specific action type.
	/// </summary>
	[JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
	public AutomodActionMetadata Metadata { get; internal set; }

	/// <summary>
	/// Creates a new empty automod action.
	/// </summary>
	internal AutomodAction() { }

	/// <summary>
	/// Creates a new automod action.
	/// </summary>
	/// <param name="actionType">The type of action.</param>
	/// <param name="metadata">The additional metadata for this action.</param>
	public AutomodAction(AutomodActionType actionType, AutomodActionMetadata metadata = null)
	{
		this.ActionType = actionType;
		this.Metadata = metadata;
	}
}
