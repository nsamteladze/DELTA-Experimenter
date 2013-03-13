using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DELTA_Common.DataModel;

namespace DELTA_Patcher.DataModel
{
    [Serializable, XmlRoot("patchManifest")]
    public class PatchManifest
    {
        #region Properties

        [XmlElement("patchName")]
        public string PatchName { get; set; }
        [XmlElement("referencePackage")]
        public ApplicationPackage ReferencePackage { get; set; }
        [XmlElement("targetPackage")]
        public ApplicationPackage TargetPackage { get; set; }

        [XmlElement("filesNEW")]
        public SerializableHashSet<string> FilesNEW
        {
            get;
            private set;
        }
        [XmlElement("filesDELETE")]
        public SerializableHashSet<string> FilesDELETE
        {
            get;
            private set;
        }
        [XmlElement("filesUPDATED")]
        public SerializableHashSet<string> FilesUPDATED
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        //public PatchManifest()
        //{
        //    PatchName = String.Empty;
        //    OldApk = target packageInfo();
        //    NewApk = target packageInfo();
        //    FilesNEW = new SerializableHashSet<string>();
        //    FilesDELETE = new SerializableHashSet<string>();
        //    FilesUPDATED = new SerializableHashSet<string>();
        //}

        //public PatchManifest(string patchName)
        //    : this()
        //{
        //    PatchName = patchName;
        //}

        public PatchManifest(string patchName, ApplicationPackage referencePackage, ApplicationPackage targetePackage)
        {
            this.PatchName = patchName;
            this.ReferencePackage = referencePackage;
            this.TargetPackage = targetePackage;
        }

        public PatchManifest(string patchName, ApplicationPackage referencePackage, ApplicationPackage targetePackage,
                             IEnumerable<string> filesNEW, IEnumerable<string> filesDELETE,
                             IEnumerable<string> filesUPDATED)
        {
            this.PatchName = patchName;
            this.ReferencePackage = referencePackage;
            this.TargetPackage = targetePackage;
            FilesNEW = new SerializableHashSet<string>(filesNEW);
            FilesDELETE = new SerializableHashSet<string>(filesDELETE);
            FilesUPDATED = new SerializableHashSet<string>(filesUPDATED);
        }

        #endregion

        #region Add Marked File Methods

        public void AddFileNEW(string file)
        {
            this.FilesNEW.Add(file);
        }

        public void AddFileUPDATED(string file)
        {
            this.FilesUPDATED.Add(file);
        }

        public void AddFileDELETE(string file)
        {
            this.FilesDELETE.Add(file);
        }

        #endregion
    }
}
