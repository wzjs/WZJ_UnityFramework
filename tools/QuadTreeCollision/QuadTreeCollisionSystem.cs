using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

enum EQuadrant
{
    None = -1,
    WestNorth = 0,
    WestSouth = 1,
    EastNorth = 2,
    EastSouth = 3,
}
class CollisionNode
{
    public CollisionNode[] childNodes;
    public List<ICollisionable> objs = new List<ICollisionable>();
    public TRect rect;
}

public class QuadTreeCollisionSystem
{
    private static QuadTreeCollisionSystem _instance;
    private int _maxDepth = 8;
    private int _threshold = 3;

    private CollisionNode _root;
    private QuadTreeCollisionSystem(TRect rootRect)
    {
        _root = new CollisionNode() { rect = rootRect };
    }
    
    public static QuadTreeCollisionSystem GetInstance(TRect rect = default(TRect))
    {
        if(_instance == null)
        {
            _instance = new QuadTreeCollisionSystem(rect);
        }
        return _instance;
    }

    bool IsLeaf(CollisionNode node)
    {
        return node.childNodes == null;
    }

    public void Update()
    {
        var dirty = new List<ICollisionable>();
        UpdateQuadrant(_root, dirty);
        foreach (var item in dirty)
        {
            Add(item);
        }
    }

    void UpdateQuadrant(CollisionNode node,List<ICollisionable> dirty)
    {
        if (!IsLeaf(node))
        {
            var index = node.objs.Count-1;
            while (index >= 0)
            {
                var obj = node.objs[index];
                EQuadrant quadrant = GetQuadrant(node.rect, obj.GetShape());
                if (quadrant != EQuadrant.None || !ShapeUtility.IsContain(node.rect, obj.GetShape()))
                {
                    dirty.Add(obj);
                    RemoveValue(node, obj);
                    Assert.IsTrue(index == 0 || node.objs[index - 1] != obj);
                    //node.objs.RemoveAt(index);
                }
                index--;
            }
            foreach (var item in node.childNodes)
            {
                UpdateQuadrant(item, dirty);
            }
        }
        else
        {
            var index = node.objs.Count - 1;
            while (index >= 0)
            {
                var obj = node.objs[index];
                if (!ShapeUtility.IsContain(node.rect, obj.GetShape()))
                {
                    dirty.Add(obj);
                    RemoveValue(node, obj);
                    Assert.IsTrue(index == 0 || node.objs[index-1] != obj);
                    //node.objs.RemoveAt(index);
                }
                index--;
            }
        }
        
    }

    public void Add(ICollisionable obj)
    {
        Add(_root, 0, obj);
    }

    void Add(CollisionNode node, int depth, ICollisionable obj)
    {
        if (IsLeaf(node))
        {
            if (depth >= _maxDepth || node.objs.Count < _threshold)
            {
                Debug.Log($"add obj name>>:{obj as Collisioner} timeframe:{Time.frameCount}");
                node.objs.Add(obj);
            }
            else
            {
                Split(node);
                Add(node, depth, obj);
            }
        }
        else
        {
            EQuadrant objQuadrant = GetQuadrant(node.rect, obj.GetShape());
            if (objQuadrant != EQuadrant.None)
            {
                Add(node.childNodes[(int)objQuadrant], depth + 1, obj);
            }
            else
            {
                Debug.Log($"add obj name>>:{obj as Collisioner} timeframe:{Time.frameCount}");
                node.objs.Add(obj);
            }
        }
    }

    void Split(CollisionNode node)
    {
        Assert.IsTrue(node.childNodes == null);
        node.childNodes = new CollisionNode[4];
        for (int i = 0; i < node.childNodes.Length; i++)
        {
            var newNode = new CollisionNode();
            newNode.rect = ComputeSubRect(node.rect, (EQuadrant)i);
            node.childNodes[i] = newNode;
        }
        List<ICollisionable> noneQuadrants = new List<ICollisionable>();
        foreach (var obj in node.objs)
        {
            EQuadrant quadrant = GetQuadrant(node.rect, obj.GetShape());
            if (quadrant != EQuadrant.None)
            {
                node.childNodes[(int)quadrant].objs.Add(obj);
            }
            else
            {
                noneQuadrants.Add(obj);
            }
        }
        node.objs = noneQuadrants;
        var nodeRect = node.rect;
        Debug.DrawLine(new Vector3(nodeRect.xMin,0, nodeRect.center.y), new Vector3(nodeRect.xMax,0, nodeRect.center.y),Color.black,9999);
        Debug.DrawLine(new Vector3(nodeRect.center.x,0, nodeRect.yMin), new Vector3(nodeRect.center.x,0, nodeRect.yMax), Color.black, 9999);
    }


    public void Remove(ICollisionable obj)
    {
        Assert.IsNotNull(obj);
        Remove(_root, obj);
    }

