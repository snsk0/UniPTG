using UnityEngine;

namespace AutoTerrainGenerator.HeightMapGenerators {
    internal class GeneratorByUnityPerlin : IHeightMapGenerator
    {
        private const float PerlinNoiseFrequency = 256f;

        public float[,] Generate(int seed, int resolutionExp, float frequency, float minAmplitude, float maxAmplitude, int octaves)
        {
            Random.InitState(seed);
            float xSeed = Random.Range(0f, PerlinNoiseFrequency);
            float ySeed = Random.Range(0f, PerlinNoiseFrequency);

            int resolution = ATGMathf.GetResolution(resolutionExp);
            float amplitude = maxAmplitude;

            float[,] heightMap = new float[resolution, resolution];

            for (int i = 0; i <= octaves; i++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    for (int y = 0; y < resolution; y++)
                    {
                        var xvalue = (float)x / resolution * frequency + xSeed;
                        var yvalue = (float)y / resolution * frequency + ySeed;
                        heightMap[x, y] += Mathf.PerlinNoise(xvalue, yvalue) * amplitude;
                    }
                }

                frequency *= ATGMathf.FBmFrequencyRate;
                amplitude *= ATGMathf.FBmPersistence;
            }

            //���̂܂܂���0-1����ɂ��邪�A���ۂ̃e���C����1���I�[�o�[��0�����Œ�l���Ⴂ���ߎ擾����K�v������
            //���������x�ȐU���ݒ��K�v�Ƃ��Ȃ��ꍇ�����0-1�ɏ���ɃX�P�[�����O���Ă��܂����߁Aif���ł̕��򂪕K�{���Ǝv����(���ꂩ�֐��𕪂���)
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    heightMap[x, y] = ATGMathf.Scaling(heightMap[x,y], 0, maxAmplitude, minAmplitude, maxAmplitude);
                }
            }

            return heightMap;
        }
    }
}