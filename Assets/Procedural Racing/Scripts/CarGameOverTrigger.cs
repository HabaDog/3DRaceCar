using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGameOverTrigger : MonoBehaviour {
	
    // 游戏管理器的引用
    GameManager manager;
	
    void Start(){
        // 查找游戏管理器
        manager = GameObject.FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter(Collider other){
        // 如果车顶与地面碰撞，说明车辆翻覆了，需要结束游戏
        if(other.gameObject.name == "World piece")
            manager.GameOver();
    }
}