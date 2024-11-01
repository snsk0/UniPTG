using UnityEngine;

namespace UniPTG.NoiseGenerators
{
    internal class UnityPerlinNoise : NoiseGeneratorBase
    {
        private const float NoiseFrequency = 256f;

        [SerializeField]
        private bool _isTimeMode;

        [SerializeField]
        private int _seed;

        private Vector2 _offset;

        public override void InitState()
        {
            if (_isTimeMode)
            {
                Random.InitState((int)(Time.time * 100f));
            }
            else
            {
                Random.InitState(_seed);
            }

            UpdateState();
        }

        public override float GetValue(float x, float y)
        {
            return UnityEngine.Mathf.PerlinNoise(x + _offset.x, y + _offset.y);
        }

        public override void UpdateState()
        {
            float xSeed = Random.Range(0f, NoiseFrequency);
            float ySeed = Random.Range(0f, NoiseFrequency);

            //シード値を保存
            _offset = new Vector2(xSeed, ySeed);
        }
    }
}
