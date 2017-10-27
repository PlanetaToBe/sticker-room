using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureCamDispatcher : MonoBehaviour {

    public static CaptureCamDispatcher instance;

    public List<Action> pending = new List<Action>();

    private void Awake()
    {
        instance = this;
    }

    void Update () {
        InvokePending();
	}

    public void Invoke(Action fn)
    {
        lock (pending)
        {
            pending.Add(fn);
        }
    }

    public void InvokePending()
    {
        lock (pending)
        {
            foreach (var action in pending)
            {
                try
                {
                    action();
                } catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            pending.Clear();
        }
    }
}
