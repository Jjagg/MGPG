// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MGPG
{
    /// <summary>
    /// The <see cref="Exception"/> thrown
    /// </summary>
    public class GeneratorException : Exception
    {
        public GeneratorException(string message) : base(message)
        {
        }

        public GeneratorException(string message, string file, int line, int col)
            : base(ToMessage(message, file, line, col))
        {
        }

        public GeneratorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public GeneratorException(string message, Exception innerException, string file, int line, int col)
            : base(ToMessage(message, file, line, col), innerException)
        {
        }

        private static string ToMessage(string message, string file, int line, int col)
        {
            return $"{file} ({line},{col}): {message}";
        }
    }
}