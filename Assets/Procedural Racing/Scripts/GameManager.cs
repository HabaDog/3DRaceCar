using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	
	// 在 Inspector 中可见的变量
	public Text scoreLabel;  // 显示当前分数的文本
	public Text timeLabel;  // 显示计时的文本
	public Text gameOverScoreLabel;  // 游戏结束时显示分数的文本
	public Text gameOverBestLabel;  // 游戏结束时显示最高分的文本
	public Animator scoreEffect;  // 分数变化时的动画效果
	public Animator UIAnimator;  // UI 动画控制器
	public Animator gameOverAnimator;  // 游戏结束动画控制器
	public AudioSource gameOverAudio;  // 游戏结束时的音效
	public Car car;  // 玩家控制的车
	
	// 在 Inspector 中不可见的变量
	float time;  // 游戏运行的时间
	int score;  // 当前分数
	
	bool gameOver;  // 标记游戏是否结束
	
	void Start(){
		// 初始化时显示分数为 0
		UpdateScore(0);
	}
	
	void Update(){
		// 更新当前时间显示
		UpdateTimer();
		
		// 如果游戏结束且玩家按下回车或左键，则重新开始游戏
		if(gameOver && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))){
			UIAnimator.SetTrigger("Start");
			StartCoroutine(LoadScene(SceneManager.GetActiveScene().name));
		}
	}
	
	void UpdateTimer(){
		// 增加游戏时间
		time += Time.deltaTime;
		int timer = (int)time;
		
		// 计算分钟和秒数
		int seconds = timer % 60;
		int minutes = timer / 60;
		
		// 格式化时间字符串，确保秒数和分钟数都是两位数
		string secondsRounded = ((seconds < 10) ? "0" : "") + seconds;
		string minutesRounded = ((minutes < 10) ? "0" : "") + minutes;
		
		// 显示时间
		timeLabel.text = minutesRounded + ":" + secondsRounded;
	}
	
	public void UpdateScore(int points){
		// 增加分数
		score += points;
		
		// 更新分数显示
		scoreLabel.text = "" + score;
		
		// 如果有得分，触发分数动画效果
		if(points != 0)
			scoreEffect.SetTrigger("Score");
	}
	
	public void GameOver(){
		// 如果游戏已经结束，则不再执行
		if(gameOver)
			return;
		
		// 更新当前分数和最高分数
		SetScore();
		
		// 播放游戏结束动画和音效
		gameOverAnimator.SetTrigger("Game over");
		gameOverAudio.Play();
		
		// 设置游戏结束标志
		gameOver = true;
		
		// 让车破坏（摧毁车辆）
		car.FallApart();
		
		// 停止世界中的所有移动或旋转（暂停世界生成的物体运动）
		foreach(BasicMovement basicMovement in GameObject.FindObjectsOfType<BasicMovement>()){
			basicMovement.movespeed = 0;  // 停止移动
			basicMovement.rotateSpeed = 0;  // 停止旋转
		}
	}
	
	void SetScore(){
		// 如果当前分数比之前的最高分高，则更新最高分
		if(score > PlayerPrefs.GetInt("best"))
			PlayerPrefs.SetInt("best", score);
		
		// 显示当前分数和最高分数
		gameOverScoreLabel.text = "score: " + score;
		gameOverBestLabel.text = "best: " + PlayerPrefs.GetInt("best");
	}
	
	// 等待不到一秒钟后加载给定的场景
	IEnumerator LoadScene(string scene){
		// 等待一段时间再加载场景（用于游戏结束后的过渡动画）
		yield return new WaitForSeconds(0.6f);
		
		// 加载指定的场景
		SceneManager.LoadScene(scene);
	}
}
