#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;
using System.Collections.Generic;
using UnityEditor.EditorTools;

namespace AutoTerrainGenerator.Editors
{
    internal class ATGEditorWindow : EditorWindow
    {
        [Serializable]
        private struct ATGWindowSettigs
        {
            //GUI
            public bool isFoldoutNoise;
            public bool isFoldoutHeightMap;
            public bool isFoldoutAsset;

            //�A���S���Y���w��
            public int generatorIndex;

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
        private List<IHeightMapGenerator> _generators;
        private Dictionary<IHeightMapGenerator, GeneratorEditor> _generatorToInspector;

        private HeightMapGeneratorParam _generatorData;
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
            string generaterJson = EditorUserSettings.GetConfigValue(nameof(_generatorData));

            //�f�V���A���C�Y
            if(!string.IsNullOrEmpty(windowJson) && ! string.IsNullOrEmpty(generaterJson)) 
            {
                _windowSettings = JsonUtility.FromJson<ATGWindowSettigs>(windowJson);

                _generatorData = CreateInstance<HeightMapGeneratorParam>();
                JsonUtility.FromJsonOverwrite(generaterJson, _generatorData);

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
                _windowSettings.isFoldoutHeightMap = true;
                _windowSettings.isFoldoutAsset = true;
                _windowSettings.isCreateAsset = true;
                _windowSettings.assetPath = "Assets";
                _windowSettings.assetName = "Terrain";
                
                _generatorData = CreateInstance<HeightMapGeneratorParam>();
            }

            _serializedObject = new SerializedObject(this);

            //settingProvider����ݒ�l���R�s�[����
            UnityEngine.Object providerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(IATGSettingProvider.SettingsPath);
            if(providerObject != null)
            {
                IATGSettingProvider settingProvider = (IATGSettingProvider)Instantiate(providerObject);
                _generators = settingProvider.GetGenerators();
            }

            //ATGCustomEditor�̂����N���X���擾
            _generatorToInspector = new Dictionary<IHeightMapGenerator, GeneratorEditor>();
            foreach(Type type in TypeCache.GetTypesWithAttribute<ATGCustomEditor>())
            {
                Debug.Log("type");
                ATGCustomEditor attributeType = type.GetCustomAttribute<ATGCustomEditor>();

                //CustomAttribute���Ȃ��Ȃ炻�̃N���X���X�L�b�v
                if (attributeType == null)
                {
                    Debug.Log("NoneAttribute");
                    break;
                }

                //��v������̂��i�[
                foreach (IHeightMapGenerator generator in _generators)
                {
                    if (generator.GetType() == attributeType.inspectedType)
                    {
                        //Editor�𐶐�����
                        GeneratorEditor editor = (GeneratorEditor)Activator.CreateInstance(type);

                        //�R�Â��Ċi�[
                        _generatorToInspector.Add(generator, editor);
                        break;
                    }
                }
            }
        }

        //�V���A���C�Y���ĕۑ�����
        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue(nameof(_windowSettings), JsonUtility.ToJson(_windowSettings));
            EditorUserSettings.SetConfigValue(nameof(_generatorData), JsonUtility.ToJson(_generatorData));

