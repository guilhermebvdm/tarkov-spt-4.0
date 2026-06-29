using System;
using System.Collections.Generic;
using System.Linq;

namespace MOARServer.Helpers;

public static class WaveUtils
{
    private static readonly Random Rnd = new Random();

    public static List<T> Shuffle<T>(List<T> list)
    {
        var result = new List<T>(list);
        int n = result.Count;
        while (n > 1)
        {
            n--;
            int k = Rnd.Next(n + 1);
            (result[k], result[n]) = (result[n], result[k]);
        }
        return result;
    }
}
