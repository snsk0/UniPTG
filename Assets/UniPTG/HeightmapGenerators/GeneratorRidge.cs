namespace UniPTG.HeightmapGenerators
{
    internal class GeneratorRidge : DefaultHeightmapGeneratorBase
    {
        private protected override float CalculateHeight(float currentAmplitude, float value)
        {
            //�����t���̒l�ɕϊ�����
            value = (value - 0.5f) * 2.0f;

            //��Βl���擾
            value = UnityEngine.Mathf.Abs(value) * currentAmplitude;

            //off���画�肵�Ēl��Ԃ�
            value = Mathf.RidgeOffset - value;
            value *= value;
            return value;
        }
    }
}
