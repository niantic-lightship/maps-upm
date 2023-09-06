// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Niantic.Lightship.Maps.Builders;
using Niantic.Lightship.Maps.Linq;
using Niantic.Lightship.Maps.Themes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Niantic.Lightship.Maps.Editor.CustomEditors.MapThemes
{
    [CustomEditor(typeof(MapTheme))]
    internal class MapThemeCustomEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset _visualTree;

        private VisualElement _inspector;
        private VisualElement _defaultInspector;

        private DropdownField _builderSelector;
        private Foldout _builderFoldout;
        private IMGUIContainer _builderInspector;

        private Button _addBuilderButton;

        private List<SerializedProperty> _builders;

        private UnityEditor.Editor _builderEditor;

        public override VisualElement CreateInspectorGUI()
        {
            _inspector ??= new VisualElement();
            _visualTree.CloneTree(_inspector);

            CreateMapThemeDefaultInspector(_inspector);
            CreateNestedBuilderInspector(_inspector);
            SetUpAddBuilderButton(_inspector);

            return _inspector;
        }

        private void SetUpAddBuilderButton(VisualElement inspector)
        {
            _addBuilderButton ??= inspector.Q<Button>("_addBuilderButton");

            _addBuilderButton.clickable.clicked += () =>
            {
                var builderTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(FeatureBuilderBase).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract)
                    .GroupBy(t =>
                    {
                        if (!t.Assembly.FullName.Contains("Niantic.Lightship.Maps"))
                        {
                            return "Custom Builders";
                        }

                        if (t.Name.Contains("Async"))
                        {
                            return "Performance Builders";
                        }

                        return "Standard Builders";
                    })
                    .ToList();

                InitializeAddBuilderDropDown(builderTypes).DropDown(_addBuilderButton.worldBound, _addBuilderButton);
            };
        }

        private GenericDropdownMenu InitializeAddBuilderDropDown(List<IGrouping<string, Type>> builderTypes)
        {
            var dropdownMenu = new GenericDropdownMenu();
            foreach (var builder in builderTypes.OrderBy(k => k.Key))
            {
                if (builder.Count() != 0)
                {
                    dropdownMenu.AddDisabledItem(builder.Key, false);
                }

                foreach (var type in builder.OrderBy(n => n.Name))
                {
                    dropdownMenu.AddItem(type.Name, false, () =>
                    {
                        var theme = (MapTheme) serializedObject.targetObject;

                        if (PrefabUtility.IsPartOfPrefabAsset(theme))
                        {
                            using var scope = new PrefabUtility.EditPrefabContentsScope(AssetDatabase.GetAssetPath(theme));
                            var prefabTheme = scope.prefabContentsRoot.GetComponent<MapTheme>();
                            AddBuilderToTheme(prefabTheme);
                        }
                        else
                        {
                           AddBuilderToTheme(theme);
                        }

                        void AddBuilderToTheme(MapTheme mapTheme)
                        {
                            var serTheme = new SerializedObject(mapTheme);
                            var newBuilderGo = new GameObject(type.Name, type);
                            newBuilderGo.transform.SetParent(mapTheme.transform, false);

                            var serBuilders = serTheme.FindProperty("_builders");
                            var builderCount = serBuilders.arraySize;
                            serBuilders.InsertArrayElementAtIndex(builderCount);
                            var builderField = serBuilders.GetArrayElementAtIndex(builderCount);
                            builderField.objectReferenceValue = newBuilderGo.GetComponent(type);
                            serTheme.ApplyModifiedProperties();
                        }
                    });
                }

                dropdownMenu.AddSeparator(null);
            }

            return dropdownMenu;
        }

        private void CreateMapThemeDefaultInspector(VisualElement inspector)
        {
            _defaultInspector ??= inspector.Q("_defaultInspector");
            InspectorElement.FillDefaultInspector(_defaultInspector, serializedObject, this);
        }

        private void CreateNestedBuilderInspector(VisualElement inspector)
        {
            // Init VisualElement references
            _builderInspector ??= inspector.Q<IMGUIContainer>("_builderInspector");
            _builderFoldout ??= inspector.Q<Foldout>("_builderFoldout");
            _builderSelector ??= inspector.Q<DropdownField>("_builderSelector");

            var builderListProp = serializedObject.FindProperty("_builders");

            if (!InitBuilderSelectorDropdown(builderListProp))
            {
                return;
            }

            var newBuilderPropIndex = _builders.FindIndex(b => b.objectReferenceValue.name.Equals(_builderSelector.value));
            newBuilderPropIndex = newBuilderPropIndex != -1 ? newBuilderPropIndex : 0;
            var newBuilderProp = _builders[newBuilderPropIndex];

            SetBuilderSelectorIndex(newBuilderPropIndex);
            ShowBuilderInspector(newBuilderProp);

            _builderSelector.RegisterCallback<ChangeEvent<string>>(OnBuilderSelectorValueChanged);
        }

        private bool InitBuilderSelectorDropdown(SerializedProperty builderListProp)
        {
            var builderCount = builderListProp.arraySize;

            if (builderCount == 0)
            {
                _builderSelector.visible = false;
                _builderFoldout.visible = false;

                return false;
            }

            _builders ??= new List<SerializedProperty>();

            _builders.Clear();
            for (int i = 0; i < builderCount; i++)
            {
                var builderProp = builderListProp.GetArrayElementAtIndex(i);
                if (builderProp?.objectReferenceValue == null)
                {
                    continue;
                }
                _builders.Add(builderProp);
            }

            if (_builders.Count == 0)
            {
                _builderSelector.visible = false;
                _builderFoldout.visible = false;

                return false;
            }

            _builderSelector.choices = _builders.Select(b => b.objectReferenceValue.name);

            return true;
        }

        private void OnBuilderSelectorValueChanged(ChangeEvent<string> evt)
        {
            var newBuilderPropIndex = _builders.FindIndex(b => b.objectReferenceValue.name.Equals(evt.newValue));
            newBuilderPropIndex = newBuilderPropIndex != -1 ? newBuilderPropIndex : 0;
            var newBuilderProp = _builders[newBuilderPropIndex];

            SetBuilderSelectorIndex(newBuilderPropIndex);
            ShowBuilderInspector(newBuilderProp);
        }

        private void SetBuilderSelectorIndex(int index)
        {
            _builderSelector.index = index;
        }

        private void ShowBuilderInspector(SerializedProperty newBuilderProp)
        {
            _builderFoldout.value = true;
            var obj = newBuilderProp.objectReferenceValue;
            _builderFoldout.text = obj.name;

            DestroyImmediate(_builderEditor);
            _builderEditor = CreateEditor(obj);

            _builderInspector.onGUIHandler = () =>
            {
                if (_builderEditor != null)
                {
                    _builderEditor.OnInspectorGUI();
                }
            };
        }

        private void OnDisable()
        {
            if (_builderInspector != null)
            {
                _builderInspector.onGUIHandler = null;
            }

            DestroyImmediate(_builderEditor);
            _builderEditor = null;
        }
    }
}
