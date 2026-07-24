using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance { get; private set; }
    private Transform poolParent;

    [System.Serializable]
    public class Pool
    {
        public string key;
        public GameObject prefab;
    }

    public List<Pool> pools = new List<Pool>();
    private Dictionary<string, List<GameObject>> poolDict = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        poolParent = new GameObject("EffectPool_Container").transform;
        poolParent.SetParent(transform);
    }

    public GameObject Get(string key, Vector3 position, Quaternion rotation)
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