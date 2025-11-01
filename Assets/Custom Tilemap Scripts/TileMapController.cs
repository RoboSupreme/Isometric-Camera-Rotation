using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 45°×2:1 等距 Tile-map 控制器
/// 流程：按按钮 → 依次进入正弦波 → 再按 → 依次淡出并归位
public class TileMapController : MonoBehaviour
{
    #region Inspector 参数 --------------------------------------------------
    [Header("Prefab / Parent")]
    [SerializeField] private Transform  tileContainer;
    [SerializeField] private GameObject tilePrefab;
    
    [Header("视图控制器")]
    [SerializeField] private IsoViewController viewCtrl; // 用于读取当前视图索引

    [Header("Grid Size")]
    [SerializeField] private int gridWidth  = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private int defaultHeight = 0;

    [Header("Tile Sprite Size (px)")]
    [SerializeField] private float tileWidth  = 64;
    [SerializeField] private float tileHeight = 32;

    [Header("World Offset")]
    [SerializeField] private Vector2 centerOffset = Vector2.zero;
    
    [Header("Camera Controls")]
    [SerializeField] private float panSpeed   = 8f;   // 拖动速度，格/秒
    [SerializeField] private float zoomSpeed  = 3f;   // 滚轮缩放速度
    [SerializeField] private float baseSize   = 5f;   // 相机基础正交尺寸
    [SerializeField] private Vector2 panLimit = new(50, 50); // 防止移出太远
    
    private Vector2 worldOffset = Vector2.zero; // 相机在"世界"里的偏移
    private float   zoom        = 1f;           // 1 = 默认

    [Header("Height Layer → world-unit")]
    [SerializeField] private float heightScale = 0.25f;
    
    // 四向矩阵缓存 & 当前索引
    private static readonly Matrix2x2[] ViewMatrices =
    {
        //          [ m00,  m01 ]
        //  NE  (45°)  halfW,  -halfW
        //             halfH,   halfH
        new Matrix2x2( 1, -1,   1,  1 ),   // 会再乘 halfW/halfH

        //  NW  (135°)
        new Matrix2x2( 1,  1,  -1,  1 ),

        //  SW  (-135°)
        new Matrix2x2(-1,  1,  -1, -1 ),

        //  SE  (-45°)
        new Matrix2x2(-1, -1,   1, -1 )
    };
    private static readonly Vector2Int[] SortDirs =
    {
        new Vector2Int(+1,+1),   // 0-NE  (x+y)
        new Vector2Int(+1,-1),   // 1-NW  (x-y)
        new Vector2Int(-1,-1),   // 2-SW (-x-y)
        new Vector2Int(-1,+1)    // 3-SE (-x+y)
    };
    private int camIndex = 0;   // 0-NE,1-NW,2-SW,3-SE
    private Matrix2x2 M;        // 当前投影矩阵
    private Matrix2x2 MInv;     // 逆矩阵
    
    // ---------- Pivot 相关 ----------
    private bool   _useCustomPivot = false;
    private Vector2 _pivotScreen;     // 旋转开始时，用户给的屏幕坐标
    private Vector2 _pivotGridF;      // 对应的浮点网格坐标（逻辑坐标系）
    private Vector2 _pivotWorld0;     // 旋转前它的世界坐标

    // 平滑旋转插值相关字段
    private Matrix2x2 _MFrom, _MTo;          // 旋转起止矩阵
    private Vector2 _dirFrom, _dirTo;        // 排序方向起止
    private float _rotT = 1f;                // 当前插值因子 0-1

    /*  单块升降（点一下 tile）  */
    // 点击功能已移除

    /*  波纹动画配置  */
    [Header("正弦波动画参数")]
    [SerializeField] private float waveSpeed     = 4f;    // 每秒波纹传播多少格 (每秒激活多少圈格子)
    [SerializeField] private float waveHeight    = 0.3f;  // 波幅高度 (上下移动距离)
    [SerializeField] private float sineSpeed     = 2f;    // 正弦波频率 (每秒完成多少个周期)
    #endregion

