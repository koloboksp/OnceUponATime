using System;
using UnityEngine;

namespace Assets.Scripts.Shared.Tags
{
    [Serializable]
    public class TagHolder
    {
        [SerializeField]
        byte[] mId;
        [NonSerialized]
        bool mInit;
        [NonSerialized]
        Guid mIdCache;
        [NonSerialized]
        ITag mTag;

        private TagHolder()
        {
           
        }

        public TagHolder(Guid id)
        {
            Id = id;
        }
    
        public TagHolder(TagHolder source)
        {
            Id = source.Id;
        }

        void CheckInit(bool force = false)
        {
            if (!mInit || force)
            {
                mInit = true;
                if (mId != null && mId.Length == 16)
                {
                    mIdCache = new Guid(mId);
                    mTag = TagsStorageManager.FindTag(mIdCache);
                }
                else
                {
                    mIdCache = Guid.Empty;
                }
            }

        }

        public Guid Id
        {
            get
            {
#if UNITY_EDITOR
                CheckInit(true);
#else
                CheckInit();
#endif

                return mIdCache;
            }
            set
            {
                mIdCache = value;
                mId = mIdCache.ToByteArray();

                mTag = TagsStorageManager.FindTag(mIdCache);
            }
        }

        public string Name
        {
            get
            {
                CheckInit();

                return mTag.Name;
            }
        }

        public bool IsEmpty
        {
            get
            {
                CheckInit();

                return mIdCache == Guid.Empty;
            }
        }
        public static explicit operator TagHolder(Tag tag)
        {
            return new TagHolder(tag.Id);
        }

        public static TagHolder CreateEmpty()
        {
            return new TagHolder(Guid.Empty);
        }
    }
}