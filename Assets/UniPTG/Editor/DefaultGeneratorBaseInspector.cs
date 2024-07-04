#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UniPTG.HeightmapGenerators;
using UniPTG.Parameters;

namespace UniPTG.Editors
{
    [CustomEditor(typeof(DefaultHeightmapGeneratorBase), true)]
    public class DefaultGeneratorBaseInspector : Editor
    {
        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            //�p�����[�^�I�u�W�F�N�g���擾
            HeightmapGenerationParameter param = (HeightmapGenerationParameter)serializedObject.FindProperty("_param").boxedValue;

            //�ݒ�l�̓ǂݍ���
            SerializedProperty inputProperty = serializedObject.FindProperty("_inputParam");
            EditorGUILayout.PropertyField(inputProperty, new GUIContent("����", "HeightMapParam����͂��܂�"));
            if (inputProperty.objectReferenceValue != null)
            {
                GUI.enabled = false;

                //�ݒ�l�̏㏑��
                param = (inputProperty.objectReferenceValue as HeightmapGenerationParamObject).parameter;
            }

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
                    param.amplitude, Mathf.MinTerrainHeight, Mathf.MaxTerrainHeight);
            }
            else
            {
                EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                    ref param.minLinearScale, ref param.maxLinearScale, Mathf.MinTerrainHeight, Mathf.MaxTerrainHeight);

                bool guiEnableTemp = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), param.minLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), param.maxLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), param.maxLinearScale - param.minLinearScale);
                GUI.enabled = guiEnableTemp;
            }

            if (param.octaves > 0 && param.maxLinearScale == Mathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
            }

            param.octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�"), param.octaves);

            if(inputProperty.objectReferenceValue == null)
            {
                //���͂��Ȃ��Ƃ��ۑ�����
                serializedObject.FindProperty("_param").boxedValue = param;
            }
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("�ݒ�l���o�͂���", "�ݒ�l���A�Z�b�g�t�@�C���ɕۑ����܂�")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "parameters", "asset", "");
                if (!string.IsNullOrEmpty(savePath))
                {
                    //�l���R�s�[����
                    HeightmapGenerationParamObject outputParam = CreateInstance<HeightmapGenerationParamObject>();
                    outputParam.parameter = param;

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
