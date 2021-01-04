using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//使用弧度曲线来修改每个物体的坐标
public class RegGrid:MonoBehaviour
{
    private readonly AnimationCurve AnimationCurve = new AnimationCurve(new Keyframe[] {
        new Keyframe(0,0,4,4,0.33f,0.33f),
        new Keyframe(0.25f,1,0,0,0.33f,0.33f),
        new Keyframe(0.5f,0,-4,-4,0.33f,0),
        new Keyframe(0.75f,-1,0.01826f,0.01826f),
        new Keyframe(1,0,4,0),
});

    private readonly AnimationCurve VerAnimationCurve = new AnimationCurve(new Keyframe[] {
        new Keyframe(0,-1,0,4),
        new Keyframe(0.5f,1),
        new Keyframe(1,-1,-4,0),
});
    public int DepthOffset = 2;
    public System.Action<RegGridCell,bool> onCenter;
    public float HorRadius = 500f;
    public float VerRadius = 200f;
    private float m_CurValue = 0f;
    [HideInInspector]
    public UIDragRedGrid UIDragRed;
    public float CurExtraOffset { get { return m_CurValue; }
        set {
            m_CurValue += value;
            if (m_CurValue >= 1)
                m_CurValue = m_CurValue - 1;
            else if (m_CurValue < 0)
                m_CurValue = 1 + m_CurValue;
        }
    }

    public float Interval { get; set; }
    protected void Init()
    {
        int maxCount = transform.childCount;
        Interval = 1.0f / maxCount;
        for (int i = 0; i < maxCount; i++)
        {
            var t = transform.GetChild(i);
            RegGridCell cell = t.gameObject.GetComponent<RegGridCell>();
            if (cell == null) cell =  t.gameObject.AddComponent<RegGridCell>();
            cell.SetData(Interval * i,this);
        }
    }

    public void Locate(float value)
    {
        float param = Mathf.Clamp01(value);
        m_CurValue = param;
        ResetPosition();
    }
    public void LocateByIndex(int index,bool needAnim)
    {
        //Debug.LogError("LocateByIndex>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>index" + index);
        if (needAnim)
        {
            var t = transform.GetChild(index);
            if (t)
            {
                var regCell = t.GetComponent<RegGridCell>();
                regCell.MoveToCenter(null);
            }
        }else
        {
            m_CurValue = 1 - Interval * index;
            ResetPosition();
        }
    }

    //在同一帧直接调用
    private void OnTransformChildrenChanged()
    {
        //Debug.Log("OnTransformChildrenChanged>>>");
        Init();
        if (UIDragRed != null)
            UIDragRed.AddUIEventPenetrateNGUI();
        ResetPosition();
    }
    private void Awake()
    {
        Init();
    }
    private void Start()
    {
        //ResetPosition();
    }
    [ContextMenu("Excute")]
    public void ResetPosition()
    {
        for (int i = 0, imax = transform.childCount; i < imax; ++i)
        {
            Transform t = transform.GetChild(i);

            Vector3 pos = t.localPosition;
            float depth = pos.z;

            float time = (1.0f/imax) * i + m_CurValue;
            if(time >= 1)
            {
                time = time - 1;
            }
            float value = AnimationCurve.Evaluate(time);
            float verValue = VerAnimationCurve.Evaluate(time);
            pos = new Vector3(value * HorRadius, verValue * VerRadius, depth);
            t.localPosition = pos;
            //Debug.Log(pos + " Name>>" + t.name + " time>>" + time + "  m_CurValue>>" + m_CurValue);
            if (Application.isPlaying)
            {
                RegGridCell cell = t.gameObject.GetComponent<RegGridCell>();
                if (cell != null)
                {
                    //float lastValue = cell.Value;
                    cell.Value = (float)System.Math.Round(time, 2);
                    onCenter?.Invoke(cell, cell.Value == 0);
                }
            }  
        }
    }
}
