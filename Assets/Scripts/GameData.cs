using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static readonly int PlanetSizeInChunks = 6;
    public static readonly int ChunkSize = 32;
    public static Vector3 ChunkSizeVector
    {
        get { return new Vector3(ChunkSize, ChunkSize, ChunkSize); }
    }
    public static readonly int ViewDistanceInChunks = 2;
    public static int PlanetSizeInBlocks
    {
        get { return PlanetSizeInChunks * ChunkSize; }
    }

    public static readonly int TextureAtlasSizeInBlocks = 8;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }
}
