using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Data_Model
{
    public class ApplicationVersionsPair
    {
        #region Properties

        public string PathOldApk { get; set; }
        public string PathNewApk { get; set; }
        public ApkInfo OldApkInfo { get; set; }
        public ApkInfo NewApkInfo { get; set; }

        #endregion

        #region Constructors

        public ApplicationVersionsPair(string pathOldApk, string pathNewApk,
                                       ApkInfo oldApkInfo, ApkInfo newApkInfo)
        {
            PathOldApk = pathOldApk;
            PathNewApk = pathNewApk;
            OldApkInfo = oldApkInfo;
            NewApkInfo = newApkInfo;
        }

        #endregion 
    }
}
