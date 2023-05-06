using System;
using System.Collections.Generic;

namespace Assets.Scripts.Shared.Tags
{
    public class Tag : ITag
    {
        private readonly Guid mId;
        private readonly string mName;
        private readonly List<string> mGroups;

        public Guid Id
        {
            get { return mId; }
        }

        public string Name
        {
            get { return mName; }
        }

        public List<string> Groups
        {
            get { return mGroups; }
        }

        public Tag(Guid id, string name, params string[] group)
        {
            mId = id;
            mName = name;
            if (group != null)
                mGroups = new List<string>(group);              
        }
    }
}