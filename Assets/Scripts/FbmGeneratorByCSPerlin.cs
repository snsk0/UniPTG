using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniPTG;

public class FbmGeneratorByCSPerlin : HeightmapGeneratorBase
{
    [SerializeField]
    private ComputeShader _perlinNoise;

    [SerializeField]
    private float _frequency;

    [SerializeField]
    private float _z;

    public override void Generate(float[,] heightmap, int size, INoiseReader noiseReader)
    {
        ComputeBuffer buffer = new ComputeBuffer(size * size, sizeof(float));

        _perlinNoise.SetBuffer(0, "heights", buffer);
        _perlinNoise.SetFloat("z", _z);
        _perlinNoise.SetInt("size", size);
        _perlinNoise.SetFloat("frequency", _frequency);

        _perlinNoise.Dispatch(0, Mathf.CeilToInt(size / 16.0f), Mathf.CeilToInt(size / 16.0f), 1);

        float[] heights = new float[size * size];
        buffer.GetData(heights, 0, 0, size * size);

        for(int x = 0; x < size; x++)
        {
            for(int y = 0; y < size; y++)
            {
                heightmap[x,y] = heights[x + y * size];
                //Debug.Log(heightmap[x, y]);
            }
        }
    }
}
