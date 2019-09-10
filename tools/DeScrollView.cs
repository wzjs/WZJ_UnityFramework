using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public delegate void CellCallback(GameObject obj, int index);

public class CellItem
{
    public GameObject obj;
    public int index;
}
public class DeScrollView : MonoBehaviour {

    //content总长度 % (item的width/height + space) - space >0    + 2
    public ScrollRect mainContainer;
    public GameObject goCell;
    private RectTransform sContent;
    private LayoutGroup lGrid;
    public float space = 0;
    private int viewNums = 0; //可视数量
    private const int cacheCount = 4;
    private int curIndex = 0;
    private int maxNum = 0;
    private List<CellItem> allObjs = new List<CellItem>();
    private CellCallback cellCallback;
    private float lastPos = 0;

    private float containerLength = 0;
    private float targetLength = 0;
    public int allGoNums
    {
        get
        {
            return viewNums + cacheCount;
        }
    }
    private void Awake()
    {
        if(mainContainer == null)
        {
            Debug.Log("main Target is Null,Please Set Value");
            return;
        }

        //sContent = mainContainer.content;
        //int curNum = mainContainer.content.
    }

    public void Init(int maxNum,CellCallback callback)
    {
        sContent = mainContainer.content;
        this.maxNum = maxNum;
        cellCallback = callback;
        SetViewNum(true);
        SetCells();
    }

    private void SetCells()
    {
        CellItem item;
        for(int i = 0; i < allGoNums; i++)
        {
            item = new CellItem();
            var go = Instantiate(goCell, sContent.transform);
            go.name = i + 1+"";
            item.obj = go;
            item.index = i;
            cellCallback(go, i);
            //go.SetActive(false);
            allObjs.Add(item);
        }
    }

    private void SetViewNum(bool hor)
    {
        var cRect = mainContainer.GetComponent<RectTransform>();
        containerLength = hor ? cRect.rect.width: cRect.rect.height;
        var cpRect = goCell.GetComponent<RectTransform>();
        targetLength = hor ? cpRect.rect.width : cpRect.rect.height;
        float express = containerLength % (targetLength + space) - space;
        int fillsNumber = (int)(containerLength / (targetLength + space));
        if (express > 0)
            fillsNumber += 2;
        else if (express == 0)
            fillsNumber += 1;
        viewNums = fillsNumber;
    }

    public void InitItems()
    {

    }

	// Use this for initialization
	void Start () {
		
	}

    private void SetCell()
    {

    }
    
    //private void LateUpdate()
    //{
        
    //}
    // Update is called once per frame
    void Update() {
        //暂时只做水平滑动
        CellItem target1 = GetLipObj(2);
        Debug.LogError("index1111111>>>>>>>>>>>>>" + target1.obj.transform.GetSiblingIndex());
        float curPos = sContent.localPosition.x;
        if(curPos < lastPos)
        {
            //往左滑动
            CellItem target = GetLipObj(2);
            if(target.index >= maxNum - 1)
            {
                lastPos = curPos;
                Debug.Log(string.Format("Now is get List ending,target.index:{0},maxNum:{1}",target.index,maxNum));
                return;
            }
            if (target.obj.transform.position.x - targetLength / 2 <= mainContainer.transform.position.x + containerLength / 2)
            {
                Debug.LogError("index>>>>>>>>>>>>>" + target.obj.transform.GetSiblingIndex());
                Debug.LogError("name>>>>>>>>>>>>>" + target.obj.name);
                Debug.LogError("targetPos.x>>>>>>>>>>>>>" + target.obj.transform.position.x);

                //开始设置下一个cell
                bool result = SetOrder(target, 2);
                int nowIndex = allObjs.IndexOf(target);
                var nextItem = allObjs[nowIndex + 1];
                cellCallback(nextItem.obj, target.index + 1);
                nextItem.index = target.index + 1;
                //if (result)
                //{
                   
                //}

            }
        }
        else if(curPos > lastPos)
        {
            //Debug.Log(string.Format("curPos>>>{0},lastPos>>>{1}", curPos, lastPos));
            //往右滑动
            CellItem target = GetLipObj(1);
            if (target.index <= 0)
            {
                lastPos = curPos;
                Debug.Log(string.Format("Now is get List ending,target.index:{0},maxNum:{1}", target.index, maxNum));
                return;
            }
            if (target.obj.transform.position.x + targetLength / 2 >= mainContainer.transform.position.x - containerLength / 2)
            {
                //开始设置下一个cell
                bool result = SetOrder(target, 1);
                int nowSiblIndex = allObjs.IndexOf(target);
                cellCallback(allObjs[nowSiblIndex - 1].obj, target.index - 1);
                allObjs[nowSiblIndex - 1].index = target.index - 1;
                //if (result)
                //{
                   
                //}
            }
        }
        lastPos = curPos;
	}

    //type 1 left 2 right
    private CellItem GetLipObj(int type)
    {
  
        for (int i = 0; i < allGoNums; i++)
        {
            CellItem temp;
            if (type == 1)
            {
                temp = allObjs[i];
                if(temp.obj.transform.position.x + targetLength / 2  >= mainContainer.transform.position.x - containerLength / 2)
                {
                    return temp;
                }
            }
            else
            {
                temp = allObjs[allGoNums - i - 1];
                if (temp.obj.transform.position.x - targetLength / 2 <= mainContainer.transform.position.x + containerLength / 2)
                {
                    return temp;
                }
            }
        }
        return allObjs[0];
    }
    //
    private bool SetOrder(CellItem item,int type)
    {
        int index = item.obj.transform.GetSiblingIndex();
        if (type == 1)
        {
            if (index < 1)
            {
                CellItem lastTemp = allObjs[allGoNums - 1];
                lastTemp.obj.transform.SetSiblingIndex(0);
                allObjs.Remove(lastTemp);
                allObjs.Insert(0, lastTemp);
                var lastPosTest = sContent.transform.localPosition;
                sContent.transform.localPosition += new Vector3(-targetLength - space, 0, 0);
                //lastPos = sContent.transform.localPosition.x;
                Debug.LogError(string.Format("CellItem Name: {0},lastPosition:{1},nowPosition: {2}", lastTemp.obj.name, lastPosTest, sContent.transform.localPosition));
                return true;
            }
        }
        else
        {
            if(index > allGoNums - 2)
            {
                CellItem lastTemp = allObjs[0];
                lastTemp.obj.transform.SetSiblingIndex(allGoNums - 1);
                allObjs.Remove(lastTemp);
                allObjs.Insert(allGoNums-1, lastTemp);
                var lastPosTest = sContent.transform.localPosition;
                sContent.transform.localPosition += new Vector3(targetLength + space, 0, 0);
                Debug.LogError("SilbingIndex>>>>>>>>>>>>>>" + lastTemp.obj.transform.GetSiblingIndex());
                //lastPos = sContent.transform.localPosition.x;
                Debug.LogError(string.Format("CellItem Name: {0},lastPosition:{1},nowPosition: {2}", lastTemp.obj.name, lastPosTest, sContent.transform.localPosition));
                return true;
            }
        }
        return false;
    }

}
