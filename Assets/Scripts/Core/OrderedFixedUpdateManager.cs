using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    public static class OrderedFixedUpdateManager
    {
        private static readonly List<IOrderedFixedUpdate> FixedUpdates = new List<IOrderedFixedUpdate>();
        private static readonly List<IOrderedFixedUpdate> CopyFixedUpdates = new List<IOrderedFixedUpdate>();

        public static void Add(IOrderedFixedUpdate obj)
        {
            if (!FixedUpdates.Contains(obj))
            {
                FixedUpdates.Add(obj);
                FixedUpdates.Sort((l,r)=>l.Order.CompareTo(r.Order));
            }
        }
        public static bool Remove(IOrderedFixedUpdate obj)
        {
            return FixedUpdates.Remove(obj);  
        }

        internal static void FixedUpdate()
        {
            CopyFixedUpdates.Clear();
            CopyFixedUpdates.AddRange(FixedUpdates);

            for (var oIndex = 0; oIndex < CopyFixedUpdates.Count; oIndex++)
            {
                var orderedFixedUpdate = CopyFixedUpdates[oIndex];
                orderedFixedUpdate.OrderedFixedUpdate();
            }
        }
    }
}