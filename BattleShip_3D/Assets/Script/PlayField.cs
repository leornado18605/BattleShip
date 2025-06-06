using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayField : MonoBehaviour
{
    public GameObject tilePrefab; // Sửa lại tên biến từ titlePrefabs thành tilePrefab cho rõ nghĩa
    List<GameObject> tileList = new List<GameObject>(); // Sửa lại tên biến từ titleList thành tileList

    List<TileInfo> tileInfoList = new List<TileInfo>();
    private void Start()
    {
        tileList.Clear();
        tileInfoList.Clear();

        // Fill GameObject
        foreach (Transform t in transform)
        {
            if (t != transform)
            {
                tileList.Add(t.gameObject);
            }
        }

        // Fill tile infos
        foreach (Transform g in transform)
        {
            TileInfo tileInfo = g.GetComponent<TileInfo>();
            if (tileInfo != null) // Thêm kiểm tra null để tránh lỗi
            {
                tileInfoList.Add(tileInfo);
            }
        }
    }

    private void Awake()
    {
        CreateTiles();
    }

    private void CreateTiles()
    {
        // Xóa các tile hiện có nếu có
        for (int i = 0; i < tileList.Count; i++)
        {
            if (tileList[i] != null)
            {
                Destroy(tileList[i]);
            }
        }
        tileList.Clear();
        tileInfoList.Clear(); // Thêm: Xóa tileInfoList để đồng bộ

        // Kiểm tra prefab có tồn tại không
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned!");
            return;
        }

        // Tạo các tile mới
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                // Căn chỉnh vị trí tile chính xác vào trung tâm ô
                Vector3 pos = new Vector3(transform.position.x + i + 0.5f, 0, transform.position.z + j + 0.5f);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform); // Gán parent là transform của PlayField

                TileInfo tileInfo = tile.GetComponent<TileInfo>();
                if (tileInfo != null)
                {
                    tileInfo.SetTileInfo(i, j);
                    tileInfoList.Add(tileInfo); // Thêm tileInfo vào danh sách
                }
                else
                {
                    Debug.LogWarning("Tile prefab doesn't have TileInfo component!");
                }

                tileList.Add(tile);
            }
        }
    }

    // Thêm phương thức để lấy vị trí trung tâm của ô dựa trên tọa độ
    public Vector3 GetTileCenter(int x, int z)
    {
        return new Vector3(transform.position.x + x + 0.5f, 0, transform.position.z + z + 0.5f);
    }

    // Thêm phương thức để kiểm tra và đồng bộ hóa trạng thái tile
    public bool IsTileAvailable(TileInfo info)
    {
        if (info == null || !tileInfoList.Contains(info))
        {
            return false;
        }
        return true;
    }

    public bool RequestTile(TileInfo info)
    {
        // Thêm kiểm tra để đảm bảo tile hợp lệ trước khi chấp nhận
        if (IsTileAvailable(info))
        {
            return tileInfoList.Contains(info);
        }
        return false;
    }

    // Giữ lại OnDrawGizmos nếu bạn vẫn muốn có chức năng fill trong Editor
    private void OnDrawGizmos()
    {
        // Bạn có thể bỏ phần này hoặc giữ lại nếu muốn
    }

    public TileInfo GetTileInfo(int x, int z)
    {
        for(int i = 0; i < tileInfoList.Count; i++)
        {
            if (tileInfoList[i].xPos == i && tileInfoList[i].zPos == z)
            {
                return tileInfoList[i];
            }
        }    
        return null;
    }
}