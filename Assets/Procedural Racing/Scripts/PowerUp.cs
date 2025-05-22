using UnityEngine;
using System.Collections;

public abstract class PowerUp : MonoBehaviour 
{
    public GameObject visualEffect;  // 道具的视觉效果
    public AudioClip collectSound;  // 收集音效
    public GameObject collectEffect;  // 收集特效
    
    protected Car playerCar;  // 玩家车辆引用
    protected GameManager gameManager;  // 游戏管理器引用
    
    protected virtual void Start()
    {
        playerCar = FindObjectOfType<Car>();
    }
    
    protected virtual void Update()
    {
        // 只检测玩家是否靠近，靠近就触发
        if (playerCar != null)
        {
            float distance = Vector3.Distance(transform.position, playerCar.transform.position);
            if (distance < 2f)
            {
                OnTriggerEnterManual(playerCar.gameObject);
            }
        }
    }
    
    protected virtual void OnTriggerEnterManual(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            ProcessPlayerCollision();
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ProcessPlayerCollision();
        }
    }
    
    protected virtual void ProcessPlayerCollision()
    {
        // 播放收集音效
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        // 生成收集特效
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        // 激活道具效果
        ActivatePowerUp();
        // 立即销毁道具对象
        Destroy(gameObject);
    }
    
    // 道具效果激活
    protected abstract void ActivatePowerUp();
    // 道具效果结束（不再需要）
} 