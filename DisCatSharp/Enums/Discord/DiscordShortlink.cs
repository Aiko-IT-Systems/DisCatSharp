// This file is part of the DisCatSharp project.
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

namespace DisCatSharp.Enums.Discord
{
    /// <summary>
    /// Discord short links
    /// </summary>
    public class DiscordShortlink
    {
        public string Support = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/support";
        public string TrustAndSafety = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/request";
        public string Contact = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/contact";
        public string BugReport = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/bugreport";
        public string TranslationError = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/lang-feedback";
        public string Status = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/status";
        public string Terms = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/terms";
        public string Guidlines = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/guidelines";
        public string Moderation = $"{DiscordDomain.GetDomain(CoreDomain.DiscordMarketing).Url}/moderation";
    }
}
