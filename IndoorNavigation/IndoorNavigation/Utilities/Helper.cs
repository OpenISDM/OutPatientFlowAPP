using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Utilities
{
    public static class Helper
    {
        public static bool isEmptyGuid(Guid uuid)
        {
            return Guid.Empty.Equals(uuid);
        }

        public static bool isSameGuid(Guid uuid1, Guid uuid2)
        {
            return uuid1.Equals(uuid2);
        }

        public static double GetPercentage(int current, int total)
        {
            return (double)Math.Round(100 * ((decimal)current /
                               (total)), 3);
        }
    }
}
