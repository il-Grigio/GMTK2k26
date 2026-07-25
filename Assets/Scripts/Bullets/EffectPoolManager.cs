using System.Collections.Generic;
using System.Linq;
using Grigios;
using UnityEngine;

public class EffectPoolManager : Singleton<EffectPoolManager>
{
    public enum EffectType
    {
        bullet,
    }
    private Transform poolParent;

    [System.Serializable]
    public class Pool
    {
        public EffectType key;
        public GameObject prefab;
    }

    public List<Pool> pools = new List<Pool>();
    private Dictionary<EffectType, List<GameObject>> poolDict = new Dictionary<EffectType, List<GameObject>>();

    private void Awake()
    {
        poolParent = new GameObject("EffectPool_Container").transform;
        poolParent.SetParent(transform);
    }

    public GameObject Get(EffectType key, Vector3 position, Quaternion rotation)
    {
        var pool = pools.FirstOrDefault(p => p.key == key);
        if (pool == null) return null;

        if (!poolDict.ContainsKey(key))
            poolDict[key] = new List<GameObject>();

        var list = poolDict[key];
        GameObject obj = list.FirstOrDefault(o => !o.activeSelf);

        if (obj == null)
        {
            obj = Instantiate(pool.prefab, poolParent);
            list.Add(obj);
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }
}