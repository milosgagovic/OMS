using PCCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SCADA.RealtimeDatabase.Model
{
    public class RTU
    {

        private ConcurrentDictionary<ushort, string> PVsAddressAndNames = null;
        private DBContext dbContext = null;

        public IndustryProtocols Protocol { get; set; }

        // (Modbus slave address), value in range 1 - 247 (0 - broadcast)
        public Byte Address { get; set; }

        public string Name { get; set; }

        public bool FreeSpaceForDigitals { get; set; }

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

        // to do: add for analog and counter
        public int MappedDig { get; set; }

        // locks that support single writers and multiple readers
        private ReaderWriterLockSlim digInLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim digOutLock = new ReaderWriterLockSlim();

        // to do: add locks for analogs and counter 

        // List of mapperd reading Addresses
        private List<int> digitalInAddresses;
        private List<int> analogInAddresses;

        // List of mapped commanding Addresses
        private List<int> digitalOutAddresses;
        private List<int> analogOutAddresses;

        private List<int> counterAddresses;

        public RTU()
        {
            this.PVsAddressAndNames = new ConcurrentDictionary<ushort, string>();
            dbContext = new DBContext();


            MappedDig = 0;

            digitalInAddresses = new List<int>(DigInCount);
            analogInAddresses = new List<int>(AnaInCount);
            digitalOutAddresses = new List<int>(DigOutCount);
            analogOutAddresses = new List<int>(AnaOutCount);
            counterAddresses = new List<int>(CounterCount);
        }


        public int GetAcqAddress(ProcessVariable variable)
        {
            int retAddress = -1;
            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:
                    Digital digital = variable as Digital;

                    digInLock.EnterReadLock();
                    retAddress = digitalInAddresses[digital.RelativeAddress];
                    digInLock.ExitReadLock();

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

                    digOutLock.EnterReadLock();
                    retAddress = digitalOutAddresses[digital.RelativeAddress];
                    digOutLock.ExitReadLock();
                    break;
                case VariableTypes.ANALOGIN:
                    break;
                default:
                    break;
            }

            return retAddress;
        }


        /// <summary>
        /// Mapping to Acquisition address in memory of concrete RTU. It is based
        /// on ProcessVariable RelativeAddress property (offset in array of Process
        /// variables of same type)
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="isSuccessfull"></param>
        /// <returns></returns>
        private int MapToAcqAddress(ProcessVariable variable)
        {
            int retAddr = -1;

            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:

                    Digital digital = variable as Digital;


                    // 1st variable of type Digital, starts on 1st address of DigitalInputs memory (inputs are for acquistion)
                    if (digital.RelativeAddress == 0)
                    {
                        digInLock.EnterWriteLock();
                        try
                        {
                            digitalInAddresses.Insert(digital.RelativeAddress, DigInStartAddr);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {

                        }
                        finally
                        {
                            digInLock.ExitWriteLock();
                        }
                    }

                    digInLock.EnterReadLock();
                    var currentAcqAddress = digitalInAddresses[digital.RelativeAddress];
                    digInLock.ExitReadLock();

                    // this is address that we need currently
                    retAddr = currentAcqAddress;

                    // if we already reached the end of memory for this type of Process Variable
                    // in this Process Controller, than we do not have to calculate nextAddress
                    if (FreeSpaceForDigitals != false)
                    {
                        // calculating address of next variable of same type, 
                        // by adding number of registers (quantity)
                        // with starting address of current variable
                        var quantity = (ushort)(Math.Floor((Math.Log(digital.ValidStates.Count, 2))));
                        var nextAddress = currentAcqAddress + quantity;

                        // error, out of range. impossible to insert next variable of same type
                        if (nextAddress >= DigInStartAddr + DigInCount)
                        {
                            FreeSpaceForDigitals = false;
                            break;
                        }

                        digInLock.EnterWriteLock();
                        try
                        {
                            digitalInAddresses.Insert(digital.RelativeAddress + 1, nextAddress);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {

                        }
                        finally
                        {
                            digInLock.ExitWriteLock();
                        }
                    }

                    break;

                case VariableTypes.ANALOGIN:
                    break;

                default:
                    break;
            }

            return retAddr;
        }

        /// <summary>
        /// Mapping to Commanding address in memory of concrete RTU. It is based
        /// on ProcessVariable RelativeAddress property (offset in array of Process
        /// variables of same type)
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="isSuccessfull"></param>
        /// <returns></returns>
        private int MapToCommandAddress(ProcessVariable variable)
        {
            int retAddr = -1;

            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:

                    Digital digital = variable as Digital;

                    // 1st variable of type Digital, starts on 1st address of DigitalOutputs memory (outputs are for commanding)
                    if (digital.RelativeAddress == 0)
                    {
                        digOutLock.EnterWriteLock();
                        try
                        {
                            digitalOutAddresses.Insert(digital.RelativeAddress, DigOutStartAddr);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {

                        }
                        finally
                        {
                            digOutLock.ExitWriteLock();
                        }
                    }

                    digOutLock.EnterReadLock();
                    var currentCommAddress = digitalOutAddresses[digital.RelativeAddress];
                    digOutLock.ExitReadLock();

                    // this is address that we need currently
                    retAddr = currentCommAddress;

                    // if we already reached the end of memory for this type of Process Variable
                    // in this Process Controller, than we do not have to calculate nextAddress
                    if (FreeSpaceForDigitals != false)
                    {
                        var quantity = (ushort)(Math.Floor((Math.Log(digital.ValidCommands.Count, 2))));
                        var nextAddress = currentCommAddress + quantity;

                        // error, out of range. impossible to insert next variable of same type
                        if (nextAddress >= DigOutStartAddr + DigInCount)
                        {
                            FreeSpaceForDigitals = false;
                            break;
                        }


                        digOutLock.EnterWriteLock();
                        try
                        {
                            digitalOutAddresses.Insert(digital.RelativeAddress + 1, nextAddress);
                        }
                        catch (ArgumentOutOfRangeException e)
                        {

                        }
                        finally
                        {
                            digOutLock.ExitWriteLock();
                        }
                    }

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

        /// <summary>
        /// Attempts to store variable by its address in RTU Memory.
        /// Mapping to reading/commanding address is done in this function.
        /// </summary>
        /// <param name="variable"></param>
        public bool MapProcessVariable(ProcessVariable variable)
        {
            bool isSuccessfull = false;

            var readAddr = MapToAcqAddress(variable);

            var writeAddr = MapToCommandAddress(variable);

            if (PVsAddressAndNames.TryAdd((ushort)readAddr, variable.Name)
                && PVsAddressAndNames.TryAdd((ushort)writeAddr, variable.Name))
            {
                isSuccessfull = true;
            }

            return isSuccessfull;
        }


        /// <summary>
        /// Check if it is possible to map new variable, calculates RelativeAddress based
        /// on previous mapped variables.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="relativeAddress"></param>
        /// <returns></returns>
        public bool TryMap(ProcessVariable variable, out ushort relativeAddress)
        {
            bool retVal = false;
            relativeAddress = ushort.MaxValue;

            switch (variable.Type)
            {
                case VariableTypes.DIGITAL:

                    Digital digital = variable as Digital;

                    int desiredDigIn = (ushort)(Math.Floor((Math.Log(digital.ValidStates.Count, 2))));
                    int desiredDigOut = (ushort)(Math.Floor((Math.Log(digital.ValidCommands.Count, 2))));

                    if (digitalInAddresses.Count + desiredDigIn <= DigInCount &&
                        digitalOutAddresses.Count + desiredDigOut <= DigOutCount)
                    {
                        relativeAddress = (ushort)MappedDig;
                        //digital.RelativeAddress = (ushort)MappedDig;
                        MappedDig++;
                        retVal = true;
                    }


                    break;
            }
            return retVal;
        }

    }
}
