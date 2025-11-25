using System.Collections.Generic;
using UnityEngine;

public interface IMatchDetector
{
    IEnumerable<GemMatch> DetectAll(Gem[,] board);
    IEnumerable<GemMatch> DetectAround(Gem[,] board, IEnumerable<Vector2Int> dirty);
}