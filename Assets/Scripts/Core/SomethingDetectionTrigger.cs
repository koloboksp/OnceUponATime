using System.Collections.Generic;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class SomethingDetectionTrigger<T> : Trigger 
    {
        List<T> mNoAllocForSearch = new List<T>();
        List<Pair> mEnemies = new List<Pair>();

        public int EnemiesCount
        {
            get { return mEnemies.Count; }
        }

        int IndexOf(T key)
        {
            for (var index = 0; index < mEnemies.Count; index++)
            {
                var enemy = mEnemies[index];
                if (ReferenceEquals(enemy.Key, key))
                    return index;
            }

            return -1;
        }

        void OnTriggerEnter2D(Collider2D collider2d)
        {
            if (collider2d.attachedRigidbody != null && !collider2d.isTrigger)
            {
                collider2d.gameObject.GetComponentsInChildren(mNoAllocForSearch);
                for (var fIndex = 0; fIndex < mNoAllocForSearch.Count; fIndex++)
                {
                    var searchResult = mNoAllocForSearch[fIndex];
                    var indexOf = IndexOf(searchResult);
                    if (indexOf >= 0)
                    {
                        mEnemies[indexOf].Add(collider2d);
                    }
                    else
                    {
                        mEnemies.Add(new Pair(searchResult, collider2d));
                    }
                }
            }
        }

        void OnTriggerExit2D(Collider2D collider2d)
        {
            for (var index = mEnemies.Count - 1; index >= 0; index--)
            {
                var enemy = mEnemies[index];
                if (enemy.Remove(collider2d))
                    if (enemy.IsEmpty)
                    {
                        mEnemies.Remove(enemy);
                    }
            }
        }


        public class Pair
        {
            public readonly T Key;
            readonly List<Collider2D> mValues = new List<Collider2D>();

            public Pair(T key, Collider2D value)
            {
                Key = key;
                mValues.Add(value);
            }

            public bool IsEmpty
            {
                get { return mValues.Count == 0; }
            }

            public void Add(Collider2D value)
            {
                mValues.Add(value);
            }

            public bool Remove(Collider2D value)
            {
                return mValues.Remove(value);
            }
        }
    }
}