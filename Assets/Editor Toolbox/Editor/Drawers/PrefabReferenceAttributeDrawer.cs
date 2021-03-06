﻿using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(PrefabReferenceAttribute))]
    public class PrefabReferenceAttributeDrawer : ToolboxNativePropertyDrawer
    {
        protected override float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeightSafe(property, label);
        }

        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (!EditorGUI.EndChangeCheck() || property.objectReferenceValue == null)
            {
                return;
            }

            if (PrefabUtility.GetPrefabAssetType(property.objectReferenceValue) == PrefabAssetType.NotAPrefab)
            {
                ToolboxEditorLog.AttributeUsageWarning(attribute, property, "Assigned object has to be a prefab.");
                property.objectReferenceValue = null;
            }
        }


        public override bool IsPropertyValid(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference;
        }
    }
}