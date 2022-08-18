using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    GameObject chunkObject;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] voxelMap = new byte[GameData.ChunkSize, GameData.ChunkSize, GameData.ChunkSize];

    Planet planet;
    private bool _isActive;
    public bool isVoxelMapPopulated = false;

    public Chunk(ChunkCoord _coord, Planet _planet, bool generateOnLoad)
    {
        coord = _coord;
        planet = _planet;
        IsActive = true;

        if (generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();

        meshRenderer.material = planet.material;


        chunkObject.transform.SetParent(planet.transform);
        chunkObject.transform.position = new Vector3(coord.x * GameData.ChunkSize, coord.y * GameData.ChunkSize, coord.z * GameData.ChunkSize);

        chunkObject.name = coord.ToString();

        PopulateVoxelMap();
        UpdateChunk();
        
       // CreateMeshData();
       // CreateMesh();

    }
    
    public bool IsActive
    {
        get { return _isActive; }
        set {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public Vector3 Position
    {
        get { return chunkObject.transform.position; }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > GameData.ChunkSize - 1 
         || y < 0 || y > GameData.ChunkSize - 1 
         || z < 0 || z > GameData.ChunkSize - 1)
            return false;
        else return true;
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < GameData.ChunkSize; y++)
        {
            for (int x = 0; x < GameData.ChunkSize; x++)
            {
                for (int z = 0; z < GameData.ChunkSize; z++)
                {
                    voxelMap[x, y, z] = planet.GetVoxel(new Vector3(x, y, z) + Position);
                }
            }
        }

        isVoxelMapPopulated = true;
    }

    void UpdateChunk()
    {
        ClearMeshData();

        for (int y = 0; y < GameData.ChunkSize; y++)
        {
            for (int x = 0; x < GameData.ChunkSize; x++)
            {
                for (int z = 0; z < GameData.ChunkSize; z++)
                {
                    if (planet.Blocks[voxelMap[x, y, z]].IsSolid)
                    {
                        UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        }

        CreateMesh();
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    public void CreateMeshData()
    {
        for (int y = 0; y < GameData.ChunkSize; y++)
        {
            for (int x = 0; x < GameData.ChunkSize; x++)
            { 
                for (int z = 0; z < GameData.ChunkSize; z++)
                {
                    if (planet.Blocks[voxelMap[x, y, z]].IsSolid)
                    {
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                    }
                }
            }
        }
    }

    void UpdateMeshData(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.Directions[p]))
            {
                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 0]]);
                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 1]]);
                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 2]]);
                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 3]]);

                AddTexture(planet.Blocks[blockID].GetTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;

            }
        }

    }

    public byte GetVoxelFromMap(Vector3 pos)
    {
        pos -= Position;

        return voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);
        int z = Mathf.RoundToInt(pos.z);

        //// If position is outside of this chunk...
        if (!IsVoxelInChunk(x, y, z))
            return false;// !planet.Blocks[planet.GetVoxel(pos + Position)].IsSolid;

        return planet.Blocks[voxelMap[x, y, z]].IsSolid;
    }
    public void EditVoxel(Vector3 pos, byte newID)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = newID;

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);

        UpdateChunk();

    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {

        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {

            Vector3 currentVoxel = thisVoxel + VoxelData.Directions[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                planet.GetChunkFromVector3(currentVoxel + Position).UpdateChunk();
            }
        }
    }

    public bool CheckForVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        if (planet.IsChunkInWorld(xCheck, yCheck, zCheck))
        {
            if(planet.Blocks[voxelMap[xCheck, yCheck, zCheck]].IsSolid)
            {
                return true;
            }
        }

        return false;
    }

    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        try
        {
            return voxelMap[xCheck, yCheck, zCheck];
        }
        catch
        {
            //Debug.LogError("Out Of Range at " + xCheck + ", " + yCheck + ", " + zCheck);
            return 1;
        }
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.Directions[p]))
            {
                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 0]]);
                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 1]]);
                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 2]]);
                vertices.Add(pos + VoxelData.Verts[VoxelData.Tris[p, 3]]);

                AddTexture(planet.Blocks[blockID].GetTextureID(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
    }

    void AddTexture(int textureID)
    {
        float y = textureID / GameData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * GameData.TextureAtlasSizeInBlocks);

        x *= GameData.NormalizedBlockTextureSize;
        y *= GameData.NormalizedBlockTextureSize;

        y = 1f - y - GameData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + GameData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + GameData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + GameData.NormalizedBlockTextureSize, y + GameData.NormalizedBlockTextureSize));
    }
}


public class ChunkCoord
{
    public int x;
    public int y;
    public int z;

    public ChunkCoord(Vector3 _pos)
    {
        x = Mathf.FloorToInt(_pos.x) / GameData.ChunkSize;
        y = Mathf.FloorToInt(_pos.y) / GameData.ChunkSize;
        z = Mathf.FloorToInt(_pos.z) / GameData.ChunkSize;
    }

    public ChunkCoord(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.y == y && other.z == z)
            return true;
        else
            return false;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }

    public Vector3 ToWorldPosition()
    {
        return new Vector3(x, y, z) * GameData.ChunkSize;
    }
}