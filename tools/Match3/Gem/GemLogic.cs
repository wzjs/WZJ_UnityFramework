

using System.Collections.Generic;

public static partial class GemLogic
{
    private static Gem[,] board;
    public static void Init(Gem[,] targetBoard)
    {
        board = targetBoard;
    }
    private static void OnHit(ref Gem gem, object param1 = null, object param2 = null)
    {
        switch (gem.type)
        {
            case GemType.None:
                break;
            case GemType.Normal1:
            case GemType.Normal2:
            case GemType.Normal3:
            case GemType.Normal4:

                break;
            case GemType.RowBomb:
                break;
            case GemType.ColBomb:
                break;
            case GemType.Rainbow:
                break;
            default:
                break;
        }
    }

    public static void OnDestroy(ref Gem gem, HashSet<Gem> toEliminate)
    {
        OnHit(ref board[gem.pos.x - 1, gem.pos.y]);
        OnHit(ref board[gem.pos.x + 1, gem.pos.y]);
        OnHit(ref board[gem.pos.x, gem.pos.y + 1]);
        OnHit(ref board[gem.pos.x, gem.pos.y - 1]);

        switch (gem.type)
        {
            case GemType.None:
                break;
            case GemType.Normal1:
                break;
            case GemType.Normal2:
                break;
            case GemType.Normal3:
                break;
            case GemType.Normal4:
                break;
            case GemType.RowBomb:
                break;
            case GemType.ColBomb:
                break;
            case GemType.Rainbow:
                OnDestroy_RainBow(ref gem, toEliminate);
                break;
            default:
                break;
        }
    }
    public static int GetWidth(this Gem[,] board)
    {
        return board.GetLength(0);
    }
    public static int GetHeight(this Gem[,] board)
    {
        return board.GetLength(1);
    }
}

