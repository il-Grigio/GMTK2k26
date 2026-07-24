using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Grigios;

public class BulletManager : Singleton<BulletManager>
{
    public Bullet prefab;
    private List<Bullet> pool = new List<Bullet>();
    private Transform poolParent;

    private void Awake()
    {
        poolParent = new GameObject("Pool_Bullets").transform;
        poolParent.SetParent(transform);
    }

    public Bullet GetObject()
    {
        Bullet obj = pool.FirstOrDefault(b => !b.gameObject.activeSelf);
        if (obj != null)
        {
            obj.gameObject.SetActive(true);
            return obj;
        }

        var bullet = Instantiate(prefab, poolParent);
        pool.Add(bullet);
        bullet.gameObject.SetActive(true);
        return bullet;
    }
}