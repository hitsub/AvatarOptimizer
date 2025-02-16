using System;
using System.Collections.Generic;
using System.Linq;
using Anatawa12.AvatarOptimizer.Processors.SkinnedMeshes;
using CustomLocalization4EditorExtension;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Anatawa12.AvatarOptimizer
{
    [CustomEditor(typeof(MergeToonLitMaterial))]
    internal class MergeToonLitMaterialEditor : AvatarTagComponentEditorBase
    {
        private Material[] _upstreamMaterials;
        private (Material mat, int index)[] _materials;
        
        private (Material mat, int index)[] _candidateMaterials;
        private string[] _candidateNames;

        private Texture[] _generatedPreviews;

        private readonly Func<MergeToonLitMaterial.MergeSource> _createNewSource;
        private readonly Func<MergeToonLitMaterial.MergeInfo> _createNewMergeInfo;

        public MergeToonLitMaterialEditor()
        {
            // ReSharper disable once PossibleNullReferenceException
            _createNewSource = () => new MergeToonLitMaterial.MergeSource
                { materialIndex = _candidateMaterials[0].index };
            _createNewMergeInfo = () => new MergeToonLitMaterial.MergeInfo 
                { source = new [] {_createNewSource()} };
        }

        private static void DrawList<T>(
            ref T[] array,
            string addButton,
            Action<T, int> drawer,
            Func<T> newElement,
            bool noEmpty = false,
            Action postButtons = null,
            Action onMoved = null,
            Action<T> onRemoved = null,
            Action<T> onAdded = null
        )
        {
            if (array == null) array = Array.Empty<T>();
            for (var i = 0; i < array.Length; i++)
            {
                drawer(array[i], i);

                GUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(i == 0))
                {
                    if (GUILayout.Button("▲"))
                    {
                        (array[i], array[i - 1]) = (array[i - 1], array[i]);
                        onMoved?.Invoke();
                    }
                }

                using (new EditorGUI.DisabledScope(i == array.Length - 1))
                {
                    if (GUILayout.Button("▼"))
                    {
                        (array[i], array[i + 1]) = (array[i + 1], array[i]);
                        onMoved?.Invoke();
                    }
                }

                using (new EditorGUI.DisabledScope(noEmpty && array.Length == 1))
                {
                    if (GUILayout.Button("✗"))
                    {
                        var removing = array[i];
                        ArrayUtility.RemoveAt(ref array, i);
                        onRemoved?.Invoke(removing);
                        i--;
                    }
                }

                GUILayout.EndHorizontal();
                postButtons?.Invoke();                
            }

            using (new EditorGUI.DisabledScope(newElement == null))
                if (GUILayout.Button(addButton))
                {
                    Debug.Assert(newElement != null, nameof(newElement) + " != null");
                    T element;
                    ArrayUtility.Add(ref array, element = newElement());
                    onAdded(element);
                }

        }

        protected override void OnInspectorGUIInner()
        {
            EditorGUI.BeginChangeCheck();

            var component = (MergeToonLitMaterial)target;

            DrawList(ref component.merges, CL4EE.Tr("MergeToonLitMaterial:button:Add Merged Material"), (componentMerge, i) =>
                {
                    DrawList(ref componentMerge.source, CL4EE.Tr("MergeToonLitMaterial:button:Add Source"), (mergeSource, _2) =>
                        {
                            var found = _materials.FirstOrDefault(x => x.index == mergeSource.materialIndex);
                            _candidateNames[0] = found.mat != null ? found.mat.name : "(invalid)";
                            EditorGUI.BeginChangeCheck();
                            var newIndex = EditorGUILayout.Popup(0, _candidateNames);
                            if (EditorGUI.EndChangeCheck() && newIndex != 0)
                                mergeSource.materialIndex = _candidateMaterials[newIndex - 1].index;

                            EditorGUI.BeginChangeCheck();
                            mergeSource.targetRect = EditorGUILayout.RectField(mergeSource.targetRect);
                        },
                        _candidateMaterials.Length != 0 ? _createNewSource : null,
                        onMoved: OnChanged,
                        onAdded: _ => OnChanged(),
                        onRemoved: _ => OnChanged()
                    );

                    componentMerge.textureSize =
                        EditorGUILayout.Vector2IntField(CL4EE.Tr("MergeToonLitMaterial:label:Texture Size"), componentMerge.textureSize);

                    var preview = _generatedPreviews != null ? _generatedPreviews[i] : Utils.PreviewHereTex;
                    EditorGUILayout.LabelField(new GUIContent(preview), GUILayout.MaxHeight(256), GUILayout.MaxHeight(256));

                    Utils.HorizontalLine();
                },
                _candidateMaterials.Length != 0 ? _createNewMergeInfo : null,
                postButtons: () => Utils.HorizontalLine(),
                onMoved: OnChanged,
                onAdded: _ => OnChanged(),
                onRemoved: _ => OnChanged()
            );

            if (EditorGUI.EndChangeCheck())
                OnChanged();

            if (GUILayout.Button(CL4EE.Tr("MergeToonLitMaterial:button:Generate Preview")))
            {
                _generatedPreviews = MergeToonLitMaterialProcessor.GenerateTextures(component, _upstreamMaterials);
            }            
        }

        private void OnChanged() => OnChanged(true);

        private void OnChanged(bool dirty)
        {
            _generatedPreviews = null;
            var component = (MergeToonLitMaterial)target;
            if (dirty) EditorUtility.SetDirty(component);
            var usedIndices = new HashSet<int>(component.merges.SelectMany(x => x.source.Select(y => y.materialIndex)));
            _candidateMaterials = _materials.Where(x => !usedIndices.Contains(x.index)).ToArray();
            _candidateNames = new []{""}.Concat(_candidateMaterials.Select(x => x.mat.name)).ToArray();
        }

        private void OnEnable()
        {
            var component = (MergeToonLitMaterial)target;

            // find materials with toonlit
            _upstreamMaterials = EditSkinnedMeshComponentUtil
                .GetMaterials(component.GetComponent<SkinnedMeshRenderer>(), component);
            _materials = _upstreamMaterials
                .Select((mat, index) => (mat, index))
                .Where(x => x.mat.shader == Utils.ToonLitShader)
                .ToArray();
            OnChanged(dirty: false);
        }
    }
}

