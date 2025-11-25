using DeepCore;
using NF;
using Project.Hotfix;
using System.Collections.Generic;
using UnityEngine;
using X.Core.Battle.Data;
using static X.Core.Battle.Data.SyncMatch3GameData;
using Random = UnityEngine.Random;

public class Match3Board
{
    public int width = 9;
    public int height = 9;


    private int normalGemCount = 4;

    public Transform gemsParent; // 父对象
    public Vector2 boardOrigin = Vector2.zero; // 世界坐标原点 (top-left)
    public float cellSize = 1f;

    // --- new line
    public InitialBoard initialBoard;    // --- new line
    private MatchService matchService;
    // 棋盘数据
    private Gem[,] board;
    private Camera camera;

    // 当发生交换并触发 Rainbow 时，记录要消灭的颜色
    private GemType rainbowTarget = GemType.None;
    private static Dictionary<GemType, int> typeMap = new Dictionary<GemType, int>()
    {
        {GemType.Normal1,2},
        {GemType.Normal2,4},
        {GemType.Normal3,6},
        {GemType.Normal4,8},
    };
    private LogicActorPlanesBattleComponent _logic;
    private LogicActorPlanesBattleComponent Logic
    {
        get
        {
            if (GameConfigManager.Instance.GameProfile == null) return null;
            if (_logic != null) return _logic;
            var _unitLogic = BattleHelper.Instance?.GetSceneLogic()?.CurActor;
            _logic = _unitLogic?.GetComponent<LogicActorPlanesBattleComponent>(LogicActorPlanesBattleComponent.ComponentName);
            return _logic;
        }
    }


    public void Start()
    {
        // 将 boardOrigin 设置为以当前 GameObject 的世界位置为中心的起点，使棋盘居中
        //这里 boardOrigin 表示网格的左上角世界坐标（(0,0) 映射到 top-left）
        //boardOrigin = new Vector2(
        //    transform.position.x - (width - 1) * cellSize * 0.5f,
        //    transform.position.y + (height - 1) * cellSize * 0.5f
        //);

        //camera = GetComponent<SwipeController>().cam;
        InitBoard();
        matchService = new MatchService(new IMatchDetector[] {
            new LineDetector(),
            new SquareDetector()
        });
        ProcessMatchesLoop();
    }

