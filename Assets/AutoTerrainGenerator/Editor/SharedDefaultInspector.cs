#if UNITY_EDITOR
using System;
using AutoTerrainGenerator.Parameters;
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator.Editors
{
    internal static class SharedDefaultInspector
    {
        internal static void OnEnable(SerializedObject serializedObject, Type type)
        {
            serializedObject.Update();

            string paramJson = EditorUserSettings.GetConfigValue(type.Name);
            SerializedProperty paramProperty = serializedObject.FindProperty("_param");

            //Json������ꍇ
            bool isDeserialized = false;
            if (!string.IsNullOrEmpty(paramJson))
            {
                //�f�V���A���C�Y�����s
                HeightMapGeneratorParam param = ScriptableObject.CreateInstance<HeightMapGeneratorParam>();
                JsonUtility.FromJsonOverwrite(paramJson, param);

                //���������ꍇ
                if (param != null)
                {
                    paramProperty.objectReferenceValue = param;
                    isDeserialized = true;
                }
            }

            //�f�V���A���C�Y�Ɏ��s�����ꍇ��������
            if (!isDeserialized) 
            {
                paramProperty.objectReferenceValue = ScriptableObject.CreateInstance<HeightMapGeneratorParam>();
            }

            //path����A�Z�b�g��ǂݍ���
            string assetPath = EditorUserSettings.GetConfigValue(type.Name + ".input");
            if (!string.IsNullOrEmpty(assetPath))
            {
                serializedObject.FindProperty("_inputParam").objectReferenceValue = AssetDatabase.LoadAssetAtPath<HeightMapGeneratorBase>(assetPath);
            }
            serializedObject.ApplyModifiedProperties();
        }

        internal static void OnDisable(SerializedObject serializedObject, Type type) 
        {
            //�V���A���C�Y�����s
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;
            EditorUserSettings.SetConfigValue(type.Name, JsonUtility.ToJson(param));

            string assetPath = AssetDatabase.GetAssetPath(serializedObject.FindProperty("_inputParam").objectReferenceValue);
            EditorUserSettings.SetConfigValue(type.Name + ".input", assetPath);

            serializedObject.Update();
        }

        internal static void OnInspectorGUI(SerializedObject serializedObject)
        {
            serializedObject.Update();

            //�p�����[�^�I�u�W�F�N�g���擾
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;

            //�ݒ�l�̓ǂݍ���
            SerializedProperty inputProperty = serializedObject.FindProperty("_inputParam");
            EditorGUILayout.PropertyField(inputProperty, new GUIContent("����", "HeightMapParam����͂��܂�"));
            if(inputProperty.objectReferenceValue != null)
            {
                GUI.enabled = false;

                //�ݒ�l�̏㏑��
                param = inputProperty.objectReferenceValue as HeightMapGeneratorParam;
            }

            param.seed = EditorGUILayout.IntField(new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"), param.seed);

            param.frequency = EditorGUILayout.FloatField(new GUIContent("���g��", "�g�p����m�C�Y�̎��g����ݒ肵�܂�"), param.frequency);
            MessageType type = MessageType.Info;
            if (param.frequency > 256)
            {
                type = MessageType.Warning;
            }
            EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂�", type);

            param.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("���`�X�P�[�����O", "���`�X�P�[�����O��L�������܂�"), param.isLinearScaling);

            if (!param.isLinearScaling)
            {
                param.amplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                    param.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
            }
            else
            {
                EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                    ref param.minLinearScale, ref param.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                bool guiEnableTemp = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), param.minLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), param.maxLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), param.maxLinearScale - param.minLinearScale);
                GUI.enabled = guiEnableTemp;
            }

            if (param.octaves > 0 && param.maxLinearScale == ATGMathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
            }

            param.octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�"), param.octaves);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("�ݒ�l���o�͂���", "�ݒ�l���A�Z�b�g�t�@�C���ɕۑ����܂�")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "parameters", "asset", "");
                if (!string.IsNullOrEmpty(savePath))
                {
                    //�l���R�s�[����
                    HeightMapGeneratorParam outputParam = UnityEngine.Object.Instantiate(param);

                    //�o�͂���
                    AssetDatabase.CreateAsset(outputParam, savePath);
                }
            }

            if (inputProperty.objectReferenceValue != null)
            {
                GUI.enabled = true;
            }
        }
    }
}
#endif
