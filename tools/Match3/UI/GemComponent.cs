using Cysharp.Threading.Tasks;
using FairyGUI;
using FairyGUI.Utils;
using UnityEngine;

public class GemComponent : GComponent
{
    private GemType _type;
    private Controller _typeController;

    public override void ConstructFromXML(XML xml)
    {
        base.ConstructFromXML(xml);
        _typeController = GetController("type");
    }

    // 消除表现
    public void PlayEliminateEffect()
    {
        var trans = GetTransition("eliminate");
        trans.Play(() => { this.visible = false; });
    }

    // 下落表现
    public async UniTask PlayDropEffect(Vector2Int to)
    {
        await UniTask.Delay(200); // 示例：延时模拟动画
        SetXY(to.x * 64, to.y * 64);
    }
}
