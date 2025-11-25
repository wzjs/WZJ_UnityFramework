using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Match3ViewProtoHandler
{
    protected BoardComponent _board;
    public abstract Match3ActionType Type { get; }
    public abstract UniTask Handle(Match3ViewProto proto);
    public Match3ViewProtoHandler(BoardComponent board)
    {
        _board = board;
    }
}

public class Match3StartHandler : Match3ViewProtoHandler
{
    public Match3StartHandler(BoardComponent board) : base(board)
    {
    }

    public override Match3ActionType Type => Match3ActionType.GameStart;
    public override async UniTask Handle(Match3ViewProto proto)
    {
        var startProto = proto as Match3GameStartViewProto;
        GenerateBoardUI(startProto);
        await UniTask.CompletedTask;
    }

    private void GenerateBoardUI(Match3GameStartViewProto proto)
    {
        //int boardWidth = proto.Board.GetWidth();
        //int boardHeight = proto.Board.GetHeight();
        //for (int x = 0; x < boardWidth; x++)
        //{
        //    for (int y = 0; y < boardHeight; y++)
        //    {
        //        var gemData = _board.GetGemData(x, y);
        //        var gemComp = UIPackage.CreateObject("Match3", "GemComponent") as GemComponent;
        //        gemComp.SetData(gemData);
        //        gemComp.SetXY(x * 64, y * 64);
        //        AddChild(gemComp);
        //        _board
        //        _gemMap[new Vector2Int(x, y)] = gemComp;
        //    }
        //}
    }
}

// 消除协议处理
public class EliminateHandler : Match3ViewProtoHandler
{
    public EliminateHandler(BoardComponent board) : base(board)
    {
    }

    public override Match3ActionType Type => Match3ActionType.Eliminate;
    public override async UniTask Handle(Match3ViewProto proto)
    {
        var eliminateProto = proto as Match3EliminateViewProto;
        foreach (var gem in eliminateProto.EliminatesGems)
        {
            PlayEliminateEffect(gem);
        }
        await UniTask.Delay(500);
    }

    private void PlayEliminateEffect(object gem)
    {

    }
}

// 下落协议处理
public class DropHandler : Match3ViewProtoHandler
{
    public DropHandler(BoardComponent board) : base(board)
    {
    }

    public override Match3ActionType Type => Match3ActionType.Drop;
    public override async UniTask Handle(Match3ViewProto proto)
    {
        var dropProto = proto as Match3DropViewProto;
        foreach (var kv in dropProto.DropTrack)
        {
            // 伪代码：播放下落动画
            PlayDropEffect(kv.Key, kv.Value);
        }
        await UniTask.Delay(350); // 假设动画350ms
        // 下落后UI更新
    }

    private void PlayDropEffect(Vector2Int from, Vector2Int to)
    {
        // 这里写具体的下落动画逻辑
        // 例如：GemUIManager.Instance.PlayDrop(from, to);
    }
}

// 其他协议类型同理扩展...