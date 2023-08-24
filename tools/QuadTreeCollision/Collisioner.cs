using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Collisioner : MonoBehaviour,ICollisionable
{
    public Action<ICollisionable> OnCollision;
    //private Image image;
    void Awake()
    {
        //image = GetComponent<Image>();
        //UIEventListener.Get(gameObject).onHover = OnHover;
       
    }

    
    //List<Image> _collCache = new List<Image>();
    //List<Image> _quarantCache = new List<Image>();

    //private void OnHover(GameObject go, bool isValue)
    //{
    //    if (isValue)
    //    {
    //        var objsInSameQuadrant = QuadTreeCollisionSystem.GetInstance().FindOtherObjInQuadrant(this);
    //        foreach (var obj in objsInSameQuadrant)
    //        {
    //            Collisioner collisioner = obj as Collisioner;
    //            _quarantCache.Add(collisioner.GetComponent<Image>());
    //            collisioner.GetComponent<Image>().color = Color.blue;
    //        }

    //        var intersectObjs = QuadTreeCollisionSystem.GetInstance().FindIntersections(this);
    //        foreach (var obj in intersectObjs)
    //        {
    //            Collisioner collisioner = obj as Collisioner;
    //            _collCache.Add(collisioner.GetComponent<Image>());
    //            collisioner.GetComponent<Image>().color = Color.red;
    //        }
    //    }
    //    else
    //    {
    //        foreach (var item in _collCache)
    //        {
    //            item.color = Color.white;
    //        }
    //        foreach (var item in _quarantCache)
    //        {
    //            item.color = Color.white;
    //        }
    //        _collCache.Clear();
    //        _quarantCache.Clear();
    //    }
    //}

    private void Update()
    {
        ProcessIntersection();
    }

    void ProcessIntersection()
    {
        //var sourceEntity = GetComponent<IntroEntityBase>();
        //var otherIntersection = QuadTreeCollisionSystem.GetInstance().FindIntersections(this);
        //foreach (var obj in otherIntersection)
        //{
        //    var collisioner = obj as Collisioner;
            //var entity = collisioner.GetComponent<IntroEntityBase>();
            //if(entity.isFriend != sourceEntity.isFriend)
            //{
            //    //sourceEntity.OnCollision(collisioner);
            //}
        //}
    }

    public virtual IRegularShape GetShape()
    {
        throw new NotImplementedException();
    }

    void ICollisionable.OnCollision(ICollisionable go)
    {
        OnCollision?.Invoke(go);
    }
}
