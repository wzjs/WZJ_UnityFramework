using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Match3Board : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("棋盘大小")]
#endif
    public int width = 9;
    public int height = 9;


    private int normalGemCount = 4;

#if UNITY_EDITOR
    [Header("下落/消除动画时间")]
#endif
    public float fallDuration = 0.3f;
    public float eliminateDuration = 0.2f;
    public float rainbowDuration = 1;

#if UNITY_EDITOR
    [Header("格子表现")]
#endif
    public GameObject[] gemPrefabs; //0..normalGemCount-1 对应普通方块预制体
    public Transform gemsParent; // 父对象
    public Vector2 boardOrigin = Vector2.zero; // 世界坐标原点 (top-left)
    public float cellSize = 1f;
    public float spawnYOffset = 2f; // 新方块生成时的额外Y偏移，用于下落动画
#if UNITY_EDITOR
    [Header("初始棋盘 (可选)")]
#endif
    // --- new line
    public InitialBoard initialBoard;    // --- new line
    private MatchService matchService;
    // 棋盘数据
    private Gem[,] board;
    //视图对象
    private GameObject[,] gemObjects;
    private Camera camera;
    // 用于协程控制
    private bool isProcessing = false;
    private bool inputLocked = false; // 阻止玩家继续交互，直到动画结束

    // 当发生交换并触发 Rainbow 时，记录要消灭的颜色
    private GemType rainbowTarget = GemType.None;
    private static Dictionary<GemType, int> typeMap = new Dictionary<GemType, int>()
    {
        {GemType.Normal1,2},
        {GemType.Normal2,4},
        {GemType.Normal3,6},
        {GemType.Normal4,8},
    };


    void Start()
    {
        // 将 boardOrigin 设置为以当前 GameObject 的世界位置为中心的起点，使棋盘居中
        //这里 boardOrigin 表示网格的左上角世界坐标（(0,0) 映射到 top-left）
        boardOrigin = new Vector2(
            transform.position.x - (width - 1) * cellSize * 0.5f,
            transform.position.y + (height - 1) * cellSize * 0.5f
        );


        camera = GetComponent<SwipeController>().cam;
        InitBoard();
        matchService = new MatchService(new IMatchDetector[] {
            new LineDetector(),
            new SquareDetector()
            // L/T 独立检测器留空（LineDetector 已识别）
        });
        StartCoroutine(ProcessMatchesLoop());
    }

    /// <summary>
    /// 初始化棋盘，保证第一局没有天然三消
    /// </summary>
    void InitBoard()
    {
        board = new Gem[width, height];
        gemObjects = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GemType type;
                if (initialBoard != null)
                {
                    normalGemCount = initialBoard.normalGemTypes;
                    initialBoard.EnsureSize();
                    int v = initialBoard.GetInt(x, y);
                    if (System.Enum.IsDefined(typeof(GemType), v))
                    {
                        type = (GemType)v;
                    }
                    else
                    {
                        // invalid value in asset, fallback to random normal gem
                        type = (GemType)Random.Range(0, normalGemCount);
                    }
                }
                else
                {
                    do
                    {
                        type = (GemType)Random.Range(0, normalGemCount);
                    } while (
                        (x >= 2 && type == board[x - 1, y]?.type && type == board[x - 2, y]?.type) ||
                        (y >= 2 && type == board[x, y - 1]?.type && type == board[x, y - 2]?.type));
                }

                board[x, y] = new Gem(type, x, y);
                gemObjects[x, y] = CreateGemView(board[x, y], false);
            }
        }
    }

    GameObject CreateGemView(Gem gem, bool spawnAbove = true)
    {
        if (gem == null) return null;
        int typeIndex = Mathf.Clamp((int)gem.type, 0, 8);
        if (gemPrefabs == null || gemPrefabs.Length == 0)
        {
            // 无预制体时不创建视图
            return null;
        }
        GameObject prefab = gemPrefabs[typeIndex];
        if (prefab == null) return null;

        Vector3 targetPos = GridToWorld(gem.pos.x, gem.pos.y);
        Vector3 spawnPos = new Vector3(targetPos.x, targetPos.y, 0);
        if (spawnAbove) spawnPos += Vector3.up * spawnYOffset;

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity, gemsParent != null ? gemsParent : null);
        go.name = $"Gem_{gem.pos.x}_{gem.pos.y}_{gem.type}";
        if ((int)gem.type <= (int)GemType.Normal4)
        {
            var normalSpirit = go.transform.Find("Normal");
            var strengthSpirit = go.transform.Find("Strength");
            normalSpirit.gameObject.SetActive(!gem.isStrength);
            strengthSpirit.gameObject.SetActive(gem.isStrength);
        }
        if (gem.isStrength)
        {
            var ani = go.GetComponent<Animation>();
            ani.Play("4SynthesisAnimation");
        }
        if (gem.type == GemType.Rainbow)
        {
            var ani = go.GetComponent<Animation>();
            ani.Play("ColorBombGenerateAnimation");
        }

        //go.GetComponent<SpriteRenderer>().color = gem.isStrength ? Color.yellow : Color.white;
        //立即移动到目标位置（可用动画）
        if (spawnAbove)
            StartCoroutine(MoveGemView(go, targetPos, fallDuration));
        else
            go.transform.position = targetPos;

        return go;
    }



    IEnumerator MoveGemView(GameObject go, Vector3 toPos, float duration)
    {
        // If the object is null (already destroyed), exit immediately
        if (go == null) yield break;

        float t = 0f;
        // capture starting position safely (object may be destroyed later)
        Vector3 from = go.transform.position;
        while (t < duration)
        {
            // if object was destroyed during animation, stop to avoid MissingReferenceException
            if (go == null) yield break;

            t += Time.deltaTime;
            go.transform.position = Vector3.Lerp(from, toPos, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }

        // final check before setting
        if (go != null)
            go.transform.position = toPos;
    }

    /// <summary>
    /// 尝试交换两个相邻格子（玩家点击或触摸调用）
    /// 特殊类型只要交换即视为匹配：在没有形成 >=3 时也要作为初始匹配处理
    /// </summary>
    public bool TrySwap(Vector2Int a, Vector2Int b)
    {
        if (inputLocked) return false;
        if (!IsAdjacent(a, b)) return false;

        // 锁定输入，防止在动画期间再次交互
        inputLocked = true;

        //记录交换前的类型，用于 Rainbow目标选择
        GemType preA = board[a.x, a.y].type;
        GemType preB = board[b.x, b.y].type;

        //交换数据
        SwapGems(a, b);

        // 如果有 Rainbow，记录目标颜色为对方的类型（优先使用非-Rainbow 的颜色）
        if (preA == GemType.Rainbow && preB != GemType.Rainbow) rainbowTarget = preB;
        else if (preB == GemType.Rainbow && preA != GemType.Rainbow) rainbowTarget = preA;
        else if (preB == GemType.Rainbow && preA == GemType.Rainbow) rainbowTarget = GemType.Rainbow;
        else rainbowTarget = GemType.None;

        //交换视图
        SwapGemViews(a, b);

        // 检测是否至少有一方能消除
        var matchesA = GetMatchesAt(a);
        var matchesB = GetMatchesAt(b);

        // collect initial matches; if none but swap involved special type, create pseudo matches
        List<GemMatch> initialMatches = new List<GemMatch>();
        if (matchesA.Count > 0) initialMatches.AddRange(matchesA);
        if (matchesB.Count > 0) initialMatches.AddRange(matchesB);

        bool involvedSpecial = IsSpecialType(preA) || IsSpecialType(preB);
        if (involvedSpecial) //initialMatches.Count == 0 && 
        {
            // preA moved to b
            if (IsSpecialType(preA)) initialMatches.Add(new GemMatch() { Cells = new List<Vector2Int> { new Vector2Int(b.x, b.y) } });
            // preB moved to a
            if (IsSpecialType(preB)) initialMatches.Add(new GemMatch() { Cells = new List<Vector2Int> { new Vector2Int(a.x, a.y) } });
        }

        bool hasMatch = initialMatches.Count > 0;

        // 等待交换动画完成后处理结果，传入初始匹配
        StartCoroutine(HandleSwapResult(a, b, hasMatch, initialMatches));
        return true;
    }

    IEnumerator HandleSwapResult(Vector2Int a, Vector2Int b, bool matchFound, List<GemMatch> initialMatches)
    {
        // 等待交换动画
        yield return new WaitForSeconds(fallDuration);

        if (matchFound)
        {
            bool hasInitialMatches = initialMatches != null && initialMatches.Count > 0;
            // 如果有初始匹配（包括特殊单格伪匹配），先处理这些匹配
            if (hasInitialMatches)
            {
                yield return StartCoroutine(ProcessGivenMatches(initialMatches));
            }

            // 然后进入正常连锁处理以处理后续连锁
            yield return StartCoroutine(ProcessMatchesLoop(hasInitialMatches));
        }
        else
        {
            // 回退交换（并播放回退动画）
            SwapGems(a, b);
            SwapGemViews(a, b);
            // 清除可能设置的 rainbowTarget，因为交换被回退
            rainbowTarget = GemType.None;
            yield return new WaitForSeconds(fallDuration);
            inputLocked = false;
        }
    }

    IEnumerator ProcessGivenMatches(List<GemMatch> matches)
    {
        // 执行给定的消除→生成特殊道具→下落→填充（类似主循环内一轮）
        yield return StartCoroutine(EliminateGems(matches));
        GenerateSpecialGems(matches);
        yield return StartCoroutine(DropGems());
        FillEmptyCells();
        yield return new WaitForSeconds(fallDuration);
        // 清除 rainbowTarget，避免影响后续操作
        rainbowTarget = GemType.None;
    }
    /// <summary>
    /// 主循环：持续检测→消除→下落→填充，直到无消除
    /// </summary>
    IEnumerator ProcessMatchesLoop(bool initMatch = false)
    {
        if (isProcessing) yield break;
        // 锁住处理流程与输入
        isProcessing = true;
        inputLocked = true;
        bool hasMatches = initMatch;
        while (true)
        {
            var allMatches = FindAllMatches();

            if (allMatches.Count == 0)
                break;
            hasMatches = true;
            //1. 消除动画
            yield return StartCoroutine(EliminateGems(allMatches));

            //2.生成特殊道具（消除之后）
            GenerateSpecialGems(allMatches);

            //3. 下落
            yield return StartCoroutine(DropGems());

            //4. 填充顶部
            FillEmptyCells();

            // 等待下落动画完成
            yield return new WaitForSeconds(fallDuration);

        }
        isProcessing = false;
        inputLocked = false;
    }

    void SwapGemViews(Vector2Int a, Vector2Int b)
    {
        var goA = gemObjects[a.x, a.y];
        var goB = gemObjects[b.x, b.y];
        gemObjects[a.x, a.y] = goB;
        gemObjects[b.x, b.y] = goA;

        // animate positions
        if (goA != null) StartCoroutine(MoveGemView(goA, GridToWorld(b.x, b.y), fallDuration));
        if (goB != null) StartCoroutine(MoveGemView(goB, GridToWorld(a.x, a.y), fallDuration));
    }

    void SwapGems(Vector2Int a, Vector2Int b)
    {
        Gem temp = board[a.x, a.y];
        board[a.x, a.y] = board[b.x, b.y];
        board[b.x, b.y] = temp;

        // 更新坐标
        board[a.x, a.y].pos = a;
        board[b.x, b.y].pos = b;
    }




    /// <summary>
    /// 根据匹配长度/形状生成特殊道具（在消除之后调用）
    /// </summary>
    void GenerateSpecialGems(List<GemMatch> allMatches)
    {
        // avoid creating duplicates when multiple matches overlap
        var handledCenters = new HashSet<Vector2Int>();

        foreach (var match in allMatches)
        {
            var cells = match.Cells;
            Vector2Int center = cells[cells.Count / 2];

            switch (match.Kind)
            {
                case MatchKind.Square:
                case MatchKind.LShape:
                case MatchKind.TShape:
                    bool isHorizontal = cells[0].y == cells[1].y;
                    GemType newType = lastEliminatedTypes[new Vector2Int(cells[0].x, cells[0].y)];
                    board[center.x, center.y].type = newType;
                    board[center.x, center.y].isStrength = true;
                    // 创建或更新视图
                    if (gemObjects[center.x, center.y] == null)
                    {
                        var go = CreateGemView(board[center.x, center.y], false);
                        gemObjects[center.x, center.y] = go;
                    }
                    break;
                case MatchKind.Line:
                    if (cells.Count == 4) // 四消 → 横/竖炸弹
                    {
                        isHorizontal = cells[0].y == cells[1].y;
                        newType = lastEliminatedTypes[new Vector2Int(cells[0].x, cells[0].y)];
                        board[center.x, center.y].type = newType;
                        board[center.x, center.y].isStrength = true;
                        // 创建或更新视图
                        if (gemObjects[center.x, center.y] == null)
                        {
                            var go = CreateGemView(board[center.x, center.y], false);
                            gemObjects[center.x, center.y] = go;
                        }
                        handledCenters.Add(center);
                    }
                    else if (cells.Count >= 5) // 五消 → 彩球
                    {
                        if (handledCenters.Contains(center)) break;

                        board[center.x, center.y].type = GemType.Rainbow;

                        if (gemObjects[center.x, center.y] == null)
                        {
                            var go = CreateGemView(board[center.x, center.y], false);
                            gemObjects[center.x, center.y] = go;
                        }

                        handledCenters.Add(center);
                    }
                    break;
                case MatchKind.Unknown:
                    break;
                default:
                    break;
            }

        }
        lastEliminatedTypes.Clear();
    }

    private Dictionary<Vector2Int, GemType> lastEliminatedTypes = new Dictionary<Vector2Int, GemType>();

    /// <summary>
    /// 消除动画（可替换为粒子/缩放动画）
    /// </summary>
    IEnumerator EliminateGems(List<GemMatch> matches)
    {
        // 初始待消除位置集合
        HashSet<Vector2Int> toEliminate = new HashSet<Vector2Int>();
        foreach (var m in matches) foreach (var p in m.Cells) toEliminate.Add(p);

        // 使用队列迭代处理特殊道具触发，确保被后续加入的特殊道具也会触发连锁
        Queue<Vector2Int> q = new Queue<Vector2Int>(toEliminate);
        HashSet<Vector2Int> processed = new HashSet<Vector2Int>();

        while (q.Count > 0)
        {
            var pos = q.Dequeue();
            if (processed.Contains(pos)) continue;
            processed.Add(pos);

            // 安全检查：位置可能在之前已被清空为 None，但我们仍需按原始类型处理特效
            Gem gem = board[pos.x, pos.y];
            if (gem == null) continue;

            if (gem.type == GemType.RowBomb)
            {
                for (int x = 0; x < width; x++)
                {
                    var np = new Vector2Int(x, pos.y);
                    if (!toEliminate.Contains(np))
                    {
                        toEliminate.Add(np);
                        q.Enqueue(np);
                    }
                }
            }
            else if (gem.type == GemType.ColBomb)
            {
                for (int y = 0; y < height; y++)
                {
                    var np = new Vector2Int(pos.x, y);
                    if (!toEliminate.Contains(np))
                    {
                        toEliminate.Add(np);
                        q.Enqueue(np);
                    }
                }
            }
            else if (gem.type == GemType.Rainbow)
            {
                // 消灭同色：优先使用最近一次交换决定的颜色（rainbowTarget），否则回退到默认 Normal1
                GemType target = rainbowTarget != GemType.None ? rainbowTarget : GemType.Normal1;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (board[x, y].type == target || rainbowTarget == GemType.Rainbow)
                        {
                            var np = new Vector2Int(x, y);
                            if (!toEliminate.Contains(np))
                            {
                                toEliminate.Add(np);
                                q.Enqueue(np);
                            }
                        }
                    }
                }
            }
        }

        foreach (var p in toEliminate)
        {
            lastEliminatedTypes[p] = board[p.x, p.y].type;
        }

        // 执行消除（播放缩放动画然后销毁视图）
        foreach (var p in toEliminate)
        {
            GemType gemType = board[p.x, p.y].type;
            if (board[p.x, p.y].type != GemType.None)
            {
                board[p.x, p.y].isLocked = true;
                board[p.x, p.y].type = GemType.None;
                board[p.x, p.y].isStrength = false;
                //视图处理
                var go = gemObjects[p.x, p.y];
                if (go != null)
                {
                    StartCoroutine(ScaleAndDestroy(go, gemType));
                    gemObjects[p.x, p.y] = null;
                }
            }
        }
        yield return new WaitForSeconds(eliminateDuration);
    }

    IEnumerator ScaleAndDestroy(GameObject go, GemType type)
    {
        if (go == null) yield break;
        var ani = go.GetComponent<Animation>();
        if (type == GemType.Rainbow)
        {
            ani.Play("ColorBombUseAnimation");
        }
        else
        {
            ani.Play("NormalAnimation");
        }

        while (ani.isPlaying)
        {
            yield return null;
        }
        //float t = 0f;
        //Vector3 from = go.transform.localScale;
        //while (t < duration)
        //{
        //    t += Time.deltaTime;
        //    float s = Mathf.Lerp(1f, 0f, t / duration);
        //    go.transform.localScale = Vector3.one * s;
        //    yield return null;
        //}
        Destroy(go);
    }

    /// <summary>
    /// 重力下落（一次性下落到列底）
    /// </summary>
    IEnumerator DropGems()
    {
        bool anyMoved = false;

        for (int x = 0; x < width; x++)
        {
            int writeY = height - 1; // write from bottom (largest index)
            for (int y = height - 1; y >= 0; y--)
            {
                bool hasGem = board[x, y].type != GemType.None;
                if (hasGem)
                {
                    if (y != writeY)
                    {
                        // move data reference down to writeY (bottomwards)
                        board[x, writeY] = board[x, y];
                        board[x, writeY].pos = new Vector2Int(x, writeY);

                        // move view reference and animate to final position
                        gemObjects[x, writeY] = gemObjects[x, y];
                        if (gemObjects[x, writeY] != null)
                        {
                            StartCoroutine(MoveGemView(gemObjects[x, writeY], GridToWorld(x, writeY), fallDuration));
                        }

                        // clear old slot
                        gemObjects[x, y] = null;
                        board[x, y] = new Gem(GemType.None, x, y);

                        anyMoved = true;
                    }
                    else
                    {
                        // no move, ensure pos is correct
                        board[x, y].pos = new Vector2Int(x, y);
                    }

                    writeY--;
                }
            }

            //// mark remaining cells above as empty (from0 to writeY)
            //for (int y = writeY; y >= 0; y--)
            //{
            //    board[x, y] = new Gem(GemType.None, x, y);
            //    gemObjects[x, y] = null;
            //}
        }

        if (anyMoved)
            yield return new WaitForSeconds(fallDuration);
    }

    /// <summary>
    /// 从顶部生成新方块填充 (top = y=0)
    /// </summary>
    void FillEmptyCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y].type == GemType.None)
                {
                    GemType newType = (GemType)Random.Range(0, normalGemCount);
                    board[x, y] = new Gem(newType, x, y);

                    //视图：从上方spawn
                    var go = CreateGemView(board[x, y], true);
                    gemObjects[x, y] = go;
                }
            }
        }
    }

    public void RefreshBoardView()
    {
        //重新计算 boardOrigin（保持棋盘居中）
        boardOrigin = new Vector2(
            transform.position.x - (width - 1) * cellSize * 0.5f,
            transform.position.y + (height - 1) * cellSize * 0.5f
        );
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var go = gemObjects[x, y];
                if (go != null)
                {
                    go.transform.position = GridToWorld(x, y);
                }
            }
        }
    }

    // ==================== 可视化调试（可选）====================
    void OnDrawGizmos()
    {
        if (board == null) return;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = GridToWorld(x, y);
                Gizmos.color = TypeToColor(board[x, y].type);
                Gizmos.DrawSphere(pos, 0.3f);
            }
        }
    }

    Color TypeToColor(GemType type)
    {
        switch (type)
        {
            case GemType.Normal1: return Color.red;
            case GemType.Normal2: return Color.blue;
            case GemType.Normal3: return Color.green;
            case GemType.Normal4: return Color.yellow;
            //case GemType.Normal5: return Color.magenta;
            //case GemType.Normal6: return Color.cyan;
            case GemType.RowBomb: return Color.black;
            case GemType.ColBomb: return Color.white;
            case GemType.Rainbow: return new Color(1, 0.5f, 0);
            default: return Color.gray;
        }
    }

    // Called by editor tool to apply a ScriptableObject configuration to the active scene board
    public void ApplyInitialBoard(InitialBoard ib)
    {
        if (ib == null) return;
        initialBoard = ib;
        // reinitialize board using the new data
        InitBoard();
    }

    List<GemMatch> FindAllMatches()
    {
        var matches = matchService.FindAll(board);
        return matches;
    }

    List<GemMatch> GetMatchesAt(Vector2Int pos)
    {
        var matches = matchService.FindAround(board, new[] { pos });
        return matches;
    }

    public Vector3 GridToWorld(int x, int y)
    {
        // boardOrigin is top-left; y increases downward
        return new Vector3(boardOrigin.x + x * cellSize, boardOrigin.y - y * cellSize, 0f);
    }

    public static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    public static bool IsSpecialType(GemType t)
    {
        return t == GemType.RowBomb || t == GemType.ColBomb || t == GemType.Rainbow;
    }

}