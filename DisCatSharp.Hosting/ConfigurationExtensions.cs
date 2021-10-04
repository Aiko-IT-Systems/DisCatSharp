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
using DisCatSharp.Configuration;
using DisCatSharp.Configuration.Models;
using Microsoft.Extensions.Configuration;

namespace DisCatSharp.Hosting
{
    internal struct ExtensionConfigResult
    {
        public ConfigSection Section { get; set; }
        public Type ConfigType { get; set; }
        public Type ImplementationType { get; set; }
    }

    internal static class ConfigurationExtensions
    {
        public static bool HasSection(this IConfiguration config, params string[] values)
        {
            // We require something to be passed in
            if (!values.Any())
                return false;

            Queue<string> queue = new(values);
            IConfigurationSection section = config.GetSection(queue.Dequeue());

            while (section != null && queue.Any())
                config.GetSection(queue.Dequeue());

            return section != null;
        }

        /// <summary>
        /// Easily identify which configuration types have been added to the <paramref name="configuration"/> <br/>
        /// This way we can dynamically load extensions without explicitly doing so
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="rootName"></param>
        /// <returns>Dictionary where Key -> Name of implemented type<br/>Value -> <see cref="ExtensionConfigResult"/></returns>
        public static Dictionary<string, ExtensionConfigResult> FindImplementedExtensions(this IConfiguration configuration,
            string rootName = Configuration.ConfigurationExtensions.DefaultRootLib)
        {
            if (string.IsNullOrEmpty(rootName))
                throw new ArgumentNullException(nameof(rootName), "Root name must be provided");

            Dictionary<string, ExtensionConfigResult> results = new();

            // Assemblies managed by DisCatSharp
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName != null && x.FullName.StartsWith(Configuration.ConfigurationExtensions.DefaultRootLib));

            foreach (var assembly in assemblies)
            {
                ExtensionConfigResult result = new();

                foreach (var type in assembly.ExportedTypes
                    .Where(x => x.Name.EndsWith(Constants.ConfigSuffix) && !x.IsAbstract && !x.IsInterface))
                {
                    string sectionName = type.Name;
                    string prefix = type.Name.Replace(Constants.ConfigSuffix, "");

                    result.ConfigType = type;

                    // Does a section exist with the classname? (DiscordConfiguration - for instance)
                    if(configuration.HasSection(rootName, sectionName))
                        result.Section = new ConfigSection(ref configuration, type.Name, rootName);

                    // Does a section exist with the classname minus Configuration? (Discord - for Instance)
                    else if (configuration.HasSection(rootName, prefix))
                        result.Section = new ConfigSection(ref configuration, prefix, rootName);

                    // We require the implemented type to exist so we'll continue onward
                    else
                        continue;

                    /*
                        Now we need to find the type which should consume our config

                        In the event a user has some "fluff" between prefix and suffix we'll
                        just check for beginning and ending values.

                        Type should not be an interface or abstract, should also be assignable to BaseExtension
                     */

                    var implementationType = assembly.ExportedTypes.FirstOrDefault(x =>
                        !x.IsAbstract && !x.IsInterface && x.Name.StartsWith(prefix) &&
                        x.Name.EndsWith(Constants.ExtensionSuffix) && x.IsAssignableTo(typeof(BaseExtension)));

                    // If the implementation type was found we can add it to our result set
                    if (implementationType != null)
                    {
                        result.ImplementationType = implementationType;
                        results.Add(implementationType.Name, result);
                    }
                }
            }

            return results;
        }
    }
}
