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

using System;
using System.Threading.Tasks;
using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters
{
    /// <summary>
    /// Represents a nullable converter.
    /// </summary>
    public class NullableConverter<T> : IArgumentConverter<Nullable<T>> where T : struct
    {
        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="Value">The string to convert.</param>
        /// <param name="Ctx">The command context.</param>
        async Task<Optional<Nullable<T>>> IArgumentConverter<Nullable<T>>.Convert(string Value, CommandContext Ctx)
        {
            if (!Ctx.Config.CaseSensitive)
                Value = Value.ToLowerInvariant();

            if (Value == "null")
                return Optional.FromValue<Nullable<T>>(null);

            if (Ctx.CommandsNext.ArgumentConverters.TryGetValue(typeof(T), out var cv))
            {
                var cvx = cv as IArgumentConverter<T>;
                var val = await cvx.Convert(Value, Ctx).ConfigureAwait(false);
                return val.HasValue ? Optional.FromValue<Nullable<T>>(val.Value) : Optional.FromNoValue<Nullable<T>>();
            }

            return Optional.FromNoValue<Nullable<T>>();
        }
    }
}
