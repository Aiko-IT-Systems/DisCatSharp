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

namespace DSharpPlusNextGen.Common.Serialization
{
    /// <summary>
    /// Specifies a decomposer for a given type or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class DecomposerAttribute : SerializationAttribute
    {
        /// <summary>
        /// Gets the type of the decomposer.
        /// </summary>
        public Type DecomposerType { get; }

        /// <summary>
        /// Specifies a decomposer for given type or property.
        /// </summary>
        /// <param name="type">Type of decomposer to use.</param>
        public DecomposerAttribute(Type type)
        {
            if (!typeof(IDecomposer).IsAssignableFrom(type) || !type.IsClass || type.IsAbstract) // abstract covers static - static = abstract + sealed
                throw new ArgumentException("Invalid type specified. Must be a non-abstract class which implements DSharpPlusNextGen.Common.Serialization.IDecomposer interface.", nameof(type));

            this.DecomposerType = type;
        }
    }
}
