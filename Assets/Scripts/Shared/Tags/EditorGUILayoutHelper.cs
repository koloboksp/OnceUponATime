#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Shared.Tags
{
    public static partial class EditorGUILayoutExtends
    {
        public static void DrawEditorField(string label, ref TagHolder value)
        {
            string tagName = "none";

            if (value != null)
            {
                if (value.Id != Guid.Empty)
                {
                    try
                    {
                        var findTag = TagsStorageManager.FindTag(value.Id);
                        tagName = findTag != null ? findTag.Name : "unknown";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(string.Format("Field '{0}' is null.", label));
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(label));

            var id = GUIUtility.GetControlID(FocusType.Passive);
            if (EditorGUILayout.DropdownButton(new GUIContent(tagName), FocusType.Passive))
            {
                var position = GUILayoutUtility.GetLastRect();
                
                TagHolderPropertyDrawerPopupWindow popupWindow = new TagHolderPropertyDrawerPopupWindow();

                popupWindow.SelectedTagId = value.Id;
                popupWindow.WindowSize = new Vector2(Mathf.Max(position.size.x, TagHolderPropertyDrawer.MinimalSizeofPopupWindow.x), Mathf.Max(position.size.y, TagHolderPropertyDrawer.MinimalSizeofPopupWindow.y));
                popupWindow.UserData = id;
                popupWindow.OnClosed += PopupWindow_OnClosed;

                PopupWindow.Show(position, popupWindow);
            }
            if(mPopupWindowJustClosed && id == mControlId)
            {
                mPopupWindowJustClosed = false;
                if (value.Id != mPopupWindowSelectionResult)
                {
                    value.Id = mPopupWindowSelectionResult;
                    GUI.changed = true;
                }
            }
            if(GUILayout.Button("X", GUILayout.Width(TagHolderPropertyDrawer.DeleteButtonSize.x), GUILayout.Height(TagHolderPropertyDrawer.DeleteButtonSize.y)))
            {
                if (value.Id != Guid.Empty)
                {
                    value.Id = Guid.Empty;
                    GUI.changed = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static bool mPopupWindowJustClosed = false;
        private static Guid mPopupWindowSelectionResult;
        private static int mControlId;

        private static void PopupWindow_OnClosed(TagHolderPropertyDrawerPopupWindow sender)
        {
            sender.OnClosed -= PopupWindow_OnClosed;
            mControlId = (int)sender.UserData;
            mPopupWindowJustClosed = true;
            mPopupWindowSelectionResult = sender.SelectedTagId;
        }
    }
}
#endif