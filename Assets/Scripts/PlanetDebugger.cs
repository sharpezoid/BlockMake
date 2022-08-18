using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDebugger : MonoBehaviour
{
    Planet planet;

    private void Awake()
    {
       planet = GetComponent<Planet>();
    }

    private void OnDrawGizmos()
    {
        if (planet)
        {
            Gizmos.color = Color.red;
            for (int x = 0; x < GameData.PlanetSizeInBlocks - 1; x++)
            {
                for (int y = 0; y < GameData.PlanetSizeInBlocks - 1; y++)
                {
                    for (int z = 0; z < GameData.PlanetSizeInBlocks - 1; z++)
                    {
                        if (planet.IsChunkInWorld(x, y, z) )//&& planet.chunks[x,y,z] != null)
                        {
                            Gizmos.DrawWireCube(new Vector3(x * GameData.ChunkSize,
                                                            y * GameData.ChunkSize, 
                                                            z * GameData.ChunkSize),
                                new Vector3(GameData.ChunkSize, 
                                            GameData.ChunkSize, 
                                            GameData.ChunkSize));
                        }
                    }
                }
            }

            Gizmos.color = Color.green;
            foreach(ChunkCoord c in planet.activeChunks)
            {
                Gizmos.DrawWireCube(c.ToWorldPosition(), GameData.ChunkSizeVector);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(planet.transform.position, new Vector3(GameData.PlanetSizeInBlocks,
                GameData.PlanetSizeInBlocks, GameData.PlanetSizeInBlocks));
        }
    }
}
