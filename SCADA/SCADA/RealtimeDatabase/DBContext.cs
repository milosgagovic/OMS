using OMSSCADACommon;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase
{
    public class DBContext
    {
        public Database Database { get; set; }

        public DBContext()
        {
            Console.WriteLine("Instancing DbContext");
            Database = Database.Instance;
        }

        // iako jedino scada model parser dodaje, imamo citanje i pisanje sa vise strana, mora se sititi
        // to je to do...
        /// <summary>
        /// Attempts to add the specified key and value. Return true if success, 
        /// false if the key already exists.
        /// </summary>
        /// <param name="rtu"></param>
        public bool AddRTU(RTU rtu)
        {
            return Database.Instance.RTUs.TryAdd(rtu.Name, rtu);
        }

        /// <summary>
        /// Attempts to get value associated with the name. Returns value 
        /// if exists, otherwise null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RTU GetRTUByName(string name)
        {
            RTU rtu;
            Database.Instance.RTUs.TryGetValue(name, out rtu);

            return rtu;
        }

        public ConcurrentDictionary<string, RTU> GettAllRTUs()
        {
            return Database.RTUs;
        }

        /// <summary>
        /// Storing the Process Varible in dictionary. Key= pv.Name, Value=pv
        /// </summary>
        /// <param name="pv"></param>
        public void AddProcessVariable(ProcessVariable pv)
        {
            Console.WriteLine("Adding process variable Name= {0}",pv.Name);
            Database.Instance.ProcessVariablesName.TryAdd(pv.Name, pv);
        }

        /// <summary>
        /// Return Process Variable if exists; 
        /// otherwise pv=null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetProcessVariableByName(string name, out ProcessVariable pv)
        {
            return (Database.Instance.ProcessVariablesName.TryGetValue(name, out pv));
        }

        public List<ProcessVariable> GetAllProcessVariables()
        {
            return Database.ProcessVariablesName.Values.ToList();
        }

        // ovu metodu srediti
        public bool ApplyDelta(ScadaDelta delta)
        {
            bool retVal = false;

            List<ScadaElement> updateOperations = delta.UpdateOps;

            List<ScadaElement> insertOperations = delta.InsertOps;

            // not supported yet
            if (updateOperations.Count != 0)
            {
                return false;
            }

            // temporary storage for elements for add. key is RTU name, value is PVs
            Dictionary<string, List<ProcessVariable>> rtuElementsMap = new Dictionary<string, List<ProcessVariable>>();

            List<RTU> availableRtus = new List<RTU>();
            availableRtus = GettAllRTUs().Values.Where(r => r.FreeSpaceForDigitals == true).ToList();
            if (availableRtus.Count == 0)
            {
                Console.WriteLine("There is no available RTU");
            }
            else
            {

                foreach (var ar in availableRtus)
                {
                    rtuElementsMap.Add(ar.Name, new List<ProcessVariable>());
                }


                int insertedCount = 0;
                foreach (var insertEl in insertOperations)
                {
                    ProcessVariable pv;
                    // variable with that name does not exists in db?

                    // napraviti deep copy ako budes menjala transakciju? ili ostaviti ovako...

                    if (!GetProcessVariableByName(insertEl.Name, out pv))
                    {
                        bool isInsertionPossible = false;
                        switch (insertEl.Type)
                        {
                            case DeviceTypes.DIGITAL:

                                if (insertEl.ValidCommands.Count != insertEl.ValidStates.Count)
                                {
                                    Console.WriteLine("Element Name = {0} -> ValidCommands.Count!=ValidStates.Count", insertEl.Name);
                                    break;
                                }

                                Digital newDigital = new Digital()
                                {
                                    Name = insertEl.Name,
                                    ValidCommands = insertEl.ValidCommands,
                                    ValidStates = insertEl.ValidStates
                                };


                                foreach (var availableRtu in availableRtus)
                                {
                                    // there is no channel with RTU-2 currently
                                    // test this case sometime in the future xD
                                    if (availableRtu.Name == "RTU-2")
                                        continue;

                                    ushort relativeAddress;
                                    // possible mapping in this rtu

                                    //potrebno je odmah proveriti da li je ova relativna adresa sledeca u sekvenci mogucih za taj tip!
                                    if (availableRtu.TryMap(newDigital, out relativeAddress))
                                    {
                                        newDigital.RelativeAddress = relativeAddress;

                                        // map it and add it to db
                                        newDigital.ProcContrName = availableRtu.Name;

                                        // podrazumevamo da insertujemo prekidac u dozvoljenom - zeljenom stanju
                                        newDigital.State = States.CLOSED;
                                        newDigital.Command = CommandTypes.CLOSE;

                                        var elements = rtuElementsMap[availableRtu.Name];
                                        elements.Add(newDigital);

                                        insertedCount++;
                                        isInsertionPossible = true;
                                        break;
                                    }
                                }

                                break;

                            default:
                                // all other - not supported yet, will return false
                                break;

                        }

                        if (!isInsertionPossible)
                            break; // nece se dodati nista! prepare nije moguc
                    }
                    else
                    {
                        Console.WriteLine("Invalid config: Name = {0} is not unique. Variable already exists", insertEl.Name);
                        break;
                    }
                }

                // ovo je kritican deo, jer se ipak promeni stanje :S 
                // srediti za sledeci sprint
                // only if it is possible to insert all, otherwise nothing 
                if (insertedCount == insertOperations.Count)
                {
                    int addedCount = 0;


                    foreach (var rtuElements in rtuElementsMap)
                    {

                        var availableRtu = GetRTUByName(rtuElements.Key);
                        var elements = rtuElementsMap[rtuElements.Key];

                        foreach (var elForAdd in elements)
                        {
                            if (availableRtu.MapProcessVariable(elForAdd))
                            {
                                AddProcessVariable(elForAdd);
                                addedCount++;
                            }
                        }

                    }

                    // SERIJALIZACIJU BAZE, TJ UPIS U FAJL OVDE RADITI
                    if (addedCount == insertOperations.Count)
                    {
                        retVal = true;
                    }

                }

            }

            return retVal;
        }
    }
}
