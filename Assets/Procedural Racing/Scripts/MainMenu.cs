using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    // UI 动画控制器引用
    public Animator UIAnimator;

    private void Start()
    {
        // 在此处可以初始化需要的内容，目前为空
    }

    private void Update()
    {
        // 检查用户是否按下回车键或者点击屏幕/鼠标左键，且点击不在 UI 元素上
        if (Input.GetKeyDown(KeyCode.Return) ||
            (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()))
        {
            // 如果是触摸屏设备，且触摸点不在 UI 元素上
            if (!(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began &&
                  EventSystem.current.IsPointerOverGameObject((Input.GetTouch(0).fingerId))))
            {
                // 开始游戏
                StartGame();
            }
        }
    }

    // 开始游戏的功能
    public void StartGame()
    {
        // 播放 UI 动画
        UIAnimator.SetTrigger("Start");
        // 启动加载场景的协程
        StartCoroutine(LoadScene("Game"));
    }

    // 加载场景的协程
    IEnumerator LoadScene(string scene)
    {
        // 等待 0.6 秒的过渡动画时间
        yield return new WaitForSeconds(0.6f);

        // 加载指定的场景
        SceneManager.LoadScene(scene);
    }
}