    #region 内部字段 ---------------------------------------------------------
    private readonly Dictionary<Vector2Int, GameObject> objs  = new();    // 基础瓦片(height=0)
    private readonly Dictionary<Vector2Int, GameObject> upperObjs = new(); // 上层瓦片(height=1)
    private readonly Dictionary<Vector2Int, int>        baseH = new();    // 基础高度
    private readonly Dictionary<Vector2Int, float>      weight= new();    // 波纹权重 0-1

    private float halfW, halfH;   // world-unit
    private bool waveActive = false;  // 是否开启波纹动画
    private bool waveBusy   = false;  // 波纹正在添加/移除
    private float waveStartTime = 0f;  // 波纹开始的时间
    private bool stopQueued = false;   // 是否已经请求停止 ramp in/out

    [SerializeField] private float hoverLift = 0.5f;   // 上浮多少“高度层”
    private Vector2Int? curHover = null;               // 当前鼠标所在格；null 表示不在网格里
    #endregion

    #region 初始化 -----------------------------------------------------------
    private void Awake()
    {
        var sr = tilePrefab.GetComponent<SpriteRenderer>();
        float ppu = sr.sprite.pixelsPerUnit;
        halfW = tileWidth  / ppu * 0.5f;
        halfH = tileHeight / ppu * 0.5f;
        if (Mathf.Approximately(heightScale,0)) heightScale = halfH;
        
        // 初始化矩阵
        camIndex = 0; // 默认 NE 视图
        M = BuildMatrix(camIndex);
        MInv = M.Inverse();
        _dirFrom = _dirTo = SortDirs[camIndex];
        
        // 订阅视图控制器的旋转事件
        if (viewCtrl != null)
            viewCtrl.OnRotateProgress += HandleRotateProgress;
    }
    private void Start() 
    { 
        BuildGrid();
        
        // 创建一些第二层的高处瓦片
        CreateElevatedTiles();
    }
    #endregion

    #region Update ----------------------------------------------------------
    private void Update()
    {
        // 检查是否需要停止正弦波
        if (stopQueued)
            CheckStopConditions();
            
        // 鼠标悬停选中
        //UpdateHover();
        HandlePanZoomInput();
        
        // 注意：不再需要直接检查视图切换
        // 已通过OnRotateProgress事件处理矩阵插值
        
        /* —— ① 每帧计算悬停格（实时跟随方块位移） —— */
        curHover = GetTileUnderCursor();
        
        // 处理所有方块位置
        UpdateAllPositions();
    }
    #endregion

