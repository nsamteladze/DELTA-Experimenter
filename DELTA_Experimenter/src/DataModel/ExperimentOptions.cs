using System;
using System.Collections.Generic;
using DELTA_Common.DataModel;
using DELTA_Patcher.DataModel;
using DELTA_Common.Utilities;

namespace DELTA_Experimenter.DataModel
{
    public class ExperimentOptions : ISelfConstructingObject
    {
        #region Internal Structures

        private Dictionary<string, EnumPatchingAlgorithms> _dictPatchingAlgorithmNames 
            =   new Dictionary<string, EnumPatchingAlgorithms> 
                {
                    { "DELTA", EnumPatchingAlgorithms.DELTA },
                    { "DELTA++", EnumPatchingAlgorithms.DELTA_PLUS_PLUS }
                };

        #endregion

        #region Private Fields

        private List<ApplicationInExperiment> _listAppsInExperiment;

        #endregion

        #region Properties

        public EnumPatchingAlgorithms PatchingAlgorithm { get; set; }

        public string OutputDir { get; set; }

        public string StatDir { get; set; }

        public List<ApplicationInExperiment> AppsInExperiment
        {
            get 
            {
                return _listAppsInExperiment; 
            }
            private set
            {

            }
        }

        #endregion

        #region Constructors

        public ExperimentOptions()
        {
            _listAppsInExperiment = null;
            PatchingAlgorithm = EnumPatchingAlgorithms.None; ;
            OutputDir = null;
            StatDir = null;
        }

        #endregion

        public bool ConstructObject(List<List<string>> data)
        {
            if (data.Count < 4) return false;

            EnumPatchingAlgorithms patchingAlgorithmName;
            string outputDir;
            string statDir;
            List<ApplicationInExperiment> appsInExperiment;

            if (!TryGetPatchingAlgorithmName(data[0], out patchingAlgorithmName)) return false;
            if (!TryGetOutputDir(data[1], out outputDir)) return false;
            if (!TryGetStatDir(data[2], out statDir)) return false;
            if (!TryGetApplicationsInExperiment(data, out appsInExperiment)) return false;

            this.PatchingAlgorithm = patchingAlgorithmName;
            this.OutputDir = outputDir;
            this.StatDir = statDir;
            _listAppsInExperiment = appsInExperiment;

            return true;
        }

        #region Data Parsing Methods

        private bool TryGetPatchingAlgorithmName(List<string> data, out EnumPatchingAlgorithms patchingAlgorithmName)
        {
            patchingAlgorithmName = EnumPatchingAlgorithms.None;

            if (data.Count > 1) return false;
            if (!_dictPatchingAlgorithmNames.TryGetValue(data[0], out patchingAlgorithmName)) return false;

            return true;
        }

        private bool TryGetOutputDir(List<string> data, out string outputDir)
        {
            outputDir = null;

            if (data.Count > 1) return false;
            if (!FileManager.IsValidPath(data[0])) return false;

            outputDir = data[0];

            return true;
        }

        private bool TryGetStatDir(List<string> data, out string statDir)
        {
            statDir = null;

            if (data.Count > 1) return false;
            if (!FileManager.IsValidPath(data[0])) return false;

            statDir = data[0];

            return true;
        }

        private bool TryGetApplicationsInExperiment(List<List<string>> data, out List<ApplicationInExperiment> appsInExperiment)
        {
            appsInExperiment = null;

            if (data.Count < 4) return false;

            var tempAppsInExperiment = new List<ApplicationInExperiment>();

            for (int i = 3; i < data.Count; ++i)
            {
                if (data[i].Count > 2) return false;

                int tempNumLatestVersionsToUse;

                if (!Int32.TryParse(data[i][1], out tempNumLatestVersionsToUse)) return false;

                tempAppsInExperiment.Add(new ApplicationInExperiment(data[i][0], tempNumLatestVersionsToUse)); 
            }

            appsInExperiment = tempAppsInExperiment;

            return true;
        }

        #endregion
    }
}