    /// <summary>
    /// 初始化棋盘，保证第一局没有天然三消
    /// </summary>
    void InitBoard()
    {
        board = new Gem[width, height];
        GemLogic.Init(board);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GemType type;
                if (initialBoard != null)
                {
                    normalGemCount = initialBoard.normalGemTypes + 1;
                    initialBoard.EnsureSize();
                    int v = initialBoard.GetInt(x, y);
                    if (System.Enum.IsDefined(typeof(GemType), v))
                    {
                        type = (GemType)v;
                    }
                    else
                    {
                        // invalid value in asset, fallback to random normal gem
                        type = (GemType)Random.Range(1, normalGemCount);
                    }
                }
                else
                {
                    do
                    {
                        type = (GemType)Random.Range(1, normalGemCount);
                    } while (
                        (x >= 2 && type == board[x - 1, y].type && type == board[x - 2, y].type) ||
                        (y >= 2 && type == board[x, y - 1].type && type == board[x, y - 2].type));
                }

                board[x, y] = Gem.Create(type, x, y);
            }
        }
    }

    /// <summary>
    /// 尝试交换两个相邻格子（玩家点击或触摸调用）
    /// 特殊类型只要交换即视为匹配：在没有形成 >=3 时也要作为初始匹配处理
    /// </summary>
    public List<Match3ViewProto> TrySwap(Vector2Int a, Vector2Int b)
    {
        if (!IsAdjacent(a, b)) return report;

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

        // 检测是否至少有一方能消除
        var matchesA = GetMatchesAt(a);
        var matchesB = GetMatchesAt(b);

        List<GemMatch> initialMatches = new List<GemMatch>();
        if (matchesA.Count > 0) initialMatches.AddRange(matchesA);
        if (matchesB.Count > 0) initialMatches.AddRange(matchesB);

        if (IsSpecialType(preA)) initialMatches.Add(new GemMatch() { Cells = new List<Gem> { Gem.Create(preA, b.x, b.y, preA == GemType.Rainbow ? (int)rainbowTarget : 0) } });
        if (IsSpecialType(preB)) initialMatches.Add(new GemMatch() { Cells = new List<Gem> { Gem.Create(preB, a.x, a.y, preB == GemType.Rainbow ? (int)rainbowTarget : 0) } });

        bool hasMatch = initialMatches.Count > 0;
        HandleSwapResult(a, b, hasMatch, initialMatches);
        return report;
    }

    private void HandleSwapResult(Vector2Int a, Vector2Int b, bool matchFound, List<GemMatch> initialMatches)
    {
        report.Add(new Match3TrySwapViewProto() { IsSuccess = matchFound, SourcePos = a, TargetPos = b });
        if (matchFound)
        {
            ProcessMatchesLoop(initialMatches);
        }
        else
        {
            SwapGems(a, b);
            rainbowTarget = GemType.None;
        }
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

    List<Match3ViewProto> report = new List<Match3ViewProto>();
    /// <summary>
    /// 主循环：持续检测→消除→下落→填充，直到无消除
    /// </summary>
    private void ProcessMatchesLoop(List<GemMatch> initMatch = null)
    {
        report.Clear();
        bool hasMatches = initMatch != null && initMatch.Count > 0;
        List<GemMatch> allMatches = initMatch;
        do
        {
            report.Add(new Match3RoundStartViewProto());
            EliminateGems(allMatches);
            GenerateSpecialGems(allMatches);
            DropGems();
            FillEmptyCells();

            rainbowTarget = GemType.None;
            allMatches = FindAllMatches();
            if (allMatches.Count == 0)
                break;
            hasMatches = true;
            report.Add(new Match3RoundEndViewProto());
        }
        while (true);

        if (hasMatches)
        {
            Logic?.SendMatch3RoundOver();
        }
    }

    /// <summary>
    /// 消除动画（可替换为粒子/缩放动画）
    /// </summary>
    private void EliminateGems(List<GemMatch> matches)
    {
        HashSet<Gem> toEliminate = new HashSet<Gem>();
        foreach (var m in matches) foreach (var p in m.Cells) toEliminate.Add(p);

        Queue<Gem> q = new Queue<Gem>(toEliminate);
        HashSet<Gem> processed = new HashSet<Gem>();
        HashSet<Gem> result = new HashSet<Gem>();
        while (q.Count > 0)
        {
            var gem = q.Dequeue();
            var pos = gem.pos;
            if (processed.Contains(gem)) continue;
            processed.Add(gem);
            result.Clear();
            GemLogic.OnDestroy(ref gem, result);
            q.EnqueueRange(result);
            toEliminate.AddRange(result);
        }

        Match3EliminateViewProto proto = new Match3EliminateViewProto();
        proto.EliminatesGems.AddRange(toEliminate);

        List<SyncMatch3GameData> data = new List<SyncMatch3GameData>();
        foreach (var p in toEliminate)
        {
            if (p.type != GemType.None)
            {
                PreperEliminateProto(p.pos, data);
                board[p.pos.x, p.pos.y].Empty();
            }
        }

        Logic?.SendMatch3GameData(data);
    }

    void GenerateSpecialGems(List<GemMatch> allMatches)
    {
        Match3SpecialViewProto proto = new Match3SpecialViewProto();
        report.Add(proto);
        foreach (var match in allMatches)
        {
            match.Result(board, proto);
        }
    }

    private void DropGems()
    {
        Match3DropViewProto proto = new Match3DropViewProto();
        report.Add(proto);
        for (int x = 0; x < width; x++)
        {
            int writeY = height - 1;
            for (int y = height - 1; y >= 0; y--)
            {
                bool hasGem = board[x, y].type != GemType.None;
                if (hasGem)
                {
                    if (y != writeY)
                    {
                        Vector2Int from = new Vector2Int(x, y);
                        Vector2Int to = new Vector2Int(x, writeY);
                        board[x, writeY] = board[x, y];
                        board[x, writeY].pos = to;
                        board[x, y] = Gem.Create(GemType.None, from);
                        if (!proto.DropTrack.ContainsKey(from))
                        {
                            proto.DropTrack.Add(from, to);
                        }
                    }
                    writeY--;
                }
            }
        }
    }

    void FillEmptyCells()
    {
        Match3SpawnViewProto proto = new Match3SpawnViewProto();
        report.Add(proto);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y].type == GemType.None)
                {
                    GemType newType = (GemType)Random.Range((int)GemType.Normal1, normalGemCount + 1);
                    Gem newGem = Gem.Create(newType, new Vector2Int(x, y));
                    board[x, y] = newGem;
                    proto.NewGems.Add(newGem);
                }
            }
        }
    }
    private void PreperEliminateProto(Vector2Int gem, List<SyncMatch3GameData> data)
    {
        GridType gridType = GridType.Element1;
        if (typeMap.TryGetValue(board[gem.x, gem.y].type, out int val))
        {
            gridType = (GridType)val;
        }
        var gridScreenPos = camera.WorldToScreenPoint(GridToWorld(gem.x, gem.y));
        Ray ray = Camera.main.ScreenPointToRay(gridScreenPos);
        var raycastHitInfos = new RaycastHit[10];
        Vector3 worldHit = new Vector3();
        int hitCount = Physics.RaycastNonAlloc(ray, raycastHitInfos, Mathf.Infinity, Project.Hotfix.Constant.Layer.TerrainDetectionLayer);
        if (hitCount > 0)
        {
            var hit = raycastHitInfos[0];
            worldHit = new Vector3(hit.point.x, hit.point.y, hit.point.z);
        }
        if (Logic != null)
        {
            var count = BattleHelper.Instance?.GetSceneLogic()?.Parent?.GetVoxelYCount() ?? 0;
            data.Add(new SyncMatch3GameData()
            {
                IsStarSkill = board[gem.x, gem.y].param1 == 1,
                Type = gridType,
                Position = worldHit.ConvertToLayerPos(count),
            });
        }

        Debug.Log("Eliminate Gem at " + gem + " Type: " + gridType + " Pos X:" + gem.x + " Pos Y:" + gem.y + " IsSkill:" + board[gem.x, gem.y].param1 == 1 + " worldHit:" + worldHit);
    }

    //IEnumerator ScaleAndDestroy(GameObject go, GemType type)
    //{
    //    if (go == null) yield break;
    //    var ani = go.GetComponent<Animation>();
    //    if (type == GemType.Rainbow)
    //    {
    //        bool isDouble = rainbowTarget == GemType.Rainbow;
    //        if (!isDouble)
    //        {
    //            HotfixEntry.Event?.FireNow(this, Match_5MatchArgs.Fill(false));
    //        }
    //        ani.Play("ColorBombUseAnimation");
    //    }
    //    else
    //    {
    //        ani.Play("NormalAnimation");
    //    }

    //    while (ani.isPlaying)
    //    {
    //        yield return null;
    //    }
    //    //float t = 0f;
    //    //Vector3 from = go.transform.localScale;
    //    //while (t < duration)
    //    //{
    //    //    t += Time.deltaTime;
    //    //    float s = Mathf.Lerp(1f, 0f, t / duration);
    //    //    go.transform.localScale = Vector3.one * s;
    //    //    yield return null;
    //    //}
    //    Destroy(go);
    //}



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

    #region // ==================== 可视化调试（可选）====================
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


    //public void RefreshBoardView()
    //{
    //    //重新计算 boardOrigin（保持棋盘居中）
    //    boardOrigin = new Vector2(
    //        transform.position.x - (width - 1) * cellSize * 0.5f,
    //        transform.position.y + (height - 1) * cellSize * 0.5f
    //    );
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            var go = gemObjects[x, y];
    //            if (go != null)
    //            {
    //                go.transform.position = GridToWorld(x, y);
    //            }
    //        }
    //    }
    //}
    #endregion

}