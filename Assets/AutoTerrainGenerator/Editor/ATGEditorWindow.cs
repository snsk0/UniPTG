#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;

namespace AutoTerrainGenerator.Editor
{
    internal class ATGEditorWindow : EditorWindow
    {         
        private SerializedObject _serializedObject;

        //GUI
        private bool _isFoldoutNoise = true;
        private bool _isFoldoutHeightMap = true;
        private bool _isFoldoutAsset = true;

        //�m�C�Y�ϐ�
        private int _noiseTypeIndex;
        private int _seed;
        private float _frequency;
        private bool _isLinearScaling = false;
        private float _amplitude;
        private float _minLinearScale;
        private float _maxLinearScale;
        private int _octaves;

        //�n�C�g�}�b�v
        private int _resolutionExp = ATGMathf.MinResolutionExp;

        //�e���C��
        private Vector3 _scale;

        //�A�Z�b�g
        private bool _isCreateAsset = true;
        private string _assetPath = "Assets";
        private string _assetName = "Terrain";

        //TODO ����
        private float _step;


        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
            Debug.Log("Enable");

            try
            {
                //�ݒ�l���f�V���A���C�Y
                //GUI�֘A
                _isFoldoutNoise = JsonUtility.FromJson<bool>(EditorUserSettings.GetConfigValue(nameof(_isFoldoutNoise)));
                _isFoldoutHeightMap = JsonUtility.FromJson<bool>(EditorUserSettings.GetConfigValue(nameof(_isFoldoutHeightMap)));
                _isFoldoutAsset = JsonUtility.FromJson<bool>(EditorUserSettings.GetConfigValue(nameof(_isFoldoutAsset)));

                //�m�C�Y�֘A
                _noiseTypeIndex = JsonUtility.FromJson<int>(EditorUserSettings.GetConfigValue(nameof(_noiseTypeIndex)));
                _seed = JsonUtility.FromJson<int>(EditorUserSettings.GetConfigValue(nameof(_seed)));
                _frequency = JsonUtility.FromJson<float>(EditorUserSettings.GetConfigValue(nameof(_frequency)));
                _isLinearScaling = JsonUtility.FromJson<bool>(EditorUserSettings.GetConfigValue(nameof(_isLinearScaling)));
                _amplitude = JsonUtility.FromJson<float>(EditorUserSettings.GetConfigValue(nameof(_amplitude)));
                _minLinearScale = JsonUtility.FromJson<float>(EditorUserSettings.GetConfigValue(nameof(_minLinearScale)));
                _maxLinearScale = JsonUtility.FromJson<float>(EditorUserSettings.GetConfigValue(nameof(_maxLinearScale)));
                _octaves = JsonUtility.FromJson<int>(EditorUserSettings.GetConfigValue(nameof(_octaves)));

                //�n�C�g�}�b�v
                _resolutionExp = JsonUtility.FromJson<int>(EditorUserSettings.GetConfigValue(nameof(_resolutionExp)));

                //�e���C��
                _scale = JsonUtility.FromJson<Vector3>(EditorUserSettings.GetConfigValue(nameof(_scale)));

                //�A�Z�b�g
                _isCreateAsset = JsonUtility.FromJson<bool>(EditorUserSettings.GetConfigValue(nameof(_isCreateAsset)));
                _assetPath = JsonUtility.FromJson<string>(EditorUserSettings.GetConfigValue(nameof(_assetPath)));
                _assetName = JsonUtility.FromJson<string>(EditorUserSettings.GetConfigValue(nameof(_assetName)));
            }
            catch(Exception e)
            {
                Debug.LogError("�f�V���A���C�Y�Ɏ��s���܂���");
            }
        }

