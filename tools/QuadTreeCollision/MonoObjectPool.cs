using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MonoObjectPool<T> where T : Object
{
    public string Tag;
    public GameObject Template;
    public Transform Parent;
    private List<GameObject> _objectPool = new List<GameObject>();
    private int _counter = 0;
    private Vector3 _infinitePos = new Vector3(9999, 9999);
    private static MonoObjectPool<T> _instance;
    public static MonoObjectPool<T> Instance
    {
        get { 
            if(_instance == null)
            {
                _instance = new MonoObjectPool<T>();
            }
            return _instance; 
        }
    }
    public MonoObjectPool(GameObject temp,Transform parent)
    {
        Init(temp, parent);
    }

    public MonoObjectPool() { }
    public void Init(GameObject temp, Transform parent)
    {
        Assert.IsNotNull(temp.GetComponent<T>());
        Template = temp;
        Parent = parent;
    }

    public T Get()
    {
        GameObject target;
        if(_objectPool.Count > 0)
        {
            target = _objectPool[_objectPool.Count - 1];
            _objectPool.RemoveAt(_objectPool.Count - 1);
        }
        else
        {
            target = Object.Instantiate(Template, Parent);
            target.name = typeof(T).Name + _counter++;
        }
        target.SetActive(true);
        T com = target.GetComponent<T>();
        Assert.IsNotNull(com);
        return com;
    }

    public void Push(GameObject obj)
    {
        T component = obj.GetComponent<T>();
        if (component == null || _objectPool.Contains(obj))
            return;
        obj.transform.localPosition = _infinitePos;
        obj.SetActive(false);
        _objectPool.Add(obj);
        //_objectPool.Insert(_objectPool.Count, obj);
    }
}
