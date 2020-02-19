using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CellexalVR.MarchingCubes
{

    public static class Helper
    {
        public static int Pow(this int bas, int exp)
        {
            return Enumerable
                  .Repeat(bas, exp)
                  .Aggregate(1, (a, b) => a * b);
        }

        public static float Dist(int x, int y, int z, int i, int j, int k)
        {
            return Mathf.Sqrt(Mathf.Pow(x - i, 2) + Mathf.Pow(y - j, 2) + Mathf.Pow(z - k, 2));
        }

    }
}
