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

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Interactivity.Enums;

/// <summary>
/// A modal page.
/// </summary>
public class ModalPage
{
	/// <summary>
	/// Creates a new modal page for the paginated modal builder.
	/// </summary>
	/// <param name="modal">The modal to display.</param>
	/// <param name="openButton">The button to display to open the current page. This is skipped if possible.</param>
	/// <param name="openText">The text to display to open the current page. This is skipped if possible.</param>
	public ModalPage(DiscordInteractionModalBuilder modal, DiscordButtonComponent openButton = null, string openText = null)
	{
		this.Modal = modal;
		this.OpenButton = openButton ?? new DiscordButtonComponent(ButtonStyle.Primary, null, "Open next page", false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("📄")));
		this.OpenMessage = new DiscordInteractionResponseBuilder().WithContent(openText ?? "`Click the button below to continue to the next page.`").AsEphemeral();
	}

	/// <summary>
	/// The modal that will be displayed.
	/// </summary>
	public DiscordInteractionModalBuilder Modal { get; set; }

	/// <summary>
	/// The button that will be displayed on the ephemeral message.
	/// </summary>
	public DiscordButtonComponent OpenButton { get; set; }

	/// <summary>
	/// The ephemeral message to display for this page.
	/// </summary>
	public DiscordInteractionResponseBuilder OpenMessage { get; set; }
}
