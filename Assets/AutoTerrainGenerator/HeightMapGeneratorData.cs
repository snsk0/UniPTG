using UnityEngine;

internal class HeightMapGeneratorData : ScriptableObject
{
    //�m�C�Y�ϐ�
    public int noiseTypeIndex;
    public int seed;
    public float frequency;
    public bool isLinearScaling;
    public float amplitude;
    public float minLinearScale;
    public float maxLinearScale;
    public int octaves;

    //�n�C�g�}�b�v
    public int resolutionExp;

    //�e���C��
    public Vector3 scale;
}
