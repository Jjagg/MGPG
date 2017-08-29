// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MGPG
{
    public class RenderEventArgs
    {
        public string Source { get; }
        public string Destination { get; }

        internal RenderEventArgs(string source, string destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}