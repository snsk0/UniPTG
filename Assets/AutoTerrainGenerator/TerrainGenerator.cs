using UnityEngine;

namespace AutoTerrainGenerator
{
    public static class TerrainGenerator
    {
        public static TerrainData Generate(Vector3 scale, int resolutionEx, float noiseScale, int seed, int octaves, float persistance)
        {
            int resolution = (int)Mathf.Pow(2, resolutionEx) + 1;

            Random.InitState(seed);
            float xInitial = Random.Range(0f, 256f);
            float yInitial = Random.Range(0f, 256f);

            TerrainData terrainData = new TerrainData();

            //�𑜓x���ŗD��Őݒ肷��
            terrainData.heightmapResolution = resolution;

            terrainData.size = scale;

            float[,] heightMap = new float[resolution, resolution];

            for(int x = 0; x < resolution; x++)
            {
                for(int y = 0; y < resolution; y++)
                {
                    heightMap[x, y] = Mathf.PerlinNoise(((float)x / resolution) * noiseScale + xInitial, ((float)y / resolution) * noiseScale + yInitial);
                }
            }

            //fBM�𗘗p���ďd�˂�
            if(octaves > 0)
            {
                //�ŏ��̃m�C�Y�̐U����persistance������������
                for (int x = 0; x < resolution; x++)
                {
                    for (int y = 0; y < resolution; y++)
                    {
                        heightMap[x, y] = heightMap[x, y] * persistance;
                    }
                }

                //�I�N�^�[�u�̐������m�C�Y���d�˂�
                for (int i = 0; i < octaves; i++)
                {
                    float octaveNoiseScale = noiseScale * Mathf.Pow(2.0f, i);
                    float heightScale = Mathf.Pow(persistance, i + 1);

                    float octaveXInitial = Random.Range(0f, 256f);
                    float octaveYInitial = Random.Range(0f, 256f);

                    for (int x = 0; x < resolution; x++)
                    {
                        for (int y = 0; y < resolution; y++)
                        {
                            float height = Mathf.PerlinNoise(((float)x / resolution) * octaveNoiseScale + octaveXInitial, 
                                ((float)y / resolution) * octaveNoiseScale + octaveYInitial);

                            heightMap[x, y] += height * heightScale;
                        }
                    }
                }
            }

            terrainData.SetHeights(0, 0, heightMap);

            Terrain.CreateTerrainGameObject(terrainData);

            return terrainData;
        }
    }
}