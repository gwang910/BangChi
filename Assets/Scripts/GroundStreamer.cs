using System.Collections.Generic;
using UnityEngine;

public class GroundStreamer : MonoBehaviour
{
    public Transform player;
    public GameObject tilePrefab;        // 바닥 타일 프리팹(Plane 등)
    public int tileSize = 10;            // 한 타일의 가로/세로 길이
    public int viewRadius = 3;           // 플레이어를 중심으로 +- 몇 타일 유지할지
    public bool usePooling = true;

    readonly Dictionary<Vector2Int, GameObject> _tiles = new Dictionary<Vector2Int, GameObject>();
    readonly Queue<GameObject> _pool = new Queue<GameObject>();
    Vector2Int _currentCenter;

    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _currentCenter = WorldToCell(player ? player.position : Vector3.zero);
        RefreshTiles(force: true);
    }

    void Update()
    {
        if (!player) return;
        var cell = WorldToCell(player.position);
        if (cell != _currentCenter)
        {
            _currentCenter = cell;
            RefreshTiles(force: false);
        }
    }

    Vector2Int WorldToCell(Vector3 pos)
    {
        int cx = Mathf.FloorToInt(pos.x / tileSize);
        int cz = Mathf.FloorToInt(pos.z / tileSize);
        return new Vector2Int(cx, cz);
    }

    void RefreshTiles(bool force)
    {
        // 유지해야 할 타일 목록
        var needed = new HashSet<Vector2Int>();
        for (int dz = -viewRadius; dz <= viewRadius; dz++)
            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                var cell = new Vector2Int(_currentCenter.x + dx, _currentCenter.y + dz);
                needed.Add(cell);
                if (_tiles.ContainsKey(cell)) continue;
                // 새로 생성
                var go = GetTile();
                go.transform.position = new Vector3(cell.x * tileSize, 0, cell.y * tileSize);
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one; // 프리팹 스케일에 맞춤
                go.name = $"Tile_{cell.x}_{cell.y}";
                _tiles[cell] = go;
            }

        // 필요 없는 타일 제거
        var toRemove = new List<Vector2Int>();
        foreach (var kv in _tiles)
            if (!needed.Contains(kv.Key)) toRemove.Add(kv.Key);

        foreach (var key in toRemove)
        {
            var go = _tiles[key];
            if (usePooling) { go.SetActive(false); _pool.Enqueue(go); }
            else Destroy(go);
            _tiles.Remove(key);
        }
    }

    GameObject GetTile()
    {
        if (usePooling)
        {
            while (_pool.Count > 0)
            {
                var t = _pool.Dequeue();
                if (t) { t.SetActive(true); return t; }
            }
        }
        return Instantiate(tilePrefab);
    }
}
