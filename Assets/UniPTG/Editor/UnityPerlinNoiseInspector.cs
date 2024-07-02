#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UniPTG.NoiseGenerators;

namespace UniPTG.Editors
{
    [CustomEditor(typeof(UnityPerlinNoise))]
    public class UnityPerlinNoiseInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty isTimeModeProperty = serializedObject.FindProperty("_isTimeMode");
            EditorGUILayout.PropertyField(isTimeModeProperty, new GUIContent("���ԕω�", "�V�[�h�l�̎��ԕω���L���ɂ��܂�"));

            if(!isTimeModeProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_seed"), new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
