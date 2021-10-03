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
using System.Collections.Generic;
using System.Linq;
using DisCatSharp.Common.Configuration;
using DisCatSharp.Common.Configuration.Models;
using Microsoft.Extensions.Configuration;

namespace DisCatSharp.Hosting
{
    internal struct ExtensionConfigResult
    {
        public string Name { get; set; }
        public DisCatSharp.Common.Configuration.Models.ConfigSection Section { get; set; }
        public Type ConfigType { get; set; }
        public Type ImplementationType { get; set; }
    }

    internal static class ConfigurationExtensions
    {
        private const string LibName = "DisCatSharp";

        /// <summary>
        /// Easily identify which configuration types have been added to the <paramref name="configuration"/> <br/>
        /// This way we can dynamically load extensions without explicitly doing so
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="rootName"></param>
        /// <returns>Dictionary </returns>
        public static Dictionary<string, ExtensionConfigResult> FindImplementedConfigs(this IConfiguration configuration,
            string rootName = "DisCatSharp")
        {
            if (string.IsNullOrEmpty(rootName))
                throw new ArgumentNullException(nameof(rootName), "Root name must be provided");

            Dictionary<string, ExtensionConfigResult> results = new();

            // Assemblies managed by DisCatSharp
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName != null && x.FullName.StartsWith(LibName));

            foreach (var assembly in assemblies)
            {
                ExtensionConfigResult result = new();

                foreach (var type in assembly.ExportedTypes
                    .Where(x => x.Name.EndsWith("Configuration") && !x.IsAbstract && !x.IsInterface))
                {

                    // Strip name to be whatever prefix
                    result.Name = type.Name
                        .Replace("Configuration", "");

                    result.ConfigType = type;

                    if (!string.IsNullOrEmpty(configuration[configuration.ConfigPath(rootName, type.Name)]))
                        result.Section = new ConfigSection(ref configuration, type.Name, rootName);
                    else if (!string.IsNullOrEmpty(configuration[configuration.ConfigPath(rootName, result.Name)]))
                        result.Section = new ConfigSection(ref configuration, result.Name, rootName);

                    /*
                        In the event a user has some "fluff" between prefix and suffix we'll
                        just check for beginning and ending values.

                        Type should not be an interface or abstract
                     */

                    var implementationType = assembly.ExportedTypes.FirstOrDefault(x =>
                        !x.IsAbstract && !x.IsInterface && x.Name.StartsWith(result.Name) &&
                        x.Name.EndsWith("Extension"));

                    if (implementationType != null)
                    {
                        results.Add(result.Name, result);
                        result.ImplementationType = implementationType;
                    }
                }
            }

            return results;
        }
    }
}
