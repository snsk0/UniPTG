using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniPTG.Parameters;

namespace UniPTG.HeightmapGenerators
{
    internal abstract class DefaultHeightmapGeneratorBase : HeightmapGeneratorBase
    {
        [SerializeField]
        private protected HeightmapGenerationParam _param;

        [SerializeField]
        private protected HeightmapGenerationParam _inputParam;

        private void OnEnable()
        {
            //�C���X�^���X��
            _param = CreateInstance<HeightmapGenerationParam>();

            //��p�t�H���_���擾
            string path = Application.dataPath.Replace("Assets", "UserSettings/") + "UniPTG";

            //�Ȃ��ꍇ���
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //�t�@�C������ǉ�
            path += "/" + GetType().FullName + ".json";


            //�t�@�C�����Ȃ��Ȃ�쐬����
            if (!File.Exists(path))
            {
                Save(path);
            }

            //json���擾����
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();

            //json������ꍇ�㏑������
            if (!string.IsNullOrEmpty(json))
            {
                JsonUtility.FromJsonOverwrite(json, _param);
            }

            //�i��������
            _param.hideFlags = HideFlags.DontSave;
        }

        private void OnDisable()
        {
            //�Z�[�u����
            Save(Application.dataPath.Replace("Assets", "UserSettings/") + "UniPTG/" + GetType().FullName + ".json");
        }

        private void Save(string path)
        {
            //Json�ɕϊ����ď�������
            string json = JsonUtility.ToJson(_param);
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(json);
            writer.Close();
        }

        public override void Generate(float[,] heightmap, int size, INoiseReader noiseReader)
        {
            //���͒l������ꍇ�͂�������g�p����
            HeightmapGenerationParam param = _param;
            if (_inputParam != null)
            {
                param = _inputParam;
            }

            float frequency = param.frequency;
            float amplitude = param.amplitude;

            if (param.isLinearScaling)
            {
                amplitude = Mathf.MaxTerrainHeight;
            }

            for (int i = 0; i <= param.octaves; i++)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        float xvalue = (float)x / size * frequency;
                        float yvalue = (float)y / size * frequency;
                        heightmap[x, y] += CalculateHeight(amplitude, noiseReader.GetValue(xvalue, yvalue));
                    }
                }

                frequency *= Mathf.FBmFrequencyRate;
                amplitude *= Mathf.FBmPersistence;
            }

            //�X�P�[�����O
            if (param.isLinearScaling)
            {
                IEnumerable<float> heightEnum = heightmap.Cast<float>();
                float minHeight = heightEnum.Min();
                float maxHeight = heightEnum.Max();

                for (int x = 0; x < heightmap.GetLength(0); x++)
                {
                    for (int y = 0; y < heightmap.GetLength(1); y++)
                    {
                        heightmap[x, y] = Mathf.LinearScaling(heightmap[x, y], minHeight, maxHeight, param.minLinearScale, param.maxLinearScale);
                    }
                }
            }
        }

        private protected abstract float CalculateHeight(float currentAmplitude, float value);
    }
}
