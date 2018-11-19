using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {

    /// <summary>
    /// Returns a height value using the Fractal Brownian Motion Algorithm. This uses Perlin Noise.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="oct"></param>
    /// <param name="persistance"></param>
    /// <param name="offsetx"></param>
    /// <param name="offsety"></param>
    /// <returns></returns>
	public static float fBM(float x, float y, int oct, float persistance)
    {
        float total         = 0;
        float frequency     = 1;
        float amplitude     = 1;
        float maxValue      = 0;

        for (int i = 0; i < oct; i++)
        {
            total           += Mathf.PerlinNoise((x) * frequency, (y) * frequency) * amplitude;
            maxValue        += amplitude;
            amplitude       *= persistance;
            frequency       *= 2;
        }
        //Brings result into the range of 0 to 1
        return total / maxValue;
    }

    /// <summary>
    /// Maps a value to a target range based on its original range.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="originalMin"></param>
    /// <param name="originalMax"></param>
    /// <param name="targetMin"></param>
    /// <param name="targetMax"></param>
    /// <returns></returns>
    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }

    public static System.Random r = new System.Random(); //Random object for Shuffle function
    /// <summary>
    /// Fisher-Yates Shuffle. Shuffles the order of a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n       = list.Count;
        while(n > 1)
        {
            n--;
            int k   = r.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
