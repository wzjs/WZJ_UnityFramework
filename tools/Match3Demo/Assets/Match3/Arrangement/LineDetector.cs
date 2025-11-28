using System.Collections.Generic;
using UnityEngine;

// 检测横/竖连续 >=3 的直线，同时在交点尝试识别 L / T 型
public class LineDetector : IMatchDetector
{
    public IEnumerable<GemMatch> DetectAll(Gem[,] board)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        var res = new List<GemMatch>();
        var visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!InBounds(x, y, width, height)) continue;
                var g = board[x, y];
                if (g == null || g.type == GemType.None || g.isLocked) continue;

                var h = GetLine(board, x, y, true);
                var v = GetLine(board, x, y, false);

                if (TryAddCombinedMatch(x, y, h, v, res, visited))
                    continue;

                if (h.Count >= 3)
                {
                    bool anyVisited = false;
                    foreach (var p in h) if (visited[p.x, p.y]) { anyVisited = true; break; }
                    if (!anyVisited) { res.Add(new GemMatch { Kind = MatchKind.Line, Cells = h }); MarkVisited(visited, h); }
                }
                if (v.Count >= 3)
                {
                    bool anyVisited = false;
                    foreach (var p in v) if (visited[p.x, p.y]) { anyVisited = true; break; }
                    if (!anyVisited) { res.Add(new GemMatch { Kind = MatchKind.Line, Cells = v }); MarkVisited(visited, v); }
                }
            }
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
            for (int ox = -2; ox <= 2; ox++)
                for (int oy = -2; oy <= 2; oy++)
                {
                    int cx = d.x + ox, cy = d.y + oy;
                    if (!InBounds(cx, cy, width, height)) continue;
                    if (!tried.Add((cx, cy))) continue;

                    var g = board[cx, cy];
                    if (g == null || g.type == GemType.None || g.isLocked) continue;

                    var h = GetLine(board, cx, cy, true);
                    var v = GetLine(board, cx, cy, false);

                    if (TryAddCombinedMatch(cx, cy, h, v, res, null))
                        continue;

                    if (h.Count >= 3) res.Add(new GemMatch { Kind = MatchKind.Line, Cells = h });
                    if (v.Count >= 3) res.Add(new GemMatch { Kind = MatchKind.Line, Cells = v });
                }
        }
        return res;
    }

    bool TryAddCombinedMatch(int cx, int cy, List<Vector2Int> h, List<Vector2Int> v, List<GemMatch> res, bool[,] visited)
    {
        if (h.Count < 3 || v.Count < 3) return false;

        int left = 0, right = 0, up = 0, down = 0;
        foreach (var p in h) 
        { 
            if (p.x < cx) 
                left++; 
            else if (p.x > cx) 
                right++; 
        }
        foreach (var p in v) 
        {
            if (p.y < cy) 
                up++; 
            else if (p.y > cy) 
                down++; 
        }

        bool hBoth = left > 0 && right > 0;
        bool vBoth = up > 0 && down > 0;
        bool hSingle = (left == 0) ^ (right == 0);
        bool vSingle = (up == 0) ^ (down == 0);

        var set = new HashSet<Vector2Int>(h);
        foreach (var p in v) set.Add(p);

        if (hSingle && vSingle)
        {
            res.Add(new GemMatch { Kind = MatchKind.LShape, Cells = new List<Vector2Int>(set) });
            if (visited != null) MarkVisited(visited, set);
            return true;
        }

        if ((hBoth && vSingle) || (vBoth && hSingle) || (hBoth && vBoth))
        {
            res.Add(new GemMatch { Kind = MatchKind.TShape, Cells = new List<Vector2Int>(set) });
            if (visited != null) MarkVisited(visited, set);
            return true;
        }

        // fallback: treat combined as T（与原实现一致，确保不漏报）
        res.Add(new GemMatch { Kind = MatchKind.TShape, Cells = new List<Vector2Int>(set) });
        if (visited != null) MarkVisited(visited, set);
        return true;
    }

    List<Vector2Int> GetLine(Gem[,] board, int x, int y, bool horizontal)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        var list = new List<Vector2Int>();
        if (!InBounds(x, y, width, height)) return list;
        var g = board[x, y];
        if (g == null || g.type == GemType.None) return list;
        if (horizontal)
        {
            int lx = x;
            while (lx >= 0 && board[lx, y] != null && board[lx, y].type == g.type) 
            { 
                list.Add(new Vector2Int(lx, y)); lx--; 
            }

            list.Reverse();
            int rx = x + 1;
            while (rx < width && board[rx, y] != null && board[rx, y].type == g.type) 
            { 
                list.Add(new Vector2Int(rx, y)); rx++; 
            }
        }
        else
        {
            int uy = y;
            while (uy >= 0 && board[x, uy] != null && board[x, uy].type == g.type) 
            { 
                list.Add(new Vector2Int(x, uy)); uy--; 
            }

            list.Reverse();
            int dy = y + 1;
            while (dy < height && board[x, dy] != null && board[x, dy].type == g.type) 
            { 
                list.Add(new Vector2Int(x, dy)); dy++; 
            }
        }
        return list;
    }

    void MarkVisited(bool[,] visited, IEnumerable<Vector2Int> cells)
    {
        foreach (var p in cells) if (InBounds(p.x, p.y, visited.GetLength(0), visited.GetLength(1))) visited[p.x, p.y] = true;
    }

    static bool InBounds(int x, int y, int width, int height) => x >= 0 && y >= 0 && x < width && y < height;
}