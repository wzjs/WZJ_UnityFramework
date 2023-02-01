using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionable
{
    public abstract IRegularShape GetShape();
}
