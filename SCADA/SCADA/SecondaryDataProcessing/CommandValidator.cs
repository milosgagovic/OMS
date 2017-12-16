using OMSSCADACommon;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.SecondaryDataProcessing
{
    public class CommandValidator
    {
        public static bool ValidateDigitalCommand(Digital digital, CommandTypes command)
        {
            foreach(CommandTypes comm in digital.ValidCommands)
            {
                if(comm == command && digital.Command != command)
                {
                    digital.Command = command;
                    return true;
                }
            }

            return false;
        }

        public static bool CheckCommandExecution()
        {
            throw new NotImplementedException();
        }
    }
}
