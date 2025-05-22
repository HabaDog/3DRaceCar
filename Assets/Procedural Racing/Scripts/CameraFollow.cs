using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	
	// 在 Inspector 窗口中可以看到的变量
	public Transform camTarget;  // 摄像机跟随的目标（通常是玩家或车辆）
	
	public float startDelay;  // 游戏开始时的延迟时间，等待一段时间后再开始切换摄像机角度
    public float distance = 6.0f;  // 摄像机与目标之间的距离
    public float height = 5.0f;  // 摄像机的高度
    public float heightDamping = 0.5f;  // 摄像机高度的平滑过渡系数
    public float rotationDamping = 1.0f;  // 摄像机旋转的平滑过渡系数
	
	// 在 Inspector 窗口不可见的变量
	float originalRotationDamping;  // 保存原始的旋转平滑系数，用于稍后恢复
	bool canSwitch;  // 用来控制是否可以切换旋转平滑系数

	void Start(){
		// 获取初始的旋转平滑系数
		originalRotationDamping = rotationDamping;
		// 将旋转平滑系数设置为一个非常小的值，这样摄像机会平滑地过渡到目标位置
		rotationDamping = 0.1f;
		
		// 在延迟一段时间后开始切换摄像机的旋转平滑系数
		StartCoroutine(SwitchAngle());
	}
	
	void Update(){
		// 当玩家第一次控制车辆时，恢复正常的旋转平滑系数
		if((Input.GetMouseButtonDown(0) || Input.GetAxis("Horizontal") != 0) && rotationDamping == 0.1f && canSwitch)
			rotationDamping = originalRotationDamping;
	}
	 
	void LateUpdate(){		
		// 如果没有设置摄像机的目标，则直接返回
        if(!camTarget)
            return;		
		
		// 私有变量：用于计算摄像机旋转和位置
        float wantedRotationAngle = camTarget.eulerAngles.y;  // 目标的旋转角度（绕 Y 轴）
        float wantedHeight = camTarget.position.y + height;  // 目标的高度，摄像机会相对目标有一定的高度
        float currentRotationAngle = transform.eulerAngles.y;  // 当前摄像机的旋转角度
        float currentHeight = transform.position.y;  // 当前摄像机的高度
		
        // 平滑过渡到目标的旋转角度
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
 
        // 平滑过渡到目标的高度
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
 
        // 计算当前的旋转四元数
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
     
        // 更新摄像机的位置，先设置为目标位置
        transform.position = camTarget.position;
        // 调整摄像机位置，使其保持一定距离在目标后方
        transform.position -= currentRotation * Vector3.forward * distance;
		
		// 设置摄像机的高度
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
		
		// 让摄像机看向目标
        transform.LookAt(camTarget);
    }
	
	// 切换旋转平滑系数的协程方法
	IEnumerator SwitchAngle(){
		// 等待指定的时间后切换旋转平滑系数
		yield return new WaitForSeconds(startDelay);
		
		// 设置可以切换平滑系数
		canSwitch = true;
	}
}
