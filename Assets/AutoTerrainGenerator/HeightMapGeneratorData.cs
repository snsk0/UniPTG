using AutoTerrainGenerator;
using UnityEngine;

namespace AutoTerrainGenerator
{
    internal class HeightMapGeneratorData : ScriptableObject
    {
        //�m�C�Y�ϐ�
        public int noiseTypeIndex = 0;
        public GenerateType generateType = GenerateType.fBm;
        public int seed = 0;
        public float frequency = 0;
        public bool isLinearScaling = false;
        public float amplitude = ATGMathf.MaxTerrainHeight;
        public float minLinearScale = (ATGMathf.MinTerrainHeight + ATGMathf.MaxTerrainHeight) / 2;
        public float maxLinearScale = (ATGMathf.MinTerrainHeight + ATGMathf.MaxTerrainHeight) / 2;
        public int octaves = 0;

        //�n�C�g�}�b�v
        public int resolutionExp = (ATGMathf.MinResolutionExp + ATGMathf.MaxResolutionExp) / 2;

        //�e���C��
        public Vector3 scale = new Vector3(1000, 600, 1000);
    }
}
