using UnityEngine;
using System.Collections;

public class SpeedBoostPowerUp : PowerUp 
{
    public float speedMultiplier = 2.0f;  // 速度提升倍数
    private float originalGlobalSpeed;      // 原始世界移动速度
    private GameObject speedEffect;  // 速度效果对象
    private WorldGenerator worldGenerator;  // 世界生成器引用
    private Coroutine flashEffectCoroutine;  // 闪烁效果协程引用
    private GameObject statusIndicator;  // 状态指示器对象
    
    protected override void Start()
    {
        base.Start();
        
        // 获取WorldGenerator引用
        worldGenerator = FindObjectOfType<WorldGenerator>();
        if (worldGenerator == null)
        {
            Debug.LogError("[SpeedBoost] 严重错误: 未找到WorldGenerator！");
        }
        
        Debug.Log($"[SpeedBoost] 初始化完成，速度倍数: {speedMultiplier}");
    }
    
    protected override void ActivatePowerUp()
    {
        if (playerCar != null)
        {
            playerCar.StartSpeedBoost(5f, speedMultiplier); // 5秒加速，可根据需要调整
        }
        Debug.Log("[SpeedBoost] 触发加速效果");
    }
}

// 面向相机的辅助脚本
public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                            mainCamera.transform.rotation * Vector3.up);
        }
    }
} 