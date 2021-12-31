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
using System.Collections;
using System.Linq;
using DisCatSharp.Configuration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Configuration
{
    /// <summary>
    /// The configuration extensions.
    /// </summary>
    internal static class ConfigurationExtensions
    {
        /// <summary>
        /// The factory error message.
        /// </summary>
        private const string FactoryErrorMessage = "Require a function which provides a default entity to work with";
        /// <summary>
        /// The default root lib.
        /// </summary>
        public const string DefaultRootLib = "DisCatSharp";
        /// <summary>
        /// The config suffix.
        /// </summary>
        private const string ConfigSuffix = "Configuration";


        /// <summary>
        /// Easily piece together paths that will work within <see cref="IConfiguration"/>
        /// </summary>
        /// <param name="Config">(not used - only for adding context based functionality)</param>
        /// <param name="Values">The strings to piece together</param>
        /// <returns>Strings joined together via ':'</returns>
        public static string ConfigPath(this IConfiguration Config, params string[] Values) => string.Join(":", Values);

        /// <summary>
        /// Skims over the configuration section and only overrides values that are explicitly defined within the config
        /// </summary>
        /// <param name="Config">Instance of config</param>
        /// <param name="Section">Section which contains values for <paramref name="Config"/></param>
        private static void HydrateInstance(ref object Config, ConfigSection Section)
        {
            var props = Config.GetType().GetProperties();

            foreach (var prop in props)
            {
                // Must have a set method for this to work, otherwise continue on
                if (prop.SetMethod == null)
                    continue;

                var entry = Section.GetValue(prop.Name);
                object? value = null;

                if (typeof(string) == prop.PropertyType)
                {
                    // We do NOT want to override value if nothing was provided
                    if (!string.IsNullOrEmpty(entry))
                        prop.SetValue(Config, entry);

                    continue;
                }

                // We need to address collections a bit differently
                // They can come in the form of    "root:section:name" with a string representation OR
                // "root:section:name:0"  <--- this is not detectable when checking the above path
                if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    value = string.IsNullOrEmpty(Section.GetValue(prop.Name))
                        ? Section.Config
                            .GetSection(Section.GetPath(prop.Name)).Get(prop.PropertyType)
                        : Newtonsoft.Json.JsonConvert.DeserializeObject(entry, prop.PropertyType);

                    if (value == null)
                        continue;

                    prop.SetValue(Config, value);
                }

                // From this point onward we require the 'entry' value to have something useful
                if (string.IsNullOrEmpty(entry))
                    continue;

                try
                {
                    // Primitive types are simple to convert
                    if (prop.PropertyType.IsPrimitive)
                        value = Convert.ChangeType(entry, prop.PropertyType);
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
                    prop.SetValue(Config, value);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"Unable to convert value of '{entry}' to type '{prop.PropertyType.Name}' for prop '{prop.Name}' in config '{Config.GetType().Name}'\n\t\t{ex.Message}");
                }
            }
        }

        /// <summary>
        /// Instantiate an entity using <paramref name="Factory"/> then walk through the specified <paramref name="Section"/>
        /// and translate user-defined config values to the instantiated instance from <paramref name="Factory"/>
        /// </summary>
        /// <param name="Section">Section containing values for targeted config</param>
        /// <param name="Factory">Function which generates a default entity</param>
        /// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="Factory"/> is null</exception>
        public static object ExtractConfig(this ConfigSection Section, Func<object> Factory)
        {
            if (Factory == null)
                throw new ArgumentNullException(nameof(Factory), FactoryErrorMessage);

            // Create default instance
            var config = Factory();

            HydrateInstance(ref config, Section);

            return config;
        }

        /// <summary>
        /// Instantiate an entity using <paramref name="Factory"/> then walk through the specified <paramref name="SectionName"/>
        /// in <paramref name="Config"/>. Translate user-defined config values to the instantiated instance from <paramref name="Factory"/>
        /// </summary>
        /// <param name="Config">Loaded App Configuration</param>
        /// <param name="SectionName">Name of section to load</param>
        /// <param name="Factory">Function which creates a default entity to work with</param>
        /// <param name="RootSectionName">(Optional) Used when section is nested within another. Default value is <see cref="DefaultRootLib"/></param>
        /// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="Factory"/> is null</exception>
        public static object ExtractConfig(this IConfiguration Config, string SectionName, Func<object> Factory,
            string? RootSectionName = DefaultRootLib)
        {
            if (Factory == null)
                throw new ArgumentNullException(nameof(Factory), FactoryErrorMessage);

            // create default instance
            var instance = Factory();

            HydrateInstance(ref instance, new ConfigSection(ref Config, SectionName, RootSectionName));

            return instance;
        }

        /// <summary>
        /// Instantiate a new instance of <typeparamref name="TConfig"/>, then walk through the specified <paramref name="SectionName"/>
        /// in <paramref name="Config"/>. Translate user-defined config values to the <typeparamref name="TConfig"/> instance.
        /// </summary>
        /// <param name="Config">Loaded App Configuration</param>
        /// <param name="ServiceProvider"></param>
        /// <param name="SectionName">Name of section to load</param>
        /// <param name="RootSectionName">(Optional) Used when section is nested with another. Default value is <see cref="DefaultRootLib"/></param>
        /// <typeparam name="TConfig">Type of instance that <paramref name="SectionName"/> represents</typeparam>
        /// <returns>Hydrated instance of <typeparamref name="TConfig"/> which contains the user-defined values (if any).</returns>
        public static TConfig ExtractConfig<TConfig>(this IConfiguration Config, IServiceProvider ServiceProvider, string SectionName, string? RootSectionName = DefaultRootLib)
            where TConfig : new()
        {
            // Default values should hopefully be provided from the constructor
            object configInstance = ActivatorUtilities.CreateInstance(ServiceProvider, typeof(TConfig));

            HydrateInstance(ref configInstance, new ConfigSection(ref Config, SectionName, RootSectionName));

            return (TConfig)configInstance;
        }

        /// <summary>
        /// Instantiate a new instance of <typeparamref name="TConfig"/>, then walk through the specified <paramref name="SectionName"/>
        /// in <paramref name="Config"/>. Translate user-defined config values to the <typeparamref name="TConfig"/> instance.
        /// </summary>
        /// <param name="Config">Loaded App Configuration</param>
        /// <param name="SectionName">Name of section to load</param>
        /// <param name="RootSectionName">(Optional) Used when section is nested with another. Default value is <see cref="DefaultRootLib"/></param>
        /// <typeparam name="TConfig">Type of instance that <paramref name="SectionName"/> represents</typeparam>
        /// <returns>Hydrated instance of <typeparamref name="TConfig"/> which contains the user-defined values (if any).</returns>
        public static TConfig ExtractConfig<TConfig>(this IConfiguration Config, string SectionName, string? RootSectionName = DefaultRootLib)
            where TConfig : new()
        {
            // Default values should hopefully be provided from the constructor
            object configInstance = new TConfig();

            HydrateInstance(ref configInstance, new ConfigSection(ref Config, SectionName, RootSectionName));

            return (TConfig)configInstance;
        }

        /// <summary>
        /// Determines if <paramref name="Config"/> contains a particular section/object (not value)
        /// </summary>
        /// <remarks>
        /// <code>
        /// {
        ///    "Discord": {  // this is a section/object
        ///
        ///    },
        ///    "Value": "something" // this is not a section/object
        /// }
        /// </code>
        /// </remarks>
        /// <param name="Config"></param>
        /// <param name="Values"></param>
        /// <returns>True if section exists, otherwise false</returns>
        public static bool HasSection(this IConfiguration Config, params string[] Values)
        {
            if (!Values.Any())
                return false;

            if (Values.Length == 1)
                return Config.GetChildren().Any(X => X.Key == Values[0]);

            if (Config.GetChildren().All(X => X.Key != Values[0]))
                return false;

            var current = Config.GetSection(Values[0]);

            for (var i = 1; i < Values.Length - 1; i++)
            {
                if (current.GetChildren().All(X => X.Key != Values[i]))
                    return false;

                current = current.GetSection(Values[i]);
            }

            return current.GetChildren().Any(X => X.Key == Values[^1]);
        }

        /// <summary>
        /// Instantiates an instance of <see cref="DiscordClient"/>, then consumes any custom
        /// configuration from user/developer from <paramref name="Config"/>. <br/>
        /// View remarks for more info
        /// </summary>
        /// <remarks>
        /// This is an example of how your JSON structure should look if you wish
        /// to override one or more of the default values from <see cref="DiscordConfiguration"/>
        /// <code>
        /// {
        ///   "DisCatSharp": {
        ///      "Discord": { }
        ///   }
        /// }
        /// </code>
        /// <br/>
        /// Alternatively, you can use the type name itself
        /// <code>
        /// {
        ///   "DisCatSharp": {
        ///      "DiscordConfiguration": { }
        ///   }
        /// }
        /// </code>
        /// <code>
        /// {
        ///   "botSectionName": {
        ///      "DiscordConfiguration": { }
        ///   }
        /// }
        /// </code>
        /// </remarks>
        /// <param name="Config"></param>
        /// <param name="ServiceProvider"></param>
        /// <param name="BotSectionName"></param>
        /// <returns>Instance of <see cref="DiscordClient"/></returns>
        public static DiscordClient BuildClient(this IConfiguration Config, IServiceProvider ServiceProvider,
            string BotSectionName = DefaultRootLib)
        {
            var section = Config.HasSection(BotSectionName, "Discord")
                ? "Discord"
                : Config.HasSection(BotSectionName, $"Discord{ConfigSuffix}")
                    ? $"Discord:{ConfigSuffix}"
                    : null;

            return string.IsNullOrEmpty(section)
                ? new DiscordClient(new(ServiceProvider))
                : new DiscordClient(Config.ExtractConfig<DiscordConfiguration>(ServiceProvider, section, BotSectionName));
        }
    }
}
