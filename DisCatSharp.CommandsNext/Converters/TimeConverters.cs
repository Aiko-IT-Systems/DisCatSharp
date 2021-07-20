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

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters
{
    /// <summary>
    /// Represents a date time converter.
    /// </summary>
    public class DateTimeConverter : IArgumentConverter<DateTime>
    {
        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        Task<Optional<DateTime>> IArgumentConverter<DateTime>.ConvertAsync(string value, CommandContext ctx)
        {
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? Task.FromResult(new Optional<DateTime>(result))
                : Task.FromResult(Optional.FromNoValue<DateTime>());
        }
    }

    /// <summary>
    /// Represents a date time offset converter.
    /// </summary>
    public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
    {
        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        Task<Optional<DateTimeOffset>> IArgumentConverter<DateTimeOffset>.ConvertAsync(string value, CommandContext ctx)
        {
            return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
        }
    }

    /// <summary>
    /// Represents a time span converter.
    /// </summary>
    public class TimeSpanConverter : IArgumentConverter<TimeSpan>
    {
        /// <summary>
        /// Gets or sets the time span regex.
        /// </summary>
        private static Regex TimeSpanRegex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanConverter"/> class.
        /// </summary>
        static TimeSpanConverter()
        {
#if NETSTANDARD1_3
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript);
#else
            TimeSpanRegex = new Regex(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);
#endif
        }

        /// <summary>
        /// Converts a string.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="ctx">The command context.</param>
        Task<Optional<TimeSpan>> IArgumentConverter<TimeSpan>.ConvertAsync(string value, CommandContext ctx)
        {
            if (value == "0")
                return Task.FromResult(Optional.FromValue(TimeSpan.Zero));

            if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
                return Task.FromResult(Optional.FromValue(result));

            var gps = new string[] { "days", "hours", "minutes", "seconds" };
            var mtc = TimeSpanRegex.Match(value);
            if (!mtc.Success)
                return Task.FromResult(Optional.FromNoValue<TimeSpan>());

            var d = 0;
            var h = 0;
            var m = 0;
            var s = 0;
            foreach (var gp in gps)
            {
                var gpc = mtc.Groups[gp].Value;
                if (string.IsNullOrWhiteSpace(gpc))
                    continue;

                var gpt = gpc[gpc.Length - 1];
                int.TryParse(gpc.Substring(0, gpc.Length - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val);
                switch (gpt)
                {
                    case 'd':
                        d = val;
                        break;

                    case 'h':
                        h = val;
                        break;

                    case 'm':
                        m = val;
                        break;

                    case 's':
                        s = val;
                        break;
                }
            }
            result = new TimeSpan(d, h, m, s);
            return Task.FromResult(Optional.FromValue(result));
        }
    }
}
