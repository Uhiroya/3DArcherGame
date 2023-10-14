using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonBase<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; set; }
    /// <summary>Awakeのタイミングで実行したい処理を書く</summary>
    protected abstract void AwakeFunction();
    protected void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
            AwakeFunction();
        }
    }
}