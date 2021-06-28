// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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

namespace DSharpPlusNextGen.Lavalink
{
    public enum LavalinkRoutePlannerType
    {
        /// <summary>
        /// Route planner that switches the IP on ban.
        /// </summary>
        RotatingIpRoutePlanner = 1,

        /// <summary>
        /// Route planner that selects random IP addresses from the given block.
        /// </summary>
        BalancingIpRoutePlanner = 2,

        /// <summary>
        /// Route planner that switches the IP on every clock update.
        /// </summary>
        NanoIpRoutePlanner = 3,

        /// <summary>
        /// Route planner that switches the IP on every clock update and rotates to next IP block on a ban as a fallback.
        /// </summary>
        RotatingNanoIpRoutePlanner = 4
    }
}
