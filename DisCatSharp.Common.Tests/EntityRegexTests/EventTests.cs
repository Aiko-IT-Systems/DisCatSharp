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

using DisCatSharp.Common.RegularExpressions;

using Xunit;
using Xunit.Abstractions;

namespace DisCatSharp.Tests.EntityRegexTests;

public class EventTests
{
	private readonly ITestOutputHelper _output;
	public EventTests(ITestOutputHelper output)
	{
		this._output = output;
	}

	[Theory, ClassData(typeof(WebsiteGenerator))]
	void TestEventMatch(string website)
	{
		var evtn = $"{website}events/123/456";
		this._output.WriteLine($"Testing \"{evtn}\".");
		var match = DiscordRegEx.Event.Match(evtn);
		Assert.True(match.Success);
		Assert.Equal("123", match.Groups["guild"].Value);
		Assert.Equal("456", match.Groups["event"].Value);
	}

	private const string VALID_EVENT = "https://discord.com/events/123/456";

	[Fact]
	void TestValidEvent() => Assert.Matches(DiscordRegEx.Event, VALID_EVENT);

	[Theory, CombinatorialData]
	void TestInvalidEvents([CombinatorialValues(
		"",
		"_",
		"asd",
		$".{VALID_EVENT}",
		$"a{VALID_EVENT}",
		$"{VALID_EVENT}b",
		$"{VALID_EVENT}_",
		"https://discord.com/events/123/",
		"https://discord.com/events/123",
		"iscord.com/events/123/456",
		"https://discord.ru/events/123/456",
		"ftp://discord.com/events/123/456",
		"https://discord.com/stneve/123/456",
		"https://discord.com/events/1x23/456",
		"https://discord.com/events/123/4z56",
		"https://discord.com/events/12_3/456",
		"https://discord.com/events/123/45%6",
		"https://discord.comevents/123/456",
		"https:/discord.comevents/123/456",
		"https:/discord.com/events/123/456",
		"https//discord.com/events/123/456",
		"/discord.com/events/123/456"
	)] string evtn) =>
		Assert.DoesNotMatch(DiscordRegEx.Event, evtn);
}
