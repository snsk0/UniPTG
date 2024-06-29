using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator
{
    internal class ATGSettingsData : ScriptableObject, IATGSettingProvider
    {
        public const string HeightMapGeneratorsName = "_heightMapGenerators";

        [SerializeField]
        private List<MonoScript> _heightMapGenerators;
        public List<MonoScript> heightMapGenerators => _heightMapGenerators;

        internal static ATGSettingsData GetOrCreateData()
        {
            //�p�X����ǂݍ���
            ATGSettingsData settingData = AssetDatabase.LoadAssetAtPath<ATGSettingsData>(IATGSettingProvider.SettingsPath);

            if (settingData == null)
            {
                settingData = CreateInstance<ATGSettingsData>();
                settingData._heightMapGenerators = new List<MonoScript>();
                AssetDatabase.CreateAsset(settingData, IATGSettingProvider.SettingsPath);
            }

            return settingData;
        }

        public List<HeightMapGeneratorBase> GetGenerators()
        {
            List<HeightMapGeneratorBase> heightMapGenerators = new List<HeightMapGeneratorBase>();

            foreach(MonoScript generatorScript in _heightMapGenerators)
            {
                if(generatorScript != null)
                {
                    //Generator�f�[�^�̓ǂݍ���
                    HeightMapGeneratorBase generator = CreateInstance(generatorScript.GetClass()) as HeightMapGeneratorBase;
                    heightMapGenerators.Add(generator);
                }
            }
            return heightMapGenerators;
        }
    }
}