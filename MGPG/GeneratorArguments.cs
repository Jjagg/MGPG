// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.IO;

namespace MGPG
{
    public class GeneratorArguments
    {

        /// <summary>
        /// The folder in which to place the rendered files.
        /// </summary>
        public string DestinationFolder { get; set; }

        /// <summary>
        /// The path to the solution to add any rendered .csproj files to.
        /// If the solution does not exist yet it will be created.
        /// Set to <code>null</code> to not add the project to a solution.
        /// </summary>
        public string Solution { get; set; }

        public Dictionary<string, string> Variables { get; set; }

        public SourceLanguage SourceLanguage { get; set; }

        public GeneratorArguments()
        {
            Variables = new Dictionary<string, string>();
            SourceLanguage = SourceLanguage.CSharp;
        }
    }
}