            if(_inputGeneratorData != null)
            {
                EditorUserSettings.SetConfigValue(nameof(_inputGeneratorData), AssetDatabase.GetAssetPath(_inputGeneratorData));
            }
            else
            {
                EditorUserSettings.SetConfigValue(nameof(_inputGeneratorData), string.Empty);
            }
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            //�ݒ�l�̓ǂݍ���
            EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_inputGeneratorData)), new GUIContent("�ݒ����"));

            if (!_canInputData)
            {
                GUI.enabled = false;
            }

            _windowSettings.isFoldoutNoise = EditorGUILayout.Foldout(_windowSettings.isFoldoutNoise, "Noise");
            if(_windowSettings.isFoldoutNoise)
            {
                _generatorData.noiseTypeIndex = EditorGUILayout.Popup(new GUIContent("�m�C�Y"), _generatorData.noiseTypeIndex, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_generatorData.noiseTypeIndex)
                {
                    case 0:
                        //�A���S���Y����GUIContent���쐬����
                        List<GUIContent> gUIContents = new List<GUIContent>();
                        List<int> values = new List<int>();
                        foreach(IHeightMapGenerator generator in _generators)
                        {
                            gUIContents.Add(new GUIContent(generator.GetType().ToString()));
                        }

                        //�A���S���Y���̈ꗗ�\��
                        _windowSettings.generatorIndex = EditorGUILayout.IntPopup(new GUIContent("�A���S���Y��"), _windowSettings.generatorIndex, gUIContents.ToArray(), values.ToArray());

                        //�I������index�̊g���𒲂ׂ�
                        IHeightMapGenerator selectedGenerator = _generators[_windowSettings.generatorIndex];
                        if (_generatorToInspector.ContainsKey(selectedGenerator))
                        {
                            _generatorToInspector[selectedGenerator].OnInspectorGUI();
                        }
                        //�g�����Ȃ��ꍇ�ASerializeField��S�ĕ\������
                        else
                        {
                        }

                        /*
                        _generatorData.seed = EditorGUILayout.IntField(new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"), _generatorData.seed);

                        _generatorData.frequency = EditorGUILayout.FloatField(new GUIContent("���g��", "�g�p����m�C�Y�̎��g����ݒ肵�܂�"), _generatorData.frequency);
                        MessageType type = MessageType.Info;
                        if(_generatorData.frequency > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂�", type);

                        _generatorData.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("���`�X�P�[�����O", "���`�X�P�[�����O��L�������܂�"), 
                            _generatorData.isLinearScaling);

                        if (!_generatorData.isLinearScaling)
                        {
                            _generatorData.amplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                                _generatorData.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
                        }
                        else
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                                ref _generatorData.minLinearScale, ref _generatorData.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                            GUI.enabled = false;
                            EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), _generatorData.minLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), _generatorData.maxLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), _generatorData.maxLinearScale - _generatorData.minLinearScale);
                            if (_canInputData)
                            {
                                GUI.enabled = true;
                            }
                        }

                        if (_generatorData.octaves > 0 && _generatorData.maxLinearScale == ATGMathf.MaxTerrainHeight)
                        {
                            EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
                        }

                        _generatorData.octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�"), _generatorData.octaves);
                        */
                        break;
                }
            }

            _windowSettings.isFoldoutHeightMap = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMap, "HeightMap");
            if (_windowSettings.isFoldoutHeightMap)
            {
                _generatorData.scale.x = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̉�����ݒ肵�܂�"), _generatorData.scale.x);
                _generatorData.scale.z = EditorGUILayout.FloatField(new GUIContent("���s", "HeightMap�̉��s��ݒ肵�܂�"), _generatorData.scale.z);
                _generatorData.scale.y = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̍�����ݒ肵�܂�"), _generatorData.scale.y);

                int[] resolutionExpArray = new int[ATGMathf.ResolutionExpRange];
                for(int i = 0; i < ATGMathf.ResolutionExpRange; i++)
                {
                    resolutionExpArray[i] = i + ATGMathf.MinResolutionExp;
                }
                _generatorData.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("�𑜓x", "HeightMap�̉𑜓x��ݒ肵�܂�"), _generatorData.resolutionExp, 
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
                _generatorData = Instantiate(_inputGeneratorData);
            }
            GUI.enabled = true;

            if (GUILayout.Button(new GUIContent("�e���C���𐶐�����", "�ݒ�l����e���C���𐶐����܂�")))
            {
                //Data���R�s�[���ēn��
                IHeightMapGenerator generator = new GeneratorByUnityPerlin();
                float[,] heightMap = generator.Generate();

                TerrainData data = TerrainGenerator.Generate(heightMap, _generatorData.scale);

                if (_windowSettings.isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
            }

            if (GUILayout.Button(new GUIContent("�ݒ�l���o�͂���", "�ݒ�l���A�Z�b�g�t�@�C���ɕۑ����܂�")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "settings", "asset", "");
                if(!string.IsNullOrEmpty(savePath)) 
                {
                    //�l���R�s�[����
                    HeightMapGeneratorParam outputGeneratorData = Instantiate(_generatorData);

                    //�o�͂���
                    AssetDatabase.CreateAsset(outputGeneratorData, savePath);
                }
            }
        }
    }
}
#endif
