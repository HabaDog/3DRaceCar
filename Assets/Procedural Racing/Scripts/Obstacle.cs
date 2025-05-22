using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
    
    // 游戏管理器的引用
    GameManager manager;
	
    void Start(){
        // 查找场景中的游戏管理器
        manager = GameObject.FindObjectOfType<GameManager>();
        
        // 确保障碍物拥有"Obstacle"标签
        if (!gameObject.CompareTag("Obstacle"))
        {
            gameObject.tag = "Obstacle";
            Debug.Log($"[Obstacle] 障碍物标签已设置为Obstacle: {gameObject.name}");
        }
    }
	
    // 碰撞逻辑已移除，由Car.cs负责处理所有碰撞
}