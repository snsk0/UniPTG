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
            public bool isFoldoutNoiseGenerator;
            public bool isFoldoutHeightmapGenerator;
            public bool isFoldoutTerrain;
            public bool isFoldoutAsset;

            //インデックス
            public int noiseIndex;
            public int heightmapIndex;

            //テレインパラメータ
            public TerrainParameters parameters;

            //アセット
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        //window情報
        private SerializedObject _serializedObject;
        private WindowSettigs _windowSettings;

        [MenuItem("Window/Unity Procedural Terrain Generator")]
        private static void Init()
        {
            GetWindow<MainWindow>("Procedural Terrain Generator");
        }

        //デシリアライズして設定取得
        private void OnEnable()
        {
            //json取得
            string windowJson = EditorUserSettings.GetConfigValue(GetType().FullName);

            //デシリアライズ
            if (!string.IsNullOrEmpty(windowJson))
            {
                _windowSettings = JsonUtility.FromJson<WindowSettigs>(windowJson);
            }
            //初期化処理
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

        //シリアライズして保存する
        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue(GetType().FullName, JsonUtility.ToJson(_windowSettings));
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            //Noise, GeneratorListを取得
            IReadOnlyList<NoiseGeneratorBase> noiseGenerators = GeneratorDatabase.GetNoiseGenerators();
            IReadOnlyList<HeightmapGeneratorBase> heightmapGenerators = GeneratorDatabase.GetHeightmapGenerators();

            //ノイズGUIContentを作成する TODO
            _windowSettings.isFoldoutNoiseGenerator = EditorGUILayout.Foldout(_windowSettings.isFoldoutNoiseGenerator, "Noise Generator");
            if (_windowSettings.isFoldoutNoiseGenerator)
            {

                //ノイズのGUIContentを作成する
                List<GUIContent> gUIContents = new List<GUIContent>();
                foreach (NoiseGeneratorBase generator in noiseGenerators)
                {
                    gUIContents.Add(new GUIContent(generator.GetType().Name));
                }

                //アルゴリズムの一覧表示
                _windowSettings.noiseIndex = EditorGUILayout.IntPopup(
                    new GUIContent("ノイズ"),
                    _windowSettings.noiseIndex,
                    gUIContents.ToArray(),
                    Enumerable.Range(0, gUIContents.Count).ToArray());

                //選択したindexからEditorを呼び出す
                GeneratorDatabase.GetNoiseEditor(noiseGenerators[_windowSettings.noiseIndex]).OnInspectorGUI();
            }

            //Heightmap関連項目
            _windowSettings.isFoldoutHeightmapGenerator = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightmapGenerator, "Heightmap Generator");
            if (_windowSettings.isFoldoutHeightmapGenerator)
            {

                //アルゴリズムのGUIContentを作成する
                List<GUIContent> gUIContents = new List<GUIContent>();
                foreach (HeightmapGeneratorBase generator in heightmapGenerators)
                {
                    gUIContents.Add(new GUIContent(generator.GetType().Name));
                }

                //アルゴリズムの一覧表示
                _windowSettings.heightmapIndex = EditorGUILayout.IntPopup(
                    new GUIContent("アルゴリズム"),
                    _windowSettings.heightmapIndex,
                    gUIContents.ToArray(),
                    Enumerable.Range(0, gUIContents.Count).ToArray());

                //選択したindexからEditorを呼び出す
                GeneratorDatabase.GetHeightmapEditor(heightmapGenerators[_windowSettings.heightmapIndex]).OnInspectorGUI();
            }

            //Terrain関連項目
            _windowSettings.isFoldoutTerrain = EditorGUILayout.Foldout(_windowSettings.isFoldoutTerrain, "Terrain");
            if (_windowSettings.isFoldoutTerrain)
            {
                TerrainParameters parameters = _windowSettings.parameters;
                parameters.scale.x = EditorGUILayout.FloatField(new GUIContent("横幅", "HeightMapの横幅を設定します"), parameters.scale.x);
                parameters.scale.z = EditorGUILayout.FloatField(new GUIContent("奥行", "HeightMapの奥行を設定します"), parameters.scale.z);
                parameters.scale.y = EditorGUILayout.FloatField(new GUIContent("高さ", "HeightMapの高さを設定します"), parameters.scale.y);

                parameters.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("解像度", "HeightMapの解像度を設定します"), parameters.resolutionExp,
                    new[]
                {
                    new GUIContent("33×33"),
                    new GUIContent("65×65"),
                    new GUIContent("129×129"),
                    new GUIContent("257×257"),
                    new GUIContent("513×513"),
                    new GUIContent("1025×1025"),
                    new GUIContent("2049×2049"),
                    new GUIContent("4097×4097"),
                }, Enumerable.Range(Mathf.MinResolutionExp, Mathf.MaxResolutionExp).ToArray());
            }

            //Asset関連項目
            _windowSettings.isFoldoutAsset = EditorGUILayout.Foldout(_windowSettings.isFoldoutAsset, "Assets");
            if (_windowSettings.isFoldoutAsset)
            {
                _windowSettings.isCreateAsset = EditorGUILayout.Toggle(new GUIContent("アセット保存", "Terrain Dataをアセットとして保存するかどうかを指定します"), _windowSettings.isCreateAsset);

                if (_windowSettings.isCreateAsset)
                {
                    _windowSettings.assetName = EditorGUILayout.TextField(new GUIContent("ファイル名", "保存するTerrain Dataのファイル名を指定します"), (_windowSettings.assetName));

                    GUI.enabled = false;
                    EditorGUILayout.TextField(new GUIContent("保存先", "Terrain Dataを保存するパスを表示します"), _windowSettings.assetPath);
                    GUI.enabled = true;

                    if (GUILayout.Button(new GUIContent("保存先を指定する", "Terrain Dataの保存するフォルダを選択します")))
                    {
                        _windowSettings.assetPath = EditorUtility.OpenFolderPanel("保存先選択", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if (_windowSettings.assetPath == string.Empty)
                        {
                            _windowSettings.assetPath = Application.dataPath;
                        }

                        //相対パスを計算
                        Uri basisUri = new Uri(projectPath);
                        Uri absoluteUri = new Uri(_windowSettings.assetPath);
                        _windowSettings.assetPath = basisUri.MakeRelativeUri(absoluteUri).OriginalString;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Terrain Dataを保存しない場合、出力されたTerrainの再使用が困難になります\n保存することを推奨します", MessageType.Warning);
                }
            }

            //更新
            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("テレインを生成する", "設定値からテレインを生成します")))
            {
                //generatorを取得
                NoiseGeneratorBase noiseGenerator = noiseGenerators[_windowSettings.noiseIndex];
                HeightmapGeneratorBase heightmapGenerator = heightmapGenerators[_windowSettings.heightmapIndex];

                //ノイズを初期化
                noiseGenerator.InitState();

                //heightmapを生成
                float[,] heightMap = new float[_windowSettings.parameters.resolution, _windowSettings.parameters.resolution];

                //生成速度を計測
                var stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                heightmapGenerator.Generate(heightMap, _windowSettings.parameters.resolution, noiseGenerator);
                stopWatch.Stop();
                Debug.Log("GenerateTime: " + stopWatch.ElapsedMilliseconds);

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
