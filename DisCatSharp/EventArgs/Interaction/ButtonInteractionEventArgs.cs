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

using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs.Interaction;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.InteractionCreated"/>.
/// Used for button interactions.
/// </summary>
internal class ButtonInteractionEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the interaction data that was invoked.
	/// </summary>
	public DiscordInteraction Interaction { get; internal set; }

	/// <summary>
	/// Gets the user who executed the button.
	/// </summary>
	public DiscordUser User
		=> this.Interaction.User;

	/// <summary>
	/// Gets the channel in which the button was executed.
	/// </summary>
	public DiscordChannel Channel
		=> this.Interaction.Channel;

	/// <summary>
	/// Gets the custom id of the button.
	/// </summary>
	public string CustomId
		=> this.Interaction.Data.CustomId;

	/// <summary>
	/// Initializes a new instance of the <see cref="ButtonInteractionEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public ButtonInteractionEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
