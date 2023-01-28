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

namespace DisCatSharp.Entities;

public readonly partial struct DiscordColor
{
	#region Black and White
	/// <summary>
	/// Represents no color, or integer 0;
	/// </summary>
	public static DiscordColor None { get; } = new(0);

	/// <summary>
	/// A near-black color. Due to API limitations, the color is #010101, rather than #000000, as the latter is treated as no color.
	/// </summary>
	public static DiscordColor Black { get; } = new(0x010101);

	/// <summary>
	/// White, or #FFFFFF.
	/// </summary>
	public static DiscordColor White { get; } = new(0xFFFFFF);

	/// <summary>
	/// Gray, or #808080.
	/// </summary>
	public static DiscordColor Gray { get; } = new(0x808080);

	/// <summary>
	/// Dark gray, or #A9A9A9.
	/// </summary>
	public static DiscordColor DarkGray { get; } = new(0xA9A9A9);

	/// <summary>
	/// Light gray, or #808080.
	/// </summary>
	public static DiscordColor LightGray { get; } = new(0xD3D3D3);

	/// <summary>
	/// Very dark gray, or #666666.
	/// </summary>
	public static DiscordColor VeryDarkGray { get; } = new(0x666666);
	#endregion

	#region Discord branding colors
	// See https://discord.com/branding.

	/// <summary>
	/// Discord Blurple, or #5865F2.
	/// </summary>
	public static DiscordColor Blurple { get; } = new(0x5865F2);

	/// <summary>
	/// Discord Fuchsia, or #EB459E.
	/// </summary>
	public static DiscordColor Fuchsia { get; } = new(0xEB459E);

	/// <summary>
	/// Discord Green, or #57F287.
	/// </summary>
	public static DiscordColor Green { get; } = new(0x57F287);

	/// <summary>
	/// Discord Yellow, or #FEE75C.
	/// </summary>
	public static DiscordColor Yellow { get; } = new(0xFEE75C);

	/// <summary>
	/// Discord Red, or #ED4245.
	/// </summary>
	public static DiscordColor Red { get; } = new(0xED4245);
	#endregion

	#region Other colors
	/// <summary>
	/// Dark red, or #7F0000.
	/// </summary>
	public static DiscordColor DarkRed { get; } = new(0x7F0000);

	/// <summary>
	/// Dark green, or #007F00.
	/// </summary>
	public static DiscordColor DarkGreen { get; } = new(0x007F00);

	/// <summary>
	/// Blue, or #0000FF.
	/// </summary>
	public static DiscordColor Blue { get; } = new(0x0000FF);

	/// <summary>
	/// Dark blue, or #00007F.
	/// </summary>
	public static DiscordColor DarkBlue { get; } = new(0x00007F);

	/// <summary>
	/// Cyan, or #00FFFF.
	/// </summary>
	public static DiscordColor Cyan { get; } = new(0x00FFFF);

	/// <summary>
	/// Magenta, or #FF00FF.
	/// </summary>
	public static DiscordColor Magenta { get; } = new(0xFF00FF);

	/// <summary>
	/// Teal, or #008080.
	/// </summary>
	public static DiscordColor Teal { get; } = new(0x008080);

	// meme
	/// <summary>
	/// Aquamarine, or #00FFBF.
	/// </summary>
	public static DiscordColor Aquamarine { get; } = new(0x00FFBF);

	/// <summary>
	/// Gold, or #FFD700.
	/// </summary>
	public static DiscordColor Gold { get; } = new(0xFFD700);

	/// <summary>
	/// Goldenrod, or #DAA520.
	/// </summary>
	public static DiscordColor Goldenrod { get; } = new(0xDAA520);

	/// <summary>
	/// Azure, or #007FFF.
	/// </summary>
	public static DiscordColor Azure { get; } = new(0x007FFF);

	/// <summary>
	/// Rose, or #FF007F.
	/// </summary>
	public static DiscordColor Rose { get; } = new(0xFF007F);

	/// <summary>
	/// Spring green, or #00FF7F.
	/// </summary>
	public static DiscordColor SpringGreen { get; } = new(0x00FF7F);

	/// <summary>
	/// Chartreuse, or #7FFF00.
	/// </summary>
	public static DiscordColor Chartreuse { get; } = new(0x7FFF00);

	/// <summary>
	/// Orange, or #FFA500.
	/// </summary>
	public static DiscordColor Orange { get; } = new(0xFFA500);

	/// <summary>
	/// Purple, or #800080.
	/// </summary>
	public static DiscordColor Purple { get; } = new(0x800080);

	/// <summary>
	/// Violet, or #EE82EE.
	/// </summary>
	public static DiscordColor Violet { get; } = new(0xEE82EE);

	/// <summary>
	/// Brown, or #A52A2A.
	/// </summary>
	public static DiscordColor Brown { get; } = new(0xA52A2A);

	/// <summary>
	/// Hot pink, or #FF69B4
	/// </summary>
	public static DiscordColor HotPink { get; } = new(0xFF69B4);

	/// <summary>
	/// Lilac, or #C8A2C8.
	/// </summary>
	public static DiscordColor Lilac { get; } = new(0xC8A2C8);

	/// <summary>
	/// Cornflower blue, or #6495ED.
	/// </summary>
	public static DiscordColor CornflowerBlue { get; } = new(0x6495ED);

	/// <summary>
	/// Midnight blue, or #191970.
	/// </summary>
	public static DiscordColor MidnightBlue { get; } = new(0x191970);

	/// <summary>
	/// Wheat, or #F5DEB3.
	/// </summary>
	public static DiscordColor Wheat { get; } = new(0xF5DEB3);

	/// <summary>
	/// Indian red, or #CD5C5C.
	/// </summary>
	public static DiscordColor IndianRed { get; } = new(0xCD5C5C);

	/// <summary>
	/// Turquoise, or #30D5C8.
	/// </summary>
	public static DiscordColor Turquoise { get; } = new(0x30D5C8);

	/// <summary>
	/// Sap green, or #507D2A.
	/// </summary>
	public static DiscordColor SapGreen { get; } = new(0x507D2A);

	// meme, specifically bob ross
	/// <summary>
	/// Phthalo blue, or #000F89.
	/// </summary>
	public static DiscordColor PhthaloBlue { get; } = new(0x000F89);

	// meme, specifically bob ross
	/// <summary>
	/// Phthalo green, or #123524.
	/// </summary>
	public static DiscordColor PhthaloGreen { get; } = new(0x123524);

	/// <summary>
	/// Sienna, or #882D17.
	/// </summary>
	public static DiscordColor Sienna { get; } = new(0x882D17);
	#endregion
}
