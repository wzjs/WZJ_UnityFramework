using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 三消方块类型
/// </summary>
public enum GemType
{
    Normal1 = 0,
    Normal2,
    Normal3,
    Normal4,
    // 特殊道具
    RowBomb,    // 横向炸弹
    ColBomb,    //纵向炸弹
    Rainbow,    // 彩球（消灭同色）
    None        // 空位（下落用）
}

/// <summary>
/// 单个格子数据
/// </summary>
[System.Serializable]
public class Gem
{
    public GemType type;
    public bool isLocked;   // 被动画锁住，防止重复消除
    public Vector2Int pos;  // 网格坐标
    public bool isStrength; //强化技
    public Gem(GemType t, int x, int y)
    {
        type = t;
        pos = new Vector2Int(x, y);
        isLocked = false;
    }
}

public enum MatchKind { Line, Square, LShape, TShape, Unknown }

public class GemMatch
{
    public MatchKind Kind;
    public List<Vector2Int> Cells = new List<Vector2Int>();
    public int Length => Cells?.Count ?? 0;
    public Vector2Int Center => Cells != null && Cells.Count > 0 ? Cells[Cells.Count / 2] : Vector2Int.zero;
}

