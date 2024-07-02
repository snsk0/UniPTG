#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniPTG
{
    [InitializeOnLoad]
    internal static class GeneratorDatabase
    {
        private static Dictionary<NoiseGeneratorBase, Editor> _noiseGeneratorToEditor;
        private static Dictionary<HeightmapGeneratorBase, Editor> _heightMapGeneratorToEditor;

        static GeneratorDatabase()
        {
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                Debug.Log("SaveAndDispose");
                SaveAndDispose();
            };


            AssemblyReloadEvents.afterAssemblyReload += () => 
            {
                Debug.Log("Load");
                Load();
            };
        }

        private static void Load()
        {
            //MonoScriptDatabase����f�[�^���擾
            IReadOnlyList<Type> noiseGeneratorTypes = MonoScriptDatabase.instance.GetNoiseGeneratorTypes();
            IReadOnlyList<Type> heightmapGeneratorTypes = MonoScriptDatabase.instance.GetHeightmapGeneratorTypes();

            //Type����C���X�^���X��Editor�𐶐�
            _noiseGeneratorToEditor = noiseGeneratorTypes
                .Select((type) =>
                {
                    NoiseGeneratorBase generator = ScriptableObject.CreateInstance(type) as NoiseGeneratorBase;

                    //UserSettings����p�����[�^��ǂݍ���
                    string json = EditorUserSettings.GetConfigValue(type.FullName);
                    if (!string.IsNullOrEmpty(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, generator);
                    }

                    //�i��������
                    generator.hideFlags = HideFlags.DontSave;
                    return generator;
                })
                .ToDictionary(generator => generator, generator => Editor.CreateEditor(generator));

            _heightMapGeneratorToEditor = heightmapGeneratorTypes
                .Select((type) =>
                {
                    HeightmapGeneratorBase generator = ScriptableObject.CreateInstance(type) as HeightmapGeneratorBase;

                    //UserSettings����p�����[�^��ǂݍ���
                    string json = EditorUserSettings.GetConfigValue(type.FullName);
                    if (!string.IsNullOrEmpty(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, generator);
                    }

                    //�i��������
                    generator.hideFlags = HideFlags.DontSave;
                    return generator;
                })
                .ToDictionary(generator => generator, generator => Editor.CreateEditor(generator));
        }

        private static void SaveAndDispose()
        {
            IEnumerable<NoiseGeneratorBase> noiseGenerators = _noiseGeneratorToEditor.Keys;
            IEnumerable<Editor> noiseEditors = _noiseGeneratorToEditor.Values;
            IEnumerable<HeightmapGeneratorBase> heightmapGenerators = _heightMapGeneratorToEditor.Keys;
            IEnumerable<Editor> heightmapEditors = _heightMapGeneratorToEditor.Values;

            //�f�[�^�����
            _noiseGeneratorToEditor.Clear();
            _noiseGeneratorToEditor = null;
            _heightMapGeneratorToEditor.Clear();
            _heightMapGeneratorToEditor = null;

            //Editor���������
            foreach (Editor editor in noiseEditors)
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
            foreach (Editor editor in heightmapEditors)
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }

            //Generator��ۑ����Ă���������
            foreach(NoiseGeneratorBase generator in noiseGenerators)
            {
                string json = JsonUtility.ToJson(generator);
                EditorUserSettings.SetConfigValue(generator.GetType().FullName, json);

                UnityEngine.Object.Destroy(generator);
            }
            foreach(HeightmapGeneratorBase generator in heightmapGenerators)
            {
                string json = JsonUtility.ToJson(generator);
                EditorUserSettings.SetConfigValue(generator.GetType().FullName, json);

                UnityEngine.Object.Destroy(generator);
            }
        }

        internal static IReadOnlyList<HeightmapGeneratorBase> GetHeightmapGenerators()
        {
            return _heightMapGeneratorToEditor.Keys.ToList().AsReadOnly();
        }

        internal static IReadOnlyList<NoiseGeneratorBase> GetNoiseGenerators()
        {
            return _noiseGeneratorToEditor.Keys.ToList().AsReadOnly();
        }

        internal static Editor GetNoiseEditor(NoiseGeneratorBase generator)
        {
            return _noiseGeneratorToEditor[generator];
        }

        internal static Editor GetHeightmapEditor(HeightmapGeneratorBase generator)
        {
            return _heightMapGeneratorToEditor[generator];
        }
    }
}
#endif