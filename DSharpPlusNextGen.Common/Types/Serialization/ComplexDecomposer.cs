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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DSharpPlusNextGen.Common.Serialization
{
    /// <summary>
    /// Decomposes <see cref="Complex"/> numbers into tuples (arrays of 2).
    /// </summary>
    public sealed class ComplexDecomposer : IDecomposer
    {
        private static Type TComplex { get; } = typeof(Complex);
        private static Type TDoubleArray { get; } = typeof(double[]);
        private static Type TDoubleEnumerable { get; } = typeof(IEnumerable<double>);
        private static Type TObjectArray { get; } = typeof(object[]);
        private static Type TObjectEnumerable { get; } = typeof(IEnumerable<object>);

        /// <inheritdoc />
        public bool CanDecompose(Type t)
            => t == TComplex;

        /// <inheritdoc />
        public bool CanRecompose(Type t)
            => t == TDoubleArray
            || t == TObjectArray
            || TDoubleEnumerable.IsAssignableFrom(t)
            || TObjectEnumerable.IsAssignableFrom(t);

        /// <inheritdoc />
        public bool TryDecompose(object obj, Type tobj, out object decomposed, out Type tdecomposed)
        {
            decomposed = null;
            tdecomposed = TDoubleArray;

            if (tobj != TComplex || obj is not Complex c)
                return false;

            decomposed = new[] { c.Real, c.Imaginary };
            return true;
        }

        /// <inheritdoc />
        public bool TryRecompose(object obj, Type tobj, Type trecomposed, out object recomposed)
        {
            recomposed = null;

            if (trecomposed != TComplex)
                return false;

            // ie<double>
            if (TDoubleEnumerable.IsAssignableFrom(tobj) && obj is IEnumerable<double> ied)
            {
                if (ied.Count() < 2)
                    return false;

                var (real, imag) = ied.FirstTwoOrDefault();
                recomposed = new Complex(real, imag);
                return true;
            }

            // ie<obj>
            if (TObjectEnumerable.IsAssignableFrom(tobj) && obj is IEnumerable<object> ieo)
            {
                if (ieo.Count() < 2)
                    return false;

                var (real, imag) = ieo.FirstTwoOrDefault();
                if (real is not double dreal || imag is not double dimag)
                    return false;

                recomposed = new Complex(dreal, dimag);
                return true;
            }

            return false;
        }
    }
}
