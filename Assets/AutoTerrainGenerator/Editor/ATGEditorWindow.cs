#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace AutoTerrainGenerator.Editor
{
    public class ATGEditorWindow : EditorWindow
    {
        private const int MinResolutionEx = 5;
         
        private SerializedObject _serializedObject;

        private bool _isFoldNoise = true;
        private bool _isFoldHeightMap = true;
        private bool _isFoldAsset = true;

        private int _noiseType;
        [SerializeField] private float _noiseScale;
        [SerializeField] private int _seed;
        [SerializeField] private int _octaves;
        [SerializeField] private float _persistance;
        [SerializeField] private bool _isCreateAsset;
        [SerializeField] private string _assetPath = "Assets";
        private Vector3 _scale;
        private int _selectedResolutionEx;
        [SerializeField] private float _step;


        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _isFoldNoise = EditorGUILayout.Foldout(_isFoldNoise, "Noise");
            if(_isFoldNoise)
            {
                _noiseType = EditorGUILayout.Popup(new GUIContent("�m�C�Y"), _noiseType, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_noiseType)
                {
                    case 0:
                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_noiseScale)),
                            new GUIContent("�X�P�[���l", "�g�p����m�C�Y�̃X�P�[���l��ݒ肵�܂�"));

                        MessageType type = MessageType.Info;
                        if(_noiseScale > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂��B", type);

                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_seed)),
                            new GUIContent("�V�[�h�l", "�g�p����m�C�Y�̃V�[�h�l��ݒ肵�܂�"));

                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_octaves)),
                            new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�\n0�ȉ��ɐݒ肷��Ɩ����ɂȂ�܂�"));

                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_persistance)),
                            new GUIContent("�e���x", "�d�˂�m�C�Y�̉e���x���w�肵�܂�\n��ʓI��0.5����������܂�\n�܂��A�K��0�ȏ�1�����̐��l��ݒ肵�Ă�������"));
                        break;
                }
            }

            _isFoldHeightMap = EditorGUILayout.Foldout(_isFoldHeightMap, "HeightMap");
            if (_isFoldHeightMap)
            {
                _scale.x = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̉�����ݒ肵�܂�"), _scale.x);
                _scale.z = EditorGUILayout.FloatField(new GUIContent("���s", "HeightMap�̉��s��ݒ肵�܂�"), _scale.z);
                _scale.y = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̍�����ݒ肵�܂�"), _scale.y);

                _selectedResolutionEx = EditorGUILayout.Popup(new GUIContent("�𑜓x"), _selectedResolutionEx, new[]
                {
                    new GUIContent("33�~33"),
                    new GUIContent("65�~65"),
                    new GUIContent("129�~129"),
                    new GUIContent("257�~257"),
                    new GUIContent("513�~513"),
                    new GUIContent("1025�~1025"),
                    new GUIContent("2049�~2049"),
                    new GUIContent("4097�~4097"),
                });

                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_step)),
                            new GUIContent("���U�l", "HeightMap�𗣎U�����܂�\n0�ȏ�̐��l�ɐݒ肷�邱�Ƃŋ@�\���܂�"));
            }

            _isFoldAsset = EditorGUILayout.Foldout(_isFoldAsset, "Assets");
            if (_isFoldAsset)
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_isCreateAsset)),
                    new GUIContent("�A�Z�b�g�ۑ�", "Terrain Data���A�Z�b�g�Ƃ��ĕۑ����邩�ǂ������w�肵�܂�"));

                if (_isCreateAsset)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_assetPath)),
                        new GUIContent("�ۑ���", "Terrain Data��ۑ�����p�X��\�����܂�"));
                    GUI.enabled = true;

                    if(GUILayout.Button(new GUIContent("�ۑ�����w�肷��", "Terrain Data�̕ۑ�����t�H���_��I�����܂�")))
                    {
                        _assetPath = EditorUtility.OpenFolderPanel("�ۑ���I��", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if(_assetPath == string.Empty)
                        {
                            _assetPath = Application.dataPath;
                        }

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
                var data = TerrainGenerator.Generate(_scale, _selectedResolutionEx + MinResolutionEx, _noiseScale, _seed, _octaves, _persistance, _step);

                if (_isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _assetPath + "/Terrain.asset");
                }
            }
        }
    }
}
#endif
