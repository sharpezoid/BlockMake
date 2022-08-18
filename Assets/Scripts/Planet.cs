using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public int Seed;
    public Biome Biome;

    public Transform player;
    public Vector3 spawn;
    public Vector3 Center;

    public Material material;
    public List<Block> Blocks = new List<Block>();

    public Chunk[,,] Chunks = new Chunk[GameData.PlanetSizeInChunks, GameData.PlanetSizeInChunks, GameData.PlanetSizeInChunks];
    public List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord PlayerChunkCoord;
    public ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> ChunksToCreate = new List<ChunkCoord>();
    private bool isCreatingChunks;

    int radius;

    private void Start()
    {
        Random.InitState(Seed);

        radius = (GameData.PlanetSizeInBlocks) / 2;

        Center = new Vector3(radius, radius, radius);

        spawn = new Vector3(radius,2 * radius + 10.0f, radius);

        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.transform.position);

    }

    private void Update()
    {
        PlayerChunkCoord = GetChunkCoordFromVector3(player.position);

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!PlayerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (ChunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine("CreateChunks");
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / GameData.ChunkSize);
        int y = Mathf.FloorToInt(pos.y / GameData.ChunkSize);
        int z = Mathf.FloorToInt(pos.z / GameData.ChunkSize);
        return new ChunkCoord(x, y, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / GameData.ChunkSize);
        int y = Mathf.FloorToInt(pos.y / GameData.ChunkSize);
        int z = Mathf.FloorToInt(pos.z / GameData.ChunkSize);
        if (!IsChunkInWorld(x, y, z))
        {
            return null;
        }
        return Chunks[x, y, z];
    }

    private void GenerateWorld()
    {
        for (int x = GameData.PlanetSizeInChunks / 2 - GameData.ViewDistanceInChunks / 2; x < GameData.PlanetSizeInChunks / 2 + GameData.ViewDistanceInChunks / 2; x++)
        {
            for (int y = GameData.PlanetSizeInChunks / 2 - GameData.ViewDistanceInChunks / 2; y < GameData.PlanetSizeInChunks / 2 + GameData.ViewDistanceInChunks / 2; y++)
            {
                for (int z = GameData.PlanetSizeInChunks / 2 - GameData.ViewDistanceInChunks / 2; z < GameData.PlanetSizeInChunks / 2 + GameData.ViewDistanceInChunks / 2; z++)
                {
                    Chunks[x,y,z] = new Chunk(new ChunkCoord(x,y,z), this, true);
                    activeChunks.Add(new ChunkCoord(x,y,z));
                }
            }
        }

        player.position = spawn;
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (ChunksToCreate.Count > 0)
        {
            Chunks[ChunksToCreate[0].x, ChunksToCreate[0].y, ChunksToCreate[0].z].Init();
            ChunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    private void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = PlayerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - GameData.ViewDistanceInChunks; x <= coord.x + GameData.ViewDistanceInChunks; x++)
        {
            for (int y = coord.y - GameData.ViewDistanceInChunks; y <= coord.y + GameData.ViewDistanceInChunks; y++)
            {
                for (int z = coord.z - GameData.ViewDistanceInChunks; z <= coord.z + GameData.ViewDistanceInChunks; z++)
                {
                    // If the chunk is within the world bounds and it has not been created.
                    if (IsChunkInWorld(x, y, z))
                    {
                        // Check if it active, if not, activate it.
                        if (Chunks[x,y,z] == null)
                        {
                            Chunks[x,y,z] = new Chunk(new ChunkCoord(x,y,z), this, false);
                            ChunksToCreate.Add(new ChunkCoord(x,y,z));
                        }
                        else if (!Chunks[x,y,z].IsActive)
                        {
                            Chunks[x,y,z].IsActive = true;
                        }
                        activeChunks.Add(new ChunkCoord(x,y,z));

                    }

                    // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        if (previouslyActiveChunks[i].Equals(new ChunkCoord(x,y,z)))
                            previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            Chunks[c.x, c.y, c.z].IsActive = false;
           //activeChunks.Remove();
        }
    }

    public bool IsChunkInWorld(int x, int y, int z)
    {
        if (x >= 0 && x <= GameData.PlanetSizeInChunks - 1 
         && y >= 0 && y <= GameData.PlanetSizeInChunks - 1
         && z >= 0 && z <= GameData.PlanetSizeInChunks - 1)
            return true;
        else
            return false;
    }


    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x >= 0 && coord.x <= GameData.PlanetSizeInChunks - 1 
         && coord.y >= 0 && coord.y <= GameData.PlanetSizeInChunks - 1
         && coord.z >= 0 && coord.z <= GameData.PlanetSizeInChunks - 1)
            return true;
        else
            return
                false;

    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > GameData.ChunkSize)
            return false;

        if (Chunks[thisChunk.x, thisChunk.y, thisChunk.z] != null && Chunks[thisChunk.x, thisChunk.y, thisChunk.z].isVoxelMapPopulated)
            return Blocks[Chunks[thisChunk.x, thisChunk.y, thisChunk.z].GetVoxelFromGlobalVector3(pos)].IsSolid;

        return Blocks[GetVoxel(pos)].IsSolid;

    }


    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < GameData.PlanetSizeInBlocks
         && pos.y >= 0 && pos.y < GameData.PlanetSizeInBlocks 
         && pos.z >= 0 && pos.z < GameData.PlanetSizeInBlocks)
            return true;
        else
            return false;
    }

    //private void CreateChunk(ChunkCoord coord)
    //{
    //    //Debug.Log("Create Chunk at Coord  : " + coord.ToString());
    //    Chunks[coord.x, coord.y, coord.z] = new Chunk(new ChunkCoord(coord.x, coord.y, coord.z), this);
    //    activeChunks.Add(new ChunkCoord(coord.x, coord.y, coord.z));
    //}

    public byte GetVoxel(Vector3 pos)
    {
        // distance from centre of the planet EXPENSIVE :DDDD
        //int yDist = Mathf.RoundToInt(Vector3.Distance(pos, spawn));

        float yDist = Vector3.Distance(pos, Center) / radius;

        //Debug.Log("Y Dist = " + yDist + "   spawn =  " + spawn);
        //
        //  IMMUTABLE PASS
        //
        // air
        if (!IsVoxelInWorld(pos) || yDist > 1.0f)
            return 0;

        // planet core in a few stages
        if (yDist < 0.2f)
            return 1;
        if (yDist < 0.35f)
            return 2;
        if (yDist < 0.6f)
            return 3;
 
        // basic terrain
        byte value = 0;

        if (yDist > 0.975f && yDist < 1.0f) //grass
            value = 4;
        else if (yDist < 0.975f && yDist > 0.9f) // dirt
            value = 5;
        else
            value = 6; // else solid stone

        if (value > 3)
        {
            foreach (Lode lode in Biome.Lodes)
            {
                if (lode.LodePatternType == Lode.eLodePatternType.Spherical)
                {
                    if (yDist > lode.MinHeight && yDist < lode.MaxHeight)
                    {
                        if (NoiseData.PerlinNoise3D(pos, lode.Octaves, lode.Threshold))
                            value = lode.BlockID;
                    }
                }
            }
        }

        return value;
    }
}