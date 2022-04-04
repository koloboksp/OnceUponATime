using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    public class ReuseList<T> where T : new()
    {
        readonly List<int> mFree = new List<int>();
        readonly List<int> mUsed = new List<int>();
        readonly List<T> mElements = new List<T>();

        public T Get()
        {
            if (mFree.Count == 0)
            {
                mUsed.Add(mUsed.Count);
                var newElement = new T();
                mElements.Add(newElement);
                return newElement;
            }

            var freeElementIndex = mFree[mFree.Count - 1];
            mFree.RemoveAt(mFree.Count - 1);
            mUsed.Add(freeElementIndex);
            return mElements[freeElementIndex];
        }

        public void Return(int index)
        {
            mFree.Add(mUsed[index]);
            mUsed.RemoveAt(index);
        }

        public int Count => mUsed.Count;

        public T this[int index]
        {
            get { return mElements[mUsed[index]]; }
        }

        public void Clear()
        {
            for (int i = 0; i < mUsed.Count; i++)
                mFree.Add(mUsed[i]);

            mUsed.Clear();
        }
    }
}