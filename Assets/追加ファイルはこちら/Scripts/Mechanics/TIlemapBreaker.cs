using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapBreaker 
{
    

    public class TileRayDestroyer : MonoBehaviour
    {
        [Header("設定")]
        /// <summary>
        /// ぶっこわすタイルマップ
        /// </summary>
        public Tilemap targetTilemap;
        /// <summary>
        /// 破壊エフェクト
        /// </summary>
        public GameObject explosionPrefab;
        public float maxDistance = 10f;
        public float stepDelay = 0.05f; // タイル破壊の間隔（秒）

        public void Fire(Vector3 direction)
        {
            StartCoroutine(DestroyTilesAlongRay(direction));
        }

        private IEnumerator DestroyTilesAlongRay(Vector3 direction)
        {
            if (targetTilemap == null) yield break;

            // 開始位置
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (Vector3)(direction.normalized * maxDistance);

            // 線分をセルごとにサンプリング
            List<Vector3Int> hitCells = RaycastTiles(startPos, endPos);

            // 近いセルから順番に処理
            foreach (var cell in hitCells)
            {
                TileBase tile = targetTilemap.GetTile(cell);
                if (tile != null)
                {
                    // タイル削除
                    targetTilemap.SetTile(cell, null);

                    // 爆破エフェクト
                    if (explosionPrefab != null)
                    {
                        Vector3 worldPos = targetTilemap.GetCellCenterWorld(cell);
                        Instantiate(explosionPrefab, worldPos, Quaternion.identity);
                    }

                    // 1ステップ待つ
                    yield return new WaitForSeconds(stepDelay);
                }
            }
        }

        /// <summary>
        /// 線分が通過するセルを求める（DDA方式）
        /// </summary>
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