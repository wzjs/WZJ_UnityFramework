using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchService
{
    private readonly List<IMatchDetector> detectors;

    public MatchService(IEnumerable<IMatchDetector> detectors)
    {
        this.detectors = detectors.ToList();
    }

    public List<GemMatch> FindAll(Gem[,] board)
    {
        var all = new List<GemMatch>();
        foreach (var d in detectors) all.AddRange(d.DetectAll(board));
        return MergeAndDeduplicate(all);
    }

    public List<GemMatch> FindAround(Gem[,] board, IEnumerable<Vector2Int> dirty)
    {
        var all = new List<GemMatch>();
        foreach (var d in detectors) all.AddRange(d.DetectAround(board, dirty));
        return MergeAndDeduplicate(all);
    }

    List<GemMatch> MergeAndDeduplicate(IEnumerable<GemMatch> raw)
    {
        var unique = new List<GemMatch>();
        var seen = new HashSet<string>();
        foreach (var m in raw)
        {
            m.Cells.Sort((a, b) => a.pos.x != b.pos.x ? a.pos.x - b.pos.x : a.pos.y - b.pos.y);
            string key = string.Join(";", m.Cells.Select(c => $"{c.pos.x},{c.pos.y}"));
            if (!seen.Contains(key))
            {
                unique.Add(m);
                seen.Add(key);
            }
        }
        return unique;
    }
}