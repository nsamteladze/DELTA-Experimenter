using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DELTA_Patcher.Data_Model;

namespace DELTA_Patcher.Data_Model
{
    [Serializable, XmlRoot("manifest")]
    public class PatchManifest
    {
        #region Properties

        [XmlElement("name")]
        public string PatchName { get; set; }
        [XmlElement("oldApk")]
        public ApkInfo OldApk { get; set; }
        [XmlElement("newApk")]
        public ApkInfo NewApk { get; set; }

        [XmlElement("filesNEW")]
        public SerializableHashSet<string> FilesNEW { get; set; }
        [XmlElement("filesDELETE")]
        public SerializableHashSet<string> FilesDELETE { get; set; }
        [XmlElement("filesUPDATED")]
        public SerializableHashSet<string> FilesUPDATED { get; set; }

        #endregion

        #region Constructors

        public PatchManifest()
        {
            PatchName = String.Empty;
            OldApk = new ApkInfo();
            NewApk = new ApkInfo();
            FilesNEW = new SerializableHashSet<string>();
            FilesDELETE = new SerializableHashSet<string>();
            FilesUPDATED = new SerializableHashSet<string>();
        }

        public PatchManifest(string patchName)
            : this()
        {
            PatchName = patchName;
        }

        public PatchManifest(string patchName, ApkInfo oldApk, ApkInfo newApk)
            : this(patchName)
        {
            OldApk = oldApk;
            NewApk = newApk;
        }

        public PatchManifest(string patchName, ApkInfo oldApk, ApkInfo newApk,
                             IEnumerable<string> filesNEW, IEnumerable<string> filesDELETE,
                             IEnumerable<string> filesUPDATED)
        {
            PatchName = patchName;
            OldApk = oldApk;
            NewApk = newApk;
            FilesNEW = new SerializableHashSet<string>(filesNEW);
            FilesDELETE = new SerializableHashSet<string>(filesDELETE);
            FilesUPDATED = new SerializableHashSet<string>(filesUPDATED);
        }

        #endregion
    }
}
