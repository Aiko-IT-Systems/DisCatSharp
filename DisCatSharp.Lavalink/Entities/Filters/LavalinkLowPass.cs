// This file is part of the DisCatSharp project.
//
// Copyright (c) 2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Filters;

/// <summary>
/// Higher frequencies get suppressed, while lower frequencies pass through this filter, thus the name low pass.
/// </summary>
public sealed class LavalinkLowPass
{
	/// <summary>
	/// The smoothing factor (<c>>1.0</c>, Any smoothing values equal to or less than 1.0 will disable the filter)
	/// </summary>
	[JsonProperty("smoothing")]
	public Optional<float> Smoothing { get; set; }

	/// <inheritdoc cref="LavalinkLowPass"/>
	/// <param name="smoothing">The smoothing factor</param>
	public LavalinkLowPass(Optional<float> smoothing)
	{
		this.Smoothing = smoothing;
	}
}
