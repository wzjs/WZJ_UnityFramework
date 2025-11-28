using System.Collections.Generic;
using UnityEngine;

// ¼ì²â 2x2 ·½¿é£¨Ìï×Ö£©
public class SquareDetector : IMatchDetector
{
    public IEnumerable<GemMatch> DetectAll(Gem[,] board)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        var res = new List<GemMatch>();
        for (int x = 0; x < width - 1; x++)
            for (int y = 0; y < height - 1; y++)
            {
                var sq = GetSquare(board, x, y);
                if (sq.Count == 4) res.Add(new GemMatch { Kind = MatchKind.Square, Cells = sq });
            }
        return res;
    }

    public IEnumerable<GemMatch> DetectAround(Gem[,] board, IEnumerable<Vector2Int> dirty)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        var res = new List<GemMatch>();
        var tried = new HashSet<(int, int)>();
        foreach (var d in dirty)
        {
            for (int ox = -1; ox <= 0; ox++)
                for (int oy = -1; oy <= 0; oy++)
                {
                    int sx = d.x + ox, sy = d.y + oy;
                    if (sx < 0 || sy < 0 || sx + 1 >= width || sy + 1 >= height) continue;
                    if (tried.Add((sx, sy)))
                    {
                        var sq = GetSquare(board, sx, sy);
                        if (sq.Count == 4) res.Add(new GemMatch { Kind = MatchKind.Square, Cells = sq });
                    }
                }
        }
        return res;
    }

    List<Vector2Int> GetSquare(Gem[,] board, int x, int y)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        var r = new List<Vector2Int>();
        if (board[x, y] == null) return r;
        var t = board[x, y].type;
        if (t == GemType.None) return r;
        if (board[x + 1, y].type == t && board[x, y + 1].type == t && board[x + 1, y + 1].type == t)
        {
            r.Add(new Vector2Int(x, y));
            r.Add(new Vector2Int(x + 1, y));
            r.Add(new Vector2Int(x, y + 1));
            r.Add(new Vector2Int(x + 1, y + 1));
        }
        return r;
    }
}