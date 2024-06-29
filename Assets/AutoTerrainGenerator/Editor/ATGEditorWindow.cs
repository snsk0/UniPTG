#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.NoiseReaders;

namespace AutoTerrainGenerator.Editors
{
    internal class ATGEditorWindow : EditorWindow
    {
        [Serializable]
        private struct ATGWindowSettigs
        {
            //GUI
            public bool isFoldoutHeightMapGenerator;
            public bool isFoldoutTerrain;
            public bool isFoldoutAsset;

            //�A���S���Y���w��
            public int generatorIndex;

            //�e���C���p�����[�^
            public TerrainParameters parameters;

            //�A�Z�b�g
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        //window���
        private SerializedObject _serializedObject;
        private ATGWindowSettigs _windowSettings;
        private List<HeightMapGeneratorBase> _generators;
        private Dictionary<HeightMapGeneratorBase, Editor> _generatorToInspector;

        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        //�f�V���A���C�Y���Đݒ�擾
        private void OnEnable()
        {
            //json�擾
            string windowJson = EditorUserSettings.GetConfigValue(nameof(_windowSettings));

            //�f�V���A���C�Y
            if(!string.IsNullOrEmpty(windowJson)) 
            {
                _windowSettings = JsonUtility.FromJson<ATGWindowSettigs>(windowJson);
            }
            //����������
            else
            {
                _windowSettings = new ATGWindowSettigs();
                _windowSettings.isFoldoutHeightMapGenerator = true;
                _windowSettings.isFoldoutTerrain = true;
                _windowSettings.isFoldoutAsset = true;
                _windowSettings.isCreateAsset = true;
                _windowSettings.assetPath = "Assets";
                _windowSettings.assetName = "Terrain";
                _windowSettings.parameters = new TerrainParameters();
            }

            _serializedObject = new SerializedObject(this);

            //settingProvider����A���S���Y�����擾
            UnityEngine.Object providerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(IATGSettingProvider.SettingsPath);
            if(providerObject != null)
            {
                IATGSettingProvider settingProvider = (IATGSettingProvider)Instantiate(providerObject);

                //�C���X�^���X���擾����
                _generators = settingProvider.GetGenerators();

                //�N���X�����L�[��generatorInstance���擾�A�ł���������㏑��
                foreach(HeightMapGeneratorBase generator in _generators)
                {
                    string generatorJson = EditorUserSettings.GetConfigValue(generator.GetType().Name);
                    if(!string.IsNullOrEmpty(generatorJson))
                    {
                        JsonUtility.FromJsonOverwrite(generatorJson, generator);
                    }
                }
            }

            //������������
            _generatorToInspector = new Dictionary<HeightMapGeneratorBase, Editor>();

            //Attribute���t�����N���X���ꊇ�擾
            TypeCache.TypeCollection editorTypes = TypeCache.GetTypesWithAttribute<CustomEditor>();

            //generator�����v������̂����邩����
            bool hasInspector;
            foreach(HeightMapGeneratorBase generator in _generators)
            {
                if(generator == null)
                {
                    Debug.LogWarning("BreakObject");
                    break;
                }
                hasInspector = false;

                foreach(Type editorType in editorTypes)
                {
                    //���t���N�V�������g�p����Attribute����^�[�Q�b�g�̃^�C�v���擾
                    CustomEditor attribute = editorType.GetCustomAttribute<CustomEditor>();
                    FieldInfo fieldInfo = attribute.GetType().GetField("m_InspectedType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    Type targetType = (Type)fieldInfo.GetValue(attribute);

                    if (generator.GetType() == targetType)
                    {
                        //Editor�𐶐�����
                        Editor editor = Editor.CreateEditor(generator, editorType);

                        //�R�Â��Ċi�[
                        _generatorToInspector.Add(generator, editor);

                        hasInspector = true;
                        break;
                    }
                }

                //�C���X�y�N�^�g����������Ȃ������ꍇ�A�ʏ��Editor���i�[
                if (!hasInspector)
                {
                    Editor editor = Editor.CreateEditor(generator);
                    _generatorToInspector.Add(generator, editor);
                }
            }
        }

        //�V���A���C�Y���ĕۑ�����
        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue(nameof(_windowSettings), JsonUtility.ToJson(_windowSettings));

            //�ǂݍ���ł���generator��serialize���Ċi�[
            foreach(HeightMapGeneratorBase generator in _generators)
            {
                EditorUserSettings.SetConfigValue(generator.GetType().Name, JsonUtility.ToJson(generator));
            }

            //Editor���ɑS�Ĕj������
            foreach(Editor editor in _generatorToInspector.Values)
            {
                DestroyImmediate(editor);
            }

            //generator��Editor�̌�ɑS�Ĕj������
            foreach (HeightMapGeneratorBase generator in _generators)
            {
                DestroyImmediate(generator);
            }
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _windowSettings.isFoldoutHeightMapGenerator = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMapGenerator, "HeightMapGenerator");
            if(_windowSettings.isFoldoutHeightMapGenerator)
            {
                //�A���S���Y����GUIContent���쐬����
                List<GUIContent> gUIContents = new List<GUIContent>();
                foreach (HeightMapGeneratorBase generator in _generators)
                {
                    gUIContents.Add(new GUIContent(generator.GetType().ToString()));
                }

                //�A���S���Y���̈ꗗ�\��
                _windowSettings.generatorIndex = EditorGUILayout.IntPopup(
                    new GUIContent("�A���S���Y��"), 
                    _windowSettings.generatorIndex, 
                    gUIContents.ToArray(), 
                    Enumerable.Range(0, gUIContents.Count).ToArray());

                //�I������index����Editor���Ăяo��
                _generatorToInspector[_generators[_windowSettings.generatorIndex]].OnInspectorGUI();
            }

