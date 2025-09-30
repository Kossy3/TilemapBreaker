using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapBreaker
{
    public class RaycastTileBreaker : MonoBehaviour
    {
        [Header("Target")]
        public Tilemap targetTilemap;
        [Header("Effect")]
        public GameObject explosionPrefab;
        [Header("Audio")]
        public AudioClip breakSound;
        [Header("Setting")]
        public float maxDistance = 10f;
        public float stepDelay = 0.05f;
        public float destroyRadius = 0f; // 半径（少数も可）

        private Vector2 direction;

        public void Fire(Vector2 dir)
        {
            direction = dir.normalized;
            StartCoroutine(DestroyTilesAlongRay());
        }

        private IEnumerator DestroyTilesAlongRay()
        {
            if (targetTilemap == null) yield break;

            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (Vector3)(direction * maxDistance);

            List<Vector3Int> lineCells = RaycastTiles(startPos, endPos);

            foreach (var cell in lineCells)
            {
                // 半径 destroyRadius 以内のセルを破壊（円形判定）
                int radiusCeil = Mathf.CeilToInt(destroyRadius);
                for (int dx = -radiusCeil; dx <= radiusCeil; dx++)
                {
                    for (int dy = -radiusCeil; dy <= radiusCeil; dy++)
                    {
                        Vector3Int targetCell = new Vector3Int(cell.x + dx, cell.y + dy, 0);

                        // セル中心と元セル中心の距離を測る
                        Vector3 worldCenter = targetTilemap.GetCellCenterWorld(cell);
                        Vector3 neighborCenter = targetTilemap.GetCellCenterWorld(targetCell);
                        float dist = Vector3.Distance(worldCenter, neighborCenter);

                        if (dist <= destroyRadius + 0.001f) // 半径以内なら破壊
                        {
                            TileBase tile = targetTilemap.GetTile(targetCell);
                            if (tile != null)
                            {
                                targetTilemap.SetTile(targetCell, null);

                                if (explosionPrefab != null)
                                {
                                    Vector3 worldPos = targetTilemap.GetCellCenterWorld(targetCell);
                                    var effect = Instantiate(explosionPrefab, worldPos, Quaternion.identity);
                                    Destroy(effect, 2f);
                                }

                                if (breakSound != null)
                                {
                                    AudioSource.PlayClipAtPoint(breakSound, transform.position);
                                }
                                
                            }
                        }
                    }
                }
                

                yield return new WaitForSeconds(stepDelay);
            }
        }

        // Bresenham 直線アルゴリズム
        private List<Vector3Int> RaycastTiles(Vector3 start, Vector3 end)
        {
            List<Vector3Int> result = new List<Vector3Int>();

            Vector3Int startCell = targetTilemap.WorldToCell(start);
            Vector3Int endCell = targetTilemap.WorldToCell(end);

            int dx = Mathf.Abs(endCell.x - startCell.x);
            int dy = Mathf.Abs(endCell.y - startCell.y);

            int sx = startCell.x < endCell.x ? 1 : -1;
            int sy = startCell.y < endCell.y ? 1 : -1;

            int err = dx - dy;

            int x = startCell.x;
            int y = startCell.y;

            while (true)
            {
                result.Add(new Vector3Int(x, y, 0));

                if (x == endCell.x && y == endCell.y) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }

            return result;
        }
    }
}
