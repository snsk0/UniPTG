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
        private bool _isAdvancedAmplitude = false;

        //�m�C�Y�ϐ�
        private int _noiseTypeIndex;
        private float _frequency;
        private float _maxAmplitude;
        private float _minAmplitude;
        private int _seed;
        private int _octaves;

        //�n�C�g�}�b�v
        private int _resolutionExp = ATGMathf.MinResolutionExp;

        //�e���C��
        private Vector3 _scale;

        //�A�Z�b�g
        private bool _isCreateAsset;
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
        }

        private void OnDisable()
        {
            
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

                        _isAdvancedAmplitude = EditorGUILayout.Toggle(new GUIContent("���x�ȐU���ݒ�", "�U���ɂ��č��x�Ȑݒ��L�������邩�ǂ�����ݒ肵�܂�"), _isAdvancedAmplitude);

                        if (!_isAdvancedAmplitude)
                        {
                            _maxAmplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                                _maxAmplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
                        }
                        else
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                                ref _minAmplitude, ref _maxAmplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                            GUI.enabled = false;
                            EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), _minAmplitude);
                            EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), _maxAmplitude);
                            GUI.enabled = true;
                        }

                        if (_octaves > 0 && _maxAmplitude == ATGMathf.MaxTerrainHeight)
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
                    EditorGUILayout.TextField(new GUIContent("�t�@�C����", "�ۑ�����Terrain Data�̃t�@�C�������w�肵�܂�"), (_assetName));

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
                var map = new GeneratorByUnityPerlin().Generate(_seed, _resolutionExp, _frequency, _minAmplitude, _maxAmplitude, _octaves);
                var data = TerrainGenerator.Generate(map, _scale);

                if (_isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _assetPath + "/" + _assetName + ".asset");
                }
            }
        }
    }
}
#endif
