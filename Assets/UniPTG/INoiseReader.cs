namespace UniPTG
{
    public interface INoiseReader
    {
        /// <summary>
        /// �w�肳�ꂽ���W�̒l��Ԃ��܂�
        /// 0�`1.0f�͈̔͂Œl��Ԃ��Ă�������
        /// </summary>
        public abstract float GetValue(float x, float y);

        /// <summary>
        /// �m�C�Y�̏�Ԃ��X�V���܂�
        /// </summary>
        public abstract void UpdateState();
    }
}