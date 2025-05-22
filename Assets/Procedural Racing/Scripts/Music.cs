using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour {
    
    // 音乐管理实例
    private static Music instance;

    void Awake(){
        // 检查是否已经有实例存在，如果没有，则将当前实例设置为唯一实例
        if(!instance){
            instance = this;  // 如果没有实例，设置当前对象为实例
        }
        else{
            Destroy(gameObject);  // 如果已经存在实例，销毁当前对象，确保只存在一个实例
        }

        // 确保这个对象在场景切换时不会被销毁，这样背景音乐会持续播放
        DontDestroyOnLoad(this.gameObject);
    }
}