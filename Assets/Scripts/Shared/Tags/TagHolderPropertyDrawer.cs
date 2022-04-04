#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Shared.Tags
{


    public class TagGroup : PropertyAttribute
    {
        List<string> mGroups = new List<string>();
        public IEnumerable<string> Groups
        {
            get { return mGroups; }
        }
        public TagGroup(params string[] groups)
        {
            mGroups.AddRange(groups);
        }
    }



    [UnityEditor.CustomPropertyDrawer(typeof(TagHolder))]
    public class TagHolderPropertyDrawer : PropertyDrawer
    {
        public static readonly Vector2 MinimalSizeofPopupWindow = new Vector2(500, 300);
        public static readonly Vector2Int DeleteButtonSize = new Vector2Int(13, 13);

        static byte[] mNoAllocGuidData = new byte[16];
 
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            List<string> selectedGroups = null;

            var customAttributes = fieldInfo.GetCustomAttributes(false);
            foreach (var customAttribute in customAttributes)
            {
                if (customAttribute is TagGroup)
                {
                    var tagGroup = customAttribute as TagGroup;
                    if(selectedGroups == null)
                        selectedGroups = new List<string>();
                    selectedGroups.AddRange(tagGroup.Groups);
                }
            }
            UnityEditor.EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = UnityEditor.EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = UnityEditor.EditorGUI.indentLevel;
            UnityEditor.EditorGUI.indentLevel = 0;

            SerializedProperty idProperty = property.FindPropertyRelative("mId");
            
            Guid gettedId = Guid.Empty;
            if (idProperty.arraySize != mNoAllocGuidData.Length)
            {
                idProperty.ClearArray();
            }
            else
            {
                for (int i = 0; i < mNoAllocGuidData.Length; i++)
                    mNoAllocGuidData[i] = (byte)idProperty.GetArrayElementAtIndex(i).intValue;
                
                gettedId = new Guid(mNoAllocGuidData);
            }
          
            string tagName = "none";
            if (gettedId != Guid.Empty)
            {
                try
                {
                    var findTag = TagsStorageManager.FindTag(gettedId);
                    tagName = findTag != null ? findTag.Name : "unknown";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            Rect dropdownBtnPosition = new Rect(position.x, position.y, position.width - DeleteButtonSize.x, position.height);
            if (EditorGUI.DropdownButton(dropdownBtnPosition, new GUIContent(tagName), FocusType.Passive))
            {
                TagHolderPropertyDrawerPopupWindow popupWindow = new TagHolderPropertyDrawerPopupWindow();
                popupWindow.SelectedTagId = gettedId;
                popupWindow.SelectedGroups = selectedGroups;
                popupWindow.WindowSize = new Vector2(Mathf.Max(position.size.x, MinimalSizeofPopupWindow.x), Mathf.Max(position.size.y, MinimalSizeofPopupWindow.y));
                popupWindow.OnClosed += PopupWindow_OnClosed;
                
                PopupWindow.Show(position, popupWindow);      
            }
            Guid newSelectedId = gettedId;
            Rect deleteBtnPosition = new Rect(position.x + (position.width - DeleteButtonSize.x), position.y, DeleteButtonSize.x, position.height);
            if (GUI.Button(deleteBtnPosition, "X"))
            {
                newSelectedId = Guid.Empty;
            }

            
            if (mHasChangesInPopup)
            {
                mHasChangesInPopup = false;
                newSelectedId = mSelectedIdFromPopup;
            }

            if (gettedId != newSelectedId)
            {
                var byteArray = newSelectedId.ToByteArray();
                idProperty.ClearArray();
                for (int i = 0; i < byteArray.Length; i++)
                {
                    idProperty.InsertArrayElementAtIndex(i);
                    var arrayElementAtIndex = idProperty.GetArrayElementAtIndex(i);
                    arrayElementAtIndex.intValue = byteArray[i];
                }

                idProperty.serializedObject.ApplyModifiedProperties();
            }
          
            UnityEditor.EditorGUI.indentLevel = indent;
            UnityEditor.EditorGUI.EndProperty();
        }
#if UNITY_2018_1_OR_NEWER
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }
#endif
        bool mHasChangesInPopup;
        Guid mSelectedIdFromPopup;

        void PopupWindow_OnClosed(TagHolderPropertyDrawerPopupWindow sender)
        {
            sender.OnClosed -= PopupWindow_OnClosed;
            mHasChangesInPopup = true;
            mSelectedIdFromPopup = sender.SelectedTagId;
        }
    }


}
#endif