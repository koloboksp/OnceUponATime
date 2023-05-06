#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Shared.Tags
{
    public class TagHolderPropertyDrawerPopupWindow : PopupWindowContent
    {
        private const string UndefinedGroupName = "Undefined";
        private static readonly Color SelectionColor = new Color(118.0f / 256.0f, 173.0f / 256.0f, 191.0f / 256.0f);

        public override Vector2 GetWindowSize()
        {
            return mWindowSize;
        }

        private static List<string> mSelectedGroups = new List<string>();

        private Vector2 mWindowSize;
        private Vector2 mFilteredTagsScrollViewPosition;
        private Vector2 mSelectedGroupsScrollViewPosition;
        private Vector2 mAvailableGroupsScrollViewPosition;
        private Guid mSelectedTagId;
        private object mUserData;

        public event Action<TagHolderPropertyDrawerPopupWindow> OnClosed;
        public Guid SelectedTagId
        {
            set { mSelectedTagId = value; }
            get { return mSelectedTagId; }
        }
        public IEnumerable<string> SelectedGroups
        {
            set
            {
                if(value != null)
                foreach (var group in value)
                {
                    if(!mSelectedGroups.Contains(group))
                        mSelectedGroups.Add(group);
                }
            }
            get { return mSelectedGroups; }
        }
        public Vector2 WindowSize
        {
            set { mWindowSize = value; }
        }
        public object UserData
        {
            get { return mUserData;}
            set
            {
                mUserData = value;
            }
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(GetWindowSize().x / 2.0f));
            mFilteredTagsScrollViewPosition = EditorGUILayout.BeginScrollView(mFilteredTagsScrollViewPosition);

            List<ITag> filteredTags = new List<ITag>();
            foreach (ITag tag in mAvailableTags)
            {
                bool allGroupsFinded = true;
                foreach (string filterGroup in mSelectedGroups)
                {
                    if (filterGroup == UndefinedGroupName)
                    {
                        if (tag.Groups == null || tag.Groups.Count == 0 || tag.Groups.Contains(UndefinedGroupName))
                        {

                        }
                        else
                        {
                            allGroupsFinded = false;
                            break;
                        }
                    }
                    else
                    {
                        if (tag.Groups != null && tag.Groups.Contains(filterGroup))
                        {

                        }
                        else
                        {
                            allGroupsFinded = false;
                            break;
                        }
                    }

                }

                if (allGroupsFinded)
                {
                    filteredTags.Add(tag);
                }
            }

            ITag selectedTagFindedInFiltered = filteredTags.Find(i => i.Id == mSelectedTagId);
            if (selectedTagFindedInFiltered != null)
            {

            }
            else
            {
                filteredTags.Find(i => i.Id == mSelectedTagId);
                ITag selectedTagFindedInAvailable = mAvailableTags.Find(i => i.Id == mSelectedTagId);
                if (selectedTagFindedInAvailable != null)
                {
                    filteredTags.Insert(0, selectedTagFindedInAvailable);
                }
            }

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;

            foreach (ITag tag in filteredTags)
            {
                Color savedForSelectionColor = GUI.color;
                Guid selectedTagId = mSelectedTagId;
                if (tag.Id == selectedTagId)
                {
                    GUI.color = SelectionColor;
                }

                if (GUILayout.Button(tag.Name, buttonStyle))
                {
                    mSelectedTagId = tag.Id;
                    this.editorWindow.Close();
                }

                if (tag.Id == selectedTagId)
                {
                    GUI.color = savedForSelectionColor;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(GetWindowSize().x / 4.0f));
            EditorGUILayout.LabelField("Selected filters", GUILayout.Width(GetWindowSize().x / 4.0f));
            mSelectedGroupsScrollViewPosition = EditorGUILayout.BeginScrollView(mSelectedGroupsScrollViewPosition);
            List<string> groupsToRemove = new List<string>();
            foreach (string groupName in mSelectedGroups)
            {
                if (GUILayout.Button(groupName, buttonStyle))
                {
                    groupsToRemove.Add(groupName);
                }
            }

            foreach (string groupName in groupsToRemove)
            {
                if (mSelectedGroups.Contains(groupName))
                    mSelectedGroups.Remove(groupName);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(GetWindowSize().x / 4.0f));
            EditorGUILayout.LabelField("Available filters", GUILayout.Width(GetWindowSize().x / 4.0f));
            mAvailableGroupsScrollViewPosition = EditorGUILayout.BeginScrollView(mAvailableGroupsScrollViewPosition);
            foreach (var groupPair in mAvailableTagByGroups)
            {
                if (GUILayout.Button(groupPair.Key, buttonStyle))
                {
                    if (!mSelectedGroups.Contains(groupPair.Key))
                        mSelectedGroups.Add(groupPair.Key);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        public override void OnOpen()
        {
            CheckInit();
        }
        public override void OnClose()
        {
            OnClosed?.Invoke(this);
        }

        private static bool mInitialized = false;

        private static void CheckInit()
        {
            if (!mInitialized)
            {
                mInitialized = true;

                mAvailableTags = TagsStorageManager.Tags.ToList();
                mAvailableTagByGroups = new Dictionary<string, List<ITag>>();
                foreach (ITag tag in mAvailableTags)
                {
                    if (tag.Groups != null && tag.Groups.Count > 0)
                    {
                        foreach (string groupName in tag.Groups)
                        {
                            if (!mAvailableTagByGroups.ContainsKey(groupName))
                                mAvailableTagByGroups.Add(groupName, new List<ITag>());

                            mAvailableTagByGroups[groupName].Add(tag);
                        }
                    }
                    else
                    {
                        if (!mAvailableTagByGroups.ContainsKey(UndefinedGroupName))
                            mAvailableTagByGroups.Add(UndefinedGroupName, new List<ITag>());

                        mAvailableTagByGroups[UndefinedGroupName].Add(tag);
                    }
                }
            }
        }

        private static Dictionary<string, List<ITag>> mAvailableTagByGroups;
        private static List<ITag> mAvailableTags;
        
    }
}
#endif