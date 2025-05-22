using UnityEngine;
using System.Collections;

public class BananaPowerUp : PowerUp 
{
    public float slipForce = 500f;     // 大幅提升滑动力度
    public float slipDuration = 1.5f;   // 持续时间
    public float upwardForce = 50f;     // 增强向上的力，制造更大的跳跃感
    public float rotationForce = 400f;   // 大幅提升旋转力度
    private Rigidbody carRigidbody;
    private GameObject slipEffect;
    private GameObject statusIndicator; // 状态指示器
    private bool isSlipActive = false;  // 滑动状态指示器
    private Coroutine slipRoutineRef;   // 滑动效果协程引用
    
    protected override void Start()
    {
        base.Start();
        carRigidbody = playerCar.GetComponent<Rigidbody>();
        Debug.Log($"[Banana] 香蕉皮道具初始化完成，力度：{slipForce}，持续时间：{slipDuration}秒");
        
        if (carRigidbody == null)
        {
            Debug.LogError("[Banana] 错误：未找到玩家车辆的Rigidbody组件！");
        }
    }
    
    protected override void ActivatePowerUp()
    {
        if (playerCar != null)
        {
            playerCar.ApplyBananaSlip(slipForce, rotationForce);
        }
        Debug.Log("[Banana] 触发香蕉皮打滑效果");
    }
    
    // 创建状态指示器 - 显示在车辆上方
    private void CreateStatusIndicator()
    {
        // 如果已经有指示器，先销毁它
        if (statusIndicator != null)
        {
            Destroy(statusIndicator);
        }
        
        // 创建一个简单的指示器
        statusIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(statusIndicator.GetComponent<Collider>()); // 移除碰撞器
        
        // 设置外观 - 使用黄色表示香蕉皮
        Renderer renderer = statusIndicator.GetComponent<Renderer>();
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(1f, 0.9f, 0.2f, 0.7f); // 黄色半透明
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.2f) * 2f); // 发光效果
        renderer.material = material;
        
        // 设置位置和大小
        statusIndicator.transform.parent = playerCar.transform;
        statusIndicator.transform.localPosition = new Vector3(0, 1.5f, 0); // 在车辆上方
        statusIndicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        // 添加UI文本显示剩余时间
        GameObject textObj = new GameObject("DurationText");
        textObj.transform.parent = statusIndicator.transform;
        textObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        
        // 添加一个面向相机的脚本
        textObj.AddComponent<LookAtCamera>();
        
        // 添加Canvas和Text组件
        Canvas canvas = textObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        
        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(rectTransform, false);
        
        UnityEngine.UI.Text text = textChild.AddComponent<UnityEngine.UI.Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = slipDuration.ToString("F1") + "s";
        
        RectTransform textRectTransform = text.GetComponent<RectTransform>();
        textRectTransform.localPosition = Vector3.zero;
        textRectTransform.sizeDelta = new Vector2(5, 5);
        
        // 更新文本显示剩余时间
        StartCoroutine(UpdateDurationText(text));
        
        // 添加一个简单的旋转动画和上下弹跳动画
        StartCoroutine(AnimateStatusIndicator(statusIndicator.transform));
    }
    
    private IEnumerator UpdateDurationText(UnityEngine.UI.Text text)
    {
        float startTime = Time.time;
        float endTime = startTime + slipDuration;
        
        while (isSlipActive && Time.time < endTime)
        {
            float remainingTime = endTime - Time.time;
            text.text = remainingTime.ToString("F1") + "s";
            yield return null;
        }
    }
    
    private IEnumerator AnimateStatusIndicator(Transform indicator)
    {
        // 添加旋转和上下跳动
        Vector3 originalPosition = indicator.localPosition;
        float time = 0;
        
        while (isSlipActive && indicator != null)
        {
            time += Time.deltaTime * 5f;
            
            // 旋转
            indicator.Rotate(Vector3.up, 15f);
            
            // 上下跳动 - 香蕉皮效果使用更激烈的动画
            float yOffset = Mathf.Sin(time) * 0.1f;
            indicator.localPosition = originalPosition + new Vector3(0, yOffset, 0);
            
            yield return null;
        }
    }
    
    private IEnumerator SlipRoutine()
    {
        // 使用车辆的方向向量
        Vector3 carUpDirection = playerCar.transform.up;
        Vector3 carRightDirection = playerCar.transform.right;
        Vector3 carForwardDirection = playerCar.transform.forward;
        
        // 随机选择一个滑动方向（相对于车辆）
        float randomAngle = Random.Range(-180f, 180f);
        Vector3 slipDirection = Quaternion.AngleAxis(randomAngle, carUpDirection) * carForwardDirection;
        
        // 计算最终的冲击力方向 (主要是横向力和一些向上的力)
        Vector3 finalForce = (slipDirection * 0.8f + carUpDirection * 0.2f).normalized * slipForce;
        
        // 施加一个强大的瞬间冲击力
        carRigidbody.AddForce(finalForce, ForceMode.Impulse);
        Debug.Log($"[Banana] 施加冲击力: {finalForce.magnitude}, 方向: {finalForce.normalized}");
        
        // 施加一个随机的扭矩，让车辆旋转
        Vector3 torqueDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
        
        carRigidbody.AddTorque(torqueDirection * rotationForce, ForceMode.Impulse);
        Debug.Log($"[Banana] 施加扭矩: {torqueDirection * rotationForce}");
        
        // 短暂的闪烁效果
        if (playerCar != null)
        {
            // 获取所有渲染器
            Renderer[] renderers = playerCar.GetComponentsInChildren<Renderer>();
            
            // 闪烁几次
            for (int i = 0; i < 3; i++)
            {
                // 改变颜色为黄色
                foreach (var renderer in renderers)
                {
                    if (renderer.material != null)
                    {
                        renderer.material.color = new Color(1f, 0.8f, 0.2f); // 黄色
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
                
                // 恢复原始颜色
                foreach (var renderer in renderers)
                {
                    if (renderer.material != null)
                    {
                        renderer.material.color = Color.white;
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        // 等待约1秒后结束效果 - 我们让滑动效果更短，更符合游戏感觉
        yield return new WaitForSeconds(0.6f);
        
        Debug.Log("[Banana] 滑动效果结束");
    }
} 