    bool Remove(CollisionNode node, ICollisionable obj)
    {
        if (IsLeaf(node))
        {
            return RemoveValue(node, obj);
        }
        else
        {
            var objQuadrant = GetQuadrant(node.rect, obj.GetShape());
            if (objQuadrant != EQuadrant.None)
            {
                if (Remove(node.childNodes[(int)objQuadrant], obj))
                {
                    return TryMerge(node);
                }
            }
            else
            {
                return RemoveValue(node, obj);
            }
        }
        return false;
    }

    bool TryMerge(CollisionNode node)
    {
        Assert.IsTrue(!IsLeaf(node));
        var nodeObjCount = node.objs.Count;
        foreach (var subNode in node.childNodes)
        {
            if (!IsLeaf(subNode))
                return false;
            nodeObjCount += subNode.objs.Count;
        }
        if (nodeObjCount < _threshold)
        {
            foreach (var subNode in node.childNodes)
            {
                node.objs.AddRange(subNode.objs);
                subNode.objs.Clear();
            }
            node.childNodes = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    bool RemoveValue(CollisionNode node, ICollisionable obj)
    {
        Debug.Log($"remove obj name>>:{obj as Collisioner} timeframe:{Time.frameCount}");
        var isSuccess = node.objs.Remove(obj);
        Assert.IsTrue(isSuccess,$"remove obj name>>:{obj as Collisioner} timeframe:{Time.frameCount}");
        return isSuccess;
    }

    public List<ICollisionable> FindIntersections(ICollisionable obj)
    {
        var result = new List<ICollisionable>();
        FindIntersections(_root, obj, result);
        return result;
    }

    public List<ICollisionable> FindOtherObjInQuadrant(ICollisionable obj)
    {
        var result = new List<ICollisionable>();
        FindOtherObjInQuadrant(_root,obj, result);
        return result;
    }

    void FindOtherObjInQuadrant(CollisionNode node,ICollisionable obj, List<ICollisionable> result)
    {
        if (IsLeaf(node))
        {
            result.AddRange(node.objs);
        }
        else
        {
            EQuadrant quadrant = GetQuadrant(node.rect, obj.GetShape());
            if (quadrant == EQuadrant.None)
            {
                result.AddRange(node.objs);
            }
            else
            {
                FindOtherObjInQuadrant(node.childNodes[(int)quadrant], obj, result);
            }
        }
    }

    void FindIntersections(CollisionNode node, ICollisionable targetObj, List<ICollisionable> result)
    {
        var targetRect = targetObj.GetShape();
        foreach (var obj in node.objs)
        {
            if (obj == targetObj) continue;
            if (ShapeUtility.IsIntersect(obj.GetShape(), targetRect))
            {
                result.Add(obj);
            }
        }
        if (!IsLeaf(node))
        {
            foreach (var subNode in node.childNodes)
            {
                if (ShapeUtility.IsIntersect(subNode.rect,targetRect))
                {
                    FindIntersections(subNode, targetObj, result);
                }
            }
        }
    }

    TRect ComputeSubRect(TRect parent, EQuadrant quadrant)
    {
        Assert.IsTrue(quadrant != EQuadrant.None);
        var LeftBottom = new Vector2(parent.xMin, parent.yMin);
        var subRectSize = new Vector2(parent.center.x - parent.xMin, parent.center.y - parent.yMin);
        //var subRectOfHalfSize = subRectSize / 2;
        TRect rect;
        switch (quadrant)
        {
            case EQuadrant.WestNorth:
                rect = new TRect(LeftBottom + new Vector2(0, subRectSize.y), subRectSize);
                break;
            case EQuadrant.WestSouth:
                rect = new TRect(LeftBottom, subRectSize);
                break;
            case EQuadrant.EastNorth:
                rect = new TRect(LeftBottom + subRectSize, subRectSize);
                break;
            case EQuadrant.EastSouth:
                rect = new TRect(LeftBottom + new Vector2(subRectSize.x,0), subRectSize);
                break;
            default:
                rect = default(TRect);
                break;
        }
        return rect;
    }
    
    static bool IsContainRect(Rect big,Rect small)
    {
        return big.xMin <= small.xMin && big.yMin <= small.yMin && big.xMax >= small.xMax && big.yMax >= small.yMax;
    }

    EQuadrant GetQuadrant(TRect parent, IRegularShape child)
    {
        Vector2 center = parent.center;

        if (child.xMax < center.x)
        {
            if (child.yMin > center.y)
            {
                return EQuadrant.WestNorth;
            }
            else if (child.yMax <= center.y)
            {
                return EQuadrant.WestSouth;
            }
            else
            {
                return EQuadrant.None;
            }
        }
        else if (child.xMin >= center.x)
        {
            if (child.yMin > center.y)
            {
                return EQuadrant.EastNorth;
            }
            else if (child.yMax <= center.y)
            {
                return EQuadrant.EastSouth;
            }
            {
                return EQuadrant.None;
            }
        }
        else
        {
            return EQuadrant.None;
        }
    }
}
