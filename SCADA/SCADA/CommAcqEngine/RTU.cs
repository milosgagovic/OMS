using PCCommon;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace SCADA.RealtimeDatabase
{
    public class RTU
    {

        // ne treba
        private Dictionary<ushort, ProcessVariable> ProcessVariables = null;
        private ConcurrentDictionary<ushort, string> PVsAddressAndNames = null;
        private DBContext dbContext = null;

        public IndustryProtocols Protocol { get; set; }

        // (Modbus slave address) 
        //  value in range 1 - 247 (0 - broadcast)
        public Byte Address { get; set; }

        public string Name { get; set; }

        // controller pI/O starting Addresses
        public int DigOutStartAddr { get; set; }
        public int DigInStartAddr { get; set; }
        public int AnaInStartAddr { get; set; }
        public int AnaOutStartAddr { get; set; }
        public int CounterStartAddr { get; set; }

        // number of pI/O
        public int DigOutCount { get; set; }
        public int DigInCount { get; set; }
        public int AnaInCount { get; set; }
        public int AnaOutCount { get; set; }
        public int CounterCount { get; set; }

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
            this.ProcessVariables = new Dictionary<ushort, ProcessVariable>();
            this.PVsAddressAndNames = new ConcurrentDictionary<ushort, string>();
            dbContext = new DBContext();

            digitalInAddresses = new List<int>(DigInCount);
            analogInAddresses = new List<int>(AnaInCount);
            digitalOutAddresses = new List<int>(DigOutCount);
            analogOutAddresses = new List<int>(AnaOutCount);
            counterAddresses = new List<int>(CounterCount);
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

        // liste ne moraju da budu konkurentne posto im se pristupa samo kad se dodaje, i to sekvencijalno

        // mapiranje - za svaku varijablu racunanje adrese u memoriji odgovarajuceg kontrolera
        // mapiramo na InAdresses - adrese za citanje (akvizicija je u pitanju)
        // mapiranje se vrsi na osnovu relativne adrese promenljive
        // i broja pI/O koje ona zauzima. 
        // relativna adresa je pozicija promenljive u nizu promenljivih istog tipa

        // poziva se prvi put na pocetku, na citanju konfiguracije
        // i posle svaki put kad se dodaje nesto novo. ali to su sve sekvencijalni pozivi!
        // znaci ne moramo da koristimo konkurent queue
        private int MapToAcqAddress(ProcessVariable variable, out bool isSuccessfull)
        {
            isSuccessfull = false;

            // i ovo se mora lockovati. ukoliko vise promenljivih insertujemo odjednom?
            // mada to realno nije moguce jer samo na jednom mestu, akd dodje delta, se insertuju promenljive. iz jedne niti
            int retAddr = -1;
            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:

                    Digital digital = variable as Digital;

                    // ovo je prva promenljiva tipa digital, i ona pocinje na
                    // prvoj adresi tog tipa 
                    if (digital.RelativeAddress == 0)
                        digitalInAddresses.Insert(digital.RelativeAddress, DigInStartAddr);

                    // racunanje adrese sledece promenljive istog tipa
                    // sabiranjem broja registara 
                    // trenutne promenljive sa njenom startnom adrese
                    var currentAddress = digitalInAddresses[digital.RelativeAddress];
                    var quantity = (ushort)(Math.Floor((Math.Log(digital.ValidStates.Count, 2))));
                    var nextAddress = currentAddress + quantity;

                    retAddr = currentAddress;

                    // error, out of range
                    if (nextAddress >= DigInStartAddr + DigInCount)
                        break;

                    digitalInAddresses.Insert(digital.RelativeAddress + 1, nextAddress);
                    isSuccessfull = true;


                    break;

                case VariableTypes.ANALOGIN:
                    break;

                default:
                    break;
            }

            return retAddr;
        }

        // mapiranje na OutAddresses - adrese za pisanje (komandovanje)
        private int MapToCommandAddress(ProcessVariable variable, out bool isSuccessfull)
        {
            isSuccessfull = false;
            int retAddr = -1;
            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:

                    Digital digital = variable as Digital;

                    if (digital.RelativeAddress == 0)
                        digitalOutAddresses.Insert(digital.RelativeAddress, DigOutStartAddr);

                    var currentAddress = digitalOutAddresses[digital.RelativeAddress];
                    var quantity = (ushort)(Math.Floor((Math.Log(digital.ValidCommands.Count, 2))));
                    var nextAddress = currentAddress + quantity;
                    retAddr = currentAddress;

                    // error, out of range
                    if (nextAddress >= DigOutStartAddr + DigInCount)
                        break;

                    digitalOutAddresses.Insert(digital.RelativeAddress + 1, nextAddress);
                    isSuccessfull = true;
                    break;

                case VariableTypes.ANALOGIN:
                    break;
                default:
                    break;
            }

            return retAddr;
        }

        /// <summary>
        /// Return Process Variable if exists, null if not.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool GetProcessVariableByAddress(ushort address, out ProcessVariable pv)
        {
            // prvo nadjemo ovde ime promenljive po adresi, i onda u bazi nadjemo promnljivu po imenu.
            // sticenje u bazi?

            string pvName;

            if (PVsAddressAndNames.TryGetValue(address, out pvName))
            {

                return (dbContext.GetProcessVariableByName(pvName, out pv));
            }
            else
            {
                pv = null;
                return false;
            }
        }

        // mozda ovde da cuvano samo ime varijable?
        /// <summary>
        /// Storing variables by its address in RTU Memory.
        /// Mapping to reading/commanding address is done in this function.
        /// </summary>
        /// <param name="variable"></param>
        public bool MapProcessVariable(ProcessVariable variable)
        {
            bool isSuccessfull = false;

            var readAddr = MapToAcqAddress(variable, out isSuccessfull);
            if (isSuccessfull)
            {
                var writeAddr = MapToCommandAddress(variable, out isSuccessfull);
                if (isSuccessfull)
                {
                    if (PVsAddressAndNames.TryAdd((ushort)readAddr, variable.Name)
                        && PVsAddressAndNames.TryAdd((ushort)writeAddr, variable.Name))
                    {
                        isSuccessfull = true;
                    }

                }
            }

            return isSuccessfull;
        }

    }
}
