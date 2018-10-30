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
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < oct; i++)
        {
            total += Mathf.PerlinNoise((x) * frequency, (y) * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }
        //Brings result into the range of 0 to 1
        return total / maxValue;
    }
}
