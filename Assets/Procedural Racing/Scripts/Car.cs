using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {
	
	// 在 Inspector 窗口中可以看到的变量
	public Rigidbody rb;  // 车辆的刚体，用于物理模拟
	
	public Transform[] wheelMeshes;  // 车辆的轮子网格（模型）
	public WheelCollider[] wheelColliders;  // 车辆的轮子碰撞器（用于物理模拟）
	
	public int rotateSpeed;  // 车辆旋转的速度
	public int rotationAngle;  // 每次旋转的角度
	public int wheelRotateSpeed;  // 轮子的旋转速度
	
	public Transform[] grassEffects;  // 草地特效（在轮子接触草地时显示）
	public Transform[] skidMarkPivots;  // 滑痕的生成位置
	public float grassEffectOffset;  // 草地特效的偏移量
	
	public Transform back;  // 车辆后方的位置，用于施加稳定的向下力
	public float constantBackForce;  // 车辆后方施加的持续向下力
	
	public GameObject skidMark;  // 滑痕预设
	public float skidMarkSize;  // 滑痕的大小
	public float skidMarkDelay;  // 滑痕生成的延迟时间
	public float minRotationDifference;  // 最小旋转差异，用于检测是否有足够的旋转来生成滑痕
	
	public GameObject ragdoll;  // 车辆的 ragdoll 物理对象（当车辆破坏时使用）
	
	// 新增护盾和指示器视觉效果引用（可选，不赋值不会影响游戏运行）
	public GameObject shieldEffect; // 护盾视觉效果对象，可为空
	public GameObject statusIndicator; // 状态指示器对象，可为空
	
	// 添加无敌模式变量，可在Inspector中设置
	public bool invincibleMode = false;  // 无敌模式标志
	
	// 在 Inspector 窗口不可见的变量
	int targetRotation;  // 目标旋转角度
	WorldGenerator generator;  // 世界生成器，用于获取世界相关数据
	GameManager gameManager;  // 游戏管理器引用

	float lastRotation;  // 上一帧的旋转角度
	bool skidMarkRoutine;  // 是否正在生成滑痕的标志
	
	private ShieldPowerUp activeShield;  // 当前激活的护盾

	private Coroutine speedBoostCoroutine;
	private float originalGlobalSpeed;
	private bool isSpeedBoosting = false;

	private Coroutine invincibleCoroutine;
	private bool isInvincible = false;

	private Coroutine magnetCoroutine;
	private bool isMagnetActive = false;

	void Start(){
		// 查找世界生成器并启动滑痕生成协程
		generator = GameObject.FindObjectOfType<WorldGenerator>();
		gameManager = GameObject.FindObjectOfType<GameManager>();
		StartCoroutine(SkidMark());
		
		// 确保车辆拥有"Player"标签
		if (!gameObject.CompareTag("Player"))
		{
			gameObject.tag = "Player";
		}
		
		// 检查Rigidbody组件
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
			if (rb == null)
			{
				rb = gameObject.AddComponent<Rigidbody>();
			}
		}
		
		// 显示无敌模式状态
		if (invincibleMode)
		{
			Debug.Log("[Car] 警告：无敌模式已开启，车辆将不会被摧毁");
		}
	}
	
	void Update()
	{
		// 按下F12键切换无敌模式
		if (Input.GetKeyDown(KeyCode.F12))
		{
			ToggleInvincibleMode();
		}
		
		// 如果正在加速，可以添加一些视觉效果
		if (isSpeedBoosting)
		{
			// 比如轻微抖动相机或产生粒子效果
			// 这里只是一个示例，实际效果可以根据需要实现
		}
	}
	
	void FixedUpdate(){
		// 更新滑痕和草地特效
		UpdateEffects();
	}
	
	void LateUpdate(){
		// 更新所有轮子的网格位置和旋转
		for(int i = 0; i < wheelMeshes.Length; i++){	
			// 获取轮子碰撞器的世界位置和旋转
			Quaternion quat;
			Vector3 pos;
			
			wheelColliders[i].GetWorldPose(out pos, out quat);
			
			// 设置轮子网格的位置
			wheelMeshes[i].position = pos;
			
			// 旋转轮子，使其看起来像是正在行驶
			wheelMeshes[i].Rotate(Vector3.right * Time.deltaTime * wheelRotateSpeed);
		}
		
		// 如果玩家想要转向，旋转车辆
		if(Input.GetMouseButton(0) || Input.GetAxis("Horizontal") != 0){
			UpdateTargetRotation();
		}
		else if(targetRotation != 0){
			// 否则，旋转回到中心位置
			targetRotation = 0;
		}
		
		// 按照目标角度进行旋转
		Vector3 rotation = new Vector3(transform.localEulerAngles.x, targetRotation, transform.localEulerAngles.z);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), rotateSpeed * Time.deltaTime);
	}
	
	void UpdateTargetRotation(){
		// 如果是用鼠标旋转
		if(Input.GetAxis("Horizontal") == 0){
			// 获取鼠标的位置（屏幕左右位置）
			if(Input.mousePosition.x > Screen.width * 0.5f){
				// 向右旋转
				targetRotation = rotationAngle;
			}
			else{
				// 向左旋转
				targetRotation = -rotationAngle;
			}
		}
		else{
			// 如果按下了方向键或者 a/d 键，根据水平输入旋转车辆
			targetRotation = (int)(rotationAngle * Input.GetAxis("Horizontal"));
		}
	}
	
	void UpdateEffects(){
		// 如果两个后轮都不接触地面，addForce 为 true
		bool addForce = true;
		// 检查车辆的旋转是否发生了变化
		bool rotated = Mathf.Abs(lastRotation - transform.localEulerAngles.y) > minRotationDifference;
		
		// 处理后轮的草地特效
		for(int i = 0; i < 2; i++){
			// 获取后轮（每次迭代选择一个后轮）
			Transform wheelMesh = wheelMeshes[i + 2];
			
			// 检查当前轮子是否接触地面
			if(Physics.Raycast(wheelMesh.position, Vector3.down, grassEffectOffset * 1.5f)){
				// 如果接触地面，显示草地特效
				if(!grassEffects[i].gameObject.activeSelf)
					grassEffects[i].gameObject.SetActive(true);
				
				// 更新草地特效的高度和滑痕的高度，使其与轮子同步
				float effectHeight = wheelMesh.position.y - grassEffectOffset;
				Vector3 targetPosition = new Vector3(grassEffects[i].position.x, effectHeight, wheelMesh.position.z);
				grassEffects[i].position = targetPosition;
				skidMarkPivots[i].position = targetPosition;
				
				// 如果轮子接触地面，则不需要施加额外的向后力
				addForce = false;
			}
			else if(grassEffects[i].gameObject.activeSelf){
				// 如果轮子没有接触地面，则隐藏草地特效
				grassEffects[i].gameObject.SetActive(false);
			}
		}
		
		// 如果两个后轮都不接触地面，施加向下的稳定力
		if(addForce){
			rb.AddForceAtPosition(back.position, Vector3.down * constantBackForce);
			// 不显示滑痕
			skidMarkRoutine = false;
		}
		else{
			if(targetRotation != 0){
				// 如果车辆正在旋转，显示滑痕
				if(rotated && !skidMarkRoutine){
					skidMarkRoutine = true;
				}
				else if(!rotated && skidMarkRoutine){
					skidMarkRoutine = false;
				}
			}
			else{
				// 如果车辆正在旋转回中心位置，不显示滑痕
				skidMarkRoutine = false;
			}
		}
		
		// 更新最后一次旋转角度
		lastRotation = transform.localEulerAngles.y;
	}
	
	// 检查是否有护盾保护
	public bool HasActiveShield()
	{
		// 只判断无敌模式
    	return invincibleMode;
	}
	
	
	// 修改 FallApart 方法
	public void FallApart(){
		// 如果开启了无敌模式，不执行散架
		if (invincibleMode)
		{
			Debug.Log("[Car] 无敌模式阻止了车辆散架");
			return;
		}

		// 创建 ragdoll 物理对象
		Instantiate(ragdoll, transform.position, transform.rotation);
		// 禁用当前车辆对象
		gameObject.SetActive(false);

		// 调用游戏结束方法
		if (gameManager != null)
		{
			Debug.Log("[Car] 车辆散架，游戏结束");
			gameManager.GameOver();
		}
	}
	
	// 滑痕生成协程
	IEnumerator SkidMark(){
		// 无限循环生成滑痕
		while(true){
			// 等待滑痕生成的延迟时间
			yield return new WaitForSeconds(skidMarkDelay);
			
			// 如果需要生成滑痕
			if(skidMarkRoutine){
				// 为后轮生成滑痕并将其附加到环境中
				for(int i = 0; i < skidMarkPivots.Length; i++){
					// 实例化滑痕对象
					GameObject newskidMark = Instantiate(skidMark, skidMarkPivots[i].position, skidMarkPivots[i].rotation);
					// 将滑痕附加到世界生成器的一个世界块上
					newskidMark.transform.parent = generator.GetWorldPiece();
					// 设置滑痕的大小
					newskidMark.transform.localScale = new Vector3(1, 1, 4) * skidMarkSize;
				}
			}
		}
	}
	
	// 增加碰撞检测方法
	void OnTriggerEnter(Collider other)
	{
		// 尝试获取PowerUp组件
		PowerUp powerUp = other.GetComponent<PowerUp>();
		if (powerUp != null)
		{
			// 触发道具效果由PowerUp组件自己处理
		}
	}
	
	// 物理碰撞检测
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Obstacle"))
		{
			Debug.Log($"[车辆] 碰撞到障碍物: {collision.gameObject.name}");
			
			if (invincibleMode || isInvincible)
			{
				Debug.Log("[车辆] 处于无敌模式，销毁障碍物");
				Destroy(collision.gameObject);  // 销毁障碍物
				return;
			}
			
			Debug.Log("[车辆] 没有任何保护，车辆将散架");
			FallApart();
		}
	}
	
	// 切换无敌模式
	public void ToggleInvincibleMode()
	{
		invincibleMode = !invincibleMode;
		Debug.Log($"[车辆] 无敌模式{(invincibleMode ? "开启" : "关闭")}");
	}

	public void SetInvincible(bool invincible)
	{
		invincibleMode = invincible;
		Debug.Log($"[车辆] 无敌模式{(invincible ? "开启" : "关闭")}");
	}

	public void StartSpeedBoost(float duration, float speedMultiplier)
	{
		if (speedBoostCoroutine != null)
		{
			StopCoroutine(speedBoostCoroutine);
			EndSpeedBoost();
		}
		speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine(duration, speedMultiplier));
	}

	private IEnumerator SpeedBoostRoutine(float duration, float speedMultiplier)
	{
		isSpeedBoosting = true;
		WorldGenerator generator = FindObjectOfType<WorldGenerator>();
		if (generator != null)
		{
			originalGlobalSpeed = generator.globalSpeed;
			float newSpeed = originalGlobalSpeed * speedMultiplier;
			generator.SetGlobalSpeed(newSpeed);
			Debug.Log($"[Car] SpeedBoost: 世界速度提升到 {newSpeed}");
		}
		yield return new WaitForSeconds(duration);
		EndSpeedBoost();
	}

	private void EndSpeedBoost()
	{
		isSpeedBoosting = false;
		WorldGenerator generator = FindObjectOfType<WorldGenerator>();
		if (generator != null)
		{
			generator.SetGlobalSpeed(originalGlobalSpeed);
			Debug.Log($"[Car] SpeedBoost: 世界速度恢复到 {originalGlobalSpeed}");
		}
	}

	public void ApplyBananaSlip(float slipForce, float rotationForce)
	{
		// 忽略参数值，直接启动一个协程来处理香蕉皮效果
		StartCoroutine(DirectBananaEffect());
	}

	private IEnumerator DirectBananaEffect()
	{
		// 只记录原始旋转
		Quaternion originalRotation = transform.rotation;
		
		// 晃动持续时间和强度
		float duration = 2.0f;
		float intensity = 30f;  // 增加强度
		float elapsed = 0f;
		
		Debug.Log("[Car] 香蕉皮效果: 开始剧烈旋转!");
		
		// 晃动循环
		while (elapsed < duration)
		{
			// 计算当前强度 (波浪式变化，不是线性衰减)
			float wave = Mathf.Sin((elapsed / duration) * Mathf.PI * 6);  // 创造波浪效果
			float currentIntensity = intensity * Mathf.Abs(wave);
			
			// 随机旋转量，主要在Y轴和Z轴
			float rotX = Random.Range(-1f, 1f) * currentIntensity * 0.4f;  // X轴轻微旋转
			float rotY = Random.Range(-2f, 2f) * currentIntensity;  // Y轴强烈旋转
			float rotZ = Random.Range(-1f, 1f) * currentIntensity * 0.7f;  // Z轴中等旋转
			
			// 应用旋转 - 直接修改旋转而不是累积
			transform.rotation = originalRotation * Quaternion.Euler(rotX, rotY, rotZ);
			
			// 更新时间
			elapsed += Time.deltaTime;
			yield return null;
		}
		
		// 恢复原始旋转
		transform.rotation = originalRotation;
		
		Debug.Log("[Car] 香蕉皮效果: 旋转结束!");
	}

	public void StartInvincible(float duration)
	{
		if (invincibleCoroutine != null)
		{
			StopCoroutine(invincibleCoroutine);
			EndInvincible();
		}
		invincibleCoroutine = StartCoroutine(InvincibleRoutine(duration));

		// 开启护盾视觉特效
		if (shieldEffect != null)
			shieldEffect.SetActive(true);
		if (statusIndicator != null)
			statusIndicator.SetActive(true);
	}

	private IEnumerator InvincibleRoutine(float duration)
	{
		isInvincible = true;
		Debug.Log($"[Car] Invincible: 无敌模式开启，持续{duration}秒");
		yield return new WaitForSeconds(duration);
		EndInvincible();
	}

	public void EndInvincible()
	{
		isInvincible = false;
		Debug.Log("[Car] Invincible: 无敌模式关闭");
		// 关闭护盾视觉特效
		if (shieldEffect != null)
			shieldEffect.SetActive(false);
		if (statusIndicator != null)
			statusIndicator.SetActive(false);
	}

	public void StartMagnet(float duration, float magnetRadius, float magnetForce)
	{
		if (magnetCoroutine != null)
		{
			StopCoroutine(magnetCoroutine);
			EndMagnet();
		}
		magnetCoroutine = StartCoroutine(MagnetRoutine(duration, magnetRadius, magnetForce));
	}

	private IEnumerator MagnetRoutine(float duration, float magnetRadius, float magnetForce)
	{
		isMagnetActive = true;
		Debug.Log($"[Car] Magnet: 磁铁效果开启，持续{duration}秒，范围{magnetRadius}，力度{magnetForce}");

		float endTime = Time.time + duration;
		float checkFrequency = 0.1f; // 每0.1秒检查一次

		while (Time.time < endTime && isMagnetActive)
		{
			// 查找范围内的所有道具
			Collider[] colliders = Physics.OverlapSphere(transform.position, magnetRadius);
			foreach (Collider col in colliders)
			{
				PowerUp powerUp = col.GetComponent<PowerUp>();
				if (powerUp != null && powerUp.gameObject != gameObject)
				{
					// 计算方向和距离
					Vector3 direction = (transform.position - col.transform.position).normalized;
					float distance = Vector3.Distance(transform.position, col.transform.position);
					// 移动道具向玩家
					float currentForce = magnetForce * (1 - distance/magnetRadius * 0.5f);
					col.transform.position += direction * currentForce * checkFrequency;
					// 如果道具很近，直接将其吸附到玩家上
					if (distance < 2.0f)
					{
						col.transform.position = transform.position;
					}
				}
			}
			yield return new WaitForSeconds(checkFrequency);
		}
		isMagnetActive = false;
		Debug.Log("[Car] Magnet: 磁铁效果已关闭");
	}

	private void EndMagnet()
	{
		isMagnetActive = false;
		Debug.Log("[Car] Magnet: 磁铁效果已关闭");
	}
}
