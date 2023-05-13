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

// ReSharper disable InconsistentNaming
namespace DisCatSharp.Enums;

/// <summary>
/// The oauth scopes.
/// </summary>
public static class OAuth
{
	/// <summary>
	/// The default scopes for bots.
	/// </summary>
	private const string BOT_DEFAULT = "bot applications.commands"; // applications.commands.permissions.update

	/// <summary>
	/// The bot minimal scopes.
	/// </summary>
	private const string BOT_MINIMAL = "bot applications.commands";

	/// <summary>
	/// The bot only scope.
	/// </summary>
	private const string BOT_ONLY = "bot";

	/// <summary>
	/// The basic identify scopes.
	/// </summary>
	private const string IDENTIFY_BASIC = "identify email";

	/// <summary>
	/// The extended identify scopes.
	/// </summary>
	private const string IDENTIFY_EXTENDED = "identify email guilds guilds.members.read connections";

	/// <summary>
	/// The role connection scope.
	/// </summary>
	private const string ROLE_CONNECTIONS_WRITE = "role_connections.write";

	/// <summary>
	/// All scopes for bots and identify.
	/// </summary>
	private const string ALL = BOT_DEFAULT + " " + IDENTIFY_EXTENDED + " " + ROLE_CONNECTIONS_WRITE;

	/// <summary>
	/// Resolves the scopes.
	/// </summary>
	/// <param name="scope">The scope.</param>
	/// <returns>A string representing the scopes.</returns>
	public static string ResolveScopes(OAuthScopes scope) =>
		scope switch
		{
			OAuthScopes.BOT_DEFAULT => BOT_DEFAULT,
			OAuthScopes.BOT_MINIMAL => BOT_MINIMAL,
			OAuthScopes.BOT_ONLY => BOT_ONLY,
			OAuthScopes.IDENTIFY_BASIC => IDENTIFY_BASIC,
			OAuthScopes.IDENTIFY_EXTENDED => IDENTIFY_EXTENDED,
			OAuthScopes.ALL => ALL,
			_ => BOT_DEFAULT,
		};
}
/// <summary>
/// The oauth scopes.
/// </summary>
public enum OAuthScopes
{
	/// <summary>
	/// Scopes: bot applications.commands (Excluding applications.commands.permissions.update for now)
	/// </summary>
	BOT_DEFAULT = 0,

	/// <summary>
	/// Scopes: bot applications.commands
	/// </summary>
	BOT_MINIMAL = 1,

	/// <summary>
	/// Scopes: bot
	/// </summary>
	BOT_ONLY = 2,

	/// <summary>
	/// Scopes: identify email
	/// </summary>
	IDENTIFY_BASIC = 3,

	/// <summary>
	/// Scopes: identify email guilds connections
	/// </summary>
	IDENTIFY_EXTENDED = 4,

	/// <summary>
	/// Scopes: bot applications.commands applications.commands.permissions.update identify email guilds connections role_connections.write
	/// </summary>
	ALL = 5,

	/// <summary>
	/// Scopes: role_connections.write
	/// </summary>
	ROLE_CONNECTIONS_WRITE = 6
}
