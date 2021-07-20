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

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.CommandsNext.Converters
{
    /// <summary>
    /// Represents the help formatter factory.
    /// </summary>
    internal class HelpFormatterFactory
    {
        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        private ObjectFactory Factory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpFormatterFactory"/> class.
        /// </summary>
        public HelpFormatterFactory() { }

        /// <summary>
        /// Sets the formatter type.
        /// </summary>
        public void SetFormatterType<T>() where T : BaseHelpFormatter => this.Factory = ActivatorUtilities.CreateFactory(typeof(T), new[] { typeof(CommandContext) });

        /// <summary>
        /// Creates the help formatter.
        /// </summary>
        /// <param name="ctx">The command context.</param>
        public BaseHelpFormatter Create(CommandContext ctx) => this.Factory(ctx.Services, new object[] { ctx }) as BaseHelpFormatter;
    }
}
