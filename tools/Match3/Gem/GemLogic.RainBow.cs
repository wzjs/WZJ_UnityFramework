

using System.Collections.Generic;

public static partial class GemLogic
{
    public static void OnDestroy_RainBow(ref Gem gem, HashSet<Gem> toEliminate)
    {
        int width = board.GetWidth();
        int height = board.GetHeight();
        GemType rainbowTarget = (GemType)gem.param1;
        GemType target = rainbowTarget != GemType.None ? rainbowTarget : GemType.Normal1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y].type == target || rainbowTarget == GemType.Rainbow)
                {
                    var np = board[x, y];
                    if (!toEliminate.Contains(np))
                    {
                        toEliminate.Add(np);
                    }
                }
            }
        }
    }


}

