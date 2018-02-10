using OMSSCADACommon;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.CommunicationAndControlling.SecondaryDataProcessing
{
    public class CommandValidator
    {
        public static bool ValidateDigitalCommand(Digital digital, CommandTypes command)
        {
            bool retVal = true;
            if (digital.ValidCommands.Contains(command))
            {
                switch (command)
                {
                    case CommandTypes.CLOSE:

                        //  command is CLOSE, last command was CLOSE, and state is CLOSED -> invalid...
                        if (digital.State == States.CLOSED && digital.Command == CommandTypes.CLOSE)
                            retVal = false;

                        // command is CLOSE, last command was CLOSE, but state is OPENED (incident) -> valid

                        break;

                    case CommandTypes.OPEN:
                        if (digital.State == States.OPENED && digital.Command == CommandTypes.OPEN)
                            retVal = false;


                        break;

                }
            }
            return retVal;
        }

        public static bool CheckCommandExecution()
        {
            throw new NotImplementedException();
        }

        // calculating a possibly command, in dependence of variable properties
        public static bool InitialCommandinfForVariable(Digital digital, out CommandTypes command)
        {
            // ovde nema neke velike logike za sada, ali je ideja da kasnije nekad bude ...prosirivo? xD

            command = CommandTypes.OPEN;

            // if previously states was OPENED, OPEN it again 
            if (digital.State == States.OPENED)
            {
                command = CommandTypes.OPEN;
                return true;
            }

            if (digital.State == States.CLOSED && digital.Command == CommandTypes.OPEN)
            {
                command = CommandTypes.OPEN;
                return true;
            }

            /*
            if (digital.State==States.OPENED && digital.Command == CommandTypes.CLOSE)
            {
                // ovde ce se u sledecem akv ciklusu promeniti stanje.
            }*/

            return false;
        }
    }
}
