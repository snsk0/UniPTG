#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UniPTG.Utility;

namespace UniPTG.Editors
{
    internal class ProjectSettingsProvider : SettingsProvider
    {
        [SettingsProvider]
        internal static SettingsProvider CreateSettingProvider()
        {
            return new ProjectSettingsProvider("Project/", SettingsScope.Project, new[] { "Procedural Terrain Generator", "UniPTG" });
        }

        internal ProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
            label = "Procedural Terrain Generator";
        }

        private class TempDisplayObject : ScriptableObject
        {
            public List<MonoScript> _noiseGenerators;
            public List<MonoScript> _heightmapGenerators;
        }

        private TempDisplayObject _tempDisplayObject;
        private SerializedObject _displaySerializedObject;
        private SerializedObject _dataBaseSerializedObject;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _tempDisplayObject = ScriptableObject.CreateInstance<TempDisplayObject>();
            _tempDisplayObject.hideFlags = HideFlags.DontSave;

            _displaySerializedObject = new SerializedObject(_tempDisplayObject);
            _dataBaseSerializedObject = new SerializedObject(MonoScriptDatabase.instance);

            //detabase����R�s�[
            EditorUtil.OverrideSerializedArray(_dataBaseSerializedObject.FindProperty("_noiseGenerators"), _displaySerializedObject.FindProperty("_noiseGenerators"));
            EditorUtil.OverrideSerializedArray(_dataBaseSerializedObject.FindProperty("_heightmapGenerators"), _displaySerializedObject.FindProperty("_heightmapGenerators"));
        }

        public override void OnDeactivate()
        {
            Object.DestroyImmediate(_tempDisplayObject);
            _tempDisplayObject = null;
        }

        public override void OnGUI(string searchContext)
        {
            _displaySerializedObject.Update();

            SerializedProperty noisesProperty = _displaySerializedObject.FindProperty("_noiseGenerators");
            SerializedProperty heightmapsProperty = _displaySerializedObject.FindProperty("_heightmapGenerators");

            EditorGUILayout.PropertyField(noisesProperty);
            EditorGUILayout.PropertyField(heightmapsProperty);

            //�w��T�u�N���X�ȊONull�ɒu������
            ReplaceNullExceptSub<NoiseGeneratorBase>(noisesProperty);
            ReplaceNullExceptSub<HeightmapGeneratorBase>(heightmapsProperty);

            _displaySerializedObject.ApplyModifiedProperties();

            //�K�p�{�^��
            if (GUILayout.Button(new GUIContent("�K�p")))
            {
                //database�ɑ΂��ăR�s�[
                EditorUtil.OverrideSerializedArray(noisesProperty, _dataBaseSerializedObject.FindProperty("_noiseGenerators"));
                EditorUtil.OverrideSerializedArray(heightmapsProperty, _dataBaseSerializedObject.FindProperty("_heightmapGenerators"));

                //�f�[�^�x�[�X���X�V
                MonoScriptDatabase.instance.Update();

                //Dialog��\��
                EditorUtility.DisplayDialog("UniPTG", "�ݒ�l��K�p���܂���", "OK");
            }
        }

        /// <summary>
        /// �w�肵���T�u�N���X�łȂ�Property�̏ꍇNull�ɂ���
        /// </summary>
        private void ReplaceNullExceptSub<T>(SerializedProperty property) where T : class
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                MonoScript script = property.GetArrayElementAtIndex(i).objectReferenceValue as MonoScript;

                if(script == null)
                {
                    continue;
                }

                if (!script.GetClass().IsSubclassOf(typeof(T)))
                {
                    property.GetArrayElementAtIndex(i).objectReferenceValue = null;
                }
            }
        }
    }
}
#endif
