using System;
using Command_Line_Parser;
using DELTA_Patcher.Utils;
using DELTA_Patcher.Data_Model;

namespace DELTA_Patcher.Command_Line
{
    public class ApplicationCommandLine : CommandLine
    {
        public ApplicationCommandLine(string pathCommandLineConfigFile)
            : base(pathCommandLineConfigFile) { }

        public override void RunCommand(CalledCommand calledCommand)
        {
            switch (calledCommand.Name)
            {
                case "patch_2":
                    {
                        Patcher.PatchTwoAPKFiles(calledCommand.MandatoryParametersValues[0],
                                                 calledCommand.MandatoryParametersValues[1],
                                                 calledCommand.MandatoryParametersValues[2],
                                                 new ApkInfo(), new ApkInfo());
                        break;
                    }
                case "patch":
                    {
                        Patcher.PatchApplicationInStorage(calledCommand.MandatoryParametersValues[0]);
                        break;
                    }
                case "patch_all":
                    {
                        Patcher.PatchAllApplicationsInStorage();
                        break;
                    }
                case "patch_finish":
                    {
                        Patcher.FinishPatchingApplicationsInStorage();
                        break;
                    }
                case "test":
                    {
                        Experimenter.StartExperiment("DELTA.experiment");
                        break;
                    }
                default:
                    {
                        FailCommand("Illegal command.");
                        break;
                    }
            }
        }
    }
}
