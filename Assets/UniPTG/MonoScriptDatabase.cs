using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniPTG
{
    [FilePath("ProjectSettings/UniPTGSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class MonoScriptDatabase : ScriptableSingleton<MonoScriptDatabase>
    {
        [SerializeField]
        private List<MonoScript> _noiseGenerators;

        [SerializeField]
        private List<MonoScript> _heightmapGenerators;

        internal IReadOnlyList<Type> GetNoiseGeneratorTypes()
        {
            return _noiseGenerators?.Where((script) => script != null).Select((script) => script.GetClass()).ToList().AsReadOnly();
        }

        internal IReadOnlyList<MonoScript> GetNoiseGeneratorScripts()
        {
            return _noiseGenerators?.Where((script) => script != null).ToList().AsReadOnly();
        }

        internal IReadOnlyList<Type> GetHeightmapGeneratorTypes()
        {
            return _heightmapGenerators?.Where((script) => script != null).Select((script) => script.GetClass()).ToList().AsReadOnly();
        }

        internal IReadOnlyList<MonoScript> GetHeightmapGeneratorScripts()
        {
            return _heightmapGenerators?.Where((script) => script != null).ToList().AsReadOnly();
        }

        internal void Update()
        {
            //TODO�L���b�V�����X�V����

            //�i��������
            Save(true);
        }
    }
}

