using UnityEngine;
using System.Collections;

public class ShieldPowerUp : PowerUp
{
    private Car car;

    protected override void Start()
    {
        base.Start();
        car = FindObjectOfType<Car>();
    }

    protected override void ActivatePowerUp()
    {
        if (car != null)
        {
            car.StartInvincible(5f); // 5秒无敌，可根据需要调整
            Debug.Log("[护盾] 无敌模式已开启");
            // 不再创建任何球体或视觉效果
        }
    }
} 