using System;
using DELTA_Common.DataModel;

namespace DELTA_Common.Utilities
{
    public static class NamingHelper
    {
        public const string NAME_PATCH_MANIFEST = "PatchManifest.xml";

        public static string GetPatchName(ApplicationPackage reference, ApplicationPackage target)
        {
            return (String.Format("{0}-{1}-{2}.deltapatch", reference.PackageName, reference.CodeVersion, target.CodeVersion));
        }

        // TODO: Document
        public static string GetPackageNameInStorage(ApplicationPackage appPackage)
        {
            if (appPackage.GetType() == typeof(AndroidApplicationPackage))
            {
                return String.Format("{0}-{1}.apk", appPackage.PackageName, appPackage.CodeVersion);
            }
            else
            {
                return String.Format("{0}-{1}", appPackage.PackageName, appPackage.CodeVersion);
            }
        }
    }
}
