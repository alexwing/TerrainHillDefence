using UnityEngine;

namespace HillDefence
{
    /// <summary>
    /// Helper to correctly despawn bullets, returning them to the pool if available.
    /// </summary>
    public static class BulletUtils
    {
        public static void Despawn(GameObject bullet)
        {
            if (bullet == null) return;
            if (ObjectPooler.instance != null)
            {
                ObjectPooler.instance.ReturnToPool(bullet);
            }
            else
            {
                Object.Destroy(bullet);
            }
        }
    }
}
