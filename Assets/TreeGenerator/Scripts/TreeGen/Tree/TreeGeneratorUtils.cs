using System.Collections.Generic;
using UnityEngine.Splines;

namespace TreeGeneration
{
    public static class TreeGeneratorUtils
    {
        public static int FindIndex(this IReadOnlyList<Spline> splines, Spline spline)
        {
            for (int i = 0; i < splines.Count; i++)
            {
                if (splines[i] == spline)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}