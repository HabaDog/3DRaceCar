using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour {
	
    // 在 Inspector 中可见的变量（用于音效播放）
    public AudioSource scoreAudio;  // 得分时播放的音效
	
    // 在 Inspector 中不可见的变量
    GameManager manager;  // 游戏管理器引用
    bool addedScore;  // 标记是否已经得分
	
    void Start(){
        // 查找游戏管理器
        manager = GameObject.FindObjectOfType<GameManager>();
    }
	
    void OnTriggerEnter(Collider other){
        // 检查玩家是否通过了这个关卡门并且还没有得分
        if(!other.gameObject.transform.root.CompareTag("Player") || addedScore)
            return;  // 如果碰撞物体不是玩家，或已经得分，则不执行
		
        // 增加分数并播放音效
        addedScore = true;  // 标记已得分
        manager.UpdateScore(1);  // 更新分数
        scoreAudio.Play();  // 播放得分音效
    }
}