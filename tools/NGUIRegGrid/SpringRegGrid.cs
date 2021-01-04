using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIDragRedGrid))]
public class SpringRegGrid : MonoBehaviour {

    UIDragRedGrid UIDragRed;
    bool isEnable = false;
    Vector3 fromDelta;
    Vector3 toDelta;
    System.Action m_FinishCallBack = null;
    private float m_CurTime = 0;
    private float m_DuringTime = 1;
	// Use this for initialization
	void Start () {
		
	}
    //float num = 0;
	// Update is called once per frame
	void Update () {
        if (isEnable)
        {
            Vector3 value = NGUIMath.SpringLerp(fromDelta, toDelta, 8, Time.deltaTime);
            Vector3 offset = value - fromDelta;
            if (Mathf.Abs(offset.x) < 0.0001f)
            {
                offset = toDelta - fromDelta;
                value = toDelta;
            }
            fromDelta = value;
            UIDragRed.OnMove(offset);
            if (fromDelta == toDelta)
            {
                isEnable = false;
                m_FinishCallBack?.Invoke();
                return;
            }
        }
    }

    public static void Begin(UIDragRedGrid reg,Vector3 fromFactor,Vector3 toFactor = default(Vector3),System.Action finishCallBack = null,float DuringTime = 1)
    {
        var com = reg.GetComponent<SpringRegGrid>();
        if (com == null)
            com = reg.gameObject.AddComponent<SpringRegGrid>();
        com.UIDragRed = reg;
        com.isEnable = true;
        com.fromDelta = fromFactor;
        com.toDelta = toFactor;
        com.m_FinishCallBack = finishCallBack;
        com.m_DuringTime = DuringTime;
        com.m_CurTime = 0;
        //Debug.Log("all>>" + (com.toDelta.x - com.fromDelta.x) );
    }

    public static void Stop(UIDragRedGrid reg)
    {
        var com = reg.GetComponent<SpringRegGrid>();
        if (com != null)
            com.isEnable = false;
    }
}
