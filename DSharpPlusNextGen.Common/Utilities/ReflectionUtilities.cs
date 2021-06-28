// This file is a part of DSharpPlusNextGen.Common project.
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DSharpPlusNextGen.Common.Utilities
{
    /// <summary>
    /// Contains various utilities for use with .NET's reflection.
    /// </summary>
    public static class ReflectionUtilities
    {
        /// <summary>
        /// <para>Creates an empty, uninitialized instance of specified type.</para>
        /// <para>This method will not call the constructor for the specified type. As such, the object might not be properly initialized.</para>
        /// </summary>
        /// <remarks>
        /// This method is intended for reflection use only.
        /// </remarks>
        /// <param name="t">Type of the object to instantiate.</param>
        /// <returns>Empty, uninitialized object of specified type.</returns>
        public static object CreateEmpty(this Type t)
            => FormatterServices.GetUninitializedObject(t);

        /// <summary>
        /// <para>Creates an empty, uninitialized instance of type <typeparamref name="T"/>.</para>
        /// <para>This method will not call the constructor for type <typeparamref name="T"/>. As such, the object might not be proerly initialized.</para>
        /// </summary>
        /// <remarks>
        /// This method is intended for reflection use only.
        /// </remarks>
        /// <typeparam name="T">Type of the object to instantiate.</typeparam>
        /// <returns>Empty, uninitalized object of specified type.</returns>
        public static T CreateEmpty<T>()
            => (T)FormatterServices.GetUninitializedObject(typeof(T));

        /// <summary>
        /// Converts a given object into a dictionary of property name to property value mappings.
        /// </summary>
        /// <typeparam name="T">Type of object to convert.</typeparam>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Converted dictionary.</returns>
        public static IReadOnlyDictionary<string, object> ToDictionary<T>(this T obj)
        {
            if (obj == null)
                throw new NullReferenceException();

            return new CharSpanLookupReadOnlyDictionary<object>(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => new KeyValuePair<string, object>(x.Name, x.GetValue(obj))));
        }
    }
}
