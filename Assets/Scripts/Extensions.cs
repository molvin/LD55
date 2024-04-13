using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class Extensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
