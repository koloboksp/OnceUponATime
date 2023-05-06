using System.Collections.Generic;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class SomethingDetectionTrigger<T> : Trigger 
    {
        private readonly List<T> _noAllocForSearch = new List<T>();
        private readonly List<Pair> _enemies = new List<Pair>();

        public int EnemiesCount
        {
            get { return _enemies.Count; }
        }

        private int IndexOf(T key)
        {
            for (var index = 0; index < _enemies.Count; index++)
            {
                var enemy = _enemies[index];
                if (ReferenceEquals(enemy.Key, key))
                    return index;
            }

            return -1;
        }

        private void OnTriggerEnter2D(Collider2D collider2d)
        {
            if (collider2d.attachedRigidbody != null && !collider2d.isTrigger)
            {
                collider2d.gameObject.GetComponentsInChildren(_noAllocForSearch);
                for (var fIndex = 0; fIndex < _noAllocForSearch.Count; fIndex++)
                {
                    var searchResult = _noAllocForSearch[fIndex];
                    var indexOf = IndexOf(searchResult);
                    if (indexOf >= 0)
                    {
                        _enemies[indexOf].Add(collider2d);
                    }
                    else
                    {
                        _enemies.Add(new Pair(searchResult, collider2d));
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collider2d)
        {
            for (var index = _enemies.Count - 1; index >= 0; index--)
            {
                var enemy = _enemies[index];
                if (enemy.Remove(collider2d))
                    if (enemy.IsEmpty)
                    {
                        _enemies.Remove(enemy);
                    }
            }
        }
        
        public class Pair
        {
            public readonly T Key;
            private readonly List<Collider2D> _values = new List<Collider2D>();

            public Pair(T key, Collider2D value)
            {
                Key = key;
                _values.Add(value);
            }

            public bool IsEmpty
            {
                get { return _values.Count == 0; }
            }

            public void Add(Collider2D value)
            {
                _values.Add(value);
            }

            public bool Remove(Collider2D value)
            {
                return _values.Remove(value);
            }
        }
    }
}