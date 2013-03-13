using Command_Line_Parser;

namespace DELTA_Experimenter.CommandLine
{
    public class ApplicationCommandLine : Command_Line_Parser.CommandLine
    {
        public ApplicationCommandLine(string pathCommandLineConfigFile)
            : base(pathCommandLineConfigFile) { }

        public override void RunCommand(CalledCommand calledCommand)
        {
            switch (calledCommand.Name)
            {
                case "experiment":
                    {
                        Experimenter.StartExperiment(calledCommand.MandatoryParametersValues[0]);
                        break;
                    }
                // Used to test functionallity. Only for debugging.
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
