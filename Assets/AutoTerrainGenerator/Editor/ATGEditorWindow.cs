#if UNITY_EDITOR 
using UnityEngine;
using UnityEditor;

namespace AutoTerrainGenerator.Editor
{
    public class ATGEditorWindow : EditorWindow
    {
        private SerializedObject _serializedObject;
        private bool _isFoldHeightMap;
        private bool _isFoldAsset;

        [SerializeField] private bool _isCreateAsset;
        [SerializeField] private string _assetPath;
        [SerializeField] private Vector3 _scale;
        [SerializeField] private int _resolution;


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

            _isFoldHeightMap = EditorGUILayout.Foldout(_isFoldHeightMap, "HeightMap");
            if (_isFoldHeightMap)
            {
                _scale.x = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̉�����ݒ肵�܂�"), _scale.x);
                _scale.z = EditorGUILayout.FloatField(new GUIContent("���s", "HeightMap�̉��s��ݒ肵�܂�"), _scale.z);
                _scale.y = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̍�����ݒ肵�܂�"), _scale.y);

                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_resolution)),
                    new GUIContent("�𑜓x", "HeightMap�̉𑜓x��ݒ肵�܂�"));

                if (!(IsOfForm2NPlus1(_resolution)))
                {
                    EditorGUILayout.HelpBox("�𑜓x��2�̗ݏ�+1�𖞂����悤�ɐݒ肵�Ă�������\n�������n�`����������܂���", MessageType.Warning);
                }
            }

            _isFoldAsset = EditorGUILayout.Foldout(_isFoldAsset, "Assets");
            if (_isFoldAsset)
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_isCreateAsset)),
                    new GUIContent("�A�Z�b�g�ۑ�", "Terrain Data���A�Z�b�g�Ƃ��ĕۑ����邩�ǂ������w�肵�܂�"));

                if (_isCreateAsset)
                {
                    EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_assetPath)),
                    new GUIContent("�p�X", "Terrain Data��ۑ�����p�X���w�肵�܂�"));
                }
                else
                {
                    EditorGUILayout.HelpBox("Terrain Data��ۑ����Ȃ��ꍇ�A�o�͂��ꂽTerrain�̍Ďg�p������ɂȂ�܂�\n�ۑ����邱�Ƃ𐄏����܂�", MessageType.Warning);
                }
            }

            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Generate Terrain"))
            {
                var data = TerrainGenerator.Generate(_scale, _resolution);

                if (_isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _assetPath + "/Terrain.asset");
                }
            }
        }

        /*
         * ���R�[�h
         */
        private bool IsOfForm2NPlus1(int x)
        {
            if (x <= 1)
            {
                return false;
            }
            int y = x - 1;
            return IsPowerOfTwo(y);
        }

        private bool IsPowerOfTwo(int n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }
    }
}
#endif
