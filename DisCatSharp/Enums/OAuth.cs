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

namespace DisCatSharp.Enums
{
    /// <summary>
    /// The oauth scopes.
    /// </summary>
    public static class OAuth
    {
        /// <summary>
        /// The default scopes for bots.
        /// </summary>
        private const string BotDefault = "bot applications.commands applications.commands.permissions.update";

        /// <summary>
        /// The bot minimal scopes.
        /// </summary>
        private const string BotMinimal = "bot applications.commands";

        /// <summary>
        /// The bot only scope.
        /// </summary>
        private const string BotOnly = "bot";

        /// <summary>
        /// The basic identify scopes.
        /// </summary>
        private const string IdentifyBasic = "identify email";

        /// <summary>
        /// The extended identify scopes.
        /// </summary>
        private const string IdentifyExtended = "identify email guilds connections";

        /// <summary>
        /// All scopes for bots and identify.
        /// </summary>
        private const string All = BotDefault + " " + IdentifyExtended;

        /// <summary>
        /// The oauth scope.
        /// </summary>


        /// <summary>
        /// Resolves the scopes.
        /// </summary>
        /// <param name="Scope">The scope.</param>
        /// <returns>A string representing the scopes.</returns>
        public static string ResolveScopes(OAuthScopes Scope)
        {
            return Scope switch
            {
                OAuthScopes.BotDefault => BotDefault,
                OAuthScopes.BotMinimal => BotMinimal,
                OAuthScopes.BotOnly => BotOnly,
                OAuthScopes.IdentifyBasic => IdentifyBasic,
                OAuthScopes.IdentifyExtended => IdentifyExtended,
                OAuthScopes.All => All,
                _ => BotDefault,
            };
        }
    }
    /// <summary>
    /// The oauth scopes.
    /// </summary>
    public enum OAuthScopes
    {
        /// <summary>
        /// Scopes: bot applications.commands applications.commands.permissions.update
        /// </summary>
        BotDefault = 0,

        /// <summary>
        /// Scopes: bot applications.commands
        /// </summary>
        BotMinimal = 1,

        /// <summary>
        /// Scopes: bot
        /// </summary>
        BotOnly = 2,

        /// <summary>
        /// Scopes: identify email
        /// </summary>
        IdentifyBasic = 3,

        /// <summary>
        /// Scopes: identify email guilds connections
        /// </summary>
        IdentifyExtended = 4,

        /// <summary>
        /// Scopes: bot applications.commands applications.commands.permissions.update identify email guilds connections
        /// </summary>
        All = 5
    }
}
