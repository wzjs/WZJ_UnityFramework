using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 三消方块类型
/// </summary>
public enum GemType
{
    None = 0,
    Normal1,
    Normal2,
    Normal3,
    Normal4,
    // 特殊道具
    RowBomb,    // 横向炸弹
    ColBomb,    //纵向炸弹
    Rainbow,    // 彩球（消灭同色）
}


/// <summary>
/// 单个格子数据
/// </summary>
[System.Serializable]
public struct Gem
{
    public GemType type { get; set; }
    public Vector2Int pos { get; set; }
    public int param1;

    private Gem(GemType t, Vector2Int pos, int param1 = 0)
    {
        type = t;
        this.pos = pos;
        this.param1 = param1;
    }

    public void Empty()
    {
        type = GemType.None;
        param1 = 0;
    }

    public static Gem Create(GemType t, int x, int y, int param1 = 0)
    {
        return new Gem(t, new Vector2Int(x, y), param1);
    }
    public static Gem Create(GemType t, Vector2Int pos, int param1 = 0)
    {
        return new Gem(t, pos, param1);
    }
}

public enum MatchKind { Line, Square, LShape, TShape, Unknown }

public class GemMatch
{
    public List<Gem> Cells = new List<Gem>();
    public int Length => Cells?.Count ?? 0;
    public Vector2Int Center => Cells != null && Cells.Count > 0 ? Cells[Cells.Count / 2].pos : Vector2Int.zero;
    public MatchKind Kind;

    public void Result(Gem[,] board, Match3SpecialViewProto proto)
    {
        switch (Kind)
        {
            case MatchKind.Square:
            case MatchKind.LShape:
            case MatchKind.TShape:
                SetCenterGem(board, Cells[0].type, true, proto);
                break;
            case MatchKind.Line:
                if (Cells.Count == 4)
                {
                    SetCenterGem(board, Cells[0].type, true, proto);
                }
                else if (Cells.Count >= 5)
                {
                    SetCenterGem(board, GemType.Rainbow, false, proto);
                }
                break;
            case MatchKind.Unknown:
                break;
            default:
                break;
        }
    }

    private void SetCenterGem(Gem[,] board, GemType type, bool isStrength, Match3SpecialViewProto proto)
    {
        Gem newGem = Gem.Create(type, Center.x, Center.y, isStrength ? 1 : 0);
        board[Center.x, Center.y] = newGem;
        proto.NewGems.Add(newGem);
    }
}

public enum Match3ActionType
{
    GameStart = 0,
    TrySwap,
    RoundStart,
    Eliminate,   // 消除
    Special,     // 生成特殊道具
    Drop,        // 下落
    Spawn,       // 新生成
    RoundEnd,
}

public class Match3ViewProto
{
    public Match3ActionType Type;
}
public class Match3GameStartViewProto : Match3ViewProto
{
    public Gem[,] Board;
}

public class Match3TrySwapViewProto : Match3ViewProto
{
    public Vector2Int SourcePos;
    public Vector2Int TargetPos;
    public bool IsSuccess;
}


public class Match3EliminateViewProto : Match3ViewProto
{
    public List<Gem> EliminatesGems;
}

public class Match3SpecialViewProto : Match3ViewProto
{
    public List<Gem> NewGems;
}

public class Match3DropViewProto : Match3ViewProto
{
    //from -> to
    public Dictionary<Vector2Int, Vector2Int> DropTrack;
}

public class Match3SpawnViewProto : Match3ViewProto
{
    public List<Gem> NewGems;
}

public class Match3RoundStartViewProto : Match3ViewProto
{

}

public class Match3RoundEndViewProto : Match3ViewProto
{

}