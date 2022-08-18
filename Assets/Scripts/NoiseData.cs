using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseData
{
    public static float PerlinNoise2D(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / GameData.ChunkSize * scale + offset, 
                                 (position.y + 0.1f) / GameData.ChunkSize * scale + offset);
    }

    public static bool PerlinNoise3D(Vector3 position, List<Octave> octaves, float threshold)
    {
        float value = 0;
        for (int i = 0; i < octaves.Count; i++)
        {
            float x = (position.x + octaves[i].Offset.x + 0.1f) * octaves[i].Scale;
            float y = (position.y + octaves[i].Offset.y + 0.1f) * octaves[i].Scale;
            float z = (position.z + octaves[i].Offset.z + 0.1f) * octaves[i].Scale;

            float AB = Mathf.PerlinNoise(x, y);
            float BC = Mathf.PerlinNoise(y, z);
            float AC = Mathf.PerlinNoise(x, z);
            float BA = Mathf.PerlinNoise(y, x);
            float CB = Mathf.PerlinNoise(z, y);
            float CA = Mathf.PerlinNoise(z, x);

            value += (AB + BC + AC + BA + CB + CA) / 6;
        }

        value /= octaves.Count;

        return (value > threshold);       
    }
}
