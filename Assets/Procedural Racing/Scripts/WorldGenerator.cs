using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WorldGenerator : MonoBehaviour
{

    // 可在检查器中看到的变量
    public Material meshMaterial;  // 网格材质
    public float scale;  // 世界比例
    public Vector2 dimensions;  // 世界的尺寸（x: 横向, y: 纵向）
    public float perlinScale;  // 用于生成噪声的比例
    public float waveHeight;  // 波动高度
    public float offset;  // 噪声偏移量
    public float randomness;  // 随机性
    public float globalSpeed;  // 世界移动的速度
    public int startTransitionLength;  // 开始过渡的长度
    public BasicMovement lampMovement;  // 灯光（或方向光）的运动
    public GameObject[] obstacles;  // 障碍物数组
    public GameObject gate;  // 大门对象
    public int startObstacleChance;  // 开始时生成障碍物的概率
    public int obstacleChanceAcceleration;  // 障碍物概率加速
    public int gateChance;  // 大门生成概率
    public int showItemDistance;  // 显示物品的距离
    public float shadowHeight;  // 阴影高度
    public GameObject[] powerUps;  // 道具预制体数组
    public int powerUpChance = 15;  // 道具生成概率（百分比）
    public float powerUpHeight = 1f;  // 道具离地高度

    // 不可见的变量
    Vector3[] beginPoints;  // 世界各部分的起始点，用于过渡效果

    GameObject[] pieces = new GameObject[2];  // 存储两个世界部分
    GameObject currentCylinder;  // 当前生成的世界部分（圆柱体）
    private List<GameObject> activePowerUps = new List<GameObject>();  // 当前激活的道具列表
    private int lastPowerUpZ = 0;          // 上一个道具的Z坐标，用于控制生成间距
    public float minPowerUpSpacing = 10f;  // 道具之间的最小间距

    void Start()
    {
        // 创建数组，用于存储每个世界部分的起始顶点（用于正确过渡）
        beginPoints = new Vector3[(int)dimensions.x + 1];

        // 先生成两个世界部分
        for (int i = 0; i < 2; i++)
        {
            GenerateWorldPiece(i);
        }
    }

    void LateUpdate()
    {
        // 如果第二个部分已经接近玩家，移除第一个部分并更新世界
        if (pieces[1] && pieces[1].transform.position.z <= 0)
            StartCoroutine(UpdateWorldPieces());

        // 更新场景中的所有物品，如障碍物和大门
        UpdateAllItems();
    }

    void UpdateAllItems()
    {
        // 同时查找所有带有 "Item" 和 "Obstacle" 标签的物品
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        // 处理所有"Item"标签的物品
        ProcessRenderers(items);

        // 处理所有"Obstacle"标签的物品
        ProcessRenderers(obstacles);
    }

    // 处理物体的MeshRenderer组件
    private void ProcessRenderers(GameObject[] objects)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            // 获取物品的所有 MeshRenderer
            foreach (MeshRenderer renderer in objects[i].GetComponentsInChildren<MeshRenderer>())
            {
                // 如果物品距离玩家足够近，则显示该物品
                bool show = objects[i].transform.position.z < showItemDistance;

                // 如果需要显示物品，更新其阴影投射模式
                // 由于世界是圆柱形的，只有底半部分的物体需要阴影
                if (show)
                    renderer.shadowCastingMode = (objects[i].transform.position.y < shadowHeight) ? ShadowCastingMode.On : ShadowCastingMode.Off;

                // 只有在需要显示物品时才启用其渲染器
                renderer.enabled = show;
            }
        }
    }

    void GenerateWorldPiece(int i)
    {
        pieces[i] = CreateCylinder();
        pieces[i].transform.Translate(Vector3.forward * (dimensions.y * scale * Mathf.PI) * i);
        UpdateSinglePiece(pieces[i]);
        Debug.Log($"[WorldGen] 生成片段 {i}，位置z={pieces[i].transform.position.z}");
    }

    IEnumerator UpdateWorldPieces()
    {
        Debug.Log($"[WorldGen] 触发片段更新，销毁片段0，片段1位置z={pieces[1].transform.position.z}，startObstacleChance={startObstacleChance}");
        ClearPowerUpsOnPiece(pieces[0]);
        Destroy(pieces[0]);
        pieces[0] = pieces[1];
        pieces[1] = CreateCylinder();
        pieces[1].transform.position = pieces[0].transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
        pieces[1].transform.rotation = pieces[0].transform.rotation;
        UpdateSinglePiece(pieces[1]);
        Debug.Log($"[WorldGen] 新片段生成完成，片段1位置z={pieces[1].transform.position.z}，dimensions={dimensions}，scale={scale}");
        yield return 0;
    }

    void UpdateSinglePiece(GameObject piece)
    {
        // 给新生成的部分添加基本运动脚本，使其朝向玩家移动
        BasicMovement movement = piece.AddComponent<BasicMovement>();
        // 设置其移动速度为 globalSpeed（负数表示朝玩家方向移动）
        movement.movespeed = -globalSpeed;

        // 设置旋转速度为灯光（方向光）的旋转速度
        if (lampMovement != null)
            movement.rotateSpeed = lampMovement.rotateSpeed;

        // 为此部分创建一个终点
        GameObject endPoint = new GameObject();
        endPoint.transform.position = piece.transform.position + Vector3.forward * (dimensions.y * scale * Mathf.PI);
        endPoint.transform.parent = piece.transform;
        endPoint.name = "End Point";

        // 改变 Perlin 噪声的偏移量，以确保每个世界部分与上一个不同
        offset += randomness;
    }

    public GameObject CreateCylinder()
    {
        // 创建世界部分的基础对象并命名
        GameObject newCylinder = new GameObject();
        newCylinder.name = "World piece";

        // 设置当前圆柱体为新创建的对象
        currentCylinder = newCylinder;

        // 给新部分添加 MeshFilter 和 MeshRenderer 组件
        MeshFilter meshFilter = newCylinder.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newCylinder.AddComponent<MeshRenderer>();

        // 给新部分设置材质
        meshRenderer.material = meshMaterial;
        // 生成网格并赋值给 MeshFilter
        meshFilter.mesh = Generate();

        // 添加与网格匹配的 MeshCollider 组件
        newCylinder.AddComponent<MeshCollider>();

        return newCylinder;
    }

    // 生成并返回新世界部分的网格
    Mesh Generate()
    {
        // 创建并命名新网格
        Mesh mesh = new Mesh();
        mesh.name = "MESH";

        // 创建数组来存储顶点、UV 坐标和三角形
        Vector3[] vertices = null;
        Vector2[] uvs = null;
        int[] triangles = null;

        // 创建网格形状并填充数组
        CreateShape(ref vertices, ref uvs, ref triangles);

        // 给网格赋值
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        // 重新计算法线
        mesh.RecalculateNormals();

        return mesh;
    }

    void CreateShape(ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles)
    {
        // 获取该部分在 x 和 z 轴的大小
        int xCount = (int)dimensions.x;  // 在 x 轴上分割的顶点数量
        int zCount = (int)dimensions.y;  // 在 z 轴上分割的顶点数量

        // 初始化顶点和 UV 数组
        vertices = new Vector3[(xCount + 1) * (zCount + 1)];
        uvs = new Vector2[(xCount + 1) * (zCount + 1)];

        int index = 0;

        // 获取圆柱体的半径
        float radius = xCount * scale * 0.5f;  // 圆柱的半径

        // 双重循环遍历 x 和 z 轴的所有顶点
        for (int x = 0; x <= xCount; x++)
        {
            for (int z = 0; z <= zCount; z++)
            {
                // 获取圆柱体的角度，以正确设置顶点位置
                float angle = x * Mathf.PI * 2f / xCount;

                // 使用角度的余弦和正弦值来设置顶点
                vertices[index] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, z * scale * Mathf.PI);

                // 更新 UV 坐标
                uvs[index] = new Vector2(x * scale, z * scale);

                // 使用 Perlin 噪声生成 X 和 Z 值
                float pX = (vertices[index].x * perlinScale) + offset;
                float pZ = (vertices[index].z * perlinScale) + offset;

                // 将顶点移动到中心位置（保持 z 坐标）并使用 Perlin 噪声调整位置
                Vector3 center = new Vector3(0, 0, vertices[index].z);
                vertices[index] += (center - vertices[index]).normalized * Mathf.PerlinNoise(pX, pZ) * waveHeight;

                // 处理世界部分之间的平滑过渡
                if (z < startTransitionLength && beginPoints[0] != Vector3.zero)
                {
                    // 如果是过渡部分，结合 Perlin 噪声和上一个部分的起始点
                    float perlinPercentage = z * (1f / startTransitionLength);
                    Vector3 beginPoint = new Vector3(beginPoints[x].x, beginPoints[x].y, vertices[index].z);
                    vertices[index] = (perlinPercentage * vertices[index]) + ((1f - perlinPercentage) * beginPoint);
                }
                else if (z == zCount)
                {
                    // 更新起始点，以确保下一部分的平滑过渡
                    beginPoints[x] = vertices[index];
                }

                if (z % 10 == 0) // 每10个单位记录一次，避免日志过多
                {
                    // Debug.Log($"[WorldGen] 当前位置 x={x}, z={z}, 实际z={vertices[index].z}, startObstacleChance={startObstacleChance}");
                }

                if (Random.Range(0, startObstacleChance) == 0 && !(gate == null && obstacles.Length == 0))
                {
                    //Debug.Log($"[WorldGen] CreateShape 生成障碍物判定通过，z={vertices[index].z}, startObstacleChance={startObstacleChance}");
                    CreateItem(vertices[index], x);
                }

                // 增加顶点索引
                index++;
            }
        }

        // 初始化三角形数组
        triangles = new int[xCount * zCount * 6];  // 每个方格有 2 个三角形，每个三角形由 3 个顶点组成，共 6 个顶点

        // 创建每个方块的基础（三角形的组成更简单）
        int[] boxBase = new int[6];  // 每个正方形面由 6 个顶点组成（两个三角形）

        int current = 0;

        // 遍历 x 轴上的所有位置
        for (int x = 0; x < xCount; x++)
        {
            boxBase = new int[]{
                x * (zCount + 1),
                x * (zCount + 1) + 1,
                (x + 1) * (zCount + 1),
                x * (zCount + 1) + 1,
                (x + 1) * (zCount + 1) + 1,
                (x + 1) * (zCount + 1),
            };

            // 遍历 z 轴上的所有位置
            for (int z = 0; z < zCount; z++)
            {
                // 增加顶点索引并创建三角形
                for (int i = 0; i < 6; i++)
                {
                    boxBase[i] = boxBase[i] + 1;
                }

                // 使用六个顶点填充三角形
                for (int j = 0; j < 6; j++)
                {
                    triangles[current + j] = boxBase[j] - 1;
                }

                // 增加当前索引
                current += 6;
            }
        }
    }

    void CreateItem(Vector3 vert, int x)
    {
        Debug.Log($"[WorldGen] CreateItem 被调用，z={vert.z}");
        // 获取圆柱体的中心位置，但使用顶点的 z 坐标
        Vector3 zCenter = new Vector3(0, 0, vert.z);

        // 检查生成物品的正确位置，优化判断条件
        if (zCenter - vert == Vector3.zero ||
           (x == (int)dimensions.x / 4 && Mathf.Abs(vert.y) < 0.1f) ||
           (x == (int)dimensions.x / 4 * 3 && Mathf.Abs(vert.y) < 0.1f))
        {
            // Debug.Log($"[WorldGen] 跳过物品生成 - zCenter-vert: {zCenter-vert}, x: {x}, dimensions.x/4: {(int)dimensions.x/4}");
            return;
        }

        GameObject newItem = null;

        // 调整道具生成逻辑，确保更均匀的分布
        bool shouldCreatePowerUp = Random.Range(0, 100) < powerUpChance &&
                                 powerUps != null &&
                                 powerUps.Length > 0;

        if (shouldCreatePowerUp)
        {
            newItem = CreatePowerUp(vert, currentCylinder.transform);
            if (newItem == null)
            {
                shouldCreatePowerUp = false;
            }
        }

        if (!shouldCreatePowerUp)
        {
            bool isGate = (Random.Range(0, gateChance) == 0);
            // if (isGate)
            //     Debug.Log("[WorldGen] 生成大门");
            // else
            //     Debug.Log("[WorldGen] 生成障碍物");
            newItem = Instantiate(isGate ? gate : obstacles[Random.Range(0, obstacles.Length)]);
            newItem.transform.rotation = Quaternion.LookRotation(zCenter - vert, Vector3.up);
            newItem.transform.position = vert;
            newItem.transform.SetParent(currentCylinder.transform, false);
        }
    }

    // 生成道具的方法
    private GameObject CreatePowerUp(Vector3 position, Transform parent)
    {
        if (powerUps == null || powerUps.Length == 0) return null;

        // 检查是否与上一个道具距离太近
        if (Mathf.Abs(position.z - lastPowerUpZ) < minPowerUpSpacing)
        {
            return null;
        }

        // 随机选择一个道具预制体
        GameObject powerUpPrefab = powerUps[Random.Range(0, powerUps.Length)];

        // 计算道具应该放置的正确高度
        // 使用顶点的位置并添加一个固定高度，这样道具就不会嵌入地形或悬在空中
        Vector3 spawnPosition = position;
        // 稍微提高道具的位置，使其位于地形上方
        spawnPosition.y += powerUpHeight;

        // 实例化道具
        GameObject powerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);

        // 设置父物体
        powerUp.transform.SetParent(parent);

        // 添加到活动道具列表
        activePowerUps.Add(powerUp);

        // 更新最后一个道具的Z坐标
        lastPowerUpZ = (int)position.z;

        return powerUp;
    }

    // 修改清理方法，确保道具被正确销毁前不触发效果
    void ClearPowerUpsOnPiece(GameObject piece)
    {
        List<GameObject> powerUpsToRemove = new List<GameObject>();

        foreach (GameObject powerUp in activePowerUps)
        {
            if (powerUp == null || (powerUp.transform.parent != null && powerUp.transform.parent.gameObject == piece))
            {
                powerUpsToRemove.Add(powerUp);
                if (powerUp != null)
                {
                    // 直接销毁道具对象，不再调用MarkAsCleared
                    Destroy(powerUp);
                }
            }
        }

        foreach (GameObject powerUp in powerUpsToRemove)
        {
            activePowerUps.Remove(powerUp);
        }
    }

    // 设置全局速度并更新所有地形片段的速度
    public void SetGlobalSpeed(float newSpeed)
    {
        // 更新全局速度变量
        globalSpeed = newSpeed;

        // 更新所有现有地形片段的速度
        foreach (GameObject piece in pieces)
        {
            if (piece != null)
            {
                BasicMovement movement = piece.GetComponent<BasicMovement>();
                if (movement != null)
                {
                    // 保持移动方向（负值表示向玩家移动）
                    movement.movespeed = -globalSpeed;
                }
            }
        }
    }

    public Transform GetWorldPiece()
    {
        // 返回第一个世界部分的 Transform
        return pieces[0].transform;
    }

    // 在 OnDestroy 中也确保正确清理
    void OnDestroy()
    {
        foreach (GameObject powerUp in activePowerUps)
        {
            if (powerUp != null)
            {
                // 直接销毁道具对象，不再调用MarkAsCleared
                Destroy(powerUp);
            }
        }
        activePowerUps.Clear();
    }

}
