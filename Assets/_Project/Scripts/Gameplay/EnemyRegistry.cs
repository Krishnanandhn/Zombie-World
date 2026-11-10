using System.Collections.Generic;
using Project.Core;

namespace Project.Gameplay
{
    public sealed class EnemyRegistry
    {
        readonly List<Enemy> _alive = new List<Enemy>(64);

        public IReadOnlyList<Enemy> Alive => _alive;
        public int Count => _alive.Count;

        public void Add(Enemy enemy)
        {
            if (!_alive.Contains(enemy)) _alive.Add(enemy);
        }

        public void Remove(Enemy enemy) => _alive.Remove(enemy);

        public void DespawnAll(IPoolService pool)
        {
            for (int i = _alive.Count - 1; i >= 0; i--) pool.Despawn(_alive[i].gameObject);
            _alive.Clear();
        }
    }
}