            _windowSettings.isFoldoutTerrain = EditorGUILayout.Foldout(_windowSettings.isFoldoutTerrain, "Terrain");
            if (_windowSettings.isFoldoutTerrain)
            {
                TerrainParameters parameters = _windowSettings.parameters;
                parameters.scale.x = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̉�����ݒ肵�܂�"), parameters.scale.x);
                parameters.scale.z = EditorGUILayout.FloatField(new GUIContent("���s", "HeightMap�̉��s��ݒ肵�܂�"), parameters.scale.z);
                parameters.scale.y = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̍�����ݒ肵�܂�"), parameters.scale.y);

                parameters.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("�𑜓x", "HeightMap�̉𑜓x��ݒ肵�܂�"), parameters.resolutionExp, 
                    new[]
                {
                    new GUIContent("33�~33"),
                    new GUIContent("65�~65"),
                    new GUIContent("129�~129"),
                    new GUIContent("257�~257"),
                    new GUIContent("513�~513"),
                    new GUIContent("1025�~1025"),
                    new GUIContent("2049�~2049"),
                    new GUIContent("4097�~4097"),
                }, Enumerable.Range(ATGMathf.MinResolutionExp, ATGMathf.MaxResolutionExp).ToArray());
            }

            _windowSettings.isFoldoutAsset = EditorGUILayout.Foldout(_windowSettings.isFoldoutAsset, "Assets");
            if (_windowSettings.isFoldoutAsset)
            {
                _windowSettings.isCreateAsset = EditorGUILayout.Toggle(new GUIContent("�A�Z�b�g�ۑ�", "Terrain Data���A�Z�b�g�Ƃ��ĕۑ����邩�ǂ������w�肵�܂�"), _windowSettings.isCreateAsset);

                if (_windowSettings.isCreateAsset)
                {
                    _windowSettings.assetName = EditorGUILayout.TextField(new GUIContent("�t�@�C����", "�ۑ�����Terrain Data�̃t�@�C�������w�肵�܂�"), (_windowSettings.assetName));

                    GUI.enabled = false;
                    EditorGUILayout.TextField(new GUIContent("�ۑ���", "Terrain Data��ۑ�����p�X��\�����܂�"), _windowSettings.assetPath);
                    GUI.enabled = true;

                    if(GUILayout.Button(new GUIContent("�ۑ�����w�肷��", "Terrain Data�̕ۑ�����t�H���_��I�����܂�")))
                    {
                        _windowSettings.assetPath = EditorUtility.OpenFolderPanel("�ۑ���I��", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if(_windowSettings.assetPath == string.Empty)
                        {
                            _windowSettings.assetPath = Application.dataPath;
                        }

                        //���΃p�X���v�Z
                        Uri basisUri = new Uri(projectPath);
                        Uri absoluteUri = new Uri(_windowSettings.assetPath);
                        _windowSettings.assetPath = basisUri.MakeRelativeUri(absoluteUri).OriginalString;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Terrain Data��ۑ����Ȃ��ꍇ�A�o�͂��ꂽTerrain�̍Ďg�p������ɂȂ�܂�\n�ۑ����邱�Ƃ𐄏����܂�", MessageType.Warning);
                }
            }

            //�X�V
            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("�e���C���𐶐�����", "�ݒ�l����e���C���𐶐����܂�")))
            {
                //Data���R�s�[���ēn��
                HeightMapGeneratorBase generator = _generators[_windowSettings.generatorIndex];
                float[,] heightMap = generator.Generate(new UnityPerlinNoise(), _windowSettings.parameters.resolution);

                TerrainData data = TerrainGenerator.Generate(heightMap, _windowSettings.parameters.scale);

                if (_windowSettings.isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
            }
        }
    }
}
#endif
