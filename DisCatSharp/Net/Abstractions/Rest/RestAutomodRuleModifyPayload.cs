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

using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

internal class RestAutomodRuleModifyPayload
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Name { get; set; }

	[JsonProperty("event_type", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<AutomodEventType> EventType { get; set; }

	[JsonProperty("trigger_type", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<AutomodTriggerType> TriggerType { get; set; }

	[JsonProperty("trigger_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<AutomodTriggerMetadata> TriggerMetadata { get; set; }

	[JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<AutomodAction>> Actions { get; set; }

	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Enabled { get; set; }

	[JsonProperty("exempt_roles", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<ulong>> ExemptRoles { get; set; }

	[JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<ulong>> ExemptChannels { get; set; }
}
