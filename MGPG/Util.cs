// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace MGPG
{
    public static class Util
    {
        public static bool IsTrue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return false;
            if (value.Equals("0", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }
    }
}