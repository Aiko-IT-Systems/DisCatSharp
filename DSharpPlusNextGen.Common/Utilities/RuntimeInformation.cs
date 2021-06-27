// This file is part of DSharpPlusNextGen.Common project
//
// Copyright 2020 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DSharpPlusNextGen.Common.Utilities
{
    /// <summary>
    /// Gets information about current runtime.
    /// </summary>
    public static class RuntimeInformation
    {
        /// <summary>
        /// Gets the current runtime's version.
        /// </summary>
        public static string Version { get; }

        static RuntimeInformation()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var mscorlib = loadedAssemblies.Select(x => new { Assembly = x, AssemblyName = x.GetName() })
                .FirstOrDefault(x => x.AssemblyName.Name == "mscorlib"
#if NETCOREAPP || NETSTANDARD
                     || x.AssemblyName.Name == "System.Private.CoreLib"
#endif
                );

#if NETCOREAPP || NETSTANDARD
            var location = mscorlib.Assembly.Location;
            var assemblyFile = new FileInfo(location);
            var versionFile = new FileInfo(Path.Combine(assemblyFile.Directory.FullName, ".version"));
            if (versionFile.Exists)
            {
                var lines = File.ReadAllLines(versionFile.FullName, new UTF8Encoding(false));

                if (lines.Length >= 2)
                {
                    Version = lines[1];
                    return;
                }
            }
#endif

            var infVersion = mscorlib.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (infVersion != null)
            {
                var infVersionString = infVersion.InformationalVersion;
                if (!string.IsNullOrWhiteSpace(infVersionString))
                {
                    Version = infVersionString.Split(' ').First();
                    return;
                }
            }

            Version = mscorlib.AssemblyName.Version.ToString();
        }
    }
}
