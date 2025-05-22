using UnityEngine;
using System.Collections;

public class MagnetPowerUp : PowerUp 
{
    public float magnetRadius = 20f;  // 磁铁效果范围
    public float magnetForce = 30f;   // 磁力大小
    
    protected override void Start()
    {
        base.Start();
        Debug.Log($"[Magnet] 磁铁道具初始化完成，范围：{magnetRadius}，力度：{magnetForce}");
    }
    
    protected override void ActivatePowerUp()
    {
        if (playerCar != null)
        {
            playerCar.StartMagnet(5f, magnetRadius, magnetForce); // 5秒磁铁效果，可根据需要调整
        }
        Debug.Log("[Magnet] 触发磁铁效果");
    }
} 