using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegGridCell : MonoBehaviour {
    private float m_Value = 0;
    public float Value {
        get {
            return m_Value;
        }
        set {
            bool beforeForward = IsForward(m_Value);
            bool curForward = IsForward(value);

            if (beforeForward != curForward)
            {
                AdjustDepth(beforeForward ? -m_RegGrid.DepthOffset : m_RegGrid.DepthOffset);
            }
            m_Value = value;
            if (m_Value == 1)
                m_Value = 0;
        }
    }
    public UIEventListener Listener;
    private float m_InitFactor = 0;
    private UIDragRedGrid m_SpringRegAnim;
    private RegGrid m_RegGrid;
    private bool isStarted = false;
	// Use this for initialization
	void Start () {
        isStarted = true;
        bool isForward = IsForward(m_Value);
        AdjustDepth(isForward ? m_RegGrid.DepthOffset : 0);
    }

    public void MoveToCenter(GameObject obj)
    {
        float result = Value;
        if (result > 0.5f)
            result = 1 - result;
        else
            result = - result;
        SpringRegGrid.Begin(m_SpringRegAnim, new Vector3(0, 0, 0), new Vector3(result, 0, 0));
    }

    private void AdjustDepth(int depthOffset)
    {
        if (!isStarted) return;
        var components = transform.GetComponentsInChildren<UIWidget>(true);
        for (int i = 0; i < components.Length; i++)
        {
            components[i].depth = components[i].depth + depthOffset;
        }
    }

    public void SetData(float initFactor,RegGrid regGrid)
    {
        m_Value = initFactor;
        m_InitFactor = initFactor;
        m_RegGrid = regGrid;
    }

    public void SetCenterEvent(UIDragRedGrid drag)
    {
        m_SpringRegAnim = drag;
        if (Listener != null)
            Listener.onClick += MoveToCenter;
        else
            Debug.LogError("RegGridCell Click Event Must Have UIEventListner Component");
    }

    public float GetInitFactor()
    {
        return m_InitFactor;
    }


    public static bool IsForward(float pValue)
    {
        return (pValue >= 0 && pValue <= 0.25f) || (pValue >= 0.75f && pValue <= 1);
    }

}
