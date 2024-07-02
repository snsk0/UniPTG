#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniPTG.Editors
{
    internal class MainWindow : EditorWindow
    {
        [Serializable]
        private struct WindowSettigs
        {
            //GUI
            public bool isFoldoutHeightmapGenerator;
            public bool isFoldoutTerrain;
            public bool isFoldoutAsset;

            //�C���f�b�N�X
            public int noiseReaderIndex;
            public int heightmapIndex;

            //�e���C���p�����[�^
            public TerrainParameters parameters;

            //�A�Z�b�g
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        //window���
        private SerializedObject _serializedObject;
        private WindowSettigs _windowSettings;

        [MenuItem("Window/Unity Procedural Terrain Generator")]
        private static void Init()
        {
            GetWindow<MainWindow>("Procedural Terrain Generator");
        }

        //�f�V���A���C�Y���Đݒ�擾
        private void OnEnable()
        {
            //json�擾
            string windowJson = EditorUserSettings.GetConfigValue(GetType().FullName);

            //�f�V���A���C�Y
            if (!string.IsNullOrEmpty(windowJson))
            {
                _windowSettings = JsonUtility.FromJson<WindowSettigs>(windowJson);
            }
            //����������
            else
            {
                _windowSettings = new WindowSettigs();
                _windowSettings.isFoldoutHeightmapGenerator = true;
                _windowSettings.isFoldoutTerrain = true;
                _windowSettings.isFoldoutAsset = true;
                _windowSettings.isCreateAsset = true;
                _windowSettings.assetPath = "Assets";
                _windowSettings.assetName = "Terrain";
                _windowSettings.parameters = new TerrainParameters();
            }

            _serializedObject = new SerializedObject(this);
        }

        //�V���A���C�Y���ĕۑ�����
        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue(GetType().FullName, JsonUtility.ToJson(_windowSettings));
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            //Noise, GeneratorList���擾
            IReadOnlyList<HeightmapGeneratorBase> heightmapGenerators = GeneratorDatabase.GetHeightmapGenerators();

            //�m�C�YGUIContent���쐬���� TODO

            //Heightmap�֘A����
            _windowSettings.isFoldoutHeightmapGenerator = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightmapGenerator, "HeightmapGenerator");
            if (_windowSettings.isFoldoutHeightmapGenerator)
            {

                //�A���S���Y����GUIContent���쐬����
                List<GUIContent> gUIContents = new List<GUIContent>();
                foreach (HeightmapGeneratorBase generator in heightmapGenerators)
                {
                    gUIContents.Add(new GUIContent(generator.GetType().Name));
                }

                //�A���S���Y���̈ꗗ�\��
                _windowSettings.heightmapIndex = EditorGUILayout.IntPopup(
                    new GUIContent("�A���S���Y��"),
                    _windowSettings.heightmapIndex,
                    gUIContents.ToArray(),
                    Enumerable.Range(0, gUIContents.Count).ToArray());

                //�I������index����Editor���Ăяo��
                GeneratorDatabase.GetEditorByGenerator(heightmapGenerators[_windowSettings.heightmapIndex]).OnInspectorGUI();
            }

            //Terrain�֘A����
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
                }, Enumerable.Range(Mathf.MinResolutionExp, Mathf.MaxResolutionExp).ToArray());
            }

            //Asset�֘A����
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

                    if (GUILayout.Button(new GUIContent("�ۑ�����w�肷��", "Terrain Data�̕ۑ�����t�H���_��I�����܂�")))
                    {
                        _windowSettings.assetPath = EditorUtility.OpenFolderPanel("�ۑ���I��", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if (_windowSettings.assetPath == string.Empty)
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
                HeightmapGeneratorBase generator = heightmapGenerators[_windowSettings.heightmapIndex];
                float[,] heightMap = generator.Generate(_windowSettings.parameters.resolution);

                //TerrainData data = TerrainGenerator.Generate(heightMap, _windowSettings.parameters.scale);

                /*if (_windowSettings.isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
                */
            }
        }
    }
}
#endif
