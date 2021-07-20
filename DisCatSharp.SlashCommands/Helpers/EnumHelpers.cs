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
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace DisCatSharp.SlashCommands
{
    /// <summary>
    /// Defines some extension methods for enums.
    /// </summary>
    public static class EnumHelpers
    {
        /// <summary>
        /// Gets the name from the <see cref="ChoiceNameAttribute"/> for this enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e"></param>
        /// <returns>The name.</returns>
        public static string GetName<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                var type = e.GetType();
                var values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));

                        return memInfo[0]
                            .GetCustomAttributes(typeof(ChoiceNameAttribute), false)
                            .FirstOrDefault() is ChoiceNameAttribute nameAttribute ? nameAttribute.Name : type.GetEnumName(val);
                    }
                }
            }
            return null;
        }
    }
}
