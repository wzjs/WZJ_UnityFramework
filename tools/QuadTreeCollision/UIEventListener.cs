using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
//using Unity.VisualScripting;

public class UIEventListener : EventTrigger
{
    public delegate void VoidDelegate(GameObject go);
    public delegate void BoolDelegate(GameObject go, bool isValue);
    public delegate void FloatDelegate(GameObject go, float fValue);
    public delegate void IntDelegate(GameObject go, int iIndex);
    public delegate void StringDelegate(GameObject go, string strValue);

    public VoidDelegate onSubmit;
    public VoidDelegate onClick;
    public VoidDelegate onClickDown;
    public VoidDelegate onClickUp;
    public BoolDelegate onHover;
    public VoidDelegate onDrag;
    public VoidDelegate onEndDrag;
    public BoolDelegate onToggleChanged;
    public FloatDelegate onSliderChanged;
    public FloatDelegate onScrollbarChanged;
    public IntDelegate onDrapDownChanged;
    public StringDelegate onInputFieldChanged;

    
    public override void OnSubmit(BaseEventData eventData)
    {  //提交
        if (onSubmit != null)
            onSubmit(gameObject);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    { //鼠标移入
        if (onHover != null)
            onHover(gameObject, true);
    }
    public override void OnPointerClick(PointerEventData eventData)
    { //点击
        if (onClick != null)
            onClick(gameObject);
        if (onToggleChanged != null)
            onToggleChanged(gameObject, gameObject.GetComponent<Toggle>().isOn);

    }

    public override void OnPointerDown(PointerEventData eventData)
    { //点击
        if (onClickDown != null)
            onClickDown(gameObject);
    }


    public override void OnPointerUp(PointerEventData eventData)
    { //点击
        if (onClickUp != null)
            onClickUp(gameObject);
    }
    public override void OnPointerExit(PointerEventData eventData)
    { //鼠标移出
        if (onHover != null)
            onHover(gameObject, false);
    }
    public override void OnDrag(PointerEventData eventData)
    { //拖动
        if (onSliderChanged != null)
            onSliderChanged(gameObject, gameObject.GetComponent<Slider>().value);
        if (onScrollbarChanged != null)
            onScrollbarChanged(gameObject, gameObject.GetComponent<Scrollbar>().value);
        if (onDrag != null)
            onDrag(gameObject);

    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        if(onEndDrag != null)
        {
            onEndDrag(gameObject);
        }
    }
    public override void OnSelect(BaseEventData eventData)
    { //选中
        if (onDrapDownChanged != null)
            onDrapDownChanged(gameObject, gameObject.GetComponent<Dropdown>().value);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    { //选中、每帧更新
        if (onInputFieldChanged != null)
            onInputFieldChanged(gameObject, gameObject.GetComponent<InputField>().text);
    }
    public override void OnDeselect(BaseEventData eventData)
    { //不选中
        if (onInputFieldChanged != null)
            onInputFieldChanged(gameObject, gameObject.GetComponent<InputField>().text);
    }

    public static UIEventListener Get(GameObject go)
    {
        UIEventListener listener = go.GetComponent<UIEventListener>();
        if (listener == null) listener = go.AddComponent<UIEventListener>();
        return listener;
    }
}