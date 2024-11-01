using Unity.Mathematics;
using UnityEngine;

namespace UniPTG.NoiseGenerators
{
    internal class UnitySimplexNoise : NoiseGeneratorBase
    {
        private const float NoiseFrequency = 256f;

        [SerializeField]
        private int _seed;

        private Vector2 _offset;

        public override void InitState()
        {
            UnityEngine.Random.InitState(_seed);

            UpdateState();
        }

        public override float GetValue(float x, float y)
        {
            return noise.snoise(new float2(x + _offset.x, y + _offset.y));
        }

        public override void UpdateState()
        {
            float xSeed = UnityEngine.Random.Range(0f, NoiseFrequency);
            float ySeed = UnityEngine.Random.Range(0f, NoiseFrequency);

            //シード値を保存
            _offset = new Vector2(xSeed, ySeed);
        }
    }
}
