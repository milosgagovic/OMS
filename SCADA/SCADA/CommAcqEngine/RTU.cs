using PCCommon;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase
{
    public class RTU
    {
        // dovoljno je da procesna varijabla zna
        // samo svoju relativnu adresu, a mapiranje se vrsi na 
        // nivou rtu-a
        public Dictionary<ushort, ProcessVariable> ProcessVariables = null;

        public IndustryProtocols Protocol { get; set; }

        // (Modbus slave address) 
        // this will be value in range 1 - 247 (0 - broadcast)
        public Byte Address { get; set; }

        public string Name { get; set; }

        // adrese na kojima pocinju registri kontrolera
        // registers starting Addresses
        public int DigOutStartAddr { get; set; }
        public int DigInStartAddr { get; set; }
        public int AnaInStartAddr { get; set; }
        public int AnaOutStartAddr { get; set; }
        public int CounterStartAddr { get; set; }

        // number of registers
        public int DigOutCount { get; set; }
        public int DigInCount { get; set; }
        public int AnaInCount { get; set; }
        public int AnaOutCount { get; set; }
        public int CounterCount { get; set; }

        //// arrays of mapped adresses
        //// reading Addresses
        //private int[] digitalInAddresses;
        //private int[] analogInAddresses;

        //// commanding Addresses
        //private int[] digitalOutAddresses;
        //private int[] analogOutAddresses;

        //private int[] counterAddresses;

        // arrays of mapped adresses
        // reading Addresses
        private List<int> digitalInAddresses;
        private List<int> analogInAddresses;

        // commanding Addresses
        private List<int> digitalOutAddresses;
        private List<int> analogOutAddresses;

        private List<int> counterAddresses;

        public RTU()
        {
            // mozda da ovde vallue bude process variable name... da se samou bazi cuvaju pv, ne u rtu..
            this.ProcessVariables = new Dictionary<ushort, ProcessVariable>();

            digitalInAddresses = new List<int>(DigInCount);
            analogInAddresses = new List<int>(AnaInCount);
            digitalOutAddresses = new List<int>(DigOutCount);
            analogOutAddresses = new List<int>(AnaOutCount);
            counterAddresses = new List<int>(CounterCount);
        }

        public void Configure(string configPath)
        {
            // read from cofigPath.xml

            //digitalInAddresses = new int[DigInCount];
            //analogInAddresses = new int[AnaInCount];
            //digitalOutAddresses = new int[DigOutCount];
            //analogOutAddresses = new int[AnaOutCount];
            //counterAddresses = new int[CounterCount];

            //setting starting addresses
            digitalInAddresses[0] = DigInStartAddr;
            analogInAddresses[0] = AnaInStartAddr;
            digitalOutAddresses[0] = DigOutStartAddr;
            analogOutAddresses[0] = AnaOutStartAddr;
            counterAddresses[0] = CounterStartAddr;
        }

        // get reading address
        public int GetAcqAddress(ProcessVariable variable)
        {
            int retAddress = -1;
            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:
                    Digital digital = variable as Digital;
                    retAddress = digitalInAddresses[digital.RelativeAddress];
                    break;
                case VariableTypes.ANALOGIN:
                    break;
                default:
                    break;
            }

            return retAddress;
        }

        public int GetCommandAddress(ProcessVariable variable)
        {
            int retAddress = -1;
            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:
                    Digital digital = variable as Digital;
                    retAddress = digitalOutAddresses[digital.RelativeAddress];
                    break;
                case VariableTypes.ANALOGIN:
                    break;
                default:
                    break;
            }

            return retAddress;
        }

        // mapiranje - za svaku varijablu racunanje adrese 
        // u memoriji odgovarajuceg kontrolera
        // mapiramo na InAdresses - adrese za citanje (akvizicija je u pitanju)
        // mapiranje se vrsi na osnovu relativne adrese promenljive
        // i broja registara koje ona zauzima. relativna adresa je pozicija
        // promenljive u nizu promenljivih istog tipa
        public int MapToAcqAddress(ProcessVariable variable)
        {
            int retAddr = -1;
            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:

                    Digital digital = variable as Digital;

                    // ovo je prva promenljiva tipa digital, i ona pocinje na
                    // startnom registru tog tipa
                    if (digital.RelativeAddress == 0)
                        //digitalInAddresses[digital.RelativeAddress] = DigInStartAddr;
                        digitalInAddresses.Insert(digital.RelativeAddress, DigInStartAddr);

                    // racunanje adrese sledece promenljive istog tipa
                    // sabiranjem broja registara 
                    // trenutne promenljive sa njenom startnom adrese
                    var currentAddress = digitalInAddresses[digital.RelativeAddress];
                    var quantity = (ushort)(Math.Floor((Math.Log(digital.ValidStates.Count, 2))));
                    var nextAddress = currentAddress + quantity;

                    retAddr = currentAddress;

                    if (nextAddress >= DigInStartAddr + DigInCount)
                        break;  // error, out of range

                    //digitalInAddresses[digital.RelativeAddress + 1] = nextAddress;
                    digitalInAddresses.Insert(digital.RelativeAddress + 1, nextAddress);
                    break;

                case VariableTypes.ANALOGIN:
                    break;

                default:
                    break;
            }

            return retAddr;
        }

        // mapiranje na OutAddresses - adrese za pisanje (komandovanje)
        public int MapToCommandAddress(ProcessVariable variable)
        {
            int retAddr = -1;
            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:

                    Digital digital = variable as Digital;

                    if (digital.RelativeAddress == 0)
                        //digitalOutAddresses[digital.RelativeAddress] = DigOutStartAddr;
                        digitalOutAddresses.Insert(digital.RelativeAddress, DigOutStartAddr);

                    var currentAddress = digitalOutAddresses[digital.RelativeAddress];
                    var quantity = (ushort)(Math.Floor((Math.Log(digital.ValidCommands.Count, 2))));
                    var nextAddress = currentAddress + quantity;
                    retAddr = currentAddress;

                    if (nextAddress >= DigOutStartAddr + DigInCount)
                        break;  // error, out of range

                    //digitalOutAddresses[digital.RelativeAddress + 1] = nextAddress;
                    digitalOutAddresses.Insert(digital.RelativeAddress + 1, nextAddress);
                    break;

                case VariableTypes.ANALOGIN:
                    break;
                default:
                    break;
            }

            return retAddr;
        }

        public ProcessVariable GetProcessVariableByAddress(ushort address)
        {
            ProcessVariable pv;
            ProcessVariables.TryGetValue(address, out pv);
            return pv;
        }

        public void AddProcessVariable(ProcessVariable variable)
        {
            var readAddr = MapToAcqAddress(variable);
            var writeAddr = MapToCommandAddress(variable);

            ProcessVariables.Add((ushort)readAddr, variable);
            ProcessVariables.Add((ushort)writeAddr, variable);
        }

    }
}
