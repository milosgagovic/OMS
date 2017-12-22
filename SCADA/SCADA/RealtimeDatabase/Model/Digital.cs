using OMSSCADACommon;
using SCADA.RealtimeDatabase.Catalogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public class Digital : ProcessVariable
    {
        public DigitalDeviceClasses Class { get; set; }
        public List<CommandTypes> ValidCommands { get; set; }
        public List<Catalogs.States> ValidStates { get; set; }
        public CommandTypes Command { get; set; }
        public Catalogs.States State { get; set; }

        public Digital()
        {
            this.Type = VariableTypes.DIGITAL;
            ValidCommands = new List<CommandTypes>();
            ValidStates = new List<Catalogs.States>();
        }

        // here should be mapping between Catalogs.States and bit values
        public void SetState(BitArray bits)
        {
            int bitQuantity = (int)Math.Floor((Math.Log(ValidStates.Count, 2)));
            switch (Class)
            {
                case DigitalDeviceClasses.SWITCH:
                    var oldState = State;
                    // 0 otvoren, 1 zatvoren
                    var isClosed = bits.Get(0);
                    if (isClosed)
                    {
                        State = Catalogs.States.CLOSED;
                    }
                    else
                    {
                        State = Catalogs.States.OPENED;
                    }
                    Console.WriteLine("{0}:  State:  {1} ->  {2}", Name, oldState, State);
                    break;
                default:
                    break;
            }
        }

    }
}
