using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Assets.Scripts.Shared.Tags
{
    public static class TagsStorageManager
    {
        private static Dictionary<Type, ITagContainer> mTagContainers = new Dictionary<Type, ITagContainer>();
        private static List<ITag> mTags = new List<ITag>();
        private static bool mInitialized;

        private static void CheckInit()
        {
            if (!mInitialized)
            {
                mInitialized = true;

                Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsClass && !type.IsAbstract)
                        {
                            if (typeof(ITag).IsAssignableFrom(type))
                            {
                                try
                                {
                                    if(type.GetConstructor(Type.EmptyTypes) != null)
                                    { 
                                        ITag tagInstance = Activator.CreateInstance(type) as ITag;

                                        ITag findedTagWithSameId = mTags.Find(i => i.Id == tagInstance.Id);
                                        if (findedTagWithSameId != null)
                                            Debug.LogError(string.Format("Tag with id:'{0}' name:'{1}' already defined. Change if of tag with name : '{2}'", findedTagWithSameId.Id, findedTagWithSameId.Name, tagInstance.Name));
                                        else
                                            mTags.Add(tagInstance as ITag);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogException(e);
                                }
                            }

                            if (typeof(ITagContainer).IsAssignableFrom(type))
                            {
                                try
                                {
                                    ITagContainer tagContainerInstance = Activator.CreateInstance(type) as ITagContainer;
                                    mTagContainers.Add(type, tagContainerInstance);

                                    foreach (ITag tagInstance in tagContainerInstance.Tags)
                                    {
                                        ITag findedTagWithSameId = mTags.Find(i => i.Id == tagInstance.Id);
                                        if (findedTagWithSameId != null)
                                            Debug.LogError(string.Format("Tag with id:'{0}' name:'{1}' already defined. Change id of tag with name : '{2}'", findedTagWithSameId.Id, findedTagWithSameId.Name, tagInstance.Name));
                                        else
                                            mTags.Add(tagInstance as ITag);
                                    }
                                    
                                }
                                catch (Exception e)
                                {
                                    Debug.LogException(e);
                                }
                            }
                        }
                    }
                }     
            }
        }

        public static IEnumerable<ITag> Tags
        {
            get
            {
                CheckInit();

                return mTags;
            }
        }
       
        public static ITag FindTag(Guid id)
        {
            CheckInit();

            if (id != Guid.Empty)
            {
                for (int tIndex = 0; tIndex < mTags.Count; tIndex++)
                {
                    ITag tag = mTags[tIndex];
                    if (tag.Id == id)
                        return tag;
                }
            }
            return null;
        }
        public static T FindTag<T>(Guid id) where T:class, ITag
        {
            CheckInit();

            if (id != Guid.Empty)
            {
                for (int tIndex = 0; tIndex < mTags.Count; tIndex++)
                {
                    ITag tag = mTags[tIndex];
                    if (tag.Id == id)
                        return tag as T;
                }
            }
            return null;
        }
        public static void FindTags<T>(List<T> result) where T : class, ITag
        {
            CheckInit();

            foreach (ITag tag in mTags)
            {
                if(tag is T)
                    result.Add(tag as T);
            }
        }
        public static void SyncTags<T>() where T : ITagContainer
        {//todo: removing
            CheckInit();

            foreach (var newTag in mTagContainers[typeof(T)].Tags)
            {
                if (mTags.Any(t => t.Id == newTag.Id))
                    continue;

                mTags.Add(newTag);
            }
        }
    }
}