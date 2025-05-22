using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour {
	
    public float movespeed;   // 移动速度
    public float rotateSpeed;  // 旋转速度
    public bool lamp;  // 是否是灯光（用于决定旋转方向）

    // 不会在 Inspector 窗口中显示的私有变量
    WorldGenerator generator;  // 世界生成器，用于获取世界维度信息
	
    Car car;  // 车辆对象
    Transform carTransform;  // 车辆的 Transform（位置、旋转信息）

    void Start(){
        car = GameObject.FindObjectOfType<Car>();
        generator = GameObject.FindObjectOfType<WorldGenerator>();
		
        if(car != null)
            carTransform = car.gameObject.transform;
    }

    void Update(){
        // 每帧沿着前方向前移动
        transform.Translate(Vector3.forward * movespeed * Time.deltaTime);
		
        // 如果有找到 Car 对象，旋转时参考车辆的旋转
        if(car != null)
            CheckRotate();
    }
	
    void CheckRotate(){
        // 根据是否是灯光决定旋转的方向
        // 如果是灯光，旋转方向为右（Vector3.right），否则为前（Vector3.forward）
        Vector3 direction = (lamp) ? Vector3.right : Vector3.forward;
		
        // 获取车辆的旋转角度（y轴）
        float carRotation = carTransform.localEulerAngles.y;
		
        // 如果车辆的旋转角度超过了一定值（此处是车辆的旋转角度*2），则将其调整为负值
        // 因为 eulerAngles 返回的是一个始终为正的角度值
        if(carRotation > car.rotationAngle * 2f)
            carRotation = (360 - carRotation) * -1f;
		
        // 根据旋转方向、旋转速度、车辆旋转角度和世界的尺寸来调整本物体的旋转
        // 通过时间差（Time.deltaTime）使得旋转保持平滑
        transform.Rotate(direction * -rotateSpeed * (carRotation / (float)car.rotationAngle) * (36f / generator.dimensions.x) * Time.deltaTime);
    }
}