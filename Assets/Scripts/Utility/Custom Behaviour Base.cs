using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomBehaviourBase : MonoBehaviour
{
    protected virtual void DelayedStart()
    {
        // ...
    }

    protected virtual void Start()
    {
        Invoke("DelayedStart", 0.1f);
    }
}
