using UnityEngine;

namespace UniPTG
{
    public abstract class NoiseGeneratorBase : ScriptableObject, INoiseReader
    {
        public abstract void InitState();
        public abstract float GetValue(float x, float y);

        public abstract void UpdateState();
    }
}
