using System;

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