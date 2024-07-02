namespace UniPTG.HeightmapGenerators
{
    internal class RidgeAndLoad : GeneratorRidge
    {
        private protected override float CalculateHeight(float currentAmplitude, float value)
        {
            value = base.CalculateHeight(currentAmplitude, value);

            //���l�ȉ��̏ꍇ���k���s��
            float threshold = 0.25f;

            if (value < threshold)
            {
                value = Mathf.LinearScaling(value, 0, 1, 0.25f, 0.3f);
            }
            return value;
        }
    }
}