﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

//TODO: handling children; excluding non-needed drawers;

namespace Toolbox.Editor
{
    /// <summary>
    /// Base editor class.
    /// </summary>
    [CanEditMultipleObjects, CustomEditor(typeof(Object), true, isFallback = true)]
    public class ComponentEditor : UnityEditor.Editor
    {
        private static ComponentEditorSettings settings;


        /// <summary>
        /// Initializes <see cref="OrderedPropertyDrawerRoot"/>s using EditorSettings asset.
        /// </summary>
        private void InitializeDrawers()
        {
            if (settings == null)
            {
                var guids = AssetDatabase.FindAssets("t:ComponentEditorSettings");
                if (guids == null || guids.First() == null) return;
                var path = AssetDatabase.GUIDToAssetPath(guids.First());

                settings = AssetDatabase.LoadAssetAtPath(path, typeof(ComponentEditorSettings)) as ComponentEditorSettings;
            }

            if (!settings || settings.HandlersCount == 0) return;

            //create all needed drawer instances and store them in list
            for (var i = 0; i < settings.HandlersCount; i++)
            {
                var type = settings.GetHandlerAt(i).Type;
                if (type == null) continue;
                drawers.Add(Activator.CreateInstance(type, properties) as OrderedPropertyDrawerRoot);
            }
            //inject nested drawers in provided order
            for (var i = 0; i < drawers.Count - 1; i++)
            {
                drawers[i].NestedDrawer = drawers[i + 1];
            }
        }


        /// <summary>
        /// All available drawers setted from <see cref="ComponentEditorSettings"/>.
        /// </summary>
        protected List<OrderedPropertyDrawerRoot> drawers = new List<OrderedPropertyDrawerRoot>();

        /// <summary>
        /// All available and serialized fields(excluding children).
        /// </summary>
        protected List<SerializedProperty> properties = new List<SerializedProperty>();


        /// <summary>
        /// Editor initialization.
        /// </summary>
        protected virtual void OnEnable()
        {
            ReflectionUtility.GetAllFields(target, f => serializedObject.FindProperty(f.Name) != null)
                .ToList()
                .ForEach(f => properties.Add(serializedObject.FindProperty(f.Name)));

            InitializeDrawers();
        }

        /// <summary>
        /// Editor deinitialization.
        /// </summary>
        protected virtual void OnDisable()
        { }

        /// <summary>
        /// Handles desired property display process. Starts drawing using first known
        /// <see cref="OrderedPropertyDrawer{T}"/>(if exists) or standard
        /// <see cref="EditorGUI.PropertyField(Rect, SerializedProperty)"/> method.
        /// </summary>
        /// <param name="property">Property to display.</param>
        protected virtual void HandleProperty(SerializedProperty property)
        {
            if (!settings || !settings.UseOrderedDrawers)
            {
                EditorGUILayout.PropertyField(property, property.isExpanded);
                return;
            }

            try
            {
                drawers.First().HandleProperty(property);
            }
            catch
            {
                EditorGUILayout.PropertyField(property, property.isExpanded);
            }
        }


        /// <summary>
        /// Inspector GUI re-draw event.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Component Editor", EditorStyles.centeredGreyMiniLabel);

            serializedObject.Update();
            var property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(property);
                EditorGUI.EndDisabledGroup();

                while (property.NextVisible(false))
                {
                    HandleProperty(property);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}