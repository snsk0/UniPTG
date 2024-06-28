using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;
using UnityEngine;
using UnityEditor;

namespace AutoTerrainGenerator.Editors
{
    [ATGCustomEditor(typeof(GeneratorByUnityPerlin))]
    public class UnityPerlinInspector : GeneratorEditor
    {
        private HeightMapGeneratorParam _generatorData = new HeightMapGeneratorParam();

        public override void OnInspectorGUI()
        {
            _generatorData.seed = EditorGUILayout.IntField(new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"), _generatorData.seed);

            _generatorData.frequency = EditorGUILayout.FloatField(new GUIContent("���g��", "�g�p����m�C�Y�̎��g����ݒ肵�܂�"), _generatorData.frequency);
            MessageType type = MessageType.Info;
            if(_generatorData.frequency > 256)
            {
                type = MessageType.Warning;
            }
            EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂�", type);

            _generatorData.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("���`�X�P�[�����O", "���`�X�P�[�����O��L�������܂�"), 
                _generatorData.isLinearScaling);

            if (!_generatorData.isLinearScaling)
            {
                _generatorData.amplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                    _generatorData.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
            }
            else
            {
                EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                    ref _generatorData.minLinearScale, ref _generatorData.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), _generatorData.minLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), _generatorData.maxLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), _generatorData.maxLinearScale - _generatorData.minLinearScale);
            }

            if (_generatorData.octaves > 0 && _generatorData.maxLinearScale == ATGMathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
            }

            _generatorData.octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�"), _generatorData.octaves);
        }
    }
}
