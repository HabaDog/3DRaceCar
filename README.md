# 3DRaceCar
# 无尽赛车游戏

## 项目简介

《无尽赛车》是一款基于Unity引擎开发的3D无尽赛车游戏。玩家控制一辆小车在不断生成的道路上前进，通过躲避障碍物、收集道具来获得高分并延长游戏时间。

## 游戏特点

- **程序化生成地图**：游戏地图会不断随机生成，每次游戏体验都不同
- **多种障碍物**：各种障碍物需要玩家灵活躲避
- **丰富的道具系统**：
  - 🛡️ **护盾道具**：暂时获得无敌效果，可以撞毁障碍物
  - 🍌 **香蕉皮**：会导致车辆旋转失控
  - ⚡ **速度提升**：暂时加速行驶
- **动态特效**：包括车辆滑动特效、草地效果、轮胎痕迹等
- **计分系统**：记录并显示玩家最高分
- **物理系统**：真实的车辆物理效果，包括翻车检测

## 游戏操作

- **左右移动**：鼠标左右点击或使用键盘方向键/A、D键
- **开发者指令**：
  - `F12`：切换无敌模式

## 技术栈

- **游戏引擎**：Unity 3D
- **编程语言**：C#
- **物理系统**：Unity内置物理引擎
- **图形**：3D模型和材质

## 项目结构

```
《无尽赛车》完整工程/
└── 3DRaceCar/
    ├── Assets/
    │   └── Procedural Racing/
    │       ├── Scripts/       - 游戏主要脚本
    │       ├── Prefabs/       - 预制体对象
    │       ├── Materials/     - 材质文件
    │       ├── Models/        - 3D模型
    │       └── Scenes/        - 游戏场景
    ├── ProjectSettings/        - Unity项目设置
    └── Packages/               - Unity包管理
```

### 主要脚本

- **Car.cs**：车辆控制和物理逻辑
- **WorldGenerator.cs**：世界程序化生成
- **GameManager.cs**：游戏流程控制
- **PowerUp.cs**：道具基类
  - ShieldPowerUp.cs：护盾道具
  - BananaPowerUp.cs：香蕉皮道具
  - SpeedBoostPowerUp.cs：加速道具

## 安装和运行

1. 确保已安装Unity（推荐Unity 2020.3或更高版本）
2. 克隆本仓库
3. 使用Unity Hub打开项目目录`《无尽赛车》完整工程/3DRaceCar`
4. 在Unity编辑器中打开`Assets/Procedural Racing/Scenes/`中的主场景
5. 点击播放按钮运行游戏


## 贡献指南

欢迎为项目做出贡献！如果你想参与开发，请：

1. Fork本仓库
2. 创建你的特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交你的更改 (`git commit -m '添加了一些很棒的特性'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启一个Pull Request


项目链接: [https://github.com/yourusername/endless-racer](https://github.com/yourusername/endless-racer)
