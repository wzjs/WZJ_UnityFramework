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
    {  //�ύ
        if (onSubmit != null)
            onSubmit(gameObject);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    { //�������
        if (onHover != null)
            onHover(gameObject, true);
    }
    public override void OnPointerClick(PointerEventData eventData)
    { //���
        if (onClick != null)
            onClick(gameObject);
        if (onToggleChanged != null)
            onToggleChanged(gameObject, gameObject.GetComponent<Toggle>().isOn);

    }

    public override void OnPointerDown(PointerEventData eventData)
    { //���
        if (onClickDown != null)
            onClickDown(gameObject);
    }


    public override void OnPointerUp(PointerEventData eventData)
    { //���
        if (onClickUp != null)
            onClickUp(gameObject);
    }
    public override void OnPointerExit(PointerEventData eventData)
    { //����Ƴ�
        if (onHover != null)
            onHover(gameObject, false);
    }
    public override void OnDrag(PointerEventData eventData)
    { //�϶�
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
    { //ѡ��
        if (onDrapDownChanged != null)
            onDrapDownChanged(gameObject, gameObject.GetComponent<Dropdown>().value);
    }
    public override void OnUpdateSelected(BaseEventData eventData)
    { //ѡ�С�ÿ֡����
        if (onInputFieldChanged != null)
            onInputFieldChanged(gameObject, gameObject.GetComponent<InputField>().text);
    }
    public override void OnDeselect(BaseEventData eventData)
    { //��ѡ��
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