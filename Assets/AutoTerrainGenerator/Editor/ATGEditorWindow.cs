#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;
using AutoTerrainGenerator.Attributes;
using AutoTerrainGenerator.NoiseReaders;

namespace AutoTerrainGenerator.Editors
{
    internal class ATGEditorWindow : EditorWindow
    {
        [Serializable]
        private struct ATGWindowSettigs
        {
            //GUI
            public bool isFoldoutNoise;
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

        //����
        [SerializeField]
        private HeightMapGeneratorParam _inputGeneratorData;

        //window���
        private SerializedObject _serializedObject;
        private ATGWindowSettigs _windowSettings;
        private List<HeightMapGeneratorBase> _generators;
        private Dictionary<HeightMapGeneratorBase, Editor> _generatorToInspector;

        private bool _canInputData => _inputGeneratorData == null;

        //TODO ����
        private float _step;

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

                string dataPath = EditorUserSettings.GetConfigValue(nameof(_inputGeneratorData));
                if (!string.IsNullOrEmpty(dataPath))
                {
                    _inputGeneratorData = AssetDatabase.LoadAssetAtPath<HeightMapGeneratorParam>(dataPath);
                }
            }
            //����������
            else
            {
                _windowSettings = new ATGWindowSettigs();
                _windowSettings.isFoldoutNoise = true;
                _windowSettings.isFoldoutTerrain = true;
                _windowSettings.isFoldoutAsset = true;
                _windowSettings.isCreateAsset = true;
                _windowSettings.assetPath = "Assets";
                _windowSettings.assetName = "Terrain";
                _windowSettings.parameters = new TerrainParameters();
            }

            _serializedObject = new SerializedObject(this);

            //settingProvider����ݒ�l���R�s�[����
            UnityEngine.Object providerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(IATGSettingProvider.SettingsPath);
            if(providerObject != null)
            {
                IATGSettingProvider settingProvider = (IATGSettingProvider)Instantiate(providerObject);
                _generators = settingProvider.GetGenerators();
            }

            //������������
            _generatorToInspector = new Dictionary<HeightMapGeneratorBase, Editor>();

            //Attribute���t�����N���X���ꊇ�擾
            TypeCache.TypeCollection types = TypeCache.GetTypesWithAttribute<ATGCustomEditor>();

            //generator�����v������̂����邩����
            bool hasInspector;
            foreach(HeightMapGeneratorBase generator in _generators)
            {
                hasInspector = false;

                foreach(Type editorType in types)
                {
                    Type targetType = editorType.GetCustomAttribute<ATGCustomEditor>().inspectedType;

                    if(generator.GetType() == targetType)
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

            if(_inputGeneratorData != null)
            {
                EditorUserSettings.SetConfigValue(nameof(_inputGeneratorData), AssetDatabase.GetAssetPath(_inputGeneratorData));
            }
            else
            {
                EditorUserSettings.SetConfigValue(nameof(_inputGeneratorData), string.Empty);
            }

            //generator��S�Ĕj������
            foreach(HeightMapGeneratorBase generator in _generators)
            {
                DestroyImmediate(generator);
            }
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            //�ݒ�l�̓ǂݍ���
            /*
            EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_inputGeneratorData)), new GUIContent("�ݒ����"));

            if (!_canInputData)
            {
                GUI.enabled = false;
            }
            */

            _windowSettings.isFoldoutNoise = EditorGUILayout.Foldout(_windowSettings.isFoldoutNoise, "Noise");
            if(_windowSettings.isFoldoutNoise)
            {
                //�A���S���Y����GUIContent���쐬����
                List<GUIContent> gUIContents = new List<GUIContent>();
                List<int> values = new List<int>();
                foreach (HeightMapGeneratorBase generator in _generators)
                {
                    gUIContents.Add(new GUIContent(generator.GetType().ToString()));
                }

                //�A���S���Y���̈ꗗ�\��
                _windowSettings.generatorIndex = EditorGUILayout.IntPopup(new GUIContent("�A���S���Y��"), _windowSettings.generatorIndex, gUIContents.ToArray(), values.ToArray());

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

                int[] resolutionExpArray = new int[ATGMathf.ResolutionExpRange];
                for(int i = 0; i < ATGMathf.ResolutionExpRange; i++)
                {
                    resolutionExpArray[i] = i + ATGMathf.MinResolutionExp;
                }
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
                }, resolutionExpArray);
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

            //Generator�̓��͂��������ꍇ���f����
            if (_inputGeneratorData != null)
            {
                //_generatorData = Instantiate(_inputGeneratorData);
            }
            GUI.enabled = true;

            if (GUILayout.Button(new GUIContent("�e���C���𐶐�����", "�ݒ�l����e���C���𐶐����܂�")))
            {
                //Data���R�s�[���ēn��
                HeightMapGeneratorBase generator = new GeneratorByUnityPerlin();
                float[,] heightMap = generator.Generate(new UnityPerlinNoise(), _windowSettings.parameters.resolution);

                //TerrainData data = TerrainGenerator.Generate(heightMap, _generatorData.scale);

                if (_windowSettings.isCreateAsset)
                {
                    //AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
            }

            if (GUILayout.Button(new GUIContent("�ݒ�l���o�͂���", "�ݒ�l���A�Z�b�g�t�@�C���ɕۑ����܂�")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "settings", "asset", "");
                if(!string.IsNullOrEmpty(savePath)) 
                {
                    //�l���R�s�[����
                    //HeightMapGeneratorParam outputGeneratorData = Instantiate(_generatorData);

                    //�o�͂���
                    //AssetDatabase.CreateAsset(outputGeneratorData, savePath);
                }
            }
        }
    }
}
#endif
