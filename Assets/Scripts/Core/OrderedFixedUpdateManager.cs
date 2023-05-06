using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    public static class OrderedFixedUpdateManager
    {
        private static readonly List<IOrderedFixedUpdate> mFixedUpdates = new List<IOrderedFixedUpdate>();
        private static readonly List<IOrderedFixedUpdate> mCopyFixedUpdates = new List<IOrderedFixedUpdate>();

        public static void Add(IOrderedFixedUpdate obj)
        {
            if (!mFixedUpdates.Contains(obj))
            {
                mFixedUpdates.Add(obj);
                mFixedUpdates.Sort((l,r)=>l.Order.CompareTo(r.Order));
            }
        }
        public static bool Remove(IOrderedFixedUpdate obj)
        {
            return mFixedUpdates.Remove(obj);  
        }

        internal static void FixedUpdate()
        {
            mCopyFixedUpdates.Clear();
            mCopyFixedUpdates.AddRange(mFixedUpdates);

            for (var oIndex = 0; oIndex < mCopyFixedUpdates.Count; oIndex++)
            {
                var orderedFixedUpdate = mCopyFixedUpdates[oIndex];
                orderedFixedUpdate.OrderedFixedUpdate();
            }
        }
    }
}