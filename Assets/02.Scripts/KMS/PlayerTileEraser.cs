using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerTileEraser : MonoBehaviour
{
    [Header("Ground Tilemap Reference")]
    public Tilemap groundTilemap;

    [Header("Erase Settings")]
    public float checkOffsetY = 0.1f; // 발 위치 보정
    public float gizmoRadius = 0.05f; // 디버그 표시 크기

    private Vector3Int lastErasedCell;

    void Update()
    {
        EraseTileBeneath();
    }

    void EraseTileBeneath()
    {
        Vector3 checkPos = transform.position + new Vector3(0f, -checkOffsetY, 0f);
        Vector3Int cell = groundTilemap.WorldToCell(checkPos);

        if (cell == lastErasedCell) return;

        TileBase tile = groundTilemap.GetTile(cell);
        if (tile != null)
        {
            groundTilemap.SetTile(cell, null);
            lastErasedCell = cell;

            Debug.Log($"타일 제거됨: {cell}");
        }
    }

    // 씬에서 디버깅을 위한 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 checkPos = transform.position + new Vector3(0f, -checkOffsetY, 0f);

        Gizmos.DrawWireSphere(checkPos, gizmoRadius);
    }
}
