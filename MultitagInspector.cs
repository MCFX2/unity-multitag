//Author & Copyright: Charles Osberg
//Licensed under MIT, public release edition
//For updates, bug reports, and feature requests, see https://github.com/MCFX2/unity-multitag


//this script is very ugly. If you are interested in writing your own property drawers,
//this is also the only script that contains information for how to do it.
//if you have suggestions for improving its organization, please open an issue or PR on my github.

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(MultitagComponent))]
public class MultitagInspector : Editor
{
    private MultitagComponent tagObj = null;
    private ReorderableList list = null;
	
    private void TagEdit(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (index >= tagObj.Tags.Count) return;

        var isLastElement = index == tagObj.Tags.Count - 1;
        
        var element = list.serializedProperty.GetArrayElementAtIndex(index);
        rect.y += 2;
        
        var tagList = Multitag.AllTags.ToList();
        var curSelection = tagList.IndexOf(element.stringValue);

        if (curSelection == -1)
        {
            curSelection = tagList.Count;
            tagList.Add(tagObj.Tags[index]);
        }

        var selection = EditorGUI.Popup(
            new Rect(rect.x, rect.y, rect.width - 100, rect.height),
            curSelection,
            tagList.ToArray()
        );


        if (curSelection != selection)
        {
            tagObj.Tags[index] = Multitag.AllTags[selection];
            EditorUtility.SetDirty(tagObj);
        }
        
        if (Multitag.AllTags.Contains(tagObj.Tags[index]))
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 100, rect.y, 80, EditorGUIUtility.singleLineHeight), "Unregister")) 
            {
                Multitag.DestroyTag(tagObj.Tags[index]);
            }
        }
        else
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 100, rect.y, 80, EditorGUIUtility.singleLineHeight), "Register"))
            {
                Multitag.AddTags(new List<string>{tagObj.Tags[index]});
            }
        }
            
        if (GUI.Button(new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight), "X"))
        {
            tagObj.Tags.RemoveAt(index); 
            EditorUtility.SetDirty(tagObj);
        }
    }
    
    public void OnEnable()
    {
        tagObj = target as MultitagComponent;

        Debug.Assert(tagObj != null, nameof(tagObj) + " != null - Something has gone very wrong.");
        
        list = new ReorderableList(serializedObject,
            serializedObject.FindProperty("_tags"),
            true,
            false,
            false,
            false);

        list.drawElementCallback += TagEdit;
    }

    private string currentNewTag = "";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();

        var forceSubmit = false;
        var forceClear = false;
        var curKeyboardInput = Event.current;
        if (curKeyboardInput.isKey)
        {
            if (curKeyboardInput.keyCode == KeyCode.KeypadEnter
                || curKeyboardInput.keyCode == KeyCode.Return)
            {
                forceSubmit = true;
            }
            else if (curKeyboardInput.keyCode == KeyCode.Escape)
            {
                forceClear = true;
            }
        }

        var tagList = Multitag.AllTags.ToList().Where(t => !tagObj.Tags.Contains(t));
        var selectionTags = tagList as string[] ?? tagList.ToArray();
        var showDropdown = selectionTags.Any();
        var tagDropdownWidth = showDropdown ? 100 : 0;
        var totalControlWidth = EditorGUIUtility.currentViewWidth;
        const int marginLeftEdge = 20;
        const int marginRightEdge = 10;
        const int addButtonWidth = 30;

        if (showDropdown)
        {
            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;
            var emptySelection = EditorGUI.Popup(
                new Rect(marginLeftEdge, list.GetHeight(), tagDropdownWidth, EditorGUIUtility.singleLineHeight),
                "Add...",
                -1,
                selectionTags.ToArray()
            );
            EditorGUIUtility.labelWidth = oldWidth;

            if (emptySelection != -1)
            {
                tagObj.Tags.Add(selectionTags[emptySelection]);
                EditorUtility.SetDirty(tagObj);
            }
        }

        currentNewTag = EditorGUI.TextField(
            new Rect(
                marginLeftEdge + tagDropdownWidth, 
                list.GetHeight(), 
                totalControlWidth - tagDropdownWidth - marginLeftEdge - marginRightEdge - addButtonWidth, 
                EditorGUIUtility.singleLineHeight),
            currentNewTag);
        
        if (GUI.Button(
                new Rect(
                    totalControlWidth - marginRightEdge - addButtonWidth,
                    list.GetHeight(),
                    addButtonWidth, 
                    EditorGUIUtility.singleLineHeight), 
                "+")
        || forceSubmit)
        {
            currentNewTag = currentNewTag.Trim();
            if (currentNewTag != "")
            {
                tagObj.Tags.Add(currentNewTag);
                currentNewTag = "";
                EditorUtility.SetDirty(tagObj);
            }
 
        }

        if (forceClear)
        {
            currentNewTag = "";
        }
        
        GUILayout.Space(20);
            
        serializedObject.ApplyModifiedProperties();

    }
}

#endif