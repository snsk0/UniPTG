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

    Vector4[] voxcels;
    // Start is called before the first frame update
    void Start()
    {
        ComputeBuffer points = new ComputeBuffer(size * size * size, sizeof(float) * 4);
        ComputeBuffer triangles = new ComputeBuffer(size * size * size * 5, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triCounter = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        Vector4[] grid = new Vector4[size * size * size];

        // ボクセルの間隔（解像度）を指定する変数
        float resolution = 0.25f;

        // 中心座標を計算（float型で正確に設定）
        float centerX = (size - 1) / 2.0f * resolution;
        float centerY = (size - 1) / 2.0f * resolution;
        float centerZ = (size - 1) / 2.0f * resolution;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    // ボクセル位置をfloatで計算（boundsの影響を適用）
                    float x = i * resolution;
                    float y = j * resolution;
                    float z = k * resolution;

                    // 中心からの距離を計算
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float dz = z - centerZ;

                    // 球体の条件を適用
                    if (dx * dx + dy * dy + dz * dz <= 64f)
                    {
                        grid[i + j * size + k * size * size] = new Vector4(x, y, z, 1.0f);
                    }
                    else
                    {
                        grid[i + j * size + k * size * size] = new Vector4(x, y, z, 0);
                    }
                }
            }
        }
        //voxcels = grid;

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
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
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

        Debug.Log("meshlength;" + mesh.vertices.Length);
        mesh.Optimize();
        Debug.Log("meshAft;" + mesh.vertices.Length);
        mesh.RecalculateNormals();
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

    private void OnDrawGizmosSelected ()
    {
        if(voxcels != null)
        {
            foreach(Vector4 v in voxcels)
            {
                Gizmos.color = new Color(v.w, v.w, v.w, 1.0f);
                Vector3 position = new Vector3(v.x, v.y, v.z);
                Gizmos.DrawSphere(transform.position + position, 0.05f);
            }
        }
    }
}
