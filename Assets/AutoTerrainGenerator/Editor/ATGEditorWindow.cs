#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;

namespace AutoTerrainGenerator.Editor
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

            //�n�`�����֘A�p�����[�^
            public HeightMapGeneratorData generatorData;

            //�A�Z�b�g
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        private SerializedObject _serializedObject;
        private ATGWindowSettigs _windowSettings;
        private HeightMapGeneratorData generatorData => _windowSettings.generatorData;

        //TODO ����
        private float _step;

        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        private void OnEnable()
        {
            string windowSettings = EditorUserSettings.GetConfigValue(nameof(_windowSettings));
            if (!string.IsNullOrEmpty(windowSettings))
            {
                _windowSettings = JsonUtility.FromJson<ATGWindowSettigs>(windowSettings);
            }
            else
            {
                _windowSettings = new ATGWindowSettigs();
                _windowSettings.isFoldoutNoise = true;
                _windowSettings.isFoldoutHeightMap = true;
                _windowSettings.isFoldoutAsset = true;
                _windowSettings.isCreateAsset = true;
                _windowSettings.assetPath = "Assets";
                _windowSettings.assetName = "Terrain";
                _windowSettings.generatorData = new HeightMapGeneratorData();
            }

            _serializedObject = new SerializedObject(this);
        }

        private void OnDisable()
        {
            //�f�V���A���C�Y���ĕۑ�
            EditorUserSettings.SetConfigValue(nameof(_windowSettings), JsonUtility.ToJson(_windowSettings));
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _windowSettings.isFoldoutNoise = EditorGUILayout.Foldout(_windowSettings.isFoldoutNoise, "Noise");
            if(_windowSettings.isFoldoutNoise)
            {
                _windowSettings.generatorData.noiseTypeIndex = EditorGUILayout.Popup(new GUIContent("�m�C�Y"), _windowSettings.generatorData.noiseTypeIndex, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_windowSettings.generatorData.noiseTypeIndex)
                {
                    case 0:
                        generatorData.seed = EditorGUILayout.IntField(new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"), generatorData.seed);

                        generatorData.frequency = EditorGUILayout.FloatField(new GUIContent("���g��", "�g�p����m�C�Y�̎��g����ݒ肵�܂�"), generatorData.frequency);
                        MessageType type = MessageType.Info;
                        if(generatorData.frequency > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂�", type);

                        generatorData.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("���`�X�P�[�����O", "���`�X�P�[�����O��L�������܂�"), 
                            generatorData.isLinearScaling);

                        if (!generatorData.isLinearScaling)
                        {
                            generatorData.amplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                                generatorData.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
                        }
                        else
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                                ref generatorData.minLinearScale, ref generatorData.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                            GUI.enabled = false;
                            EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), generatorData.minLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), generatorData.maxLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), generatorData.maxLinearScale - generatorData.minLinearScale);
                            GUI.enabled = true;
                        }

                        if (generatorData.octaves > 0 && generatorData.maxLinearScale == ATGMathf.MaxTerrainHeight)
                        {
                            EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
                        }

                        generatorData.octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�"), generatorData.octaves);
                        break;
                }
            }

            _windowSettings.isFoldoutHeightMap = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMap, "HeightMap");
            if (_windowSettings.isFoldoutHeightMap)
            {
                generatorData.scale.x = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̉�����ݒ肵�܂�"), generatorData.scale.x);
                generatorData.scale.z = EditorGUILayout.FloatField(new GUIContent("���s", "HeightMap�̉��s��ݒ肵�܂�"), generatorData.scale.z);
                generatorData.scale.y = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̍�����ݒ肵�܂�"), generatorData.scale.y);

                int[] resolutionExpArray = new int[ATGMathf.ResolutionExpRange];
                for(int i = 0; i < ATGMathf.ResolutionExpRange; i++)
                {
                    resolutionExpArray[i] = i + ATGMathf.MinResolutionExp;
                }
                generatorData.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("�𑜓x", "HeightMap�̉𑜓x��ݒ肵�܂�"), generatorData.resolutionExp, 
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

            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("�e���C���𐶐�����", "�ݒ�l����e���C���𐶐����܂�")))
            {
                IHeightMapGenerator generator = new GeneratorByUnityPerlin();
                float[,] heightMap = generator.Generate(generatorData);

                TerrainData data = TerrainGenerator.Generate(heightMap, generatorData.scale);

                if (_windowSettings.isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
            }
        }
    }
}
#endif
