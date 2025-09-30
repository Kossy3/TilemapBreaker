using UnityEngine;
using UnityEngine.InputSystem;
using TilemapBreaker;
using Unity.VisualScripting;
public class PlayerShoot : MonoBehaviour
{
    public InputAction breakTileAction;
    public RaycastTileBreaker breaker;

    private void OnEnable()
    {
        breakTileAction.Enable();
    }

    private void OnDisable()
    {
        breakTileAction.Disable();
    }

    void Update()
    {
        if (breakTileAction.WasReleasedThisFrame())
        {
            // マウスのワールド座標を取得
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorld.z = 0;

            // 方向ベクトル
            Vector2 dir = (mouseWorld - transform.position).normalized;

            // ビーム生成
            
            breaker.Fire(dir);
        }
    }
}
