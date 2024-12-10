using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MarchingCubeCS : MonoBehaviour
{
    [SerializeField]
    ComputeShader marchingCube;

    [SerializeField]
    int size;

    [SerializeField]
    float isolevel;

    // Start is called before the first frame update
    void Start()
    {
        ComputeBuffer points = new ComputeBuffer(size * size * size, sizeof(float) * 4);
        ComputeBuffer triangles = new ComputeBuffer(size * size * size * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCounter = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        Vector4[] grid = new Vector4[size * size * size];
        int centerX = 8;
        int centerY = 8;
        int centerZ = 8;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    // ’†S‚©‚ç‚Ì‹——£‚ðŒvŽZ
                    int dx = i - centerX;
                    int dy = j - centerY;
                    int dz = k - centerZ;

                    if (dx * dx + dy * dy + dz * dz > 30)
                    {
                        grid[i + j * size + k * size * size] = new Vector4(dx, dy, dz, 1);
                    }
                    else
                    {
                        grid[i + j * size + k * size * size] = new Vector4(dx, dy, dz, 0);
                    }
                }
            }
        }
        points.SetData(grid);

        marchingCube.SetBuffer(0, "points", points);
        marchingCube.SetBuffer(0, "vertices", triangles);
        marchingCube.SetFloat("isolevel", isolevel);
        marchingCube.SetInt("size", size);

        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        marchingCube.Dispatch(0, Mathf.CeilToInt((size - 1)/ 8.0f), Mathf.CeilToInt((size - 1) / 8.0f), Mathf.CeilToInt((size - 1) / 8.0f));

        ComputeBuffer.CopyCount(triangles, triCounter, 0);
        int[] triCountArray = new int[1];
        triCounter.GetData(triCountArray);
        int triCount = triCountArray[0];

        Triangle[] t = new Triangle[triCount];
        triangles.GetData(t, 0, 0, triCount);
        stopWatch.Stop();
        Debug.Log("GenerateTime: " + stopWatch.ElapsedMilliseconds);

        Mesh mesh = new Mesh();

        var vertices = new Vector3[triCount * 3];
        var meshTriangles = new int[triCount * 3];

        for (int i = 0; i < triCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                switch (j)
                {
                    case 0:
                        vertices[i * 3 + j] = t[i].vertex0;
                        break;
                    case 1:
                        vertices[i * 3 + j] = t[i].vertex1;
                        break;
                    case 2:
                        vertices[i * 3 + j] = t[i].vertex2;
                        break;
                }
                meshTriangles[i * 3 + j] = i * 3 + j;
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private struct Triangle
    {
        public Vector3 vertex0;
        public Vector3 vertex1;
        public Vector3 vertex2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
