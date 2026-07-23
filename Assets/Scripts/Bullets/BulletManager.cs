using System.Collections.Generic;
using System.Linq;
using Grigios;

public class BulletManager : Singleton<BulletManager>
{
    public Bullet prefab;
    public int maxBullets = 3;
    private List<Bullet> pool = new List<Bullet>();

    public Bullet GetObject()
    {
        Bullet obj = pool.FirstOrDefault(b => !b.gameObject.activeSelf);
        if (obj != null)
        {
            obj.gameObject.SetActive(true);
            return obj;
        }

        if (pool.Count >= maxBullets)
            return null; 

        var bullet = Instantiate(prefab);
        pool.Add(bullet);
        bullet.gameObject.SetActive(true);
        return bullet;
    }
}