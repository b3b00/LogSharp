using System.Collections.Generic;

namespace logSharp.helpers
{
    public static class ListHelper
    {
        public static List<T> BuildList<T>(params T[] items)
        {
            return new List<T>(items);
        }
    }
}