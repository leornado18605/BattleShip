using System.Collections.Generic;
using UnityEngine;

public class PlayField: MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab; // Prefab for creating tiles
    private readonly List<GameObject> tileList = new List<GameObject>(); // List of tile GameObjects
    private readonly List<TileInfo> tileInfoList = new List<TileInfo>(); // List of tile information components

    private void Awake()
    {
        CreateTiles();
    }
    /// Creates a 10x10 grid of tiles, clearing any existing tiles first
    private void CreateTiles()
    {
        // Clear existing tiles
        foreach (GameObject tile in tileList)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
        tileList.Clear();
        tileInfoList.Clear();

        // Validate tile prefab
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned in PlayField!", this);
            return;
        }

        // Create new 10x10 grid of tiles
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Vector3 pos = new Vector3(transform.position.x + i + 0.5f, 0, transform.position.z + j + 0.5f);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                
                if (tile.TryGetComponent(out TileInfo tileInfo))
                {
                    tileInfo.SetTileInfo(i, j);
                    tileInfoList.Add(tileInfo);
                    tileList.Add(tile);
                }
                else
                {
                    Debug.LogWarning($"Tile prefab at ({i},{j}) is missing ", tile);
                    Destroy(tile); 
                }
            }
        }
    }
    /// <param name="x">X coordinate in grid</param>
    /// <param name="z">Z coordinate in grid</param>

    public Vector3 GetTileCenter(int x, int z)
    {
        return new Vector3(transform.position.x + x + 0.5f, 0, transform.position.z + z + 0.5f);
    }

    /// Checks if a tile is valid and exists in the playfield
    public bool IsTileAvailable(TileInfo info)
    {
        return info != null && tileInfoList.Contains(info);
    }

    public bool RequestTile(TileInfo info)
    {
        return IsTileAvailable(info);
    }

    public TileInfo GetTileInfo(int x, int z)
    {
        // Validate coordinates
        if (x < 0 || x >= 10 || z < 0 || z >= 10)
        {
            Debug.LogWarning($"Invalid tile coordinates ({x},{z}) requested!");
            return null;
        }

        foreach (TileInfo tileInfo in tileInfoList)
        {
            if (tileInfo.xPos == x && tileInfo.zPos == z)
            {
                return tileInfo;
            }
        }
        return null;
    }
}
