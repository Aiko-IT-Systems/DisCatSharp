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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DisCatSharp.Tests.EntityRegexTests;

class WebsiteGenerator : IEnumerable<object[]>
{
	private static readonly ImmutableArray<string> s_protocols = new[] { "", "http://", "https://" }.ToImmutableArray();
	private static readonly ImmutableArray<string> s_subdomains = new[] { "", "www.", "ptb.", "canary." }.ToImmutableArray();
	private static readonly ImmutableArray<string> s_domains = new[] { "discord", "discordapp" }.ToImmutableArray();

	public IEnumerator<object[]> GetEnumerator()
	{
		foreach (var protocol in s_protocols)
		foreach (var subdomain in s_subdomains)
		foreach (var domain in s_domains)
			yield return new object[] { $"{protocol}{subdomain}{domain}.com/" };
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
