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
using System.Reflection;
using DisCatSharp.Configuration.Models;
using Microsoft.Extensions.Configuration;

namespace DisCatSharp.Configuration
{
    internal static class ConfigurationExtensions
    {
        private const string FactoryErrorMessage = "Require a function which provides a default entity to work with";
        public const string DefaultRootLib = "DisCatSharp";

        /// <summary>
        /// Easily piece together paths that will work within <see cref="IConfiguration"/>
        /// </summary>
        /// <param name="config">(not used - only for adding context based functionality)</param>
        /// <param name="values">The strings to piece together</param>
        /// <returns>Strings joined together via ':'</returns>
        public static string ConfigPath(this IConfiguration config, params string[] values) => string.Join(":", values);

        /// <summary>
        /// Skims over the configuration section and only overrides values that are explicitly defined within the config
        /// </summary>
        /// <param name="config">Instance of config</param>
        /// <param name="section">Section which contains values for <paramref name="config"/></param>
        private static void HydrateInstance(ref object config, ConfigSection section)
        {
            PropertyInfo[] props = config.GetType().GetProperties();

            foreach (var prop in props)
                // If found in the config -- user/dev wants to override default value
                if (section.ContainsKey(prop.Name, out string path))
                {
                    // Must have a set method for this to work, otherwise continue on
                    if (prop.SetMethod == null)
                        continue;

                    string entry = section.GetValue(path);

                    try
                    {
                        object? value = null;

                        // Primitive types are simple to convert
                        if (prop.PropertyType.IsPrimitive)
                            value = Convert.ChangeType(entry, prop.PropertyType);
                        else if (prop.PropertyType == typeof(string))
                            value = entry;
                        else
                        {
                            // The following types require a different approach
                            if (prop.PropertyType.IsEnum)
                                value = Enum.Parse(prop.PropertyType, entry);
                            else if (typeof(TimeSpan) == prop.PropertyType)
                                value = TimeSpan.Parse(entry);
                            else if (typeof(DateTime) == prop.PropertyType)
                                value = DateTime.Parse(entry);
                            else if (typeof(DateTimeOffset) == prop.PropertyType)
                                value = DateTimeOffset.Parse(entry);
                        }

                        // Update value within our config instance
                        prop.SetValue(config, value);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Unable to convert value of '{entry}' to type '{prop.PropertyType.Name}' for prop '{prop.Name}' in config '{config.GetType().Name}'\n\t\t{ex.Message}");
                    }
                }
        }

        /// <summary>
        /// Instantiate an entity using <paramref name="factory"/> then walk through the specified <paramref name="section"/>
        /// and translate user-defined config values to the instantiated instance from <paramref name="factory"/>
        /// </summary>
        /// <param name="section">Section containing values for targeted config</param>
        /// <param name="factory">Function which generates a default entity</param>
        /// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="factory"/> is null</exception>
        public static object ExtractConfig(this ConfigSection section, Func<object> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory),FactoryErrorMessage);

            // Create default instance
            object config = factory();

            HydrateInstance(ref config, section);

            return config;
        }

        /// <summary>
        /// Instantiate an entity using <paramref name="factory"/> then walk through the specified <paramref name="sectionName"/>
        /// in <paramref name="config"/>. Translate user-defined config values to the instantiated instance from <paramref name="factory"/>
        /// </summary>
        /// <param name="config">Loaded App Configuration</param>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="factory">Function which creates a default entity to work with</param>
        /// <param name="rootSectionName">(Optional) Used when section is nested within another. Default value is <see cref="DefaultRootLib"/></param>
        /// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="factory"/> is null</exception>
        public static object ExtractConfig(this IConfiguration config, string sectionName, Func<object> factory,
            string? rootSectionName = DefaultRootLib)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory), FactoryErrorMessage);

            // create default instance
            object instance = factory();

            HydrateInstance(ref instance, new ConfigSection(ref config, sectionName, rootSectionName));

            return instance;
        }

        /// <summary>
        /// Instantiate a new instance of <typeparamref name="TConfig"/>, then walk through the specified <paramref name="sectionName"/>
        /// in <paramref name="config"/>. Translate user-defined config values to the <typeparamref name="TConfig"/> instance.
        /// </summary>
        /// <param name="config">Loaded App Configuration</param>
        /// <param name="sectionName">Name of section to load</param>
        /// <param name="rootSectionName">(Optional) Used when section is nested with another. Default value is <see cref="DefaultRootLib"/></param>
        /// <typeparam name="TConfig">Type of instance that <paramref name="sectionName"/> represents</typeparam>
        /// <returns>Hydrated instance of <typeparamref name="TConfig"/> which contains the user-defined values (if any).</returns>
        public static TConfig ExtractConfig<TConfig>(this IConfiguration config, string sectionName, string? rootSectionName = DefaultRootLib)
            where TConfig : new()
        {
            // Default values should hopefully be provided from the constructor
            object configInstance = new TConfig();

            HydrateInstance(ref configInstance, new ConfigSection(ref config, sectionName, rootSectionName));

            return (TConfig) configInstance;
        }
    }
}
