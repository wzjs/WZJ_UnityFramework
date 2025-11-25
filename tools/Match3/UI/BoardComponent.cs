using Cysharp.Threading.Tasks;
using FairyGUI;
using FairyGUI.Utils;
using System.Collections.Generic;
using UnityEngine;


public class BoardComponent : GComponent
{
    private Match3Board _board;
    private Dictionary<Match3ActionType, Match3ViewProtoHandler> _handlers;
    private Dictionary<Vector2Int, GemComponent> _gemMap = new Dictionary<Vector2Int, GemComponent>();

    public override void ConstructFromXML(XML xml)
    {
        base.ConstructFromXML(xml);
    }

    public void Init()
    {
        _board = new Match3Board();
        _board.Start();

        _handlers = new Dictionary<Match3ActionType, Match3ViewProtoHandler>
        {
            {Match3ActionType.GameStart,new Match3StartHandler(this) },
            { Match3ActionType.Eliminate, new EliminateHandler(this) },
            { Match3ActionType.Drop, new DropHandler(this) },
            // 其他协议类型...
        };
    }

    // 提供棋子查找接口给协议处理类
    public GemComponent GetGemComponent(Vector2Int pos)
    {
        _gemMap.TryGetValue(pos, out var comp);
        return comp;
    }

    private void TrySwap(Vector2Int from, Vector2Int to)
    {
        var report = _board.TrySwap(from, to);
        Process(report).Forget();
    }

    private async UniTaskVoid Process(List<Match3ViewProto> protos)
    {
        foreach (var proto in protos)
        {
            if (_handlers.TryGetValue(proto.Type, out var handler))
            {
                await handler.Handle(proto);
            }
        }
    }
}

