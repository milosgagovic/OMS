using OMSSCADACommon;
using PCCommon;
using SCADA.RealtimeDatabase;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SCADA.ConfigurationParser
{
    // to do: ili staviti concurent dictionary u bazu, ili lock pristup...
    public class ScadaModelParser
    {
        private string configurationPath;
        XmlDocument doc;

        private DBContext dbContext = null;

        public ScadaModelParser(string configPath)
        {
            configurationPath = configPath;

            doc = new XmlDocument();
            dbContext = new DBContext();
        }

        public bool DoParse()
        {
            string message = string.Empty;

            try
            {
                XElement xdocument = XElement.Load(configurationPath);

                // access RTUS, DIGITALS, ANALOGS, COUNTERS from ScadaModel root
                IEnumerable<XElement> elements = xdocument.Elements();

                var rtus = xdocument.Element("RTUS").Elements("RTU").ToList();
                var digitals = xdocument.Element("Digitals").Elements("Digital").ToList();
                var analogs = xdocument.Element("Analogs").Elements("Analog").ToList();
                var counters = xdocument.Element("Counters").Elements("Counter").ToList();

                // parsing RTUS
                if (rtus.Count != 0)
                {
                    foreach (var rtu in rtus)
                    {
                        RTU newRtu;
                        string name = (string)rtu.Element("Name");

                        // if does not already exist
                        if ((newRtu = dbContext.GetRTUByName(name)) == null)
                        {
                            byte address = (byte)(int)rtu.Element("Address");

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
                                // error! mora biti isto...dodati to i za analogne kasnije nekad...
                            }

                            newRtu = new RTU()
                            {
                                Name = name,
                                Address = address,
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

                    }
                }
                else
                {
                    // error: ivalid config, return!
                }

                // parsing DIGITALS
                if (digitals.Count != 0)
                {
                    foreach (var d in digitals)
                    {
                        string procContr = (string)d.Element("ProcContrName");

                        // does RTU exists?
                        RTU associatedRtu;
                        if ((associatedRtu = dbContext.GetRTUByName(procContr)) != null)
                        {
                            // SETTING Command parameter is hard coded in constructor...
                            Digital newDigital = new Digital();

                            // SETTING ProcContrName
                            newDigital.ProcContrName = procContr;

                            string name = (string)d.Element("Name");


                            ProcessVariable pv;
                            // variable with that name does not exists in db?
                            if (!dbContext.GetProcessVariableByName(name, out pv))
                            {

                                // SETTING Name
                                newDigital.Name = name;

                                // SETTING State                             
                                string stringCurrentState = (string)d.Element("State");
                                States stateValue = (States)Enum.Parse(typeof(States), stringCurrentState);
                                newDigital.State = stateValue;

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
                                        CommandTypes command = (CommandTypes)Enum.Parse(typeof(CommandTypes), stringCommand);
                                        newDigital.ValidCommands.Add(command);
                                    }
                                }
                                else
                                {
                                    message = string.Format("Invalid config: Variable = {0} does not contain commands.", name);
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
                                    message = string.Format("Invalid config: Variable = {0} does not contain commands.", name);
                                    return false;
                                }

                                if (associatedRtu.MapProcessVariable(newDigital))
                                {
                                    dbContext.AddProcessVariable(newDigital);
                                }
                            }
                            else
                            {
                                message = string.Format("Invalid config: Name = {0} is not unique. Variable already exists", name);
                                return false;
                            }

                        }
                        else
                        {
                            message = string.Format("Invalid config: ProcContrName = {0} does not exists.", procContr);
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
            }
            catch (XmlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

    }
}
