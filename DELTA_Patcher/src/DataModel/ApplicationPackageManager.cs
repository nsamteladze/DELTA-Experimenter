using DELTA_Common.DataModel;

namespace DELTA_Patcher.DataModel
{
    public abstract class ApplicationPackageManager
    {
        public bool ValidatePackage(ApplicationPackage appPackage)
        {
            return true;
        }
    }
}
