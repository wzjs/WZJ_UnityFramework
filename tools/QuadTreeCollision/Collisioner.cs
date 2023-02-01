using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public abstract class Collisioner : MonoBehaviour,ICollisionable
{
    //private Image image;
    void Awake()
    {
        //image = GetComponent<Image>();
        //UIEventListener.Get(gameObject).onHover = OnHover;
       
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable>>" + transform.localPosition + " name>>" + name);
        //IntroExecutor.Instance.CollisionSystem.Add(this);
    }
    private void OnDisable()
    {
        Debug.Log("OnDisable>>" + transform.localPosition + " name>>" + name);
        //IntroExecutor.Instance.CollisionSystem.Remove(this);
    }
    void Start()
    {
        
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
        var sourceEntity = GetComponent<IntroEntityBase>();
        var otherIntersection = QuadTreeCollisionSystem.GetInstance().FindIntersections(this);
        foreach (var obj in otherIntersection)
        {
            var collisioner = obj as Collisioner;
            var entity = collisioner.GetComponent<IntroEntityBase>();
            if(entity.isFriend != sourceEntity.isFriend)
            {
                //sourceEntity.OnCollision(collisioner);
            }
        }
    }

    public virtual IRegularShape GetShape()
    {
        throw new NotImplementedException();
    }
}
