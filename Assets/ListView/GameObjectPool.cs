using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    Queue<GameObject> m_pool;
    GameObject m_prefab;
    Transform m_root;

    public GameObjectPool(Transform trans, GameObject prefab)
    {
        m_root = trans;
        m_prefab = prefab;
        m_pool = new Queue<GameObject>();
    }

    public GameObject Get()
    {
        if(m_pool.Count > 0)
            return m_pool.Dequeue();

        GameObject go = Object.Instantiate(m_prefab);
        return go;
    }
    
    public void Put(GameObject go)
    {
        if(go == null) return;
        go.transform.SetParent(m_root);
        go.SetActive(false);
        m_pool.Enqueue(go);
    }

    public void Clear()
    {
        foreach (var item in m_pool)
            Object.Destroy(item);
        m_pool.Clear();
    }
}
