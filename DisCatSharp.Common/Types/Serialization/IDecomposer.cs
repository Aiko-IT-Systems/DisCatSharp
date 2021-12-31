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

namespace DisCatSharp.Common.Serialization
{
    /// <summary>
    /// Provides an interface to decompose an object into another object or combination of objects.
    /// </summary>
    public interface IDecomposer
    {
        /// <summary>
        /// Checks whether the decomposer can decompose a specific type.
        /// </summary>
        /// <param name="T">Type to check.</param>
        /// <returns>Whether the decomposer can decompose a given type.</returns>
        bool CanDecompose(Type T);

        /// <summary>
        /// <para>Checks whether the decomposer can recompose a specific decomposed type.</para>
        /// <para>Note that while a type might be considered recomposable, other factors might prevent recomposing operation from being successful.</para>
        /// </summary>
        /// <param name="T">Decomposed type to check.</param>
        /// <returns>Whether the decomposer can decompose a given type.</returns>
        bool CanRecompose(Type T);

        /// <summary>
        /// Attempts to decompose a given object of specified source type. The operation produces the decomposed object and the type it got decomposed into.
        /// </summary>
        /// <param name="Obj">Object to decompose.</param>
        /// <param name="Tobj">Type to decompose.</param>
        /// <param name="Decomposed">Decomposition result.</param>
        /// <param name="Tdecomposed">Type of the result.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool TryDecompose(object Obj, Type Tobj, out object Decomposed, out Type Tdecomposed);

        /// <summary>
        /// Attempts to recompose given object of specified source type, into specified target type. The operation produces the recomposed object.
        /// </summary>
        /// <param name="Obj">Object to recompose from.</param>
        /// <param name="Tobj">Type of data to recompose.</param>
        /// <param name="Trecomposed">Type to recompose into.</param>
        /// <param name="Recomposed">Recomposition result.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool TryRecompose(object Obj, Type Tobj, Type Trecomposed, out object Recomposed);
    }
}
