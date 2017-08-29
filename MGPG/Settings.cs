// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Linq;

namespace MGPG
{
    public class Settings
    {
        /// <summary>
        /// Set this flag to run the generator even when the <see cref="DestinationFolder"/> exists and is not empty.
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Set this flag to log errors instead of throwing a <see cref="GeneratorException"/>.
        /// </summary>
        public bool SupressErrors { get; set; }

        /// <summary>
        /// Log message of lower priority than this level will not get logged. If set to <see cref="MGPG.LogLevel.None"/> logging is disabled.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// The output stream to write log messages to. Defaults to <see cref="System.Console.Out"/>.
        /// </summary>
        public TextWriter LogWriter { get; set; }

        public Settings()
        {
            SupressErrors = false;
            LogLevel = LogLevel.Info;
        }
   }
}