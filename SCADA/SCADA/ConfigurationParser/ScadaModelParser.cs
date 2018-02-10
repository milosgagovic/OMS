using OMSSCADACommon;
using PCCommon;
using SCADA.RealtimeDatabase;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;


namespace SCADA.ConfigurationParser
{
    public class ScadaModelParser
    {
        private string basePath;

        private DBContext dbContext = null;

        public ScadaModelParser(string basePath = "")
        {
            this.basePath = basePath == "" ? Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName : basePath;
            dbContext = new DBContext();
        }

        public bool DeserializeScadaModel(string deserializationSource = "ScadaModel.xml")
        {
            string message = string.Empty;
            string configurationName = deserializationSource;
            string source = Path.Combine(basePath, configurationName);
           
            // to do: DODATI DA SE PRVO OBRISE BAZA. neka promenljiva koja ce sluziti kao indikacija svima koji koriste pvs i rtus da stanu za taj period cleean i brisanja

            try
            {
                XElement xdocument = XElement.Load(source);

                // access RTUS, DIGITALS, ANALOGS, COUNTERS from ScadaModel root
                IEnumerable<XElement> elements = xdocument.Elements();

                var rtus = xdocument.Element("RTUS").Elements("RTU").ToList();
                //var digitals = xdocument.Element("Digitals").Elements("Digital").ToList();
                var digitals = (from dig in xdocument.Element("Digitals").Elements("Digital")
                                orderby (int)dig.Element("RelativeAddress")
                                select dig).ToList();

                // order will be important here too...
                var analogs = xdocument.Element("Analogs").Elements("Analog").ToList();
                var counters = xdocument.Element("Counters").Elements("Counter").ToList();

                // parsing RTUS
                if (rtus.Count != 0)
                {
                    foreach (var rtu in rtus)
                    {
                        RTU newRtu;
                        string uniqueName = (string)rtu.Element("Name");

                        // if RTU with that name does not already exist?
                        if (!dbContext.Database.RTUs.ContainsKey(uniqueName))
                        {
                            byte address = (byte)(int)rtu.Element("Address");

                            bool freeSpaceForDigitals = (bool)rtu.Element("FreeSpaceForDigitals");

                            string stringProtocol = (string)rtu.Element("Protocol");
                            IndustryProtocols protocol = (IndustryProtocols)Enum.Parse(typeof(IndustryProtocols), stringProtocol);

                            int digOutStartAddr = (int)rtu.Element("DigOutStartAddr");
                            int digInStartAddr = (int)rtu.Element("DigInStartAddr");
                            int anaInStartAddr = (int)rtu.Element("AnaInStartAddr");
                            int anaOutStartAddr = (int)rtu.Element("AnaOutStartAddr");
                            int counterStartAddr = (int)rtu.Element("CounterStartAddr");

                            int digOutCount = (int)rtu.Element("DigOutCount");
                            int digInCount = (int)rtu.Element("DigInCount");
                            int anaInCount = (int)rtu.Element("AnaInCount");
                            int anaOutCount = (int)rtu.Element("AnaOutCount");
                            int counterCount = (int)rtu.Element("CounterCount");

                            if (digOutCount != digInCount)
                            {
                                message = string.Format("Invalid config: RTU - {0}: Value of DigOutCount must be the same as Value of DigInCount", uniqueName);
                                Console.WriteLine(message);
                                return false;
                            }

                            newRtu = new RTU()
                            {
                                Name = uniqueName,
                                Address = address,
                                FreeSpaceForDigitals = freeSpaceForDigitals,
                                Protocol = protocol,

                                DigOutStartAddr = digOutStartAddr,
                                DigInStartAddr = digInStartAddr,
                                AnaInStartAddr = anaInStartAddr,
                                AnaOutStartAddr = anaOutStartAddr,
                                CounterStartAddr = counterStartAddr,

                                DigOutCount = digOutCount,
                                DigInCount = digInCount,
                                AnaInCount = anaInCount,
                                AnaOutCount = anaOutCount,
                                CounterCount = counterCount
                            };

                            dbContext.AddRTU(newRtu);
                        }
                        else
                        {
                            message = string.Format("Invalid config: There is multiple RTUs with Name={0}!", uniqueName);
                            Console.WriteLine(message);
                            return false;
                        }
                    }
                }
                else
                {
                    message = string.Format("Invalid config: file must contain at least 1 RTU!");
                    Console.WriteLine(message);
                    return false;
                }

                //parsing DIGITALS. ORDER OF RELATIVE ADDRESSES IS IMPORTANT
                if (digitals.Count != 0)
                {
                    foreach (var d in digitals)
                    {
                        string procContr = (string)d.Element("ProcContrName");

                        // does RTU exists?
                        RTU associatedRtu;
                        if ((associatedRtu = dbContext.GetRTUByName(procContr)) != null)
                        {
                            Digital newDigital = new Digital();

                            // SETTING ProcContrName
                            newDigital.ProcContrName = procContr;

                            string uniqueName = (string)d.Element("Name");

                            // variable with that name does not exists in db?
                            if (!dbContext.Database.ProcessVariablesName.ContainsKey(uniqueName))
                            {
                                // SETTING Name
                                newDigital.Name = uniqueName;

                                // SETTING State                             
                                string stringCurrentState = (string)d.Element("State");
                                States stateValue = (States)Enum.Parse(typeof(States), stringCurrentState);
                                newDigital.State = stateValue;

                                // SETTING Command parameter - for initializing Simulator with last command
                                string lastCommandString = (string)d.Element("Command");
                                CommandTypes command = (CommandTypes)Enum.Parse(typeof(CommandTypes), lastCommandString);

                                // SETTING Class
                                string digDevClass = (string)d.Element("Class");
                                DigitalDeviceClasses devClass = (DigitalDeviceClasses)Enum.Parse(typeof(DigitalDeviceClasses), digDevClass);
                                newDigital.Class = devClass;

                                // SETTING RelativeAddress
                                ushort relativeAddress = (ushort)(int)d.Element("RelativeAddress");
                                newDigital.RelativeAddress = relativeAddress;

                                var hasCommands = d.Element("ValidCommands");
                                if (hasCommands.HasElements)
                                {
                                    var validCommands = hasCommands.Elements("Command").ToList();

                                    // SETTING ValidCommands
                                    foreach (var xElementCommand in validCommands)
                                    {
                                        string stringCommand = (string)xElementCommand;
                                        CommandTypes validCommand = (CommandTypes)Enum.Parse(typeof(CommandTypes), stringCommand);
                                        newDigital.ValidCommands.Add(validCommand);
                                    }
                                }
                                else
                                {
                                    message = string.Format("Invalid config: Variable = {0} does not contain commands.", uniqueName);
                                    Console.WriteLine(message);
                                    return false;
                                }

                                var hasStates = d.Element("ValidStates");
                                if (hasStates.HasElements)
                                {
                                    var validStates = hasStates.Elements("State").ToList();

                                    // SETTING ValidStates
                                    foreach (var xElementState in validStates)
                                    {
                                        string stringState = (string)xElementState;
                                        States state = (States)Enum.Parse(typeof(States), stringState);
                                        newDigital.ValidStates.Add(state);
                                    }
                                }
                                else
                                {
                                    message = string.Format("Invalid config: Variable = {0} does not contain states.", uniqueName);
                                    Console.WriteLine(message);
                                    return false;
                                }

                                ushort calculatedRelativeAddres;
                                if (associatedRtu.TryMap(newDigital, out calculatedRelativeAddres))
                                {
                                    if (relativeAddress == calculatedRelativeAddres)
                                    {
                                        if (associatedRtu.MapProcessVariable(newDigital))
                                        {
                                            dbContext.AddProcessVariable(newDigital);
                                        }
                                    }
                                    else
                                    {
                                        message = string.Format("Invalid config: Variable = {0} RelativeAddress = {1} is not valid.", uniqueName, relativeAddress);
                                        Console.WriteLine(message);
                                        return false;
                                    }
                                }

                            }
                            else
                            {
                                message = string.Format("Invalid config: Name = {0} is not unique. Variable already exists", uniqueName);
                                Console.WriteLine(message);
                                return false;
                            }

                        }
                        else
                        {
                            message = string.Format("Invalid config: Parsing Digitals, ProcContrName = {0} does not exists.", procContr);
                            Console.WriteLine(message);
                            return false;
                        }
                    }
                }

                // to do: 
                if (analogs.Count != 0)
                {

                }

                // to do:
                if (counters.Count != 0)
                {

                }

                Console.WriteLine("Configuration passed successfully.");

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            catch (XmlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return false;
            }

            return true;
        }

        public void SerializeScadaModel(string serializationTarget = "ScadaModel.xml")
        {
            string target = Path.Combine(basePath, serializationTarget);

            XElement scadaModel = new XElement("ScadaModel");

            XElement rtus = new XElement("RTUS");
            XElement digitals = new XElement("Digitals");
            XElement analogs = new XElement("Analogs");
            XElement counters = new XElement("Counters");

            var rtusSnapshot = dbContext.Database.RTUs.ToArray();
            foreach (var rtu in rtusSnapshot)
            {
                XElement rtuEl = new XElement(
                    "RTU",
                    new XElement("Address", rtu.Value.Address),
                    new XElement("Name", rtu.Value.Name),
                    new XElement("FreeSpaceForDigitals", rtu.Value.FreeSpaceForDigitals),
                    new XElement("Protocol", Enum.GetName(typeof(IndustryProtocols), rtu.Value.Protocol)),
                    new XElement("DigOutStartAddr", rtu.Value.DigOutStartAddr),
                    new XElement("DigInStartAddr", rtu.Value.DigInStartAddr),
                    new XElement("AnaOutStartAddr", rtu.Value.AnaOutStartAddr),
                    new XElement("AnaInStartAddr", rtu.Value.AnaInStartAddr),
                    new XElement("CounterStartAddr", rtu.Value.CounterStartAddr),
                    new XElement("DigOutCount", rtu.Value.DigOutCount),
                    new XElement("DigInCount", rtu.Value.DigInCount),
                    new XElement("AnaInCount", rtu.Value.AnaInCount),
                    new XElement("AnaOutCount", rtu.Value.AnaOutCount),
                    new XElement("CounterCount", rtu.Value.CounterCount)
                    );

                rtus.Add(rtuEl);
            }

            var pvsSnapshot = dbContext.Database.ProcessVariablesName.ToArray().OrderBy(pv => pv.Value.RelativeAddress);
            foreach (var pv in pvsSnapshot)
            {
                switch (pv.Value.Type)
                {
                    case VariableTypes.DIGITAL:

                        Digital dig = pv.Value as Digital;

                        XElement validCommands = new XElement("ValidCommands");
                        XElement validStates = new XElement("ValidStates");

                        foreach (var state in dig.ValidStates)
                        {
                            validStates.Add(new XElement("State", Enum.GetName(typeof(States), state)));
                        }

                        foreach (var command in dig.ValidCommands)
                        {
                            validCommands.Add(new XElement("Command", Enum.GetName(typeof(CommandTypes), command)));
                        }

                        XElement digEl = new XElement(
                            "Digital",
                                new XElement("Name", dig.Name),
                                new XElement("State", dig.State),
                                new XElement("Command", dig.Command),
                                new XElement("ProcContrName", dig.ProcContrName),
                                new XElement("RelativeAddress", dig.RelativeAddress),
                                new XElement("Class", Enum.GetName(typeof(DigitalDeviceClasses), dig.Class)),
                                validCommands,
                                validStates
                            );

                        digitals.Add(digEl);

                        break;

                    case VariableTypes.ANALOG:
                        Analog analog = pv.Value as Analog;

                        // to do: 

                        break;

                }
            }

            scadaModel.Add(rtus);
            scadaModel.Add(digitals);
            scadaModel.Add(analogs);
            scadaModel.Add(counters);

            var xdocument = new XDocument(scadaModel);
            try
            {
                xdocument.Save(target);
                Console.WriteLine("Serializing ScadaModel succeed.");
            }
            catch (Exception)
            {

                throw;
            }            
        }

        public void SwapConfigs(string config1, string config2)
        {
            string config1path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, config1);
            string config2path = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, config2);

            File.Move(config1path, "temp.txt");
            File.Move(config2path, config1path);
            File.Move("temp.txt", config2path);
        }
    }
}
