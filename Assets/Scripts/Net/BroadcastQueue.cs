using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataModel;
using ServerCommon;

public class BroadcastQueue 
{
    /// <summary>
    /// 与服务器连接出错 
    /// </summary>
    private Queue<(string action, IDynamicType obj)> broadcastQueue =new Queue<(string, IDynamicType)>();

    ///<summary>
    ///当前数量
    ///<summary>
    public int Count
    {
        get
        {
            lock(broadcastQueue) 
            { 
                return broadcastQueue.Count;
            }
        }
    }

    public void Enqueue((string action,IDynamicType message) message)
    {
        lock(broadcastQueue)
        {
            broadcastQueue.Enqueue(message);
        }
    }

    public (string action, IDynamicType subData) Dequeue()
    {
        lock(broadcastQueue)
        {
            return broadcastQueue.Dequeue();
        }
    }


}