    #region Grid / Tile -----------------------------------------------------
    private void BuildGrid()
    {
        foreach (Transform c in tileContainer) Destroy(c.gameObject);
        objs.Clear(); baseH.Clear(); weight.Clear();

        for (int y=0; y<gridHeight; y++)
        for (int x=0; x<gridWidth ; x++)
        {
            Vector2Int p = new Vector2Int(x,y);
            int h = weight.ContainsKey(p) ? baseH[p] : defaultHeight;
            GameObject go = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, tileContainer);
            UpdateSorting(go, x, y, 0);

            // 放入字典
            objs.Add(p, go);
            baseH[p] = h;
            weight[p]= 0;
        }
    }
    private GameObject SpawnTile(int x,int y,int h)
    {
        var go = Instantiate(tilePrefab,tileContainer);
        go.name = $"Tile_{x}_{y}_{h}";
        go.transform.position = IsoPos(x,y,h);

        UpdateSorting(go, x, y, h);

        return go;
    }
    
    /// <summary>
    /// 设置瓦片的高度并更新其位置
    /// </summary>
    public void SetTileHeight(Vector2Int position, int height)
    {
        // 基座永远存在
        if (!objs.ContainsKey(position))
            objs[position] = SpawnTile(position.x, position.y, 0);
            
        baseH[position] = height;
        
        // 上层的增删
        if (height > 0)
        {
            if (!upperObjs.ContainsKey(position))
                upperObjs[position] = SpawnTile(position.x, position.y, 1);
        }
        else
        {
            if (upperObjs.TryGetValue(position, out var top) && top)
                Destroy(top);
            upperObjs.Remove(position);
        }
    }
    
    /// <summary>
    /// 创建一些高处瓦片模式 (只使用 height = 0 和 height = 1 两种高度)
    /// </summary>
    private void CreateElevatedTiles()
    {
        // 创建一个十字形高台在地图中央
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;
        
        // 水平线
        for (int x = -4; x <= 4; x++)
        {
            Vector2Int pos = new Vector2Int(centerX + x, centerY);
            if (objs.ContainsKey(pos))
            {
                SetTileHeight(pos, 1);
            }
        }
        
        // 垂直线
        for (int y = -4; y <= 4; y++)
        {
            if (y == 0) continue; // 跳过中心点，避免重复设置
            Vector2Int pos = new Vector2Int(centerX, centerY + y);
            if (objs.ContainsKey(pos))
            {
                SetTileHeight(pos, 1);
            }
        }
        
        // 在地图四个角落创建四个3x3的方块
        CreateSquarePlatform(2, 2, 1);                      // 左上
        CreateSquarePlatform(2, gridHeight - 3, 1);         // 左下
        CreateSquarePlatform(gridWidth - 3, 2, 1);          // 右上
        CreateSquarePlatform(gridWidth - 3, gridHeight - 3, 1); // 右下
    }
    
    /// <summary>
    /// 创建一个正方形高台
    /// </summary>
    private void CreateSquarePlatform(int startX, int startY, int height)
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Vector2Int pos = new Vector2Int(startX + x, startY + y);
                if (objs.ContainsKey(pos))
                {
                    SetTileHeight(pos, height);
                }
            }
        }
    }
    #endregion

    #region 坐标换算 ---------------------------------------------------------
    private Vector3 IsoPos(int x,int y,float h)
    {
        Vector2 v = M.Mul(x,y);          // 用矩阵得到 X,Y
        v.y += h * heightScale;          // 再加高度
        
        // 加上全局平移，不再乘以 zoom
        v = v + centerOffset + worldOffset;
        
        return new Vector3(v.x, v.y, 0f);
    }
    #endregion

    #region 波纹动画 Start / Stop ---------------------------------------------
    public void StartSineWave()
    {
        if (waveBusy || waveActive) return;
        StopAllCoroutines();
        waveBusy   = true;
        waveActive = true;
        waveStartTime = Time.time;
        
        // 重置所有权重
        foreach (var p in objs.Keys)
            weight[p] = 0f;
            
        StartCoroutine(InitiateWave(() => waveBusy = false));
    }
    
    public void StopSineWave()
    {
        if (waveBusy || !waveActive) return;
        stopQueued = true;
    }

    private IEnumerator InitiateWave(System.Action cb)
    {
        // 获取所有格子并根据到中心的距离排序
        List<Vector2Int> allTiles = new(objs.Keys);
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;
        
        // 按曼哈顿距离排序（距离中心点的格子数）
        Dictionary<int, List<Vector2Int>> tilesByDistance = new();
        int maxDistance = 0;
        
        foreach (Vector2Int p in allTiles)
        {
            int distance = Mathf.Abs(p.x - centerX) + Mathf.Abs(p.y - centerY);
            maxDistance = Mathf.Max(maxDistance, distance);
            
            if (!tilesByDistance.ContainsKey(distance))
                tilesByDistance[distance] = new List<Vector2Int>();
                
            tilesByDistance[distance].Add(p);
        }
        
        // 逐层激活瓦片
        for (int distance = 0; distance <= maxDistance; distance++)
        {
            if (tilesByDistance.ContainsKey(distance))
            {
                foreach (Vector2Int p in tilesByDistance[distance])
                {
                    weight[p] = 1.0f;  // 激活这个瓦片
                }
                
                // 等待波纹传播到下一圈瓦片
                yield return new WaitForSeconds(1.0f / waveSpeed);
            }
        }
        
        // 重置停止请求状态
        stopQueued = false;
        
        cb?.Invoke();
    }
    
    // 检测所有激活瓦片的当前状态，并在合适时机停止它们
    private void CheckStopConditions()
    {
        if (!stopQueued) return;
        
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;
        Dictionary<int, List<Vector2Int>> tilesByDistance = new();
        Dictionary<int, bool> ringFinished = new();
        int maxDistance = 0;
        
        // 按距离排序所有激活的瓦片
        foreach (Vector2Int p in objs.Keys)
        {
            if (weight.ContainsKey(p) && weight[p] > 0)
            {
                int distance = Mathf.Abs(p.x - centerX) + Mathf.Abs(p.y - centerY);
                maxDistance = Mathf.Max(maxDistance, distance);
                
                if (!tilesByDistance.ContainsKey(distance))
                {
                    tilesByDistance[distance] = new List<Vector2Int>();
                    ringFinished[distance] = false;
                }
                
                tilesByDistance[distance].Add(p);
            }
        }
        
        // 检查每一圈瓦片的位置
        for (int distance = 0; distance <= maxDistance; distance++)
        {
            if (!tilesByDistance.ContainsKey(distance) || ringFinished[distance])
                continue;
                
            bool allTilesAtZero = true;
            foreach (Vector2Int p in tilesByDistance[distance])
            {
                // 获取这个瓦片当前的正弦波位置
                float tileStartTime = waveStartTime + distance / waveSpeed;
                float timeSinceStart = Time.time - tileStartTime;
                float sineTime = timeSinceStart * sineSpeed;
                
                // 计算当前高度
                float currentHeight = Mathf.Sin(sineTime * Mathf.PI * 2) * waveHeight;
                
                // 如果高度非常接近零，认为可以停止
                if (Mathf.Abs(currentHeight) > 0.01f)
                {
                    allTilesAtZero = false;
                    break;
                }
            }
            
            // 如果这一圈的所有瓦片都在原始高度，停止它们
            if (allTilesAtZero)
            {
                ringFinished[distance] = true;
                foreach (Vector2Int p in tilesByDistance[distance])
                {
                    weight[p] = 0.0f;
                }
            }
        }
        
        // 检查是否所有瓦片都已经停止
        bool allRingsFinished = true;
        foreach (var finished in ringFinished.Values)
        {
            if (!finished)
            {
                allRingsFinished = false;
                break;
            }
        }
        
        if (allRingsFinished)
        {
            stopQueued = false;
            waveActive = false;
        }
    }
    #endregion

    // 鼠标点击处理已移除
    
    // 判断鼠标落在哪个 tile：返回 key；找不到则返回 null
    private Vector2Int? GetTileUnderCursor()
    {
        Vector2 mWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 原样世界坐标
        Vector2Int? pick = null;
        int bestOrder = int.MinValue;

        foreach (var kv in objs)
        {
            Vector2Int p = kv.Key;
            GameObject go = kv.Value;
            SpriteRenderer rd = go.GetComponent<SpriteRenderer>();

            /* -------- 计算未悬浮中心 -------- */
            float extra = 0f;
            if (waveActive && weight.ContainsKey(p) && weight[p] > 0)
            {
                int centerX = gridWidth / 2;
                int centerY = gridHeight / 2;
                int distance = Mathf.Abs(p.x - centerX) + Mathf.Abs(p.y - centerY);
                
                // 计算这个格子开始动画的时间
                float tileStartTime = waveStartTime + distance / waveSpeed;
                float timeSinceStart = Time.time - tileStartTime;
                
                // 已经开始激活的格子
                if (timeSinceStart >= 0)
                {
                    // 使用正弦波持续上下运动
                    float sineTime = timeSinceStart * sineSpeed;
                    extra = Mathf.Sin(sineTime * Mathf.PI * 2) * waveHeight * weight[p];
                }
            }
            Vector2 center = IsoPos(p.x, p.y, baseH[p] + extra);

            /* -------- 关键：计算世界坐标差值，不再除以 zoom -------- */
            Vector2 delta = mWorld - center;   // 移除除法 / zoom
            Vector2 gDelta = MInv.Mul(delta.x, delta.y);
            
            if (Mathf.Abs(gDelta.x) + Mathf.Abs(gDelta.y) > 1f) continue;

            if (rd.sortingOrder > bestOrder)
            {
                bestOrder = rd.sortingOrder;
                pick = p;
            }
        }
        return pick;
    }

    /* 反算等距网格；成功返回 true 并给出 g */
    // 更新看向矩阵
    /// <summary>
    /// 把鼠标点（或任何屏幕坐标）设为旋转中心
    /// </summary>
    public void SetPivotByScreen(Vector2 screenPos)
    {
        _pivotScreen   = screenPos;
        _useCustomPivot= true;
    }

    /// <summary>
    /// 把某个格子中心当成 pivot（整格）
    /// </summary>
    public void SetPivotByGrid(Vector2Int g)
    {
        if (baseH.ContainsKey(g))
            _pivotScreen = Camera.main.WorldToScreenPoint(IsoPos(g.x, g.y, baseH[g]));
        else
            _pivotScreen = Camera.main.WorldToScreenPoint(IsoPos(g.x, g.y, defaultHeight));
        _useCustomPivot = true;
    }
    
    // 旋转事件回调
    private void HandleRotateProgress(float t, int delta)
    {
        if (t < 0.0001f)
        {
            /* ------------ ① 先确定 pivot ------------ */
            if (!_useCustomPivot)                 // 默认用屏幕中央
                _pivotScreen = new Vector2(Screen.width * .5f, Screen.height * .5f);

            _pivotWorld0 = Camera.main.ScreenToWorldPoint(_pivotScreen);

            Vector2 pLocal = _pivotWorld0 - centerOffset - worldOffset; // 不再除以 zoom
            _pivotGridF    = MInv.Mul(pLocal.x, pLocal.y);

            /* ------------ ② 记录 From/To 矩阵 ------------ */
            _MFrom   = M;
            _dirFrom = SortDirs[camIndex];

            int next = (camIndex + delta + 4) % 4;
            _MTo     = BuildMatrix(next);
            _dirTo   = SortDirs[next];
            camIndex = next;

            _useCustomPivot = false;              // 用过就清掉
        }

        /* ------------ ③ 每帧插值 & 修正 worldOffset ------------ */
        _rotT = t;
        M     = LerpMatrix(_MFrom, _MTo, t);
        MInv  = M.Inverse();

        Vector2 pivotLocalNow = M.Mul(_pivotGridF.x, _pivotGridF.y);
        worldOffset = _pivotWorld0 - centerOffset - pivotLocalNow;  // 不需要缩放相关的计算
        
        // 限制相机可移动范围
        worldOffset.x = Mathf.Clamp(worldOffset.x, -panLimit.x, panLimit.x);
        worldOffset.y = Mathf.Clamp(worldOffset.y, -panLimit.y, panLimit.y);
        
        // 旋转刚开始时立即重新计算所有排序
        if (t < 0.0001f)
        {
            ResortAllTiles();
        }
    }

    // 构建特定索引的矩阵
    private Matrix2x2 BuildMatrix(int index)
    {
        Matrix2x2 m = ViewMatrices[index];
        m.m00 *= halfW; m.m01 *= halfW;
        m.m10 *= halfH; m.m11 *= halfH;
        return m;
    }
    
    // 矩阵插值
    private static Matrix2x2 LerpMatrix(Matrix2x2 a, Matrix2x2 b, float t) =>
        new Matrix2x2(
            Mathf.Lerp(a.m00,b.m00,t),
            Mathf.Lerp(a.m01,b.m01,t),
            Mathf.Lerp(a.m10,b.m10,t),
            Mathf.Lerp(a.m11,b.m11,t));
    
    private bool WorldToGrid(Vector2 pos, out Vector2Int g)
    {
        // 从 world 坐标减去全局偏移 - 不再除以 zoom
        pos = new Vector2(pos.x, pos.y) - centerOffset - worldOffset;   // 全用 Vector2

        float gx = ( pos.x / halfW + pos.y / halfH) * 0.5f;
        float gy = (-pos.x / halfW + pos.y / halfH) * 0.5f;

        g = new Vector2Int(Mathf.RoundToInt(gx),
                        Mathf.RoundToInt(gy));

        // 判断是否落在网格内
        return g.x >= 0 && g.x < gridWidth &&
               g.y >= 0 && g.y < gridHeight;
    }
    
    // ------------ 统一的深度公式（照搬 WorldCamera.GetSortZ） ------------
    private int DepthKey(int x, int y)
    {
        return camIndex switch
        {
            0 =>  (x + y),   //  NE :  y-(x+z)  →  -(x+z)   (z≈y)
            1 => -(x - y),   //  NW :  y+(x-z)  →  -(x-y)
            2 => -(x + y),   //  SW :  y+(x+z)  →  -(x+y)
            3 =>  (x - y),   //  SE :  y-(x-z)  →   (x-y)
            _ =>  (x + y)
        };
    }

    // —— 根据当前视图方向给 SpriteRenderer 设排序层 ——  
    private void UpdateSorting(GameObject go, int gx, int gy, int hLayer)
    {
        // "越靠近镜头" → sortingOrder 越大；同格里高度再往上加
        // 10 是留给同一深度的小扰动（防并列）；高度直接 +1 就够了
        int order = -DepthKey(gx, gy) * 10    // 远 < 近
                  + hLayer;                // 高 < 低（+1 保证同格上层>下层）

        go.GetComponent<SpriteRenderer>().sortingOrder = order;
    }
    
    // 重新计算所有瓦片的排序
    private void ResortAllTiles()
    {
        // 更新基本层排序
        foreach (var kv in objs)
            UpdateSorting(kv.Value, kv.Key.x, kv.Key.y, 0);
            
        // 更新上层排序
        foreach (var kv in upperObjs)
            if (kv.Value) UpdateSorting(kv.Value, kv.Key.x, kv.Key.y, 1);
    }
    
    private void UpdateAllPositions()
    {
        // 更新所有基础层瓦片
        foreach (var p in objs.Keys)
        {
            var go = objs[p];
            go.transform.position = IsoPos(p.x, p.y, 0);  // 基础层始终是高度 0

            // 正弦波动画
            if (waveActive && weight.ContainsKey(p) && weight[p] > 0)
            {
                Vector3 pos = go.transform.position;
                
                // 计算格子距中心的距离
                int centerX = gridWidth / 2;
                int centerY = gridHeight / 2;
                int distance = Mathf.Abs(p.x - centerX) + Mathf.Abs(p.y - centerY);
                
                // 计算这个格子开始动画的时间
                float tileStartTime = waveStartTime + distance / waveSpeed;
                float timeSinceStart = Time.time - tileStartTime;
                
                // 已经开始激活的格子
                if (timeSinceStart >= 0)
                {
                    // 使用正弦波持续上下运动
                    float sineTime = timeSinceStart * sineSpeed;
                    float h = Mathf.Sin(sineTime * Mathf.PI * 2) * waveHeight * weight[p];
                    
                    go.transform.position = new(pos.x, pos.y + h, pos.z);
                }
            }
            
            // 悬停效果
            if (curHover.HasValue && curHover.Value == p)
            {
                Vector3 pos = go.transform.position;
                go.transform.position = new(pos.x, pos.y + hoverLift, pos.z);
            }
            
            UpdateSorting(go, p.x, p.y, 0);
        }
        
        // 更新所有上层瓦片
        foreach (var p in upperObjs.Keys)
        {
            var go = upperObjs[p];
            if (go == null) continue;  // 安全检查
            
            go.transform.position = IsoPos(p.x, p.y, 1);  // 上层始终是高度 1
            
            // 正弦波动画 (与基础层保持一致)
            if (waveActive && weight.ContainsKey(p) && weight[p] > 0)
            {
                Vector3 pos = go.transform.position;
                
                // 计算格子距中心的距离
                int centerX = gridWidth / 2;
                int centerY = gridHeight / 2;
                int distance = Mathf.Abs(p.x - centerX) + Mathf.Abs(p.y - centerY);
                
                // 计算这个格子开始动画的时间
                float tileStartTime = waveStartTime + distance / waveSpeed;
                float timeSinceStart = Time.time - tileStartTime;
                
                // 已经开始激活的格子
                if (timeSinceStart >= 0)
                {
                    // 使用正弦波持续上下运动
                    float sineTime = timeSinceStart * sineSpeed;
                    float h = Mathf.Sin(sineTime * Mathf.PI * 2) * waveHeight * weight[p];
                    
                    go.transform.position = new(pos.x, pos.y + h, pos.z);
                }
            }
            
            // 悬停效果
            if (curHover.HasValue && curHover.Value == p)
            {
                Vector3 pos = go.transform.position;
                go.transform.position = new(pos.x, pos.y + hoverLift, pos.z);
            }
            
            UpdateSorting(go, p.x, p.y, 1);
        }
    }
    
    // 处理平移和缩放输入
    private void HandlePanZoomInput()
    {
        // --- 平移：鼠标右键拖拽 或 键盘 WASD ---
        Vector2 delta = Vector2.zero;

        // 键盘 - 方向反转以符合直觉
        if (Input.GetKey(KeyCode.W)) delta += Vector2.down;
        if (Input.GetKey(KeyCode.S)) delta += Vector2.up;
        if (Input.GetKey(KeyCode.A)) delta += Vector2.right;
        if (Input.GetKey(KeyCode.D)) delta += Vector2.left;

        // 鼠标右键
        if (Input.GetMouseButton(1))
        {
            Vector2 m = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            // 使鼠标拖拽方向与屏幕移动方向一致
            delta += m * 0.05f; // 手感系数
        }

        if (delta != Vector2.zero)
        {
            // delta 是屏幕像素方向 → 换算成"格"
            float step = panSpeed * Time.deltaTime / zoom;
            worldOffset += delta.normalized * step;

            // 限制范围
            worldOffset.x = Mathf.Clamp(worldOffset.x, -panLimit.x, panLimit.x);
            worldOffset.y = Mathf.Clamp(worldOffset.y, -panLimit.y, panLimit.y);
            
            // 更新相机正交尺寸
            Camera.main.orthographicSize = baseSize / zoom;
        }

        // --- 缩放：鼠标滚轮 ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            zoom = Mathf.Clamp(zoom + scroll * zoomSpeed, 0.5f, 3f);
            
            // 更新相机正交尺寸
            Camera.main.orthographicSize = baseSize / zoom;
        }
    }
    
    // 矩阵进行 2D 变换
    private struct Matrix2x2
    {
        public float m00, m01, m10, m11;
        public Matrix2x2(float a,float b,float c,float d)
        { m00=a; m01=b; m10=c; m11=d; }
        public Vector2 Mul(float x,float y)
            => new Vector2(m00*x + m01*y, m10*x + m11*y);
        public Matrix2x2 Inverse()
        {
            float det = m00*m11 - m01*m10;
            return new Matrix2x2( m11/det, -m01/det, -m10/det, m00/det );
        }
    }
}