        private void OnDisable()
        {
            Debug.Log("Disable");
            //�ݒ�l���V���A���C�Y���ĕۑ�
            //GUI�֘A
            EditorUserSettings.SetConfigValue(nameof(_isFoldoutNoise), JsonUtility.ToJson(_isFoldoutNoise));
            EditorUserSettings.SetConfigValue(nameof(_isFoldoutHeightMap), JsonUtility.ToJson(_isFoldoutHeightMap));
            EditorUserSettings.SetConfigValue(nameof(_isFoldoutAsset), JsonUtility.ToJson(_isFoldoutAsset));

            //�m�C�Y�֘A
            EditorUserSettings.SetConfigValue(nameof(_noiseTypeIndex), JsonUtility.ToJson(_noiseTypeIndex));
            EditorUserSettings.SetConfigValue(nameof(_seed), JsonUtility.ToJson(_seed));
            EditorUserSettings.SetConfigValue(nameof(_frequency), JsonUtility.ToJson(_frequency));
            EditorUserSettings.SetConfigValue(nameof(_isLinearScaling), JsonUtility.ToJson(_isLinearScaling));
            EditorUserSettings.SetConfigValue(nameof(_amplitude), JsonUtility.ToJson(_amplitude));
            EditorUserSettings.SetConfigValue(nameof(_minLinearScale), JsonUtility.ToJson(_minLinearScale));
            EditorUserSettings.SetConfigValue(nameof(_maxLinearScale), JsonUtility.ToJson(_maxLinearScale));
            EditorUserSettings.SetConfigValue(nameof(_octaves), JsonUtility.ToJson(_octaves));

            //�n�C�g�}�b�v
            EditorUserSettings.SetConfigValue(nameof(_resolutionExp), JsonUtility.ToJson(_resolutionExp));

            //�e���C��
            EditorUserSettings.SetConfigValue(nameof(_scale), JsonUtility.ToJson(_scale));

            //�A�Z�b�g
            EditorUserSettings.SetConfigValue(nameof(_isCreateAsset), JsonUtility.ToJson(_isCreateAsset));
            EditorUserSettings.SetConfigValue(nameof(_assetPath), JsonUtility.ToJson(_assetPath));
            EditorUserSettings.SetConfigValue(nameof(_assetName), JsonUtility.ToJson(_assetName));
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _isFoldoutNoise = EditorGUILayout.Foldout(_isFoldoutNoise, "Noise");
            if(_isFoldoutNoise)
            {
                _noiseTypeIndex = EditorGUILayout.Popup(new GUIContent("�m�C�Y"), _noiseTypeIndex, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_noiseTypeIndex)
                {
                    case 0:
                        _seed = EditorGUILayout.IntField(new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"), _seed);

                        _frequency = EditorGUILayout.FloatField(new GUIContent("���g��", "�g�p����m�C�Y�̎��g����ݒ肵�܂�"), _frequency);
                        MessageType type = MessageType.Info;
                        if(_frequency > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂�", type);

                        _isLinearScaling = EditorGUILayout.Toggle(new GUIContent("���`�X�P�[�����O", "���`�X�P�[�����O��L�������܂�"), _isLinearScaling);

                        if (!_isLinearScaling)
                        {
                            _amplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                                _amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
                        }
                        else
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                                ref _minLinearScale, ref _maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                            GUI.enabled = false;
                            EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), _minLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), _maxLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), _maxLinearScale - _minLinearScale);
                            GUI.enabled = true;
                        }

                        if (_octaves > 0 && _maxLinearScale == ATGMathf.MaxTerrainHeight)
                        {
                            EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
                        }

                        _octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�\n0�ȉ��ɐݒ肷��Ɩ����ɂȂ�܂�"), _octaves);
                        break;
                }
            }

            _isFoldoutHeightMap = EditorGUILayout.Foldout(_isFoldoutHeightMap, "HeightMap");
            if (_isFoldoutHeightMap)
            {
                _scale.x = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̉�����ݒ肵�܂�"), _scale.x);
                _scale.z = EditorGUILayout.FloatField(new GUIContent("���s", "HeightMap�̉��s��ݒ肵�܂�"), _scale.z);
                _scale.y = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̍�����ݒ肵�܂�"), _scale.y);

                int[] resolutionExpArray = new int[ATGMathf.ResolutionExpRange];
                for(int i = 0; i < ATGMathf.ResolutionExpRange; i++)
                {
                    resolutionExpArray[i] = i + ATGMathf.MinResolutionExp;
                }
                _resolutionExp = EditorGUILayout.IntPopup(new GUIContent("�𑜓x", "HeightMap�̉𑜓x��ݒ肵�܂�"), _resolutionExp, 
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

            _isFoldoutAsset = EditorGUILayout.Foldout(_isFoldoutAsset, "Assets");
            if (_isFoldoutAsset)
            {
                _isCreateAsset = EditorGUILayout.Toggle(new GUIContent("�A�Z�b�g�ۑ�", "Terrain Data���A�Z�b�g�Ƃ��ĕۑ����邩�ǂ������w�肵�܂�"), _isCreateAsset);

                if (_isCreateAsset)
                {
                    _assetName = EditorGUILayout.TextField(new GUIContent("�t�@�C����", "�ۑ�����Terrain Data�̃t�@�C�������w�肵�܂�"), (_assetName));

                    GUI.enabled = false;
                    EditorGUILayout.TextField(new GUIContent("�ۑ���", "Terrain Data��ۑ�����p�X��\�����܂�"), _assetPath);
                    GUI.enabled = true;

                    if(GUILayout.Button(new GUIContent("�ۑ�����w�肷��", "Terrain Data�̕ۑ�����t�H���_��I�����܂�")))
                    {
                        _assetPath = EditorUtility.OpenFolderPanel("�ۑ���I��", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if(_assetPath == string.Empty)
                        {
                            _assetPath = Application.dataPath;
                        }

                        //���΃p�X���v�Z
                        Uri basisUri = new Uri(projectPath);
                        Uri absoluteUri = new Uri(_assetPath);
                        _assetPath = basisUri.MakeRelativeUri(absoluteUri).OriginalString;
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
                float[,] heightMap;
                IHeightMapGenerator generator = new GeneratorByUnityPerlin();

                if (!_isLinearScaling)
                {
                    heightMap = generator.Generate(_seed, _resolutionExp, _frequency, _amplitude, _octaves);
                }
                else
                {
                    heightMap = generator.Generate(_seed, _resolutionExp, _frequency, _minLinearScale, _maxLinearScale, _octaves);
                }
                
                TerrainData data = TerrainGenerator.Generate(heightMap, _scale);

                if (_isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _assetPath + "/" + _assetName + ".asset");
                }
            }
        }
    }
}
#endif
